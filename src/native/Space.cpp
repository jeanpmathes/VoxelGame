#include "stdafx.h"

bool Material::IsAnimated() const
{
    return animationID.has_value();
}

Space::Space(NativeClient& nativeClient) :
    m_nativeClient(nativeClient),
    m_camera(nativeClient),
    m_light(nativeClient),
    m_resultBufferAllocator(nativeClient, D3D12_RESOURCE_STATE_RAYTRACING_ACCELERATION_STRUCTURE),
    m_scratchBufferAllocator(nativeClient, D3D12_RESOURCE_STATE_UNORDERED_ACCESS),
    m_indexBuffer(*this)
{
    DirectX::XMVECTOR windDirection = XMLoadFloat3(&m_windDirection);
    windDirection = DirectX::XMVector3Normalize(windDirection);
    XMStoreFloat3(&m_windDirection, windDirection);
}

void Space::PerformInitialSetupStepOne(const ComPtr<ID3D12CommandQueue> commandQueue)
{
    REQUIRE(m_meshes.IsEmpty());

    auto* spaceCommandGroup = &m_commandGroup; // Improves the naming of the objects.
    INITIALIZE_COMMAND_ALLOCATOR_GROUP(m_nativeClient.GetDevice(), spaceCommandGroup, D3D12_COMMAND_LIST_TYPE_DIRECT);
    m_commandGroup.Reset(0);

    CreateTLAS();

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

    const MeshObject* mesh = m_meshes[static_cast<size_t>(handle)].get();

    if (mesh->GetMaterial().IsAnimated() && mesh->GetActiveIndex().has_value())
    {
        m_animations[mesh->GetMaterial().animationID.value()].UpdateMesh(*mesh);
    }
}

size_t Space::ActivateMeshObject(const MeshObject::Handle handle)
{
    MeshObject* mesh = m_meshes[static_cast<size_t>(handle)].get();
    REQUIRE(!mesh->GetActiveIndex());

    size_t index = m_activeMeshes.Push(mesh);
    
    m_activatedMeshes.emplace(index);

    if (mesh->GetMaterial().IsAnimated())
    {
        m_animations[mesh->GetMaterial().animationID.value()].AddMesh(*mesh);
    }

    return index;
}

void Space::DeactivateMeshObject(const size_t index)
{
    MeshObject* mesh = m_activeMeshes.Pop(index);
    
    m_activatedMeshes.erase(index);

    if (mesh->GetMaterial().IsAnimated())
    {
        m_animations[mesh->GetMaterial().animationID.value()].RemoveMesh(*mesh);
    }
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

std::pair<Allocation<ID3D12Resource>, UINT> Space::GetIndexBuffer(const UINT vertexCount)
{
    return m_indexBuffer.GetIndexBuffer(vertexCount);
}

void Space::Update(double)
{
    m_globalConstantBufferMapping->lightDirection = m_light.GetDirection();

    for (const auto& mesh : m_meshes)
    {
        mesh->Update();
    }

    m_camera.Update();
}

void Space::Render(double delta, Allocation<ID3D12Resource> outputBuffer)
{
    m_renderTime += delta;
    m_globalConstantBufferMapping->time = static_cast<float>(m_renderTime);

    {
        PIXScopedEvent(GetCommandList().Get(), PIX_COLOR_DEFAULT, L"Space");

        EnqueueUploads();
        UpdateGlobalShaderResources();
        m_globalShaderResources.Bind(GetCommandList());
        RunAnimations();
        BuildAccelerationStructures();
        DispatchRays();
        CopyOutputToBuffer(outputBuffer);
    }

    TRY_DO(GetCommandList()->Close());
}

void Space::CleanupRender()
{
    for (const auto handle : m_modifiedMeshes)
    {
        MeshObject* mesh = m_meshes[static_cast<size_t>(handle)].get();
        REQUIRE(mesh != nullptr);

        mesh->CleanupMeshUpload();
    }
    m_modifiedMeshes.clear();

    m_indexBuffer.CleanupRender();
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
    m_globalConstantBufferSize = sizeof(GlobalConstantBuffer);
    m_globalConstantBuffer = util::AllocateConstantBuffer(m_nativeClient, &m_globalConstantBufferSize);
    NAME_D3D12_OBJECT(m_globalConstantBuffer);

    TRY_DO(m_globalConstantBuffer.Map(&m_globalConstantBufferMapping, 1));

    m_globalConstantBufferMapping.Write({
        .time = 0.0f,
        .windDirection = m_windDirection,
        .lightDirection = DirectX::XMFLOAT3{0.0f, -1.0f, 0.0f},
        .minLight = 0.4f,
        .textureSize = DirectX::XMUINT2{1, 1}
    });
}

void Space::InitializePipelineResourceViews(const SpacePipeline& pipeline)
{
    UpdateOutputResourceView();
    UpdateTopLevelAccelerationStructureView();

    {
        std::optional<DirectX::XMUINT2> textureSize = std::nullopt;

        auto getTexturesCountInSlot = [&](UINT count) -> std::optional<UINT>
        {
            if (count == 0) return std::nullopt;
            return count;
        };
        auto fillSlots = [&](const ShaderResources::Table::Entry entry,
                             const UINT base,
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

        m_globalConstantBufferMapping->textureSize = textureSize.value_or(DirectX::XMUINT2{1, 1});
    }
}

bool Space::CreateRaytracingPipeline(const SpacePipeline& pipelineDescription)
{
    m_textureSlot1.size = std::max(pipelineDescription.description.textureCountFirstSlot, 1u);
    m_textureSlot2.size = std::max(pipelineDescription.description.textureCountSecondSlot, 1u);

    nv_helpers_dx12::RayTracingPipelineGenerator pipeline(GetDevice());

    bool ok = true;
    std::tie(m_shaderBlobs, ok) = CompileShaderLibraries(m_nativeClient, pipelineDescription, pipeline);
    if (!ok) return false;

    m_rayGenSignature = CreateRayGenSignature();
    NAME_D3D12_OBJECT(m_rayGenSignature);

    m_missSignature = CreateMissSignature();
    NAME_D3D12_OBJECT(m_missSignature);

    for (UINT index = 0; index < pipelineDescription.description.materialCount; index++)
    {
        m_materials.push_back(SetupMaterial(pipelineDescription.materials[index], index, pipeline));
    }

    CreateAnimations(pipelineDescription);
    
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

            for (auto& animation : m_animations)
            {
                animation.SetupResourceLayout(&compute);
            }
        },
        GetDevice());

    NAME_D3D12_OBJECT(m_globalShaderResources.GetComputeRootSignature());
    NAME_D3D12_OBJECT(m_globalShaderResources.GetGraphicsRootSignature());

    InitializeAnimations();
    
    pipeline.SetMaxPayloadSize(8 * sizeof(float));
    pipeline.SetMaxAttributeSize(2 * sizeof(float));
    pipeline.SetMaxRecursionDepth(2);

    m_rtStateObject = pipeline.Generate(m_globalShaderResources.GetComputeRootSignature());
    NAME_D3D12_OBJECT(m_rtStateObject);

    TRY_DO(m_rtStateObject->QueryInterface(IID_PPV_ARGS(&m_rtStateObjectProperties)));

    return true;
}

std::pair<std::vector<ComPtr<IDxcBlob>>, bool>
Space::CompileShaderLibraries(NativeClient& nativeClient,
                              const SpacePipeline& pipelineDescription,
                              nv_helpers_dx12::RayTracingPipelineGenerator& pipeline)
{
    std::vector<ComPtr<IDxcBlob>> shaderBlobs(pipelineDescription.description.shaderCount);
    
    UINT currentSymbolIndex = 0;
    bool ok = true;
    
    for (UINT shader = 0; shader < pipelineDescription.description.shaderCount; shader++)
    {
        if (pipelineDescription.shaderFiles[shader].symbolCount > 0)
        {
            shaderBlobs[shader] = CompileShader(
                pipelineDescription.shaderFiles[shader].path,
                L"", L"lib_6_7",
                VG_SHADER_REGISTRY(nativeClient),
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
        else
        {
            shaderBlobs[shader] = CompileShader(
                pipelineDescription.shaderFiles[shader].path,
                L"Main", L"cs_6_7",
                VG_SHADER_REGISTRY(nativeClient),
                pipelineDescription.description.onShaderLoadingError);

            ok &= shaderBlobs[shader] != nullptr;
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

void Space::CreateAnimations(const SpacePipeline& pipeline)
{
    std::map<UINT, UINT> animationShaderIndexToID;

    for (UINT shaderIndex = 0; shaderIndex < pipeline.description.shaderCount; shaderIndex++)
    {
        const ShaderFileDescription& shaderFile = pipeline.shaderFiles[shaderIndex];
        if (shaderFile.symbolCount > 0) continue;

        const UINT animationID = static_cast<UINT>(m_animations.size());
        const ComPtr<IDxcBlob> blob = m_shaderBlobs[shaderIndex];

        constexpr UINT offset = 3;
        m_animations.emplace_back(blob, offset + animationID);

        animationShaderIndexToID[shaderIndex] = animationID;
    }

    for (UINT materialID = 0; materialID < pipeline.description.materialCount; materialID++)
    {
        const MaterialDescription& materialDescription = pipeline.materials[materialID];
        if (materialDescription.isAnimated)
        {
            UINT animationID = animationShaderIndexToID[materialDescription.animationShaderIndex];
            m_materials[materialID]->animationID = animationID;
        }
    }
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

void Space::SetupAnimationResourceLayout(ShaderResources::Description* description)
{
    for (auto& animation : m_animations)
    {
        animation.SetupResourceLayout(description);
    }
}

void Space::InitializeAnimations()
{
    for (auto& animation : m_animations)
    {
        animation.Initialize(m_nativeClient, m_globalShaderResources.GetComputeRootSignature());
    }
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

    util::ReAllocateBuffer(&m_sbtStorage,
                           m_nativeClient, sbtSize, D3D12_RESOURCE_FLAG_NONE,
                           D3D12_RESOURCE_STATE_GENERIC_READ, D3D12_HEAP_TYPE_UPLOAD);
    NAME_D3D12_OBJECT(m_sbtStorage);

    m_sbtHelper.Generate(m_sbtStorage.Get(), m_rtStateObjectProperties.Get());
}

void Space::EnqueueUploads()
{
    for (const auto handle : m_modifiedMeshes)
    {
        MeshObject* mesh = m_meshes[static_cast<size_t>(handle)].get();
        REQUIRE(mesh != nullptr);

        mesh->EnqueueMeshUpload(GetCommandList());
    }
}

void Space::RunAnimations()
{
    for (auto& animation : m_animations)
    {
        animation.Run(GetCommandList());
    }
}

void Space::BuildAccelerationStructures()
{
    std::vector<ID3D12Resource*> uavs;

    for (auto& animation : m_animations)
    {
        animation.CreateBLAS(GetCommandList(), &uavs);
    }

    for (const auto handle : m_modifiedMeshes)
    {
        MeshObject* mesh = m_meshes[static_cast<size_t>(handle)].get();
        REQUIRE(mesh != nullptr);

        mesh->CreateBLAS(GetCommandList(), &uavs);
    }

    m_resultBufferAllocator.CreateBarriers(GetCommandList(), std::move(uavs));

    CreateTLAS();
    UpdateTopLevelAccelerationStructureView();
}

void Space::CreateTLAS()
{
    nv_helpers_dx12::TopLevelASGenerator topLevelASGenerator;
    
    for (const auto& mesh : m_activeMeshes)
    {
        // The CCW flag is used because DirectX uses left-handed coordinates.

        REQUIRE(mesh->GetActiveIndex().has_value());
        const UINT instanceID = static_cast<UINT>(mesh->GetActiveIndex().value());

        topLevelASGenerator.AddInstance(mesh->GetBLAS().result.GetAddress(), mesh->GetTransform(),
                                        instanceID, mesh->GetMaterial().index,
                                        static_cast<BYTE>(mesh->GetMaterial().flags),
                                        D3D12_RAYTRACING_INSTANCE_FLAG_TRIANGLE_FRONT_COUNTERCLOCKWISE);
    }

    UINT64 scratchSize, resultSize, instanceDescriptionSize;
    topLevelASGenerator.ComputeASBufferSizes(GetDevice().Get(), false, &scratchSize, &resultSize,
                                             &instanceDescriptionSize);

    const bool committed = m_nativeClient.SupportPIX();

    util::ReAllocateBuffer(&m_topLevelASBuffers.scratch, m_nativeClient, scratchSize,
                           D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS,
                           D3D12_RESOURCE_STATE_COMMON,
                           D3D12_HEAP_TYPE_DEFAULT,
                           committed);
    util::ReAllocateBuffer(&m_topLevelASBuffers.result, m_nativeClient, resultSize,
                           D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS,
                           D3D12_RESOURCE_STATE_RAYTRACING_ACCELERATION_STRUCTURE,
                           D3D12_HEAP_TYPE_DEFAULT,
                           committed);
    util::ReAllocateBuffer(&m_topLevelASBuffers.instanceDescription, m_nativeClient, instanceDescriptionSize,
                           D3D12_RESOURCE_FLAG_NONE,
                           D3D12_RESOURCE_STATE_GENERIC_READ,
                           D3D12_HEAP_TYPE_UPLOAD,
                           committed);

    NAME_D3D12_OBJECT(m_topLevelASBuffers.scratch);
    NAME_D3D12_OBJECT(m_topLevelASBuffers.result);
    NAME_D3D12_OBJECT(m_topLevelASBuffers.instanceDescription);

    topLevelASGenerator.Generate(m_commandGroup.commandList.Get(),
                                 m_topLevelASBuffers.scratch,
                                 m_topLevelASBuffers.result,
                                 m_topLevelASBuffers.instanceDescription);
}

void Space::DispatchRays() const
{
    const std::vector barriers = {
        CD3DX12_RESOURCE_BARRIER::Transition(
            m_outputResource.Get(), D3D12_RESOURCE_STATE_COPY_SOURCE,
            D3D12_RESOURCE_STATE_UNORDERED_ACCESS)
    };
    GetCommandList()->ResourceBarrier(static_cast<UINT>(barriers.size()), barriers.data());

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

void Space::UpdateOutputResourceView()
{
    if (!m_outputTextureEntry.IsValid()) return;

    if (!m_outputResourceFresh) return;
    m_outputResourceFresh = false;

    D3D12_UNORDERED_ACCESS_VIEW_DESC uavDesc = {};
    uavDesc.ViewDimension = D3D12_UAV_DIMENSION_TEXTURE2D;
    m_globalShaderResources.CreateUnorderedAccessView(m_outputTextureEntry, 0, {m_outputResource, &uavDesc});
}

void Space::UpdateTopLevelAccelerationStructureView()
{
    D3D12_SHADER_RESOURCE_VIEW_DESC srvDescription;
    srvDescription.Format = DXGI_FORMAT_UNKNOWN;
    srvDescription.ViewDimension = D3D12_SRV_DIMENSION_RAYTRACING_ACCELERATION_STRUCTURE;
    srvDescription.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
    srvDescription.RaytracingAccelerationStructure.Location = m_topLevelASBuffers.result.resource->
        GetGPUVirtualAddress();

    m_globalShaderResources.CreateShaderResourceView(m_bvhEntry, 0, {{}, &srvDescription});
}

void Space::UpdateGlobalShaderResources()
{
    std::set<size_t> meshesToRefresh = m_activatedMeshes;
    for (const auto handle : m_modifiedMeshes)
    {
        const MeshObject* mesh = m_meshes[static_cast<size_t>(handle)].get();
        REQUIRE(mesh != nullptr);

        std::optional<size_t> index = mesh->GetActiveIndex();
        if (!index.has_value()) continue;

        meshesToRefresh.insert(index.value());
    }

    for (auto& animation : m_animations)
    {
        animation.Update(m_globalShaderResources, GetCommandList());
    }

    m_globalShaderResources.RequestListRefresh(m_meshInstanceDataList, meshesToRefresh);
    m_globalShaderResources.RequestListRefresh(m_meshGeometryBufferList, meshesToRefresh);
    m_globalShaderResources.Update();

    m_activatedMeshes.clear();
}
