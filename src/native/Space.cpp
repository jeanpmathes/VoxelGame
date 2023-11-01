#include "stdafx.h"

#undef min
#undef max

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

    if (!CreateRaytracingPipeline(pipeline)) return false;

    InitializePipelineResourceViews(pipeline);
    m_globalShaderResources.Update();

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

    std::set<size_t> meshesToRefresh = m_activatedMeshes;
    for (const auto handle : m_modifiedMeshes)
    {
        const MeshObject* mesh = m_meshes[static_cast<size_t>(handle)].get();
        REQUIRE(mesh != nullptr);

        std::optional<size_t> index = mesh->GetActiveIndex();
        if (!index.has_value()) continue;

        meshesToRefresh.insert(index.value());
    }

    m_globalShaderResources.RequestListRefresh(m_meshInstanceDataList, meshesToRefresh);
    m_globalShaderResources.RequestListRefresh(m_meshGeometryBufferList, meshesToRefresh);
    m_globalShaderResources.Update();

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

    m_globalShaderResources.Bind(GetCommandList());

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

void Space::InitializePipelineResourceViews(const SpacePipeline& pipeline)
{
    UpdateOutputResourceView();
    UpdateAccelerationStructureView();

    {
        std::optional<DirectX::XMUINT2> textureSize = std::nullopt;

        auto getTexturesCountInSlot = [&](UINT count) -> std::optional<UINT>
        {
            if (count == 0) return std::nullopt;
            return count;
        };
        auto fillSlots = [&](const ShaderResources::Table::Entry entry, const UINT base,
                             const std::optional<UINT> count)
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

                    m_globalShaderResources.CreateShaderResourceView(entry, index,
                                                                     {texture->GetResource(), &texture->GetView()});
                }
            }
            else
            {
                m_globalShaderResources.CreateShaderResourceView(entry, 0,
                                                                 {
                                                                     m_sentinelTexture,
                                                                     &m_sentinelTextureViewDescription
                                                                 });
            }
        };

        const UINT firstSlotArraySize = pipeline.description.textureCountFirstSlot;
        const UINT secondSlotArraySize = pipeline.description.textureCountSecondSlot;

        fillSlots(m_textureSlot1.entry, 0, getTexturesCountInSlot(firstSlotArraySize));
        fillSlots(m_textureSlot2.entry, firstSlotArraySize, getTexturesCountInSlot(secondSlotArraySize));

        m_globalConstantBufferData.textureSize = textureSize.value_or(DirectX::XMUINT2{1, 1});
        UpdateGlobalConstBuffer();
    }
}

void Space::UpdateOutputResourceView()
{
    if (!m_outputTextureEntry.IsValid()) return;

    if (!m_outputResourceFresh) return;
    m_outputResourceFresh = false;

    D3D12_UNORDERED_ACCESS_VIEW_DESC uavDesc = {};
    uavDesc.ViewDimension = D3D12_UAV_DIMENSION_TEXTURE2D;
    m_globalShaderResources.CreateUnorderedAccessView(m_outputTextureEntry, 0, {m_outputResource, &uavDesc});
}

void Space::UpdateAccelerationStructureView()
{
    D3D12_SHADER_RESOURCE_VIEW_DESC srvDescription;
    srvDescription.Format = DXGI_FORMAT_UNKNOWN;
    srvDescription.ViewDimension = D3D12_SRV_DIMENSION_RAYTRACING_ACCELERATION_STRUCTURE;
    srvDescription.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
    srvDescription.RaytracingAccelerationStructure.Location = m_topLevelASBuffers.result.resource->
        GetGPUVirtualAddress();

    m_globalShaderResources.CreateShaderResourceView(m_bvhEntry, 0, {{}, &srvDescription});
}

bool Space::CreateRaytracingPipeline(const SpacePipeline& pipelineDescription)
{
    m_textureSlot1.size = std::max(pipelineDescription.description.textureCountFirstSlot, 1u);
    m_textureSlot2.size = std::max(pipelineDescription.description.textureCountSecondSlot, 1u);

    nv_helpers_dx12::RayTracingPipelineGenerator pipeline(GetDevice());

    bool ok = true;
    std::tie(m_shaderBlobs, ok) = CompileShaderLibraries(pipelineDescription, pipeline);
    if (!ok) return false;

    m_rayGenSignature = CreateRayGenSignature();
    NAME_D3D12_OBJECT(m_rayGenSignature);

    m_missSignature = CreateMissSignature();
    NAME_D3D12_OBJECT(m_missSignature);

    for (UINT index = 0; index < pipelineDescription.description.materialCount; index++)
    {
        m_materials.push_back(SetupMaterial(pipelineDescription.materials[index], index, pipeline));
    }

    pipeline.AddRootSignatureAssociation(m_rayGenSignature.Get(), true, {L"RayGen"});
    pipeline.AddRootSignatureAssociation(m_missSignature.Get(), true, {L"Miss", L"ShadowMiss"});

    m_globalShaderResources.Initialize( // todo: use two static heaps (one for the changing stuff, one for textures)
        [&](auto&)
        {
        },
        [&](auto& compute)
        {
            SetupStaticResourceLayout(&compute);
            SetupDynamicResourceLayout(&compute);
        },
        GetDevice());

    NAME_D3D12_OBJECT(m_globalShaderResources.GetComputeRootSignature());
    NAME_D3D12_OBJECT(m_globalShaderResources.GetGraphicsRootSignature());

    pipeline.SetMaxPayloadSize(8 * sizeof(float));
    pipeline.SetMaxAttributeSize(2 * sizeof(float));
    pipeline.SetMaxRecursionDepth(2);

    m_rtStateObject = pipeline.Generate(m_globalShaderResources.GetComputeRootSignature());
    NAME_D3D12_OBJECT(m_rtStateObject);

    TRY_DO(m_rtStateObject->QueryInterface(IID_PPV_ARGS(&m_rtStateObjectProperties)));

    return true;
}

std::pair<std::vector<ComPtr<IDxcBlob>>, bool> Space::CompileShaderLibraries(const SpacePipeline& pipelineDescription,
                                                                             nv_helpers_dx12::RayTracingPipelineGenerator
                                                                             & pipeline)
{
    std::vector<ComPtr<IDxcBlob>> shaderBlobs(pipelineDescription.description.shaderCount);
    
    UINT currentSymbolIndex = 0;
    bool ok = true;
    
    for (UINT shader = 0; shader < pipelineDescription.description.shaderCount; shader++)
    {
        shaderBlobs[shader] = CompileShader(
            pipelineDescription.shaderFiles[shader].path,
            L"", L"lib_6_7",
            pipelineDescription.description.onShaderLoadingError);

        if (shaderBlobs[shader] != nullptr)
        {
            const UINT currentSymbolCount = pipelineDescription.shaderFiles[shader].symbolCount;

            std::vector<std::wstring> symbols;
            symbols.reserve(currentSymbolCount);

            for (UINT symbolOffset = 0; symbolOffset < currentSymbolCount; symbolOffset++)
            {
                symbols.push_back(pipelineDescription.symbols[currentSymbolIndex++]);
            }

            pipeline.AddLibrary(shaderBlobs[shader].Get(), symbols);
        }
        else
        {
            ok = false;
        }
    }

    return {shaderBlobs, ok};
}

std::unique_ptr<Material> Space::SetupMaterial(const MaterialDescription& description, const UINT index,
                                               nv_helpers_dx12::RayTracingPipelineGenerator& pipeline) const
{
    auto material = std::make_unique<Material>();

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
        pipeline.AddRootSignatureAssociation(rootSignature.Get(), true, {hitGroup});

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

    const std::wstring normalIntersectionSymbol = description.normalIntersectionSymbol;
    const std::wstring shadowIntersectionSymbol = description.shadowIntersectionSymbol;
    REQUIRE(normalIntersectionSymbol.empty() == shadowIntersectionSymbol.empty());

    material->geometryType = normalIntersectionSymbol.empty()
                                 ? D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES
                                 : D3D12_RAYTRACING_GEOMETRY_TYPE_PROCEDURAL_PRIMITIVE_AABBS;

    UINT64 materialConstantBufferSize = sizeof MaterialConstantBuffer;
    material->materialConstantBuffer = util::AllocateConstantBuffer(m_nativeClient, &materialConstantBufferSize);
    NAME_D3D12_OBJECT(material->materialConstantBuffer);

    const MaterialConstantBuffer materialConstantBufferData = {.index = index};
    TRY_DO(util::MapAndWrite(material->materialConstantBuffer, materialConstantBufferData));

#if defined(VG_DEBUG)
    const std::wstring debugName = description.name;
    // DirectX seems to return the same pointer for both signatures, so naming them is not very useful.
    TRY_DO(material->normalRootSignature->SetName((L"RT Material RS " + debugName).c_str()));
    TRY_DO(material->shadowRootSignature->SetName((L"RT Material RS " + debugName).c_str()));
#endif

    return material;
}

void Space::SetupStaticResourceLayout(ShaderResources::Description* description)
{
    description->AddConstantBufferView(m_camera.GetCameraBufferAddress(), {.reg = 0});
    description->AddConstantBufferView(m_globalConstantBuffer.GetGPUVirtualAddress(), {.reg = 1});

    m_commonResourceTable = description->AddHeapDescriptorTable([this](auto& table)
    {
        m_outputTextureEntry = table.AddUnorderedAccessView({.reg = 0});
        m_bvhEntry = table.AddShaderResourceView({.reg = 0});
        m_textureSlot1.entry = table.AddShaderResourceView({.reg = 0, .space = 1}, m_textureSlot1.size);
        m_textureSlot2.entry = table.AddShaderResourceView({.reg = 0, .space = 2}, m_textureSlot2.size);
    });
}

void Space::SetupDynamicResourceLayout(ShaderResources::Description* description)
{
    const std::function<UINT(MeshObject* const&)> getIndexOfMesh = [this](auto* mesh)
    {
        REQUIRE(mesh != nullptr);
        REQUIRE(mesh->GetActiveIndex().has_value());

        return static_cast<UINT>(mesh->GetActiveIndex().value());
    };

    m_meshInstanceDataList = description->AddConstantBufferViewDescriptorList({.reg = 3, .space = 0},
                                                                              CreateSizeGetter(&m_activeMeshes),
                                                                              [this](const UINT index)
                                                                              {
                                                                                  return m_activeMeshes[index]->
                                                                                      GetInstanceDataViewDescriptor();
                                                                              },
                                                                              CreateListBuilder(
                                                                                  &m_activeMeshes, getIndexOfMesh));

    m_meshGeometryBufferList = description->AddShaderResourceViewDescriptorList({.reg = 1, .space = 0},
        CreateSizeGetter(&m_activeMeshes),
        [this](const UINT index)
        {
            return m_activeMeshes[index]->
                GetGeometryBufferViewDescriptor();
        },
        CreateListBuilder(&m_activeMeshes, getIndexOfMesh));
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
