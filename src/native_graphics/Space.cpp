﻿#include "stdafx.h"

bool Material::IsAnimated() const { return animationID.has_value(); }

Space::Space(NativeClient& nativeClient)
    : m_client(&nativeClient)
  , m_resultBufferAllocator(nativeClient, D3D12_RESOURCE_STATE_RAYTRACING_ACCELERATION_STRUCTURE)
  , m_scratchBufferAllocator(nativeClient, D3D12_RESOURCE_STATE_UNORDERED_ACCESS)
  , m_camera(nativeClient)
  , m_light(nativeClient)
  , m_indexBuffer(*this)
{
}

void Space::PerformInitialSetupStepOne(ComPtr<ID3D12CommandQueue> const& commandQueue)
{
    Require(m_drawables.IsEmpty());

    auto* spaceCommandGroup = &m_commandGroup; // Improves the naming of the objects.
    INITIALIZE_COMMAND_ALLOCATOR_GROUP(*m_client, spaceCommandGroup, D3D12_COMMAND_LIST_TYPE_DIRECT);
    m_commandGroup.Reset(0);

    CreateTLAS();

    m_commandGroup.Close();
    std::array<ID3D12CommandList*, 1> const commandLists = {GetCommandList().Get()};
    commandQueue->ExecuteCommandLists(static_cast<UINT>(commandLists.size()), commandLists.data());

    m_client->WaitForGPU();

    m_camera.Initialize();

    m_sentinelTexture    = Texture::Create(*m_client, TextureDescription());
    m_sentinelTextureSRV = m_sentinelTexture->GetView();
}

void Space::PerformResolutionDependentSetup(Resolution const& resolution)
{
    m_resolution = resolution;
    CreateRaytracingOutputBuffer();

    m_camera.Update();
}

bool Space::PerformInitialSetupStepTwo(SpacePipelineDescription const& pipeline)
{
    m_meshSpoolCount   = pipeline.meshSpoolCount;
    m_effectSpoolCount = pipeline.effectSpoolCount;

    CreateGlobalConstBuffer();

    if (!CreateRaytracingPipeline(pipeline)) return false;

    InitializePipelineResourceViews(pipeline);
    m_globalShaderResources->Update();

    CreateShaderBindingTable();

    return true;
}

Mesh& Space::CreateMesh(UINT const materialIndex)
{
    return m_meshes.Create([&materialIndex](Mesh& mesh) { mesh.Initialize(materialIndex); });
}

Effect& Space::CreateEffect(RasterPipeline* pipeline)
{
    return m_effects.Create([&pipeline](Effect& effect) { effect.Initialize(*pipeline); });
}

void Space::MarkDrawableModified(Drawable* drawable)
{
    drawable->Accept(
        Drawable::Visitor::Empty().OnMesh(
            [this](Mesh& mesh)
            {
                m_meshes.MarkModified(mesh);

                if (mesh.GetMaterial().IsAnimated() && mesh.GetActiveIndex().has_value())
                    m_animations[mesh.GetMaterial().animationID.value()].UpdateMesh(mesh);
            }).OnEffect([this](Effect& effect) { m_effects.MarkModified(effect); }).OnElseFail());
}

void Space::ActivateDrawable(Drawable* drawable)
{
    drawable->Accept(
        Drawable::Visitor::Empty().OnMesh(
            [this](Mesh& mesh)
            {
                m_meshes.Activate(mesh);

                if (mesh.GetMaterial().IsAnimated()) m_animations[mesh.GetMaterial().animationID.value()].AddMesh(mesh);
            }).OnEffect([this](Effect& effect) { m_effects.Activate(effect); }).OnElseFail());
}

void Space::DeactivateDrawable(Drawable* drawable)
{
    drawable->Accept(
        Drawable::Visitor::Empty().OnMesh(
            [this](Mesh& mesh)
            {
                m_meshes.Deactivate(mesh);

                if (mesh.GetMaterial().IsAnimated())
                    m_animations[mesh.GetMaterial().animationID.value()].RemoveMesh(mesh);
            }).OnEffect([this](Effect& effect) { m_effects.Deactivate(effect); }).OnElseFail());
}

void Space::ReturnDrawable(Drawable* drawable)
{
    drawable->Accept(
        Drawable::Visitor::Empty().OnMesh([this](Mesh& mesh) { m_meshes.Return(mesh); }).OnEffect(
            [this](Effect& effect) { m_effects.Return(effect); }).OnElseFail());
}

Material const& Space::GetMaterial(UINT const index) const { return *m_materials[index]; }

void Space::Reset(UINT const frameIndex) { m_commandGroup.Reset(frameIndex); }

std::pair<Allocation<ID3D12Resource>, UINT> Space::GetIndexBuffer(
    UINT const                           vertexCount,
    std::vector<D3D12_RESOURCE_BARRIER>* barriers) { return m_indexBuffer.GetIndexBuffer(vertexCount, barriers); }

void Space::SpoolUp()
{
    m_meshes.Spool(m_meshSpoolCount);
    m_effects.Spool(m_effectSpoolCount);
}

void Space::Update(double)
{
    m_globalConstantBufferMapping->lightDirection = m_light.GetDirection();

    m_camera.Update();

    m_drawables.ForEach([](Drawable* drawable) { drawable->Update(); });
}

void Space::Render(
    Allocation<ID3D12Resource> const& color,
    Allocation<ID3D12Resource> const& depth,
    RenderData const&                 data)
{
    m_globalConstantBufferMapping->time = static_cast<float>(m_client->GetTotalRenderUpdateTime());

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

    TryDo(GetCommandList()->Close());
}

void Space::CleanupRender()
{
    for (auto* group : m_drawableGroups) group->CleanupDataUpload();

    m_indexBuffer.CleanupRender();
}

NativeClient& Space::GetNativeClient() const { return *m_client; }

ShaderBuffer* Space::GetCustomDataBuffer() const { return m_customDataBuffer.get(); }

Camera* Space::GetCamera() { return &m_camera; }

Light* Space::GetLight() { return &m_light; }

Resolution const& Space::GetResolution() const { return m_resolution; }

std::shared_ptr<ShaderResources> Space::GetShaderResources() { return m_globalShaderResources; }

std::shared_ptr<RasterPipeline::Bindings> Space::GetEffectBindings() { return m_effectBindings; }

ComPtr<ID3D12GraphicsCommandList4> Space::GetCommandList() const { return m_commandGroup.commandList; }

BLAS Space::AllocateBLAS(UINT64 const resultSize, UINT64 const scratchSize)
{
    return {
        .result = m_resultBufferAllocator.Allocate(resultSize),
        .scratch = m_scratchBufferAllocator.Allocate(scratchSize)
    };
}

ComPtr<ID3D12Device5> Space::GetDevice() const { return m_client->GetDevice(); }

void Space::CreateGlobalConstBuffer()
{
    m_globalConstantBufferSize = sizeof(GlobalBuffer);
    m_globalConstantBuffer     = util::AllocateConstantBuffer(*m_client, &m_globalConstantBufferSize);
    NAME_D3D12_OBJECT(m_globalConstantBuffer);

    TryDo(m_globalConstantBuffer.Map(&m_globalConstantBufferMapping, 1));

    m_globalConstantBufferMapping.Write(
        {
            .time = 0.0f,
            .textureSize = DirectX::XMUINT3{1, 1, 1},
            .lightDirection = DirectX::XMFLOAT3{0.0f, -1.0f, 0.0f},
            .minLight = 0.4f,
            .minShadow = 0.2f
        });
}

void Space::InitializePipelineResourceViews(SpacePipelineDescription const& pipeline)
{
    UpdateOutputResourceViews();
    UpdateTopLevelAccelerationStructureView();

    {
        std::optional<DirectX::XMUINT3> textureSize = std::nullopt;

        auto getTexturesCountInSlot = [&](UINT count) -> std::optional<UINT>
        {
            if (count == 0) return std::nullopt;
            return count;
        };
        auto fillSlots = [&](
            ShaderResources::Table::Entry const entry,
            UINT const                          base,
            std::optional<UINT> const           count)
        {
            if (count.has_value())
            {
                textureSize = textureSize.value_or(pipeline.textures[base]->GetSize());

                for (UINT index = 0; index < count.value(); index++)
                {
                    Texture const* texture = pipeline.textures[base + index];

                    Require(texture != nullptr);
                    Require(texture->GetSize().x == textureSize.value().x);
                    Require(texture->GetSize().y == textureSize.value().y);

                    m_globalShaderResources->CreateShaderResourceView(
                        entry,
                        index,
                        {texture->GetResource(), &texture->GetView()});
                }
            }
            else
                m_globalShaderResources->CreateShaderResourceView(
                    entry,
                    0,
                    {m_sentinelTexture->GetResource(), &m_sentinelTextureSRV});
        };

        UINT const firstSlotArraySize  = pipeline.textureCountFirstSlot;
        UINT const secondSlotArraySize = pipeline.textureCountSecondSlot;

        fillSlots(m_textureSlot1.entry, 0, getTexturesCountInSlot(firstSlotArraySize));
        fillSlots(m_textureSlot2.entry, firstSlotArraySize, getTexturesCountInSlot(secondSlotArraySize));

        m_globalConstantBufferMapping->textureSize = textureSize.value_or(DirectX::XMUINT3{1, 1, 1});
    }
}

bool Space::CreateRaytracingPipeline(SpacePipelineDescription const& pipelineDescription)
{
    m_textureSlot1.size = std::max(pipelineDescription.textureCountFirstSlot, 1u);
    m_textureSlot2.size = std::max(pipelineDescription.textureCountSecondSlot, 1u);

    if (pipelineDescription.customDataBufferSize > 0)
        m_customDataBuffer = std::make_unique<ShaderBuffer>(*m_client, pipelineDescription.customDataBufferSize);

    nv_helpers_dx12::RayTracingPipelineGenerator pipeline(GetDevice());

    bool ok                     = true;
    std::tie(m_shaderBlobs, ok) = CompileShaderLibraries(*m_client, pipelineDescription, pipeline);
    if (!ok) return false;

    m_rayGenSignature = CreateRayGenSignature();
    NAME_D3D12_OBJECT(m_rayGenSignature);

    m_missSignature = CreateMissSignature();
    NAME_D3D12_OBJECT(m_missSignature);

    for (UINT index = 0; index < pipelineDescription.materialCount; index++) m_materials.push_back(
        SetUpMaterial(pipelineDescription.materials[index], index, pipeline));

    CreateAnimations(pipelineDescription);

    pipeline.AddRootSignatureAssociation(m_rayGenSignature.Get(), true, {L"RayGen"});
    pipeline.AddRootSignatureAssociation(m_missSignature.Get(), true, {L"Miss", L"ShadowMiss"});

    m_globalShaderResources = std::make_shared<ShaderResources>();
    m_globalShaderResources->Initialize(
        [this](auto& graphics)
        {
            graphics.AddHeapDescriptorTable(
                [&](auto& table)
                {
                    m_rtColorDataForRasterEntry = table.AddShaderResourceView({.reg = 0});
                    m_rtDepthDataForRasterEntry = table.AddShaderResourceView({.reg = 1});
                });

            m_effectBindings = RasterPipeline::SetUpEffectBindings(*m_client, graphics);
        },
        [this](auto& compute)
        {
            SetUpStaticResourceLayout(&compute);
            SetUpDynamicResourceLayout(&compute);

            for (auto& animation : m_animations) animation.SetUpResourceLayout(&compute);
        },
        GetDevice());

    NAME_D3D12_OBJECT(m_globalShaderResources->GetComputeRootSignature());
    NAME_D3D12_OBJECT(m_globalShaderResources->GetGraphicsRootSignature());

    InitializeAnimations();

    pipeline.SetMaxPayloadSize(sizeof(float) * (3 /* Color */ + 1 /* Alpha */ + 3 /* Normal */ + 1 /* Distance */));
    pipeline.SetMaxAttributeSize(sizeof(float) * 2 /* Barycentrics */);
    pipeline.SetMaxRecursionDepth(2);

    m_rtStateObject = pipeline.Generate(m_globalShaderResources->GetComputeRootSignature());
    NAME_D3D12_OBJECT(m_rtStateObject);

    TryDo(m_rtStateObject->QueryInterface(IID_PPV_ARGS(&m_rtStateObjectProperties)));

    return true;
}

std::pair<std::vector<ComPtr<IDxcBlob>>, bool> Space::CompileShaderLibraries(
    NativeClient&                                 client,
    SpacePipelineDescription const&               pipelineDescription,
    nv_helpers_dx12::RayTracingPipelineGenerator& pipeline)
{
    std::vector<ComPtr<IDxcBlob>> shaderBlobs(pipelineDescription.shaderCount);

    UINT currentSymbolIndex = 0;
    bool ok                 = true;

    auto compileShaderLibrary = [&](UINT const shader)
    {
        shaderBlobs[shader] = CompileShader(
            pipelineDescription.shaderFiles[shader].path,
            L"",
            L"lib_6_7",
            VG_SHADER_REGISTRY(client),
            pipelineDescription.onShaderLoadingError);

        if (shaderBlobs[shader] == nullptr) return false;

        UINT const currentSymbolCount = pipelineDescription.shaderFiles[shader].symbolCount;

        std::vector<std::wstring> symbols;
        symbols.reserve(currentSymbolCount);

        for (UINT symbolOffset = 0; symbolOffset < currentSymbolCount; symbolOffset++) symbols.emplace_back(
            pipelineDescription.symbols[currentSymbolIndex++]);

        pipeline.AddLibrary(shaderBlobs[shader].Get(), symbols);

        return true;
    };

    auto compileComputeShader = [&](UINT const shader)
    {
        shaderBlobs[shader] = CompileShader(
            pipelineDescription.shaderFiles[shader].path,
            L"Main",
            L"cs_6_7",
            VG_SHADER_REGISTRY(client),
            pipelineDescription.onShaderLoadingError);

        return shaderBlobs[shader] != nullptr;
    };

    for (UINT shader = 0; shader < pipelineDescription.shaderCount; shader++)
    {
        bool shaderOk;

        if (pipelineDescription.shaderFiles[shader].symbolCount > 0) shaderOk = compileShaderLibrary(shader);
        else shaderOk                                                         = compileComputeShader(shader);

        ok = ok && shaderOk;
    }

    return {shaderBlobs, ok};
}

std::unique_ptr<Material> Space::SetUpMaterial(
    MaterialDescription const&                    description,
    UINT const                                    index,
    nv_helpers_dx12::RayTracingPipelineGenerator& pipeline) const
{
    auto material = std::make_unique<Material>();

    material->name     = description.name;
    material->index    = index * 2;
    material->isOpaque = description.opaque;

    if (description.visible) material->flags |= MaterialFlags::VISIBLE;
    if (description.shadowCaster) material->flags |= MaterialFlags::SHADOW_CASTER;

    auto addHitGroup = [&](
        std::wstring const& prefix,
        std::wstring const& closestHitSymbol,
        std::wstring const& anyHitSymbol,
        std::wstring const& intersectionSymbol) -> std::tuple<std::wstring, ComPtr<ID3D12RootSignature>>
    {
        ComPtr<ID3D12RootSignature> rootSignature = CreateMaterialSignature();
        std::wstring                hitGroup      = prefix + L"_" + description.name;

        pipeline.AddHitGroup(hitGroup, closestHitSymbol, anyHitSymbol, intersectionSymbol);
        pipeline.AddRootSignatureAssociation(rootSignature.Get(), true, {hitGroup});

        return {hitGroup, rootSignature};
    };

    std::tie(material->normalHitGroup, material->normalRootSignature) = addHitGroup(
        L"N",
        description.normalClosestHitSymbol,
        description.normalAnyHitSymbol,
        description.normalIntersectionSymbol);

    std::tie(material->shadowHitGroup, material->shadowRootSignature) = addHitGroup(
        L"S",
        description.shadowClosestHitSymbol,
        description.shadowAnyHitSymbol,
        description.shadowIntersectionSymbol);

    std::wstring const normalIntersectionSymbol = description.normalIntersectionSymbol;
    std::wstring const shadowIntersectionSymbol = description.shadowIntersectionSymbol;
    Require(normalIntersectionSymbol.empty() == shadowIntersectionSymbol.empty());

    material->geometryType = normalIntersectionSymbol.empty()
                                 ? D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES
                                 : D3D12_RAYTRACING_GEOMETRY_TYPE_PROCEDURAL_PRIMITIVE_AABBS;

    UINT64 materialConstantBufferSize = sizeof MaterialBuffer;
    material->materialConstantBuffer  = util::AllocateConstantBuffer(*m_client, &materialConstantBufferSize);
    NAME_D3D12_OBJECT(material->materialConstantBuffer);

    MaterialBuffer const materialConstantBufferData = {.index = index};
    TryDo(util::MapAndWrite(material->materialConstantBuffer, materialConstantBufferData));

#if defined(NATIVE_DEBUG)
    std::wstring const debugName = description.name;
    // DirectX seems to return the same pointer for both signatures, so naming them is not very useful.
    TryDo(material->normalRootSignature->SetName((L"RT Material RS " + debugName).c_str()));
    TryDo(material->shadowRootSignature->SetName((L"RT Material RS " + debugName).c_str()));
#endif

    return material;
}

void Space::CreateAnimations(SpacePipelineDescription const& pipeline)
{
    std::map<UINT, UINT> animationShaderIndexToID;

    for (UINT shaderIndex = 0; shaderIndex < pipeline.shaderCount; shaderIndex++)
    {
        if (auto const& [path, symbolCount] = pipeline.shaderFiles[shaderIndex];
            symbolCount > 0)
            continue;

        auto const             animationID = static_cast<UINT>(m_animations.size());
        ComPtr<IDxcBlob> const blob        = m_shaderBlobs[shaderIndex];

        constexpr UINT offset = 3;
        m_animations.emplace_back(blob, offset + animationID);

        animationShaderIndexToID[shaderIndex] = animationID;
    }

    for (UINT materialID = 0; materialID < pipeline.materialCount; materialID++)
    {
        MaterialDescription const& materialDescription = pipeline.materials[materialID];
        if (materialDescription.isAnimated)
        {
            UINT animationID                     = animationShaderIndexToID[materialDescription.animationShaderIndex];
            m_materials[materialID]->animationID = animationID;
        }
    }
}

void Space::SetUpStaticResourceLayout(ShaderResources::Description* description)
{
    description->AddConstantBufferView(m_camera.GetCameraBufferAddress(), {.reg = 0});
    if (m_customDataBuffer != nullptr) description->AddConstantBufferView(
        m_customDataBuffer->GetGPUVirtualAddress(),
        {.reg = 1});
    description->AddConstantBufferView(m_globalConstantBuffer.GetGPUVirtualAddress(), {.reg = 2});

    m_unchangedCommonResourceHandle = description->AddHeapDescriptorTable(
        [this](auto& table)
        {
            m_textureSlot1.entry = table.AddShaderResourceView({.reg = 0, .space = 1}, m_textureSlot1.size);
            m_textureSlot2.entry = table.AddShaderResourceView({.reg = 0, .space = 2}, m_textureSlot2.size);
        });

    m_changedCommonResourceHandle = description->AddHeapDescriptorTable(
        [this](auto& table)
        {
            m_bvhEntry         = table.AddShaderResourceView({.reg = 0});
            m_colorOutputEntry = table.AddUnorderedAccessView({.reg = 0});
            m_depthOutputEntry = table.AddUnorderedAccessView({.reg = 1});
        });
}

void Space::SetUpDynamicResourceLayout(ShaderResources::Description* description)
{
    std::function<UINT(Mesh* const&)> const getIndexOfMesh = [this](auto* mesh)
    {
        Require(mesh != nullptr);
        Require(mesh->GetActiveIndex().has_value());

        return static_cast<UINT>(mesh->GetActiveIndex().value());
    };

    m_meshInstanceDataList = description->AddConstantBufferViewDescriptorList(
        {.reg = 4, .space = 0},
        CreateSizeGetter(&m_meshes.GetActive()),
        [this](UINT const index)
        {
            return m_meshes.GetActive()[static_cast<Drawable::ActiveIndex>(index)]->GetInstanceDataViewDescriptor();
        },
        CreateBagBuilder(&m_meshes.GetActive(), getIndexOfMesh));

    m_meshGeometryBufferList = description->AddShaderResourceViewDescriptorList(
        {.reg = 1, .space = 0},
        CreateSizeGetter(&m_meshes.GetActive()),
        [this](UINT const index)
        {
            return m_meshes.GetActive()[static_cast<Drawable::ActiveIndex>(index)]->GetGeometryBufferViewDescriptor();
        },
        CreateBagBuilder(&m_meshes.GetActive(), getIndexOfMesh));
}

void Space::SetUpAnimationResourceLayout(ShaderResources::Description* description)
{
    for (auto& animation : m_animations) animation.SetUpResourceLayout(description);
}

void Space::InitializeAnimations()
{
    for (auto& animation : m_animations) animation.Initialize(
        *m_client,
        m_globalShaderResources->GetComputeRootSignature());
}

void Space::CreateRaytracingOutputBuffer()
{
    m_colorOutputDescription.DepthOrArraySize = 1;
    m_colorOutputDescription.Dimension        = D3D12_RESOURCE_DIMENSION_TEXTURE2D;

    m_colorOutputDescription.Format           = DXGI_FORMAT_B8G8R8A8_UNORM;
    m_colorOutputDescription.Flags            = D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;
    m_colorOutputDescription.Width            = m_resolution.width;
    m_colorOutputDescription.Height           = m_resolution.height;
    m_colorOutputDescription.Layout           = D3D12_TEXTURE_LAYOUT_UNKNOWN;
    m_colorOutputDescription.MipLevels        = 1;
    m_colorOutputDescription.SampleDesc.Count = 1;

    m_colorOutput = util::AllocateResource<ID3D12Resource>(
        *m_client,
        m_colorOutputDescription,
        D3D12_HEAP_TYPE_DEFAULT,
        D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE);
    NAME_D3D12_OBJECT(m_colorOutput);

    m_depthOutputDescription.DepthOrArraySize = 1;
    m_depthOutputDescription.Dimension        = D3D12_RESOURCE_DIMENSION_TEXTURE2D;

    m_depthOutputDescription.Format           = DXGI_FORMAT_R32_FLOAT;
    m_depthOutputDescription.Flags            = D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;
    m_depthOutputDescription.Width            = m_resolution.width;
    m_depthOutputDescription.Height           = m_resolution.height;
    m_depthOutputDescription.Layout           = D3D12_TEXTURE_LAYOUT_UNKNOWN;
    m_depthOutputDescription.MipLevels        = 1;
    m_depthOutputDescription.SampleDesc.Count = 1;

    m_depthOutput = util::AllocateResource<ID3D12Resource>(
        *m_client,
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

    Require(!m_outputResourcesFresh);

    m_sbtHelper.AddRayGenerationProgram(L"RayGen", {});

    m_sbtHelper.AddMissProgram(L"Miss", {});
    m_sbtHelper.AddMissProgram(L"ShadowMiss", {});

    for (auto const& material : m_materials)
    {
        auto materialCB = std::bit_cast<void*>(material->materialConstantBuffer.GetGPUVirtualAddress());
        m_sbtHelper.AddHitGroup(material->normalHitGroup, {materialCB});
        m_sbtHelper.AddHitGroup(material->shadowHitGroup, {materialCB});
    }

    uint32_t const sbtSize = m_sbtHelper.ComputeSBTSize();

    util::ReAllocateBuffer(
        &m_sbtStorage,
        *m_client,
        sbtSize,
        D3D12_RESOURCE_FLAG_NONE,
        D3D12_RESOURCE_STATE_GENERIC_READ,
        D3D12_HEAP_TYPE_UPLOAD);
    NAME_D3D12_OBJECT(m_sbtStorage);

    m_sbtHelper.Generate(m_sbtStorage.Get(), m_rtStateObjectProperties.Get());
}

void Space::EnqueueUploads() const { for (auto* group : m_drawableGroups) group->EnqueueDataUpload(GetCommandList()); }

void Space::RunAnimations()
{
    for (auto& animation : m_animations) animation.Run(*m_globalShaderResources, GetCommandList());
}

void Space::BuildAccelerationStructures()
{
    m_uavs.clear();
    m_uavs.reserve(m_animations.size() + m_meshes.GetModifiedCount());

    for (auto& animation : m_animations) animation.CreateBLAS(GetCommandList(), &m_uavs);

    for (Mesh* mesh : m_meshes.GetModified()) mesh->CreateBLAS(GetCommandList(), &m_uavs);

    m_resultBufferAllocator.CreateBarriers(GetCommandList(), m_uavs);

    CreateTLAS();
    UpdateTopLevelAccelerationStructureView();
}

void Space::CreateTLAS()
{
    m_tlasGenerator.Clear();

    m_meshes.GetActive().ForEach(
        [this](Mesh* mesh)
        {
            Require(mesh->GetActiveIndex().has_value());
            auto const instanceID = static_cast<UINT>(mesh->GetActiveIndex().value());

            // The CCW flag is used because DirectX uses left-handed coordinates.

            m_tlasGenerator.AddInstance(
                mesh->GetBLAS().result.GetAddress(),
                mesh->GetTransform(),
                instanceID,
                mesh->GetMaterial().index,
                static_cast<BYTE>(mesh->GetMaterial().flags),
                D3D12_RAYTRACING_INSTANCE_FLAG_TRIANGLE_FRONT_COUNTERCLOCKWISE);
        });

    UINT64 scratchSize;
    UINT64 resultSize;
    UINT64 instanceDescriptionSize;

    m_tlasGenerator.ComputeASBufferSizes(GetDevice().Get(), false, &scratchSize, &resultSize, &instanceDescriptionSize);

    bool const committed = m_client->SupportPIX();

    util::ReAllocateBuffer(
        &m_topLevelASBuffers.scratch,
        *m_client,
        scratchSize,
        D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS,
        D3D12_RESOURCE_STATE_COMMON,
        D3D12_HEAP_TYPE_DEFAULT,
        committed);
    util::ReAllocateBuffer(
        &m_topLevelASBuffers.result,
        *m_client,
        resultSize,
        D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS,
        D3D12_RESOURCE_STATE_RAYTRACING_ACCELERATION_STRUCTURE,
        D3D12_HEAP_TYPE_DEFAULT,
        committed);
    util::ReAllocateBuffer(
        &m_topLevelASBuffers.instanceDescription,
        *m_client,
        instanceDescriptionSize,
        D3D12_RESOURCE_FLAG_NONE,
        D3D12_RESOURCE_STATE_GENERIC_READ,
        D3D12_HEAP_TYPE_UPLOAD,
        committed);

    NAME_D3D12_OBJECT(m_topLevelASBuffers.scratch);
    NAME_D3D12_OBJECT(m_topLevelASBuffers.result);
    NAME_D3D12_OBJECT(m_topLevelASBuffers.instanceDescription);

    m_tlasGenerator.Generate(
        GetCommandList().Get(),
        m_topLevelASBuffers.scratch,
        m_topLevelASBuffers.result,
        m_topLevelASBuffers.instanceDescription);
}

void Space::DispatchRays() const
{
    std::array const barriers = {
        CD3DX12_RESOURCE_BARRIER::Transition(
            m_colorOutput.Get(),
            D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE,
            D3D12_RESOURCE_STATE_UNORDERED_ACCESS),
        CD3DX12_RESOURCE_BARRIER::Transition(
            m_depthOutput.Get(),
            D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE,
            D3D12_RESOURCE_STATE_UNORDERED_ACCESS)
    };
    GetCommandList()->ResourceBarrier(static_cast<UINT>(barriers.size()), barriers.data());

    D3D12_DISPATCH_RAYS_DESC desc = {};

    desc.RayGenerationShaderRecord.StartAddress = m_sbtStorage.GetGPUVirtualAddress() + m_sbtHelper.
        GetRayGenSectionOffset();
    desc.RayGenerationShaderRecord.SizeInBytes = m_sbtHelper.GetRayGenSectionSize();

    desc.MissShaderTable.StartAddress  = m_sbtStorage.GetGPUVirtualAddress() + m_sbtHelper.GetMissSectionOffset();
    desc.MissShaderTable.SizeInBytes   = m_sbtHelper.GetMissSectionSize();
    desc.MissShaderTable.StrideInBytes = m_sbtHelper.GetMissEntrySize();

    desc.HitGroupTable.StartAddress  = m_sbtStorage.GetGPUVirtualAddress() + m_sbtHelper.GetHitGroupSectionOffset();
    desc.HitGroupTable.SizeInBytes   = m_sbtHelper.GetHitGroupSectionSize();
    desc.HitGroupTable.StrideInBytes = m_sbtHelper.GetHitGroupEntrySize();

    desc.Width  = m_resolution.width;
    desc.Height = m_resolution.height;
    desc.Depth  = 1;

    GetCommandList()->SetPipelineState1(m_rtStateObject.Get());
    GetCommandList()->DispatchRays(&desc);
}

void Space::CopyOutputToBuffers(Allocation<ID3D12Resource> const& color, Allocation<ID3D12Resource> const& depth) const
{
    std::array const entry = {
        CD3DX12_RESOURCE_BARRIER::Transition(
            m_colorOutput.Get(),
            D3D12_RESOURCE_STATE_UNORDERED_ACCESS,
            D3D12_RESOURCE_STATE_COPY_SOURCE),
        CD3DX12_RESOURCE_BARRIER::Transition(
            m_depthOutput.Get(),
            D3D12_RESOURCE_STATE_UNORDERED_ACCESS,
            D3D12_RESOURCE_STATE_COPY_SOURCE),
        CD3DX12_RESOURCE_BARRIER::Transition(
            color.Get(),
            D3D12_RESOURCE_STATE_RENDER_TARGET,
            D3D12_RESOURCE_STATE_COPY_DEST),
        CD3DX12_RESOURCE_BARRIER::Transition(
            depth.Get(),
            D3D12_RESOURCE_STATE_DEPTH_WRITE,
            D3D12_RESOURCE_STATE_COPY_DEST)
    };
    GetCommandList()->ResourceBarrier(static_cast<UINT>(entry.size()), entry.data());

    GetCommandList()->CopyResource(color.Get(), m_colorOutput.Get());
    GetCommandList()->CopyResource(depth.Get(), m_depthOutput.Get());

    std::array const exit = {
        CD3DX12_RESOURCE_BARRIER::Transition(
            color.Get(),
            D3D12_RESOURCE_STATE_COPY_DEST,
            D3D12_RESOURCE_STATE_RENDER_TARGET),
        CD3DX12_RESOURCE_BARRIER::Transition(
            depth.Get(),
            D3D12_RESOURCE_STATE_COPY_DEST,
            D3D12_RESOURCE_STATE_DEPTH_WRITE)
    };
    GetCommandList()->ResourceBarrier(static_cast<UINT>(exit.size()), exit.data());
}

void Space::DrawEffects(RenderData const& data)
{
    std::array const barriers = {
        CD3DX12_RESOURCE_BARRIER::Transition(
            m_colorOutput.Get(),
            D3D12_RESOURCE_STATE_COPY_SOURCE,
            D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE),
        CD3DX12_RESOURCE_BARRIER::Transition(
            m_depthOutput.Get(),
            D3D12_RESOURCE_STATE_COPY_SOURCE,
            D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE)
    };
    GetCommandList()->ResourceBarrier(static_cast<UINT>(barriers.size()), barriers.data());

    GetCommandList()->OMSetRenderTargets(1, data.rtv, FALSE, data.dsv);

    data.viewport->Set(GetCommandList());

    m_effects.GetActive().ForEach([this](Effect const* effect) { effect->Draw(GetCommandList()); });
}

void Space::UpdateOutputResourceViews()
{
    if (!m_colorOutputEntry.IsValid() || !m_depthOutputEntry.IsValid()) return;

    if (!m_outputResourcesFresh) return;
    m_outputResourcesFresh = false;

    D3D12_UNORDERED_ACCESS_VIEW_DESC uavDesc = {};
    uavDesc.ViewDimension                    = D3D12_UAV_DIMENSION_TEXTURE2D;

    uavDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
    m_globalShaderResources->CreateUnorderedAccessView(m_colorOutputEntry, 0, {m_colorOutput, &uavDesc});

    uavDesc.Format = DXGI_FORMAT_R32_FLOAT;
    m_globalShaderResources->CreateUnorderedAccessView(m_depthOutputEntry, 0, {m_depthOutput, &uavDesc});

    D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc = {};
    srvDesc.ViewDimension                   = D3D12_SRV_DIMENSION_TEXTURE2D;
    srvDesc.Shader4ComponentMapping         = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;

    srvDesc.Format              = m_colorOutputDescription.Format;
    srvDesc.Texture2D.MipLevels = m_colorOutputDescription.MipLevels;
    m_globalShaderResources->CreateShaderResourceView(m_rtColorDataForRasterEntry, 0, {m_colorOutput, &srvDesc});

    srvDesc.Format              = m_depthOutputDescription.Format;
    srvDesc.Texture2D.MipLevels = m_depthOutputDescription.MipLevels;
    m_globalShaderResources->CreateShaderResourceView(m_rtDepthDataForRasterEntry, 0, {m_depthOutput, &srvDesc});
}

void Space::UpdateTopLevelAccelerationStructureView() const
{
    D3D12_SHADER_RESOURCE_VIEW_DESC srvDescription;
    srvDescription.Format                                   = DXGI_FORMAT_UNKNOWN;
    srvDescription.ViewDimension                            = D3D12_SRV_DIMENSION_RAYTRACING_ACCELERATION_STRUCTURE;
    srvDescription.Shader4ComponentMapping                  = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
    srvDescription.RaytracingAccelerationStructure.Location = m_topLevelASBuffers.result.resource->
        GetGPUVirtualAddress();

    m_globalShaderResources->CreateShaderResourceView(m_bvhEntry, 0, {{}, &srvDescription});
}

void Space::UpdateGlobalShaderResources()
{
    IntegerSet const meshesToRefresh = m_meshes.ClearChanged();
    for (auto& animation : m_animations) animation.Update(*m_globalShaderResources);

    m_globalShaderResources->RequestListRefresh(m_meshInstanceDataList, meshesToRefresh);
    m_globalShaderResources->RequestListRefresh(m_meshGeometryBufferList, meshesToRefresh);
    m_globalShaderResources->Update();

    m_effects.ClearChanged();
}
