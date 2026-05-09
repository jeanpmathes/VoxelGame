#include "stdafx.h"

bool Material::IsAnimated() const { return animationID.has_value(); }

Space::Space(NativeClient& nativeClient)
    : client(&nativeClient)
  , resultBufferAllocator(nativeClient, D3D12_RESOURCE_STATE_RAYTRACING_ACCELERATION_STRUCTURE)
  , scratchBufferAllocator(nativeClient, D3D12_RESOURCE_STATE_UNORDERED_ACCESS)
  , camera(nativeClient)
  , light(nativeClient)
  , indexBuffer(*this)
{
}

void Space::PerformInitialSetupStepOne(ComPtr<ID3D12CommandQueue> const& commandQueue)
{
    Require(drawables.IsEmpty());

    auto* spaceCommandGroup = &commandGroup; // Improves the naming of the objects.
    INITIALIZE_COMMAND_ALLOCATOR_GROUP(*client, spaceCommandGroup, D3D12_COMMAND_LIST_TYPE_DIRECT);
    commandGroup.Reset(0);

    CreateTLAS();

    commandGroup.Close();
    std::array<ID3D12CommandList*, 1> const commandLists = {GetCommandList().Get()};
    commandQueue->ExecuteCommandLists(static_cast<UINT>(commandLists.size()), commandLists.data());

    client->WaitForGPU();

    camera.Initialize();

    sentinelTexture    = Texture::Create(*client, TextureDescription());
    sentinelTextureSRV = sentinelTexture->GetView();
}

void Space::PerformResolutionDependentSetup(Resolution const& newResolution)
{
    resolution = newResolution;
    CreateRaytracingOutputBuffer();

    camera.Update();
}

bool Space::PerformInitialSetupStepTwo(SpacePipelineDescription const& pipeline)
{
    meshSpoolCount   = pipeline.meshSpoolCount;
    effectSpoolCount = pipeline.effectSpoolCount;

    CreateGlobalConstBuffer();

    if (!CreateRaytracingPipeline(pipeline)) return false;

    InitializePipelineResourceViews(pipeline);
    globalShaderResources->Update();

    CreateShaderBindingTable();

    return true;
}

Mesh& Space::CreateMesh(UINT const materialIndex) { return meshes.Create([&materialIndex](Mesh& mesh) { mesh.Initialize(materialIndex); }); }

Effect& Space::CreateEffect(RasterPipeline* pipeline) { return effects.Create([&pipeline](Effect& effect) { effect.Initialize(*pipeline); }); }

void Space::MarkDrawableModified(Drawable* drawable)
{
    drawable->Accept(
        Drawable::Visitor::Empty().OnMesh(
            [this](Mesh& mesh)
            {
                meshes.MarkModified(mesh);

                if (mesh.GetMaterial().IsAnimated() && mesh.GetActiveIndex().has_value()) animations[mesh.GetMaterial().animationID.value()].UpdateMesh(mesh);
            }).OnEffect([this](Effect& effect) { effects.MarkModified(effect); }).OnElseFail());
}

void Space::ActivateDrawable(Drawable* drawable)
{
    drawable->Accept(
        Drawable::Visitor::Empty().OnMesh(
            [this](Mesh& mesh)
            {
                meshes.Activate(mesh);

                if (mesh.GetMaterial().IsAnimated()) animations[mesh.GetMaterial().animationID.value()].AddMesh(mesh);
            }).OnEffect([this](Effect& effect) { effects.Activate(effect); }).OnElseFail());
}

void Space::DeactivateDrawable(Drawable* drawable)
{
    drawable->Accept(
        Drawable::Visitor::Empty().OnMesh(
            [this](Mesh& mesh)
            {
                meshes.Deactivate(mesh);

                if (mesh.GetMaterial().IsAnimated()) animations[mesh.GetMaterial().animationID.value()].RemoveMesh(mesh);
            }).OnEffect([this](Effect& effect) { effects.Deactivate(effect); }).OnElseFail());
}

void Space::ReturnDrawable(Drawable* drawable)
{
    drawable->Accept(Drawable::Visitor::Empty().OnMesh([this](Mesh& mesh) { meshes.Return(mesh); }).OnEffect([this](Effect& effect) { effects.Return(effect); }).OnElseFail());
}

Material const& Space::GetMaterial(UINT const index) const { return *materials[index]; }

void Space::Reset(UINT const frameIndex) { commandGroup.Reset(frameIndex); }

std::pair<Allocation<ID3D12Resource>, UINT> Space::GetIndexBuffer(UINT const vertexCount, std::vector<D3D12_RESOURCE_BARRIER>* barriers)
{
    return indexBuffer.GetIndexBuffer(vertexCount, barriers);
}

void Space::SpoolUp()
{
    meshes.Spool(meshSpoolCount);
    effects.Spool(effectSpoolCount);
}

void Space::Update()
{
    globalConstantBufferMapping->lightDirection = light.GetDirection();
    globalConstantBufferMapping->lightIntensity = light.GetIntensity();
    globalConstantBufferMapping->lightColor     = light.GetColor();

    camera.Update();

    drawables.ForEach([](Drawable* drawable) { drawable->Update(); });
}

void Space::Render(Allocation<ID3D12Resource> const& color, Allocation<ID3D12Resource> const& depth, RenderData const& data)
{
    globalConstantBufferMapping->time = static_cast<float>(client->GetTotalScaledRenderUpdateTime());

    {
        PIXScopedEvent(GetCommandList().Get(), PIX_COLOR_DEFAULT, L"Space");

        EnqueueUploads();
        UpdateGlobalShaderResources();
        globalShaderResources->Bind(GetCommandList());
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
    for (auto* group : drawableGroups) group->CleanupDataUpload();

    indexBuffer.CleanupRender();
}

void Space::SetIsRendered(bool const newState) { isRendered = newState; }

bool Space::IsRendered() const { return isRendered; }

NativeClient& Space::GetNativeClient() const { return *client; }

ShaderBuffer* Space::GetCustomDataBuffer() const { return customDataBuffer.get(); }

Camera* Space::GetCamera() { return &camera; }

Light* Space::GetLight() { return &light; }

Resolution const& Space::GetResolution() const { return resolution; }

std::shared_ptr<ShaderResources> Space::GetShaderResources() { return globalShaderResources; }

std::shared_ptr<RasterPipeline::Bindings> Space::GetEffectBindings() { return effectBindings; }

ComPtr<ID3D12GraphicsCommandList4> Space::GetCommandList() const { return commandGroup.commandList; }

BLAS Space::AllocateBLAS(UINT64 const resultSize, UINT64 const scratchSize)
{
    return {.result = resultBufferAllocator.Allocate(resultSize), .scratch = scratchBufferAllocator.Allocate(scratchSize)};
}

ComPtr<ID3D12Device5> Space::GetDevice() const { return client->GetDevice(); }

void Space::CreateGlobalConstBuffer()
{
    globalConstantBufferSize = sizeof(GlobalBuffer);
    globalConstantBuffer     = util::AllocateConstantBuffer(*client, &globalConstantBufferSize);
    NAME_D3D12_OBJECT(globalConstantBuffer);

    TryDo(globalConstantBuffer.Map(&globalConstantBufferMapping, 1));

    globalConstantBufferMapping.Write(
        {
            .time = 0.0f,
            .textureSize = DirectX::XMUINT3{1, 1, 1},
            .lightDirection = DirectX::XMFLOAT3{0.0f, -1.0f, 0.0f},
            .lightIntensity = 1.0f,
            .lightColor = DirectX::XMFLOAT3{1.0f, 1.0f, 1.0f}
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
        auto fillSlots = [&](ShaderResources::Table::Entry const entry, UINT const base, std::optional<UINT> const count)
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

                    globalShaderResources->CreateShaderResourceView(entry, index, {texture->GetResource(), &texture->GetView()});
                }
            }
            else globalShaderResources->CreateShaderResourceView(entry, 0, {sentinelTexture->GetResource(), &sentinelTextureSRV});
        };

        UINT const firstSlotArraySize  = pipeline.textureCountFirstSlot;
        UINT const secondSlotArraySize = pipeline.textureCountSecondSlot;

        fillSlots(textureSlot1.entry, 0, getTexturesCountInSlot(firstSlotArraySize));
        fillSlots(textureSlot2.entry, firstSlotArraySize, getTexturesCountInSlot(secondSlotArraySize));

        globalConstantBufferMapping->textureSize = textureSize.value_or(DirectX::XMUINT3{1, 1, 1});
    }
}

bool Space::CreateRaytracingPipeline(SpacePipelineDescription const& pipelineDescription)
{
    textureSlot1.size = std::max(pipelineDescription.textureCountFirstSlot, 1u);
    textureSlot2.size = std::max(pipelineDescription.textureCountSecondSlot, 1u);

    if (pipelineDescription.customDataBufferSize > 0) customDataBuffer = std::make_unique<ShaderBuffer>(*client, pipelineDescription.customDataBufferSize);

    nv_helpers_dx12::RayTracingPipelineGenerator pipeline(GetDevice());

    bool ok                   = true;
    std::tie(shaderBlobs, ok) = CompileShaderLibraries(*client, pipelineDescription, pipeline);
    if (!ok) return false;

    rayGenSignature = CreateRayGenSignature();
    NAME_D3D12_OBJECT(rayGenSignature);

    missSignature = CreateMissSignature();
    NAME_D3D12_OBJECT(missSignature);

    for (UINT index = 0; index < pipelineDescription.materialCount; index++) materials.push_back(SetUpMaterial(pipelineDescription.materials[index], index, pipeline));

    CreateAnimations(pipelineDescription);

    pipeline.AddRootSignatureAssociation(rayGenSignature.Get(), true, {L"RayGen"});
    pipeline.AddRootSignatureAssociation(missSignature.Get(), true, {L"Miss", L"ShadowMiss"});

    constexpr D3D12_FILTER               filter = D3D12_FILTER_ANISOTROPIC;
    constexpr D3D12_TEXTURE_ADDRESS_MODE mode   = D3D12_TEXTURE_ADDRESS_MODE_CLAMP;

    globalShaderResources = std::make_shared<ShaderResources>();
    globalShaderResources->Initialize(
        [&pipelineDescription, this](ShaderResources::Description& graphics)
        {
            graphics.AddHeapDescriptorTable(
                [&](auto& table)
                {
                    rtColorDataForRasterEntry = table.AddShaderResourceView({.reg = 0});
                    rtDepthDataForRasterEntry = table.AddShaderResourceView({.reg = 1});
                });

            effectBindings = RasterPipeline::SetUpEffectBindings(*client, graphics);

            graphics.AddStaticSampler({.reg = 0}, filter, mode, pipelineDescription.anisotropy);
        },
        [&pipelineDescription, this](ShaderResources::Description& compute)
        {
            SetUpStaticResourceLayout(&compute);
            SetUpDynamicResourceLayout(&compute);

            for (auto& animation : animations) animation.SetUpResourceLayout(&compute);

            compute.AddStaticSampler({.reg = 0}, filter, mode, pipelineDescription.anisotropy);
        },
        GetDevice());

    NAME_D3D12_OBJECT(globalShaderResources->GetComputeRootSignature());
    NAME_D3D12_OBJECT(globalShaderResources->GetGraphicsRootSignature());

    InitializeAnimations();

    pipeline.SetMaxPayloadSize(sizeof(float) * (3 /* Color */ + 1 /* Alpha */ + 3 /* Normal */ + 1 /* Distance */));
    pipeline.SetMaxAttributeSize(sizeof(float) * 2 /* Barycentrics */);
    pipeline.SetMaxRecursionDepth(2);

    rtStateObject = pipeline.Generate(globalShaderResources->GetComputeRootSignature());
    NAME_D3D12_OBJECT(rtStateObject);

    TryDo(rtStateObject->QueryInterface(IID_PPV_ARGS(&rtStateObjectProperties)));

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
        shaderBlobs[shader] = CompileShader(pipelineDescription.shaderFiles[shader].path, L"", L"lib_6_7", VG_SHADER_REGISTRY(client), pipelineDescription.onShaderLoadingError);

        if (shaderBlobs[shader] == nullptr) return false;

        UINT const currentSymbolCount = pipelineDescription.shaderFiles[shader].symbolCount;

        std::vector<std::wstring> symbols;
        symbols.reserve(currentSymbolCount);

        for (UINT symbolOffset = 0; symbolOffset < currentSymbolCount; symbolOffset++) symbols.emplace_back(pipelineDescription.symbols[currentSymbolIndex++]);

        pipeline.AddLibrary(shaderBlobs[shader].Get(), symbols);

        return true;
    };

    auto compileComputeShader = [&](UINT const shader)
    {
        shaderBlobs[shader] = CompileShader(pipelineDescription.shaderFiles[shader].path, L"Main", L"cs_6_7", VG_SHADER_REGISTRY(client), pipelineDescription.onShaderLoadingError);

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

std::unique_ptr<Material> Space::SetUpMaterial(MaterialDescription const& description, UINT const index, nv_helpers_dx12::RayTracingPipelineGenerator& pipeline) const
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

    material->geometryType = normalIntersectionSymbol.empty() ? D3D12_RAYTRACING_GEOMETRY_TYPE_TRIANGLES : D3D12_RAYTRACING_GEOMETRY_TYPE_PROCEDURAL_PRIMITIVE_AABBS;

    UINT64 materialConstantBufferSize = sizeof MaterialBuffer;
    material->materialConstantBuffer  = util::AllocateConstantBuffer(*client, &materialConstantBufferSize);
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

        auto const             animationID = static_cast<UINT>(animations.size());
        ComPtr<IDxcBlob> const blob        = shaderBlobs[shaderIndex];

        constexpr UINT offset = 3;
        animations.emplace_back(blob, offset + animationID);

        animationShaderIndexToID[shaderIndex] = animationID;
    }

    for (UINT materialID = 0; materialID < pipeline.materialCount; materialID++)
    {
        MaterialDescription const& materialDescription = pipeline.materials[materialID];
        if (materialDescription.isAnimated)
        {
            UINT animationID                   = animationShaderIndexToID[materialDescription.animationShaderIndex];
            materials[materialID]->animationID = animationID;
        }
    }
}

void Space::SetUpStaticResourceLayout(ShaderResources::Description* description)
{
    description->AddConstantBufferView(camera.GetCameraBufferAddress(), {.reg = 0});
    if (customDataBuffer != nullptr) description->AddConstantBufferView(customDataBuffer->GetGPUVirtualAddress(), {.reg = 1});
    description->AddConstantBufferView(globalConstantBuffer.GetGPUVirtualAddress(), {.reg = 2});

    unchangedCommonResourceHandle = description->AddHeapDescriptorTable(
        [this](ShaderResources::Table& table)
        {
            textureSlot1.entry = table.AddShaderResourceView({.reg = 0, .space = 1}, textureSlot1.size);
            textureSlot2.entry = table.AddShaderResourceView({.reg = 0, .space = 2}, textureSlot2.size);
        });

    changedCommonResourceHandle = description->AddHeapDescriptorTable(
        [this](ShaderResources::Table& table)
        {
            bvhEntry         = table.AddShaderResourceView({.reg = 0});
            colorOutputEntry = table.AddUnorderedAccessView({.reg = 0});
            depthOutputEntry = table.AddUnorderedAccessView({.reg = 1});
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

    meshInstanceDataList = description->AddConstantBufferViewDescriptorList(
        {.reg = 4, .space = 0},
        CreateSizeGetter(&meshes.GetActive()),
        [this](UINT const index) { return meshes.GetActive()[static_cast<Drawable::ActiveIndex>(index)]->GetInstanceDataViewDescriptor(); },
        CreateBagBuilder(&meshes.GetActive(), getIndexOfMesh));

    meshGeometryBufferList = description->AddShaderResourceViewDescriptorList(
        {.reg = 1, .space = 0},
        CreateSizeGetter(&meshes.GetActive()),
        [this](UINT const index) { return meshes.GetActive()[static_cast<Drawable::ActiveIndex>(index)]->GetGeometryBufferViewDescriptor(); },
        CreateBagBuilder(&meshes.GetActive(), getIndexOfMesh));
}

void Space::SetUpAnimationResourceLayout(ShaderResources::Description* description) { for (auto& animation : animations) animation.SetUpResourceLayout(description); }

void Space::InitializeAnimations() { for (auto& animation : animations) animation.Initialize(*client, globalShaderResources->GetComputeRootSignature()); }

void Space::CreateRaytracingOutputBuffer()
{
    colorOutputDescription.DepthOrArraySize = 1;
    colorOutputDescription.Dimension        = D3D12_RESOURCE_DIMENSION_TEXTURE2D;

    colorOutputDescription.Format           = DXGI_FORMAT_B8G8R8A8_UNORM;
    colorOutputDescription.Flags            = D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;
    colorOutputDescription.Width            = resolution.width;
    colorOutputDescription.Height           = resolution.height;
    colorOutputDescription.Layout           = D3D12_TEXTURE_LAYOUT_UNKNOWN;
    colorOutputDescription.MipLevels        = 1;
    colorOutputDescription.SampleDesc.Count = 1;

    colorOutput = util::AllocateResource<ID3D12Resource>(*client, colorOutputDescription, D3D12_HEAP_TYPE_DEFAULT, D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE);
    NAME_D3D12_OBJECT(colorOutput);

    depthOutputDescription.DepthOrArraySize = 1;
    depthOutputDescription.Dimension        = D3D12_RESOURCE_DIMENSION_TEXTURE2D;

    depthOutputDescription.Format           = DXGI_FORMAT_R32_FLOAT;
    depthOutputDescription.Flags            = D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;
    depthOutputDescription.Width            = resolution.width;
    depthOutputDescription.Height           = resolution.height;
    depthOutputDescription.Layout           = D3D12_TEXTURE_LAYOUT_UNKNOWN;
    depthOutputDescription.MipLevels        = 1;
    depthOutputDescription.SampleDesc.Count = 1;

    depthOutput = util::AllocateResource<ID3D12Resource>(*client, depthOutputDescription, D3D12_HEAP_TYPE_DEFAULT, D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE);
    NAME_D3D12_OBJECT(depthOutput);

    outputResourcesFresh = true;
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
    sbtHelper.Reset();

    Require(!outputResourcesFresh);

    sbtHelper.AddRayGenerationProgram(L"RayGen", {});

    sbtHelper.AddMissProgram(L"Miss", {});
    sbtHelper.AddMissProgram(L"ShadowMiss", {});

    for (auto const& material : materials)
    {
        auto materialCB = std::bit_cast<void*>(material->materialConstantBuffer.GetGPUVirtualAddress());
        sbtHelper.AddHitGroup(material->normalHitGroup, {materialCB});
        sbtHelper.AddHitGroup(material->shadowHitGroup, {materialCB});
    }

    uint32_t const sbtSize = sbtHelper.ComputeSBTSize();

    util::ReAllocateBuffer(&sbtStorage, *client, sbtSize, D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_GENERIC_READ, D3D12_HEAP_TYPE_UPLOAD);
    NAME_D3D12_OBJECT(sbtStorage);

    sbtHelper.Generate(sbtStorage.Get(), rtStateObjectProperties.Get());
}

void Space::EnqueueUploads() const { for (auto* group : drawableGroups) group->EnqueueDataUpload(GetCommandList()); }

void Space::RunAnimations() { for (auto& animation : animations) animation.Run(*globalShaderResources, GetCommandList()); }

void Space::BuildAccelerationStructures()
{
    uavs.clear();
    uavs.reserve(animations.size() + meshes.GetModifiedCount());

    for (auto& animation : animations) animation.CreateBLAS(GetCommandList(), &uavs);

    for (Mesh* mesh : meshes.GetModified()) mesh->CreateBLAS(GetCommandList(), &uavs);

    resultBufferAllocator.CreateBarriers(GetCommandList(), uavs);

    CreateTLAS();
    UpdateTopLevelAccelerationStructureView();
}

void Space::CreateTLAS()
{
    tlasGenerator.Clear();

    meshes.GetActive().ForEach(
        [this](Mesh* mesh)
        {
            Require(mesh->GetActiveIndex().has_value());
            auto const instanceID = static_cast<UINT>(mesh->GetActiveIndex().value());

            // The CCW flag is used because DirectX uses left-handed coordinates.

            tlasGenerator.AddInstance(
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

    tlasGenerator.ComputeASBufferSizes(GetDevice().Get(), false, &scratchSize, &resultSize, &instanceDescriptionSize);

    bool const committed = client->SupportPIX();

    util::ReAllocateBuffer(
        &topLevelASBuffers.scratch,
        *client,
        scratchSize,
        D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS,
        D3D12_RESOURCE_STATE_COMMON,
        D3D12_HEAP_TYPE_DEFAULT,
        committed);
    util::ReAllocateBuffer(
        &topLevelASBuffers.result,
        *client,
        resultSize,
        D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS,
        D3D12_RESOURCE_STATE_RAYTRACING_ACCELERATION_STRUCTURE,
        D3D12_HEAP_TYPE_DEFAULT,
        committed);
    util::ReAllocateBuffer(
        &topLevelASBuffers.instanceDescription,
        *client,
        instanceDescriptionSize,
        D3D12_RESOURCE_FLAG_NONE,
        D3D12_RESOURCE_STATE_GENERIC_READ,
        D3D12_HEAP_TYPE_UPLOAD,
        committed);

    NAME_D3D12_OBJECT(topLevelASBuffers.scratch);
    NAME_D3D12_OBJECT(topLevelASBuffers.result);
    NAME_D3D12_OBJECT(topLevelASBuffers.instanceDescription);

    tlasGenerator.Generate(GetCommandList().Get(), topLevelASBuffers.scratch, topLevelASBuffers.result, topLevelASBuffers.instanceDescription);
}

void Space::DispatchRays() const
{
    std::array const barriers = {
        CD3DX12_RESOURCE_BARRIER::Transition(colorOutput.Get(), D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE, D3D12_RESOURCE_STATE_UNORDERED_ACCESS),
        CD3DX12_RESOURCE_BARRIER::Transition(depthOutput.Get(), D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE, D3D12_RESOURCE_STATE_UNORDERED_ACCESS)
    };
    GetCommandList()->ResourceBarrier(static_cast<UINT>(barriers.size()), barriers.data());

    D3D12_DISPATCH_RAYS_DESC desc = {};

    desc.RayGenerationShaderRecord.StartAddress = sbtStorage.GetGPUVirtualAddress() + sbtHelper.GetRayGenSectionOffset();
    desc.RayGenerationShaderRecord.SizeInBytes  = sbtHelper.GetRayGenSectionSize();

    desc.MissShaderTable.StartAddress  = sbtStorage.GetGPUVirtualAddress() + sbtHelper.GetMissSectionOffset();
    desc.MissShaderTable.SizeInBytes   = sbtHelper.GetMissSectionSize();
    desc.MissShaderTable.StrideInBytes = sbtHelper.GetMissEntrySize();

    desc.HitGroupTable.StartAddress  = sbtStorage.GetGPUVirtualAddress() + sbtHelper.GetHitGroupSectionOffset();
    desc.HitGroupTable.SizeInBytes   = sbtHelper.GetHitGroupSectionSize();
    desc.HitGroupTable.StrideInBytes = sbtHelper.GetHitGroupEntrySize();

    desc.Width  = resolution.width;
    desc.Height = resolution.height;
    desc.Depth  = 1;

    GetCommandList()->SetPipelineState1(rtStateObject.Get());
    GetCommandList()->DispatchRays(&desc);
}

void Space::CopyOutputToBuffers(Allocation<ID3D12Resource> const& color, Allocation<ID3D12Resource> const& depth) const
{
    std::array const entry = {
        CD3DX12_RESOURCE_BARRIER::Transition(colorOutput.Get(), D3D12_RESOURCE_STATE_UNORDERED_ACCESS, D3D12_RESOURCE_STATE_COPY_SOURCE),
        CD3DX12_RESOURCE_BARRIER::Transition(depthOutput.Get(), D3D12_RESOURCE_STATE_UNORDERED_ACCESS, D3D12_RESOURCE_STATE_COPY_SOURCE),
        CD3DX12_RESOURCE_BARRIER::Transition(color.Get(), D3D12_RESOURCE_STATE_RENDER_TARGET, D3D12_RESOURCE_STATE_COPY_DEST),
        CD3DX12_RESOURCE_BARRIER::Transition(depth.Get(), D3D12_RESOURCE_STATE_DEPTH_WRITE, D3D12_RESOURCE_STATE_COPY_DEST)
    };
    GetCommandList()->ResourceBarrier(static_cast<UINT>(entry.size()), entry.data());

    GetCommandList()->CopyResource(color.Get(), colorOutput.Get());
    GetCommandList()->CopyResource(depth.Get(), depthOutput.Get());

    std::array const exit = {
        CD3DX12_RESOURCE_BARRIER::Transition(color.Get(), D3D12_RESOURCE_STATE_COPY_DEST, D3D12_RESOURCE_STATE_RENDER_TARGET),
        CD3DX12_RESOURCE_BARRIER::Transition(depth.Get(), D3D12_RESOURCE_STATE_COPY_DEST, D3D12_RESOURCE_STATE_DEPTH_WRITE)
    };
    GetCommandList()->ResourceBarrier(static_cast<UINT>(exit.size()), exit.data());
}

void Space::DrawEffects(RenderData const& data)
{
    std::array const barriers = {
        CD3DX12_RESOURCE_BARRIER::Transition(colorOutput.Get(), D3D12_RESOURCE_STATE_COPY_SOURCE, D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE),
        CD3DX12_RESOURCE_BARRIER::Transition(depthOutput.Get(), D3D12_RESOURCE_STATE_COPY_SOURCE, D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE)
    };
    GetCommandList()->ResourceBarrier(static_cast<UINT>(barriers.size()), barriers.data());

    GetCommandList()->OMSetRenderTargets(1, data.rtv, FALSE, data.dsv);

    data.viewport->Set(GetCommandList());

    effects.GetActive().ForEach([this](Effect const* effect) { effect->Draw(GetCommandList()); });
}

void Space::UpdateOutputResourceViews()
{
    if (!colorOutputEntry.IsValid() || !depthOutputEntry.IsValid()) return;

    if (!outputResourcesFresh) return;
    outputResourcesFresh = false;

    D3D12_UNORDERED_ACCESS_VIEW_DESC uavDesc = {};
    uavDesc.ViewDimension                    = D3D12_UAV_DIMENSION_TEXTURE2D;

    uavDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
    globalShaderResources->CreateUnorderedAccessView(colorOutputEntry, 0, {colorOutput, &uavDesc});

    uavDesc.Format = DXGI_FORMAT_R32_FLOAT;
    globalShaderResources->CreateUnorderedAccessView(depthOutputEntry, 0, {depthOutput, &uavDesc});

    D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc = {};
    srvDesc.ViewDimension                   = D3D12_SRV_DIMENSION_TEXTURE2D;
    srvDesc.Shader4ComponentMapping         = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;

    srvDesc.Format              = colorOutputDescription.Format;
    srvDesc.Texture2D.MipLevels = colorOutputDescription.MipLevels;
    globalShaderResources->CreateShaderResourceView(rtColorDataForRasterEntry, 0, {colorOutput, &srvDesc});

    srvDesc.Format              = depthOutputDescription.Format;
    srvDesc.Texture2D.MipLevels = depthOutputDescription.MipLevels;
    globalShaderResources->CreateShaderResourceView(rtDepthDataForRasterEntry, 0, {depthOutput, &srvDesc});
}

void Space::UpdateTopLevelAccelerationStructureView() const
{
    D3D12_SHADER_RESOURCE_VIEW_DESC srvDescription;
    srvDescription.Format                                   = DXGI_FORMAT_UNKNOWN;
    srvDescription.ViewDimension                            = D3D12_SRV_DIMENSION_RAYTRACING_ACCELERATION_STRUCTURE;
    srvDescription.Shader4ComponentMapping                  = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
    srvDescription.RaytracingAccelerationStructure.Location = topLevelASBuffers.result.resource->GetGPUVirtualAddress();

    globalShaderResources->CreateShaderResourceView(bvhEntry, 0, {{}, &srvDescription});
}

void Space::UpdateGlobalShaderResources()
{
    IntegerSet const meshesToRefresh = meshes.ClearChanged();
    for (auto& animation : animations) animation.Update(*globalShaderResources);

    globalShaderResources->RequestListRefresh(meshInstanceDataList, meshesToRefresh);
    globalShaderResources->RequestListRefresh(meshGeometryBufferList, meshesToRefresh);
    globalShaderResources->Update();

    effects.ClearChanged();
}
