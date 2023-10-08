﻿#include "stdafx.h"

#undef min
#undef max

static constexpr UINT OUTPUT_DESCRIPTOR_OFFSET = 0;
static constexpr UINT BVH_DESCRIPTOR_OFFSET = 1;
static constexpr UINT TEXTURE_BASE_DESCRIPTOR_OFFSET = 2; // Has to be last as many textures are bound here.

Space::Space(NativeClient& nativeClient) :
    m_nativeClient(nativeClient),
    m_camera(nativeClient),
    m_light(nativeClient),
    m_resultBufferAllocator(nativeClient, D3D12_RESOURCE_STATE_RAYTRACING_ACCELERATION_STRUCTURE),
    m_scratchBufferAllocator(nativeClient, D3D12_RESOURCE_STATE_UNORDERED_ACCESS),
    m_indexBuffer(*this)
{
}

void Space::PerformInitialSetupStepOne(const ComPtr<ID3D12CommandQueue> commandQueue)
{
    REQUIRE(m_meshes.IsEmpty());

    auto* spaceCommandGroup = &m_commandGroup; // Improves the naming of the objects.
    INITIALIZE_COMMAND_ALLOCATOR_GROUP(GetDevice(), spaceCommandGroup, D3D12_COMMAND_LIST_TYPE_DIRECT);
    m_commandGroup.Reset(0);

    CreateTopLevelAS();

    m_commandGroup.Close();
    ID3D12CommandList* ppCommandLists[] = {m_commandGroup.commandList.Get()};
    commandQueue->ExecuteCommandLists(_countof(ppCommandLists), ppCommandLists);

    m_nativeClient.WaitForGPU();

    m_camera.Initialize();

    const D3D12_RESOURCE_DESC textureDescription = CD3DX12_RESOURCE_DESC::Tex2D(
        DXGI_FORMAT_B8G8R8A8_UNORM, 1, 1,
        1, 1,
        1, 0,
        D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS);
    m_sentinelTexture = util::AllocateResource<ID3D12Resource>(
        m_nativeClient, textureDescription,
        D3D12_HEAP_TYPE_DEFAULT, D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE);

    m_sentinelTextureViewDescription.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
    m_sentinelTextureViewDescription.Format = textureDescription.Format;
    m_sentinelTextureViewDescription.ViewDimension = D3D12_SRV_DIMENSION_TEXTURE2D;
    m_sentinelTextureViewDescription.Texture2DArray.ArraySize = textureDescription.DepthOrArraySize;
    m_sentinelTextureViewDescription.Texture2DArray.MipLevels = textureDescription.MipLevels;
}

void Space::PerformResolutionDependentSetup(const Resolution& resolution)
{
    m_resolution = resolution;
    CreateRaytracingOutputBuffer();
}

bool Space::PerformInitialSetupStepTwo(const SpacePipeline& pipeline)
{
    CreateGlobalConstBuffer();
    CreateShaderResourceHeap(pipeline);

    if (!CreateRaytracingPipeline(pipeline)) return false;

    CreateShaderBindingTable();

    return true;
}

MeshObject& Space::CreateMeshObject(const UINT materialIndex)
{
    std::unique_ptr<MeshObject> stored;

    if (m_meshPool.empty())
    {
        stored = std::make_unique<MeshObject>(m_nativeClient);
    }
    else
    {
        stored = std::move(m_meshPool.back());
        m_meshPool.pop_back();
    }

    auto& object = *stored;
    object.Initialize(materialIndex);

    const size_t index = m_meshes.Push(std::move(stored));
    object.AssociateWithHandle(static_cast<MeshObject::Handle>(index));

    return object;
}

void Space::MarkMeshObjectModified(MeshObject::Handle handle)
{
    m_modifiedMeshes.emplace(handle);
}

size_t Space::ActivateMeshObject(const MeshObject::Handle handle)
{
    MeshObject* mesh = m_meshes[static_cast<size_t>(handle)].get();
    REQUIRE(!mesh->GetActiveIndex());

    size_t index = m_activeMeshes.Push(mesh);
    
    m_activatedMeshes.emplace(index);

    return index;
}

void Space::DeactivateMeshObject(const size_t index)
{
    m_activeMeshes.Pop(index);
    m_activatedMeshes.erase(index);
}

void Space::ReturnMeshObject(const MeshObject::Handle handle)
{
    m_modifiedMeshes.erase(handle);
    m_meshPool.push_back(m_meshes.Pop(static_cast<size_t>(handle)));
}

const Material& Space::GetMaterial(const UINT index) const
{
    return *m_materials[index];
}

void Space::Reset(const UINT frameIndex)
{
    m_commandGroup.Reset(frameIndex);
}

void Space::EnqueueRenderSetup()
{
    std::vector<ID3D12Resource*> uavs;
    
    for (const auto handle : m_modifiedMeshes)
    {
        MeshObject* mesh = m_meshes[static_cast<size_t>(handle)].get();
        REQUIRE(mesh != nullptr);

        mesh->EnqueueMeshUpload(GetCommandList());
        mesh->CreateBLAS(GetCommandList(), &uavs);
    }

    m_resultBufferAllocator.CreateBarriers(GetCommandList(), std::move(uavs));

    CreateTopLevelAS();
    UpdateAccelerationStructureView();
    UpdateGlobalShaderResourceHeap();

    m_activatedMeshes.clear();
}

void Space::CleanupRenderSetup()
{
    for (const auto handle : m_modifiedMeshes)
    {
        MeshObject* mesh = m_meshes[static_cast<size_t>(handle)].get();
        REQUIRE(mesh != nullptr);
        
        mesh->CleanupMeshUpload();
    }
    m_modifiedMeshes.clear();
    
    m_indexBuffer.CleanupRenderSetup();
}

std::pair<Allocation<ID3D12Resource>, UINT> Space::GetIndexBuffer(const UINT vertexCount)
{
    return m_indexBuffer.GetIndexBuffer(vertexCount);
}

void Space::DispatchRays()
{
    const CD3DX12_RESOURCE_BARRIER barrier = CD3DX12_RESOURCE_BARRIER::Transition(
        m_outputResource.Get(), D3D12_RESOURCE_STATE_COPY_SOURCE,
        D3D12_RESOURCE_STATE_UNORDERED_ACCESS);
    GetCommandList()->ResourceBarrier(1, &barrier);

    GetCommandList()->SetComputeRootSignature(m_globalRootSignature.Get());

    GetCommandList()->SetDescriptorHeaps(1, m_globalShaderResourceHeap.GetAddressOf());

    GetCommandList()->SetComputeRootConstantBufferView(0, m_camera.GetCameraBufferAddress());
    GetCommandList()->SetComputeRootConstantBufferView(
        1, m_globalConstantBuffer.GetGPUVirtualAddress());
    GetCommandList()->SetComputeRootDescriptorTable(2, m_globalShaderResourceHeap.GetDescriptorHandleGPU());
    GetCommandList()->SetComputeRootDescriptorTable(3, m_instanceDataHeap);
    GetCommandList()->SetComputeRootDescriptorTable(4, m_geometryDataHeap);
    
    D3D12_DISPATCH_RAYS_DESC desc = {};

    desc.RayGenerationShaderRecord.StartAddress
        = m_sbtStorage.GetGPUVirtualAddress()
        + m_sbtHelper.GetRayGenSectionOffset();
    desc.RayGenerationShaderRecord.SizeInBytes = m_sbtHelper.GetRayGenSectionSize();

    desc.MissShaderTable.StartAddress
        = m_sbtStorage.GetGPUVirtualAddress()
        + m_sbtHelper.GetMissSectionOffset();
    desc.MissShaderTable.SizeInBytes = m_sbtHelper.GetMissSectionSize();
    desc.MissShaderTable.StrideInBytes = m_sbtHelper.GetMissEntrySize();

    desc.HitGroupTable.StartAddress
        = m_sbtStorage.GetGPUVirtualAddress()
        + m_sbtHelper.GetHitGroupSectionOffset();
    desc.HitGroupTable.SizeInBytes = m_sbtHelper.GetHitGroupSectionSize();
    desc.HitGroupTable.StrideInBytes = m_sbtHelper.GetHitGroupEntrySize();

    desc.Width = m_resolution.width;
    desc.Height = m_resolution.height;
    desc.Depth = 1;

    GetCommandList()->SetPipelineState1(m_rtStateObject.Get());
    GetCommandList()->DispatchRays(&desc);
}

void Space::CopyOutputToBuffer(const Allocation<ID3D12Resource> buffer) const
{
    D3D12_RESOURCE_BARRIER barriers[] = {
        CD3DX12_RESOURCE_BARRIER::Transition(
            m_outputResource.Get(), D3D12_RESOURCE_STATE_UNORDERED_ACCESS,
            D3D12_RESOURCE_STATE_COPY_SOURCE),
        CD3DX12_RESOURCE_BARRIER::Transition(
            buffer.Get(), D3D12_RESOURCE_STATE_RENDER_TARGET,
            D3D12_RESOURCE_STATE_COPY_DEST)
    };

    m_commandGroup.commandList->ResourceBarrier(_countof(barriers), barriers);

    m_commandGroup.commandList->CopyResource(buffer.Get(),
                                             m_outputResource.Get());

    const CD3DX12_RESOURCE_BARRIER barrier = CD3DX12_RESOURCE_BARRIER::Transition(
        buffer.Get(), D3D12_RESOURCE_STATE_COPY_DEST,
        D3D12_RESOURCE_STATE_RENDER_TARGET);
    m_commandGroup.commandList->ResourceBarrier(1, &barrier);
}

void Space::Update(const double delta)
{
    m_globalConstantBufferData.time += static_cast<float>(delta);
    m_globalConstantBufferData.lightDirection = m_light.GetDirection();

    for (const auto& mesh : m_meshes)
    {
        mesh->Update();
    }

    m_camera.Update();

    UpdateGlobalConstBuffer();
}

NativeClient& Space::GetNativeClient() const
{
    return m_nativeClient;
}

Camera* Space::GetCamera()
{
    return &m_camera;
}

Light* Space::GetLight()
{
    return &m_light;
}

ComPtr<ID3D12GraphicsCommandList4> Space::GetCommandList() const
{
    return m_commandGroup.commandList;
}

BLAS Space::AllocateBLAS(const UINT64 resultSize, const UINT64 scratchSize)
{
    return {
        .result = m_resultBufferAllocator.Allocate(resultSize),
        .scratch = m_scratchBufferAllocator.Allocate(scratchSize)
    };
}

ComPtr<ID3D12Device5> Space::GetDevice() const
{
    return m_nativeClient.GetDevice();
}

void Space::CreateGlobalConstBuffer()
{
    m_globalConstantBufferData = {
        .time = 0.0f,
        .lightDirection = DirectX::XMFLOAT3{0.0f, -1.0f, 0.0f},
        .minLight = 0.4f,
        .textureSize = DirectX::XMUINT2{1, 1}
    };

    m_globalConstantBufferSize = sizeof m_globalConstantBufferData;
    m_globalConstantBuffer = util::AllocateConstantBuffer(m_nativeClient, &m_globalConstantBufferSize);
    NAME_D3D12_OBJECT(m_globalConstantBuffer);

    TRY_DO(m_globalConstantBuffer.Map(&m_globalConstantBufferMapping));
    
    UpdateGlobalConstBuffer();
}

void Space::UpdateGlobalConstBuffer()
{
    m_globalConstantBufferMapping.Write(m_globalConstantBufferData);
}

void Space::CreateShaderResourceHeap(const SpacePipeline& pipeline)
{
    m_textureSlot1.size = std::max(pipeline.description.textureCountFirstSlot, 1u);
    m_textureSlot2.size = std::max(pipeline.description.textureCountSecondSlot, 1u);
    const UINT descriptorCount = TEXTURE_BASE_DESCRIPTOR_OFFSET + m_textureSlot1.size + m_textureSlot2.size;

    m_commonPipelineResourceHeap.Create(
        GetDevice(),
        descriptorCount,
        D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV,
        false);
    NAME_D3D12_OBJECT(m_commonPipelineResourceHeap);

    InitializePipelineResourceHeap(pipeline);
    UpdateGlobalShaderResourceHeap();
}

void Space::InitializePipelineResourceHeap(const SpacePipeline& pipeline)
{
    REQUIRE(m_commonPipelineResourceHeap.IsCreated());

    UpdateOutputResourceView();
    UpdateAccelerationStructureView();

    {
        std::optional<DirectX::XMUINT2> textureSize = std::nullopt;

        auto getTexturesCountInSlot = [&](UINT count) -> std::optional<UINT>
        {
            if (count == 0) return std::nullopt;
            return count;
        };
        auto fillSlots = [&](const UINT base, const std::optional<UINT> count, UINT* offset)
        {
            if (count.has_value())
            {
                textureSize = textureSize.value_or(pipeline.textures[base]->GetSize());

                for (UINT index = 0; index < count.value(); index++)
                {
                    const Texture* texture = pipeline.textures[base + index];
                    REQUIRE(texture != nullptr);
                    REQUIRE(texture->GetSize().x == textureSize.value().x);
                    REQUIRE(texture->GetSize().y == textureSize.value().y);

                    GetDevice()->CreateShaderResourceView(
                        texture->GetResource().Get(),
                        &texture->GetView(),
                        m_commonPipelineResourceHeap.GetDescriptorHandleCPU((*offset)++));
                }
            }
            else
            {
                GetDevice()->CreateShaderResourceView(
                    m_sentinelTexture.Get(),
                    &m_sentinelTextureViewDescription,
                    m_commonPipelineResourceHeap.GetDescriptorHandleCPU((*offset)++));
            }
        };

        UINT offset = TEXTURE_BASE_DESCRIPTOR_OFFSET;
        const UINT firstSlotArraySize = pipeline.description.textureCountFirstSlot;
        const UINT secondSlotArraySize = pipeline.description.textureCountSecondSlot;

        m_textureSlot1.offset = offset;
        fillSlots(0, getTexturesCountInSlot(firstSlotArraySize), &offset);

        m_textureSlot2.offset = offset;
        fillSlots(firstSlotArraySize, getTexturesCountInSlot(secondSlotArraySize), &offset);

        m_globalConstantBufferData.textureSize = textureSize.value_or(DirectX::XMUINT2{1, 1});
        UpdateGlobalConstBuffer();
    }
}

void Space::UpdateGlobalShaderResourceHeap()
{
    if (m_globalShaderResourceHeapSlots < m_activeMeshes.GetCapacity() || !m_globalShaderResourceHeap.IsCreated())
    {
        UpdateGSRHeapSize();
    }
    else if (!m_activatedMeshes.empty() || !m_modifiedMeshes.empty())
    {
        UpdateGSRHeapContents();
    }

    UpdateGSRHeapBase();
}

void Space::UpdateGSRHeapSize()
{
    do
    {
        m_globalShaderResourceHeapSlots = std::max(4u, m_globalShaderResourceHeapSlots * 2u);
    }
    while (m_globalShaderResourceHeapSlots < m_activeMeshes.GetCapacity());

    m_globalShaderResourceHeap.Create(
        GetDevice(),
        m_commonPipelineResourceHeap.GetDescriptorCount() + 2 * m_globalShaderResourceHeapSlots,
        D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV,
        true);
    NAME_D3D12_OBJECT(m_globalShaderResourceHeap);

    const UINT offset = m_commonPipelineResourceHeap.GetDescriptorCount();

    for (const MeshObject* mesh : m_activeMeshes)
    {
        auto [data, geometry] = GetTextureSlotIndices(mesh, offset);
        mesh->CreateInstanceResourceViews(m_globalShaderResourceHeap, data, geometry);
    }

    m_instanceDataHeap = m_globalShaderResourceHeap.GetDescriptorHandleGPU(offset);
    m_geometryDataHeap = m_globalShaderResourceHeap.GetDescriptorHandleGPU(offset + m_globalShaderResourceHeapSlots);
}

void Space::UpdateGSRHeapContents()
{
    const UINT offset = m_commonPipelineResourceHeap.GetDescriptorCount();

    for (const size_t activated : m_activatedMeshes)
    {
        const MeshObject* mesh = m_activeMeshes[activated];
        REQUIRE(mesh != nullptr);

        auto [data, geometry] = GetTextureSlotIndices(mesh, offset);
        mesh->CreateInstanceResourceViews(m_globalShaderResourceHeap, data, geometry);
    }

    for (const auto handle : m_modifiedMeshes)
    {
        const MeshObject* mesh = m_meshes[static_cast<size_t>(handle)].get();
        REQUIRE(mesh != nullptr);

        if (std::optional<size_t> index = mesh->GetActiveIndex(); !index.has_value() || m_activatedMeshes.
            contains(index.value())) continue;

        auto [data, geometry] = GetTextureSlotIndices(mesh, offset);
        mesh->CreateInstanceResourceViews(m_globalShaderResourceHeap, data, geometry);
    }
}

void Space::UpdateGSRHeapBase() const
{
    GetDevice()->CopyDescriptorsSimple(
        m_commonPipelineResourceHeap.GetDescriptorCount(),
        // todo: use two heaps (one for the changing stuff, one for textures), only copy textures after resize
        m_globalShaderResourceHeap.GetDescriptorHandleCPU(),
        m_commonPipelineResourceHeap.GetDescriptorHandleCPU(),
        D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
}

std::pair<UINT, UINT> Space::GetTextureSlotIndices(const MeshObject* mesh, const UINT offset) const
{
    REQUIRE(mesh->GetActiveIndex());

    const UINT index = static_cast<UINT>(*mesh->GetActiveIndex());
    return GetTextureSlotIndices(index, offset);
}

std::pair<UINT, UINT> Space::GetTextureSlotIndices(const UINT slot, const UINT offset) const
{
    const UINT data = offset + slot;
    const UINT geometry = offset + m_globalShaderResourceHeapSlots + slot;

    return {data, geometry};
}

void Space::UpdateOutputResourceView()
{
    if (!m_commonPipelineResourceHeap.IsCreated()) return;

    if (!m_outputResourceFresh) return;
    m_outputResourceFresh = false;

    {
        D3D12_UNORDERED_ACCESS_VIEW_DESC uavDesc = {};
        uavDesc.ViewDimension = D3D12_UAV_DIMENSION_TEXTURE2D;
        GetDevice()->CreateUnorderedAccessView(m_outputResource.Get(), nullptr, &uavDesc,
                                               m_commonPipelineResourceHeap.GetDescriptorHandleCPU(
                                                   OUTPUT_DESCRIPTOR_OFFSET));
    }
}

void Space::UpdateAccelerationStructureView() const
{
    REQUIRE(m_commonPipelineResourceHeap.IsCreated());

    {
        D3D12_SHADER_RESOURCE_VIEW_DESC srvDescription;
        srvDescription.Format = DXGI_FORMAT_UNKNOWN;
        srvDescription.ViewDimension = D3D12_SRV_DIMENSION_RAYTRACING_ACCELERATION_STRUCTURE;
        srvDescription.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
        srvDescription.RaytracingAccelerationStructure.Location = m_topLevelASBuffers.result.resource->
            GetGPUVirtualAddress();

        GetDevice()->CreateShaderResourceView(nullptr, &srvDescription,
                                              m_commonPipelineResourceHeap.
                                              GetDescriptorHandleCPU(BVH_DESCRIPTOR_OFFSET));
    }
}

bool Space::CreateRaytracingPipeline(const SpacePipeline& pipelineDescription)
{
    m_globalRootSignature = CreateGlobalRootSignature();
    NAME_D3D12_OBJECT(m_globalRootSignature);

    nv_helpers_dx12::RayTracingPipelineGenerator pipeline(GetDevice(), m_globalRootSignature);
    m_shaderBlobs = std::vector<ComPtr<IDxcBlob>>(pipelineDescription.description.shaderCount);

    UINT currentSymbolIndex = 0;

    for (UINT shader = 0; shader < pipelineDescription.description.shaderCount; shader++)
    {
        m_shaderBlobs[shader] = CompileShader(
            pipelineDescription.shaderFiles[shader].path,
            L"", L"lib_6_7",
            pipelineDescription.description.onShaderLoadingError);
        if (m_shaderBlobs[shader] == nullptr) return false;

        const UINT currentSymbolCount = pipelineDescription.shaderFiles[shader].symbolCount;

        std::vector<std::wstring> symbols;
        symbols.reserve(currentSymbolCount);

        for (UINT symbolOffset = 0; symbolOffset < currentSymbolCount; symbolOffset++)
        {
            symbols.push_back(pipelineDescription.symbols[currentSymbolIndex++]);
        }

        pipeline.AddLibrary(m_shaderBlobs[shader].Get(), symbols);
    }
    
    m_rayGenSignature = CreateRayGenSignature();
    NAME_D3D12_OBJECT(m_rayGenSignature);
    
    m_missSignature = CreateMissSignature();
    NAME_D3D12_OBJECT(m_missSignature);
    
    auto addLocalRootSignatureAssociation = [&](const ComPtr<ID3D12RootSignature>& rootSignature,
                                                const std::vector<std::wstring>& associatedSymbols)
    {
        pipeline.AddRootSignatureAssociation(rootSignature.Get(), true, associatedSymbols);
    };

    for (UINT index = 0; index < pipelineDescription.description.materialCount; index++)
    {
        auto material = std::make_unique<Material>();

        const MaterialDescription& description = pipelineDescription.materials[index];

        material->name = description.name;
        material->index = index * 2;
        material->isOpaque = description.opaque;

        if (description.visible) material->flags |= MaterialFlags::VISIBLE;
        if (description.shadowCaster) material->flags |= MaterialFlags::SHADOW_CASTER;

        auto addHitGroup = [&](const std::wstring& prefix,
                               const std::wstring& closestHitSymbol,
                               const std::wstring& anyHitSymbol,
                               const std::wstring& intersectionSymbol)
            -> std::tuple<std::wstring, ComPtr<ID3D12RootSignature>>
        {
            ComPtr<ID3D12RootSignature> rootSignature = CreateMaterialSignature();
            std::wstring hitGroup = prefix + L"_" + description.name;

            pipeline.AddHitGroup(hitGroup, closestHitSymbol, anyHitSymbol, intersectionSymbol);
            addLocalRootSignatureAssociation(rootSignature, {hitGroup});

            return {hitGroup, rootSignature};
        };

        std::tie(material->normalHitGroup, material->normalRootSignature)
            = addHitGroup(L"N",
                          description.normalClosestHitSymbol,
                          description.normalAnyHitSymbol,
                          description.normalIntersectionSymbol);

        std::tie(material->shadowHitGroup, material->shadowRootSignature)
            = addHitGroup(L"S",
                          description.shadowClosestHitSymbol,
                          description.shadowAnyHitSymbol,
                          description.shadowIntersectionSymbol);

        std::wstring normalIntersectionSymbol = description.normalIntersectionSymbol;
        std::wstring shadowIntersectionSymbol = description.shadowIntersectionSymbol;
        REQUIRE(normalIntersectionSymbol.empty() == shadowIntersectionSymbol.empty());

        material->geometryType = normalIntersectionSymbol.empty()
                                     ? D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES
                                     : D3D12_RAYTRACING_GEOMETRY_TYPE_PROCEDURAL_PRIMITIVE_AABBS;

        UINT64 materialConstantBufferSize = sizeof MaterialConstantBuffer;
        material->materialConstantBuffer = util::AllocateConstantBuffer(m_nativeClient, &materialConstantBufferSize);
        NAME_D3D12_OBJECT(material->materialConstantBuffer);

        MaterialConstantBuffer materialConstantBufferData = {.index = index};
        TRY_DO(util::MapAndWrite(material->materialConstantBuffer, materialConstantBufferData));

#if defined(VG_DEBUG)
        std::wstring debugName = pipelineDescription.materials[index].name;
        // DirectX seems to return the same pointer for both signatures, so naming them is not very useful.
        TRY_DO(material->normalRootSignature->SetName((L"RT Material RS " + debugName).c_str()));
        TRY_DO(material->shadowRootSignature->SetName((L"RT Material RS " + debugName).c_str()));
#endif

        m_materials.push_back(std::move(material));
    }

    addLocalRootSignatureAssociation(m_rayGenSignature, {L"RayGen"});
    addLocalRootSignatureAssociation(m_missSignature, {L"Miss", L"ShadowMiss"});
    
    pipeline.SetMaxPayloadSize(8 * sizeof(float));
    pipeline.SetMaxAttributeSize(2 * sizeof(float));
    pipeline.SetMaxRecursionDepth(2);

    m_rtStateObject = pipeline.Generate();
    NAME_D3D12_OBJECT(m_rtStateObject);

    TRY_DO(m_rtStateObject->QueryInterface(IID_PPV_ARGS(&m_rtStateObjectProperties)));

    return true;
}

void Space::CreateRaytracingOutputBuffer()
{
    D3D12_RESOURCE_DESC outputDescription = {};
    outputDescription.DepthOrArraySize = 1;
    outputDescription.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D;

    outputDescription.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    outputDescription.Flags = D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;
    outputDescription.Width = m_resolution.width;
    outputDescription.Height = m_resolution.height;
    outputDescription.Layout = D3D12_TEXTURE_LAYOUT_UNKNOWN;
    outputDescription.MipLevels = 1;
    outputDescription.SampleDesc.Count = 1;

    m_outputResource = util::AllocateResource<ID3D12Resource>(
        m_nativeClient,
        outputDescription,
        D3D12_HEAP_TYPE_DEFAULT,
        D3D12_RESOURCE_STATE_COPY_SOURCE);
    NAME_D3D12_OBJECT(m_outputResource);

    m_outputResourceFresh = true;
    UpdateOutputResourceView();
}

ComPtr<ID3D12RootSignature> Space::CreateGlobalRootSignature() const
{
    nv_helpers_dx12::RootSignatureGenerator rsc;

    rsc.AddRootParameter(D3D12_ROOT_PARAMETER_TYPE_CBV, 0); // Camera Data (b0, space0)
    rsc.AddRootParameter(D3D12_ROOT_PARAMETER_TYPE_CBV, 1); // Global Data (b1, space0)
    
    rsc.AddHeapRangesParameter({
        // Constant Buffer Views:
        /* none */
        // Unordered Access Views:
        /* Output Texture (u0, space0) */ {
            0, 1, 0,
            D3D12_DESCRIPTOR_RANGE_TYPE_UAV, OUTPUT_DESCRIPTOR_OFFSET
        },
        // Shader Resource Views:
        /* BVH (t0, space0) */ {
            0, 1, 0,
            D3D12_DESCRIPTOR_RANGE_TYPE_SRV, BVH_DESCRIPTOR_OFFSET
        },
        /* First Texture Slot (t0, space1) */ {
            0, m_textureSlot1.size, 1,
            D3D12_DESCRIPTOR_RANGE_TYPE_SRV, m_textureSlot1.offset
        },
        /* Second Texture Slot (t0, space2) */ {
            0, m_textureSlot2.size, 2,
            D3D12_DESCRIPTOR_RANGE_TYPE_SRV, m_textureSlot2.offset
        }
    });

    rsc.AddHeapRangesParameter({
        /* Instance Data (b3, space0) */ {
            3, UINT_MAX, 0,
            D3D12_DESCRIPTOR_RANGE_TYPE_CBV, 0
        },
    });

    rsc.AddHeapRangesParameter({
        /* Geometry Buffer (t1, space0) */ {
            1, UINT_MAX, 0,
            D3D12_DESCRIPTOR_RANGE_TYPE_SRV, 0
        },
    });

    return rsc.Generate(GetDevice().Get(), false);
}

ComPtr<ID3D12RootSignature> Space::CreateRayGenSignature() const
{
    nv_helpers_dx12::RootSignatureGenerator rsc;
    return rsc.Generate(GetDevice().Get(), true);
}

ComPtr<ID3D12RootSignature> Space::CreateMissSignature() const
{
    nv_helpers_dx12::RootSignatureGenerator rsc;
    return rsc.Generate(GetDevice().Get(), true);
}

ComPtr<ID3D12RootSignature> Space::CreateMaterialSignature() const
{
    nv_helpers_dx12::RootSignatureGenerator rsc;

    rsc.AddRootParameter(D3D12_ROOT_PARAMETER_TYPE_CBV, 2); // Material Data (b2, space0)
    
    return rsc.Generate(GetDevice().Get(), true);
}

void Space::CreateShaderBindingTable()
{
    m_sbtHelper.Reset();

    REQUIRE(m_globalShaderResourceHeap.IsCreated());
    REQUIRE(!m_outputResourceFresh);

    m_sbtHelper.AddRayGenerationProgram(L"RayGen", {});

    m_sbtHelper.AddMissProgram(L"Miss", {});
    m_sbtHelper.AddMissProgram(L"ShadowMiss", {});

    for (const auto& material : m_materials)
    {
        auto* materialCB = reinterpret_cast<void*>(material->materialConstantBuffer.GetGPUVirtualAddress());
        m_sbtHelper.AddHitGroup(material->normalHitGroup, {materialCB});
        m_sbtHelper.AddHitGroup(material->shadowHitGroup, {materialCB});
    }

    const uint32_t sbtSize = m_sbtHelper.ComputeSBTSize();

    m_sbtStorage = util::AllocateBuffer(
        m_nativeClient, sbtSize, D3D12_RESOURCE_FLAG_NONE,
        D3D12_RESOURCE_STATE_GENERIC_READ, D3D12_HEAP_TYPE_UPLOAD);
    NAME_D3D12_OBJECT(m_sbtStorage);

    m_sbtHelper.Generate(m_sbtStorage.Get(), m_rtStateObjectProperties.Get());
}

void Space::CreateTopLevelAS()
{
    nv_helpers_dx12::TopLevelASGenerator topLevelASGenerator;

    for (const auto& mesh : m_activeMeshes)
    {
        // The CCW flag is used because DirectX uses left-handed coordinates.

        REQUIRE(mesh->GetActiveIndex());
        const UINT instanceID = static_cast<UINT>(*mesh->GetActiveIndex());

        topLevelASGenerator.AddInstance(mesh->GetBLAS().result.GetAddress(), mesh->GetTransform(),
                                        instanceID, mesh->GetMaterial().index,
                                        static_cast<BYTE>(mesh->GetMaterial().flags),
                                        D3D12_RAYTRACING_INSTANCE_FLAG_TRIANGLE_FRONT_COUNTERCLOCKWISE);
    }

    UINT64 scratchSize, resultSize, instanceDescriptionSize;
    topLevelASGenerator.ComputeASBufferSizes(GetDevice().Get(), false, &scratchSize, &resultSize,
                                             &instanceDescriptionSize);

    const bool committed = m_nativeClient.SupportPIX();

    m_topLevelASBuffers.scratch = util::AllocateBuffer(m_nativeClient, scratchSize,
                                                       D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS,
                                                       D3D12_RESOURCE_STATE_COMMON,
                                                       D3D12_HEAP_TYPE_DEFAULT,
                                                       committed);
    m_topLevelASBuffers.result = util::AllocateBuffer(m_nativeClient, resultSize,
                                                      D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS,
                                                      D3D12_RESOURCE_STATE_RAYTRACING_ACCELERATION_STRUCTURE,
                                                      D3D12_HEAP_TYPE_DEFAULT,
                                                      committed);
    m_topLevelASBuffers.instanceDescription = util::AllocateBuffer(m_nativeClient, instanceDescriptionSize,
                                                                   D3D12_RESOURCE_FLAG_NONE,
                                                                   D3D12_RESOURCE_STATE_GENERIC_READ,
                                                                   D3D12_HEAP_TYPE_UPLOAD,
                                                                   committed);

    NAME_D3D12_OBJECT(m_topLevelASBuffers.scratch);
    NAME_D3D12_OBJECT(m_topLevelASBuffers.result);
    NAME_D3D12_OBJECT(m_topLevelASBuffers.instanceDescription);

    topLevelASGenerator.Generate(m_commandGroup.commandList.Get(),
                                 m_topLevelASBuffers.scratch.Get(),
                                 m_topLevelASBuffers.result.Get(),
                                 m_topLevelASBuffers.instanceDescription.Get());
}
