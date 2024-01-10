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
}

void Space::PerformInitialSetupStepOne(const ComPtr<ID3D12CommandQueue> commandQueue)
{
    REQUIRE(m_drawables.IsEmpty());

    auto* spaceCommandGroup = &m_commandGroup; // Improves the naming of the objects.
    INITIALIZE_COMMAND_ALLOCATOR_GROUP(m_nativeClient.GetDevice(), spaceCommandGroup, D3D12_COMMAND_LIST_TYPE_DIRECT);
    m_commandGroup.Reset(0);

    CreateTLAS();

    m_commandGroup.Close();
    ID3D12CommandList* ppCommandLists[] = {GetCommandList().Get()};
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
    m_globalShaderResources->Update();

    CreateShaderBindingTable();

    return true;
}

Mesh& Space::CreateMesh(const UINT materialIndex)
{
    return m_meshes.Create([&](Mesh& mesh) { mesh.Initialize(materialIndex); });
}

Effect& Space::CreateEffect(RasterPipeline* pipeline)
{
    return m_effects.Create([&](Effect& effect) { effect.Initialize(*pipeline); });
}

void Space::MarkDrawableModified(Drawable* drawable)
{
    drawable->Accept(Drawable::Visitor::Empty()
                     .OnMesh([&](Mesh& mesh)
                     {
                         m_meshes.MarkModified(mesh);

                         if (mesh.GetMaterial().IsAnimated() && mesh.GetActiveIndex().has_value())
                         {
                             m_animations[mesh.GetMaterial().animationID.value()].UpdateMesh(mesh);
                         }
                     })
                     .OnEffect([&](Effect& effect)
                     {
                         m_effects.MarkModified(effect);
                     })
                     .OnElseFail());
}

void Space::ActivateDrawable(Drawable* drawable)
{
    drawable->Accept(Drawable::Visitor::Empty()
                     .OnMesh([&](Mesh& mesh)
                     {
                         m_meshes.Activate(mesh);

                         if (mesh.GetMaterial().IsAnimated())
                         {
                             m_animations[mesh.GetMaterial().animationID.value()].AddMesh(mesh);
                         }
                     })
                     .OnEffect([&](Effect& effect)
                     {
                         m_effects.Activate(effect);
                     })
                     .OnElseFail());
}

void Space::DeactivateDrawable(Drawable* drawable)
{
    drawable->Accept(Drawable::Visitor::Empty()
                     .OnMesh([this](Mesh& mesh)
                     {
                         m_meshes.Deactivate(mesh);

                         if (mesh.GetMaterial().IsAnimated())
                         {
                             m_animations[mesh.GetMaterial().animationID.value()].RemoveMesh(mesh);
                         }
                     })
                     .OnEffect([this](Effect& effect)
                     {
                         m_effects.Deactivate(effect);
                     })
                     .OnElseFail());
}

void Space::ReturnDrawable(Drawable* drawable)
{
    drawable->Accept(Drawable::Visitor::Empty()
                     .OnMesh([this](Mesh& mesh)
                     {
                         m_meshes.Return(mesh);
                     })
                     .OnEffect([this](Effect& effect)
                     {
                         m_effects.Return(effect);
                     })
                     .OnElseFail());
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

    m_camera.Update();

    for (const auto& drawable : m_drawables)
    {
        drawable->Update();
    }
}

void Space::Render(Allocation<ID3D12Resource> color, Allocation<ID3D12Resource> depth, const RenderData& data)
{
    m_globalConstantBufferMapping->time = static_cast<float>(m_nativeClient.GetTotalRenderTime());

    {
        PIXScopedEvent(GetCommandList().Get(), PIX_COLOR_DEFAULT, L"Space");

        EnqueueUploads();
        UpdateGlobalShaderResources();
        m_globalShaderResources->Bind(GetCommandList());
        RunAnimations();
        BuildAccelerationStructures();
        DispatchRays();
        CopyOutputToBuffers(color, depth);
        DrawEffects(data);
    }

    TRY_DO(GetCommandList()->Close());
}

void Space::CleanupRender()
{
    for (auto* group : m_drawableGroups)
    {
        group->CleanupDataUpload();
    }

    m_indexBuffer.CleanupRender();
}

NativeClient& Space::GetNativeClient() const
{
    return m_nativeClient;
}

ShaderBuffer* Space::GetCustomDataBuffer() const
{
    return m_customDataBuffer.get();
}

Camera* Space::GetCamera()
{
    return &m_camera;
}

Light* Space::GetLight()
{
    return &m_light;
}

std::shared_ptr<ShaderResources> Space::GetShaderResources()
{
    return m_globalShaderResources;
}

std::shared_ptr<RasterPipeline::Bindings> Space::GetEffectBindings()
{
    return m_effectBindings;
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
        .lightDirection = DirectX::XMFLOAT3{0.0f, -1.0f, 0.0f},
        .minLight = 0.4f,
        .minShadow = 0.2f,
        .textureSize = DirectX::XMUINT2{1, 1}
    });
}

void Space::InitializePipelineResourceViews(const SpacePipeline& pipeline)
{
    UpdateOutputResourceViews();
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

                    m_globalShaderResources->CreateShaderResourceView(entry, index,
                                                                      {texture->GetResource(), &texture->GetView()});
                }
            }
            else
            {
                m_globalShaderResources->CreateShaderResourceView(entry, 0,
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

    if (pipelineDescription.description.customDataBufferSize > 0)
    {
        m_customDataBuffer = std::make_unique<ShaderBuffer>(m_nativeClient,
                                                            pipelineDescription.description.customDataBufferSize);
    }

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

    m_globalShaderResources = std::make_shared<ShaderResources>();
    m_globalShaderResources->Initialize( // todo: use two static heaps (one for the changing stuff, one for textures)
        [&](auto& graphics)
        {
            graphics.AddHeapDescriptorTable([&](auto& table)
            {
                m_rtColorDataForRasterEntry = table.AddShaderResourceView({.reg = 0});
                m_rtDepthDataForRasterEntry = table.AddShaderResourceView({.reg = 1});
            });

            m_effectBindings = RasterPipeline::SetupEffectBindings(m_nativeClient, graphics);
            // todo: update wiki article about shader resources (write section about compute resources)
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

    NAME_D3D12_OBJECT(m_globalShaderResources->GetComputeRootSignature());
    NAME_D3D12_OBJECT(m_globalShaderResources->GetGraphicsRootSignature());

    InitializeAnimations();

    pipeline.SetMaxPayloadSize(8 * sizeof(float));
    pipeline.SetMaxAttributeSize(2 * sizeof(float));
    pipeline.SetMaxRecursionDepth(2);

    m_rtStateObject = pipeline.Generate(m_globalShaderResources->GetComputeRootSignature());
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
    if (m_customDataBuffer != nullptr)
    {
        description->AddConstantBufferView(m_customDataBuffer->GetGPUVirtualAddress(), {.reg = 1});
    }
    description->AddConstantBufferView(m_globalConstantBuffer.GetGPUVirtualAddress(), {.reg = 2});

    m_commonResourceTable = description->AddHeapDescriptorTable([this](auto& table)
    {
        m_bvhEntry = table.AddShaderResourceView({.reg = 0});
        m_textureSlot1.entry = table.AddShaderResourceView({.reg = 0, .space = 1}, m_textureSlot1.size);
        m_textureSlot2.entry = table.AddShaderResourceView({.reg = 0, .space = 2}, m_textureSlot2.size);
        m_colorOutputEntry = table.AddUnorderedAccessView({.reg = 0});
        m_depthOutputEntry = table.AddUnorderedAccessView({.reg = 1});
    });
}

void Space::SetupDynamicResourceLayout(ShaderResources::Description* description)
{
    const std::function<UINT(Mesh* const&)> getIndexOfMesh = [this](auto* mesh)
    {
        REQUIRE(mesh != nullptr);
        REQUIRE(mesh->GetActiveIndex().has_value());

        return static_cast<UINT>(mesh->GetActiveIndex().value());
    };

    m_meshInstanceDataList = description->AddConstantBufferViewDescriptorList({.reg = 4, .space = 0},
                                                                              CreateSizeGetter(&m_meshes.GetActive()),
                                                                              [this](const UINT index)
                                                                              {
                                                                                  return m_meshes.GetActive()[
                                                                                          static_cast<
                                                                                              Drawable::ActiveIndex>(
                                                                                              index)]->
                                                                                      GetInstanceDataViewDescriptor();
                                                                              },
                                                                              CreateListBuilder(
                                                                                  &m_meshes.GetActive(),
                                                                                  getIndexOfMesh));

    m_meshGeometryBufferList = description->AddShaderResourceViewDescriptorList({.reg = 1, .space = 0},
        CreateSizeGetter(&m_meshes.GetActive()),
        [this](const UINT index)
        {
            return m_meshes.GetActive()[static_cast<Drawable::ActiveIndex>(index)]->
                GetGeometryBufferViewDescriptor();
        },
        CreateListBuilder(&m_meshes.GetActive(), getIndexOfMesh));
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
        animation.Initialize(m_nativeClient, m_globalShaderResources->GetComputeRootSignature());
    }
}

void Space::CreateRaytracingOutputBuffer()
{
    m_colorOutputDescription.DepthOrArraySize = 1;
    m_colorOutputDescription.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D;

    m_colorOutputDescription.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    m_colorOutputDescription.Flags = D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;
    m_colorOutputDescription.Width = m_resolution.width;
    m_colorOutputDescription.Height = m_resolution.height;
    m_colorOutputDescription.Layout = D3D12_TEXTURE_LAYOUT_UNKNOWN;
    m_colorOutputDescription.MipLevels = 1;
    m_colorOutputDescription.SampleDesc.Count = 1;

    m_colorOutput = util::AllocateResource<ID3D12Resource>(
        m_nativeClient,
        m_colorOutputDescription,
        D3D12_HEAP_TYPE_DEFAULT,
        D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE);
    NAME_D3D12_OBJECT(m_colorOutput);

    m_depthOutputDescription.DepthOrArraySize = 1;
    m_depthOutputDescription.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D;

    m_depthOutputDescription.Format = DXGI_FORMAT_R32_FLOAT;
    m_depthOutputDescription.Flags = D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;
    m_depthOutputDescription.Width = m_resolution.width;
    m_depthOutputDescription.Height = m_resolution.height;
    m_depthOutputDescription.Layout = D3D12_TEXTURE_LAYOUT_UNKNOWN;
    m_depthOutputDescription.MipLevels = 1;
    m_depthOutputDescription.SampleDesc.Count = 1;

    m_depthOutput = util::AllocateResource<ID3D12Resource>(
        m_nativeClient,
        m_depthOutputDescription,
        D3D12_HEAP_TYPE_DEFAULT,
        D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE);

    m_outputResourcesFresh = true;
    UpdateOutputResourceViews();
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

    rsc.AddRootParameter(D3D12_ROOT_PARAMETER_TYPE_CBV, 3); // Material Data (b3, space0)

    return rsc.Generate(GetDevice().Get(), true);
}

void Space::CreateShaderBindingTable()
{
    m_sbtHelper.Reset();

    REQUIRE(!m_outputResourcesFresh);

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

void Space::EnqueueUploads() const
{
    for (auto* group : m_drawableGroups)
    {
        group->EnqueueDataUpload(GetCommandList());
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

    for (Mesh* mesh : m_meshes.GetModified())
    {
        mesh->CreateBLAS(GetCommandList(), &uavs);
    }

    m_resultBufferAllocator.CreateBarriers(GetCommandList(), std::move(uavs));

    CreateTLAS();
    UpdateTopLevelAccelerationStructureView();
}

void Space::CreateTLAS()
{
    nv_helpers_dx12::TopLevelASGenerator topLevelASGenerator;

    for (Mesh* mesh : m_meshes.GetActive())
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

    topLevelASGenerator.Generate(GetCommandList().Get(),
                                 m_topLevelASBuffers.scratch,
                                 m_topLevelASBuffers.result,
                                 m_topLevelASBuffers.instanceDescription);
}

void Space::DispatchRays() const
{
    const std::vector barriers = {
        CD3DX12_RESOURCE_BARRIER::Transition(
            m_colorOutput.Get(), D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE,
            D3D12_RESOURCE_STATE_UNORDERED_ACCESS),
        CD3DX12_RESOURCE_BARRIER::Transition(
            m_depthOutput.Get(), D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE,
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

void Space::CopyOutputToBuffers(Allocation<ID3D12Resource> color, Allocation<ID3D12Resource> depth) const
{
    D3D12_RESOURCE_BARRIER entry[] = {
        CD3DX12_RESOURCE_BARRIER::Transition(
            m_colorOutput.Get(), D3D12_RESOURCE_STATE_UNORDERED_ACCESS,
            D3D12_RESOURCE_STATE_COPY_SOURCE),
        CD3DX12_RESOURCE_BARRIER::Transition(
            m_depthOutput.Get(), D3D12_RESOURCE_STATE_UNORDERED_ACCESS,
            D3D12_RESOURCE_STATE_COPY_SOURCE),
        CD3DX12_RESOURCE_BARRIER::Transition(
            color.Get(), D3D12_RESOURCE_STATE_RENDER_TARGET,
            D3D12_RESOURCE_STATE_COPY_DEST),
        CD3DX12_RESOURCE_BARRIER::Transition(
            depth.Get(), D3D12_RESOURCE_STATE_DEPTH_WRITE,
            D3D12_RESOURCE_STATE_COPY_DEST)
    };
    GetCommandList()->ResourceBarrier(_countof(entry), entry);

    GetCommandList()->CopyResource(color.Get(),
                                   m_colorOutput.Get());

    GetCommandList()->CopyResource(depth.Get(),
                                   m_depthOutput.Get());

    D3D12_RESOURCE_BARRIER exit[] = {
        CD3DX12_RESOURCE_BARRIER::Transition(
            color.Get(), D3D12_RESOURCE_STATE_COPY_DEST,
            D3D12_RESOURCE_STATE_RENDER_TARGET),
        CD3DX12_RESOURCE_BARRIER::Transition(
            depth.Get(), D3D12_RESOURCE_STATE_COPY_DEST,
            D3D12_RESOURCE_STATE_DEPTH_WRITE)
    };
    GetCommandList()->ResourceBarrier(_countof(exit), exit);
}

void Space::DrawEffects(const RenderData& data)
{
    D3D12_RESOURCE_BARRIER barriers[] = {
        CD3DX12_RESOURCE_BARRIER::Transition(
            m_colorOutput.Get(), D3D12_RESOURCE_STATE_COPY_SOURCE, D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE),
        CD3DX12_RESOURCE_BARRIER::Transition(
            m_depthOutput.Get(), D3D12_RESOURCE_STATE_COPY_SOURCE, D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE)
    };
    GetCommandList()->ResourceBarrier(_countof(barriers), barriers);
    
    GetCommandList()->OMSetRenderTargets(1, data.rtv, FALSE, data.dsv);

    data.viewport->Set(GetCommandList());

    for (const Effect* effect : m_effects.GetActive())
    {
        effect->Draw(GetCommandList());
    }
}

void Space::UpdateOutputResourceViews()
{
    if (!m_colorOutputEntry.IsValid() || !m_depthOutputEntry.IsValid()) return;

    if (!m_outputResourcesFresh) return;
    m_outputResourcesFresh = false;

    {
        D3D12_UNORDERED_ACCESS_VIEW_DESC uavDesc = {};
        uavDesc.ViewDimension = D3D12_UAV_DIMENSION_TEXTURE2D;

        uavDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
        m_globalShaderResources->CreateUnorderedAccessView(m_colorOutputEntry, 0, {m_colorOutput, &uavDesc});

        uavDesc.Format = DXGI_FORMAT_R32_FLOAT;
        m_globalShaderResources->CreateUnorderedAccessView(m_depthOutputEntry, 0, {m_depthOutput, &uavDesc});
    }

    {
        D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc = {};
        srvDesc.ViewDimension = D3D12_SRV_DIMENSION_TEXTURE2D;
        srvDesc.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;

        srvDesc.Format = m_colorOutputDescription.Format;
        srvDesc.Texture2D.MipLevels = m_colorOutputDescription.MipLevels;
        m_globalShaderResources->CreateShaderResourceView(m_rtColorDataForRasterEntry, 0, {m_colorOutput, &srvDesc});

        srvDesc.Format = m_depthOutputDescription.Format;
        srvDesc.Texture2D.MipLevels = m_depthOutputDescription.MipLevels;
        m_globalShaderResources->CreateShaderResourceView(m_rtDepthDataForRasterEntry, 0, {m_depthOutput, &srvDesc});
    }
}

void Space::UpdateTopLevelAccelerationStructureView() const
{
    D3D12_SHADER_RESOURCE_VIEW_DESC srvDescription;
    srvDescription.Format = DXGI_FORMAT_UNKNOWN;
    srvDescription.ViewDimension = D3D12_SRV_DIMENSION_RAYTRACING_ACCELERATION_STRUCTURE;
    srvDescription.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
    srvDescription.RaytracingAccelerationStructure.Location = m_topLevelASBuffers.result.resource->
        GetGPUVirtualAddress();

    m_globalShaderResources->CreateShaderResourceView(m_bvhEntry, 0, {{}, &srvDescription});
}

void Space::UpdateGlobalShaderResources()
{
    const IntegerSet meshesToRefresh = m_meshes.ClearChanged();
    for (auto& animation : m_animations)
    {
        animation.Update(*m_globalShaderResources, GetCommandList());
    }

    m_globalShaderResources->RequestListRefresh(m_meshInstanceDataList, meshesToRefresh);
    m_globalShaderResources->RequestListRefresh(m_meshGeometryBufferList, meshesToRefresh);
    m_globalShaderResources->Update();

    m_effects.ClearChanged();
}
