#include "stdafx.h"

#include <iostream>

Space::Space(NativeClient& nativeClient) :
    m_nativeClient(nativeClient),
    m_camera(nativeClient),
    m_light(nativeClient)
{
}

void Space::PerformInitialSetupStepOne(ComPtr<ID3D12CommandQueue> commandQueue)
{
    REQUIRE(m_meshes.empty());

    INITIALIZE_COMMAND_ALLOCATOR_GROUP(GetDevice(), &m_commandGroup, D3D12_COMMAND_LIST_TYPE_DIRECT);
    m_commandGroup.Reset(0);
    
    CreateTopLevelAS();

    m_commandGroup.Close();
    ID3D12CommandList* ppCommandLists[] = {m_commandGroup.commandList.Get()};
    commandQueue->ExecuteCommandLists(_countof(ppCommandLists), ppCommandLists);

    m_nativeClient.WaitForGPU();
}

void Space::PerformResolutionDependentSetup(const Resolution& resolution)
{
    m_resolution = resolution;

    CreateRaytracingOutputBuffer();
    m_camera.Initialize();
}

bool Space::PerformInitialSetupStepTwo(const SpacePipeline& pipeline)
{
    CreateShaderResourceHeap();
    if (!CreateRaytracingPipeline(pipeline)) return false;

    CreateGlobalConstBuffer();
    CreateShaderBindingTable();

    return true;
}

MeshObject& Space::CreateMeshObject(UINT materialIndex)
{
    auto object = std::make_unique<MeshObject>(m_nativeClient, materialIndex);
    auto& indexedMeshObject = *object;

    const auto handle = m_meshes.insert(m_meshes.end(), std::move(object));
    indexedMeshObject.AssociateWithHandle(handle);

    return indexedMeshObject;
}

void Space::FreeMeshObject(const MeshObject::Handle handle)
{
    m_meshes.erase(handle);
}

const Material& Space::GetMaterial(const UINT index) const
{
    return *m_materials[index];
}

void Space::Reset(const UINT frameIndex) const
{
    m_commandGroup.Reset(frameIndex);
}

void Space::EnqueueRenderSetup()
{
    bool modified = false;

    for (const auto& mesh : m_meshes)
    {
        if (mesh->IsMeshModified())
        {
            mesh->EnqueueMeshUpload(m_commandGroup.commandList);
            mesh->CreateBLAS(m_commandGroup.commandList);

            modified = true;
        }
    }

    CreateTopLevelAS();
    UpdateShaderResourceHeap();

    if (modified)
    {
        CreateShaderBindingTable();
    }
}

void Space::CleanupRenderSetup() const
{
    for (const auto& mesh : m_meshes)
    {
        mesh->CleanupMeshUpload();
    }
}

void Space::DispatchRays() const
{
    const CD3DX12_RESOURCE_BARRIER barrier = CD3DX12_RESOURCE_BARRIER::Transition(
        m_outputResource.Get(), D3D12_RESOURCE_STATE_COPY_SOURCE,
        D3D12_RESOURCE_STATE_UNORDERED_ACCESS);
    m_commandGroup.commandList->ResourceBarrier(1, &barrier);

    const std::vector heaps = {m_srvUavHeap.Get()};
    m_commandGroup.commandList->SetDescriptorHeaps(static_cast<UINT>(heaps.size()),
                                                   heaps.data());

    D3D12_DISPATCH_RAYS_DESC desc = {};

    desc.RayGenerationShaderRecord.StartAddress = m_sbtStorage->GetGPUVirtualAddress() + m_sbtHelper.
        GetRayGenSectionOffset();
    desc.RayGenerationShaderRecord.SizeInBytes = m_sbtHelper.GetRayGenSectionSize();

    desc.MissShaderTable.StartAddress = m_sbtStorage->GetGPUVirtualAddress() + m_sbtHelper.GetMissSectionOffset();
    desc.MissShaderTable.SizeInBytes = m_sbtHelper.GetMissSectionSize();
    desc.MissShaderTable.StrideInBytes = m_sbtHelper.GetMissEntrySize();

    desc.HitGroupTable.StartAddress = m_sbtStorage->GetGPUVirtualAddress() + m_sbtHelper.GetHitGroupSectionOffset();
    desc.HitGroupTable.SizeInBytes = m_sbtHelper.GetHitGroupSectionSize();
    desc.HitGroupTable.StrideInBytes = m_sbtHelper.GetHitGroupEntrySize();

    desc.Width = m_resolution.width;
    desc.Height = m_resolution.height;
    desc.Depth = 1;

    m_commandGroup.commandList->SetPipelineState1(m_rtStateObject.Get());
    m_commandGroup.commandList->DispatchRays(&desc);
}

void Space::CopyOutputToBuffer(const ComPtr<ID3D12Resource> buffer) const
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

ComPtr<ID3D12Device5> Space::GetDevice() const
{
    return m_nativeClient.GetDevice();
}

void Space::CreateGlobalConstBuffer()
{
    m_globalConstantBufferData = {.time = 0.0f, .minLight = 0.4f};
    m_globalConstantBufferAlignedSize = sizeof m_globalConstantBufferData;
    m_globalConstantBuffer = nv_helpers_dx12::CreateConstantBuffer(GetDevice().Get(),
                                                                   &m_globalConstantBufferAlignedSize,
                                                                   D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_GENERIC_READ,
                                                                   nv_helpers_dx12::kUploadHeapProps);

    UpdateGlobalConstBuffer();
}

void Space::UpdateGlobalConstBuffer() const
{
    uint8_t* pData;
    TRY_DO(m_globalConstantBuffer->Map(0, nullptr, reinterpret_cast<void**>(&pData)));

    memcpy(pData, &m_globalConstantBufferData, sizeof m_globalConstantBufferData);

    m_globalConstantBuffer->Unmap(0, nullptr);
}

void Space::CreateShaderResourceHeap()
{
    m_srvUavHeap = nv_helpers_dx12::CreateDescriptorHeap(
        GetDevice().Get(),
        3,
        D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV,
        true);

    NAME_D3D12_OBJECT(m_srvUavHeap);

    UpdateShaderResourceHeap();
}

void Space::UpdateShaderResourceHeap() const
{
    if (m_srvUavHeap == nullptr) return;

    D3D12_CPU_DESCRIPTOR_HANDLE srvHandle = m_srvUavHeap->GetCPUDescriptorHandleForHeapStart();

    D3D12_UNORDERED_ACCESS_VIEW_DESC uavDesc = {};
    uavDesc.ViewDimension = D3D12_UAV_DIMENSION_TEXTURE2D;
    GetDevice()->CreateUnorderedAccessView(m_outputResource.Get(), nullptr, &uavDesc, srvHandle);

    srvHandle.ptr += GetDevice()->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
    D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc;
    srvDesc.Format = DXGI_FORMAT_UNKNOWN;
    srvDesc.ViewDimension = D3D12_SRV_DIMENSION_RAYTRACING_ACCELERATION_STRUCTURE;
    srvDesc.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
    srvDesc.RaytracingAccelerationStructure.Location = m_topLevelASBuffers.result->GetGPUVirtualAddress();
    GetDevice()->CreateShaderResourceView(nullptr, &srvDesc, srvHandle);

    srvHandle.ptr += GetDevice()->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
    D3D12_CONSTANT_BUFFER_VIEW_DESC cbvDesc = {};
    m_camera.SetBufferViewDescription(&cbvDesc);
    GetDevice()->CreateConstantBufferView(&cbvDesc, srvHandle);
}

bool Space::CreateRaytracingPipeline(const SpacePipeline& pipelineDescription)
{
    nv_helpers_dx12::RayTracingPipelineGenerator pipeline(GetDevice().Get());
    m_shaderBlobs = std::vector<ComPtr<IDxcBlob>>(pipelineDescription.description.shaderCount);

    UINT currentSymbolIndex = 0;

    for (UINT shader = 0; shader < pipelineDescription.description.shaderCount; shader++)
    {
        m_shaderBlobs[shader] = nv_helpers_dx12::CompileShaderLibrary(
            pipelineDescription.shaderFiles[shader].path,
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

#if defined(_DEBUG) || defined(DBG)


#endif
    }
    
    m_rayGenSignature = CreateRayGenSignature();
    m_missSignature = CreateMissSignature();

    UINT currentHitGroupIndex = 0;

    for (UINT material = 0; material < pipelineDescription.description.materialCount; material++)
    {
        auto m = std::make_unique<Material>();

        auto addHitGroup = [&](const std::wstring& closestHitSymbol)
            -> std::tuple<std::wstring, ComPtr<ID3D12RootSignature>>
        {
            ComPtr<ID3D12RootSignature> rootSignature = CreateMaterialSignature();
            std::wstring hitGroup = std::to_wstring(currentHitGroupIndex++);

            pipeline.AddHitGroup(hitGroup, closestHitSymbol);
            pipeline.AddRootSignatureAssociation(rootSignature.Get(), {hitGroup});

            return {hitGroup, rootSignature};
        };

        std::tie(m->normalHitGroup, m->normalRootSignature)
            = addHitGroup(pipelineDescription.materials[material].closestHitSymbol);

        std::tie(m->shadowHitGroup, m->shadowRootSignature)
            = addHitGroup(pipelineDescription.materials[material].shadowHitSymbol);

#if defined(_DEBUG) || defined(DBG)
        std::wstring debugName = pipelineDescription.materials[material].debugName;
        m->normalRootSignature->SetName((L"RT Material Normal RS " + debugName).c_str());
        m->shadowRootSignature->SetName((L"RT Material Shadow RS " + debugName).c_str());
#endif

        m_materials.push_back(std::move(m));
    }

    NAME_D3D12_OBJECT(m_rayGenSignature);
    NAME_D3D12_OBJECT(m_missSignature);

    pipeline.AddRootSignatureAssociation(m_rayGenSignature.Get(), {L"RayGen"});
    pipeline.AddRootSignatureAssociation(m_missSignature.Get(), {L"Miss", L"ShadowMiss"});

    pipeline.SetMaxPayloadSize(4 * sizeof(float));
    pipeline.SetMaxAttributeSize(2 * sizeof(float));
    pipeline.SetMaxRecursionDepth(2);

    m_rtStateObject = pipeline.Generate();
    NAME_D3D12_OBJECT(m_rtStateObject);

    TRY_DO(m_rtStateObject->QueryInterface(IID_PPV_ARGS(&m_rtStateObjectProperties)));

    return true;
}

void Space::CreateRaytracingOutputBuffer()
{
    D3D12_RESOURCE_DESC resDesc = {};
    resDesc.DepthOrArraySize = 1;
    resDesc.Dimension = D3D12_RESOURCE_DIMENSION_TEXTURE2D;

    resDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    resDesc.Flags = D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS;
    resDesc.Width = m_resolution.width;
    resDesc.Height = m_resolution.height;
    resDesc.Layout = D3D12_TEXTURE_LAYOUT_UNKNOWN;
    resDesc.MipLevels = 1;
    resDesc.SampleDesc.Count = 1;

    TRY_DO(GetDevice()->CreateCommittedResource(
        &nv_helpers_dx12::kDefaultHeapProps,
        D3D12_HEAP_FLAG_NONE,
        &resDesc,
        D3D12_RESOURCE_STATE_COPY_SOURCE,
        nullptr,
        IID_PPV_ARGS(&m_outputResource)));

    NAME_D3D12_OBJECT(m_outputResource);
}

ComPtr<ID3D12RootSignature> Space::CreateRayGenSignature() const
{
    nv_helpers_dx12::RootSignatureGenerator rsc;

    rsc.AddHeapRangesParameter({
        {0, 1, 0, D3D12_DESCRIPTOR_RANGE_TYPE_UAV, 0}, // Output Texture
        {0, 1, 0, D3D12_DESCRIPTOR_RANGE_TYPE_SRV, 1}, // BVH
        {0, 1, 0, D3D12_DESCRIPTOR_RANGE_TYPE_CBV, 2} // Camera Data
    });

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

    rsc.AddRootParameter(D3D12_ROOT_PARAMETER_TYPE_SRV, 0); // Vertex Buffer
    rsc.AddRootParameter(D3D12_ROOT_PARAMETER_TYPE_SRV, 1); // Index Buffer

    rsc.AddHeapRangesParameter({
        {2, 1, 0, D3D12_DESCRIPTOR_RANGE_TYPE_SRV, 1} // BVH
    });

    rsc.AddRootParameter(D3D12_ROOT_PARAMETER_TYPE_CBV, 0); // Global Data
    rsc.AddRootParameter(D3D12_ROOT_PARAMETER_TYPE_CBV, 1); // Instance Data

    return rsc.Generate(GetDevice().Get(), true);
}

void Space::CreateShaderBindingTable()
{
    m_sbtHelper.Reset();

    const auto [srvUavHeapHandlePtr] =
        m_srvUavHeap->GetGPUDescriptorHandleForHeapStart();

    auto heapPointer = reinterpret_cast<UINT64*>(srvUavHeapHandlePtr);

    m_sbtHelper.AddRayGenerationProgram(L"RayGen", {heapPointer});

    m_sbtHelper.AddMissProgram(L"Miss", {});
    m_sbtHelper.AddMissProgram(L"ShadowMiss", {});

    StandardShaderArguments arguments = {
        .heap = heapPointer,
        .globalBuffer = reinterpret_cast<void*>(m_globalConstantBuffer->GetGPUVirtualAddress()),
        .instanceBuffer = nullptr // Will be set per instance.
    };

    for (const auto& mesh : m_meshes)
    {
        if (mesh->IsEnabled())
        {
            mesh->FillArguments(arguments);
            mesh->SetupHitGroup(m_sbtHelper, arguments);
        }
    }

    // todo: add a proxy hit group if we have no meshes to make PIX happy 

    const uint32_t sbtSize = m_sbtHelper.ComputeSBTSize();

    m_sbtStorage = nv_helpers_dx12::CreateBuffer(
        GetDevice().Get(), sbtSize, D3D12_RESOURCE_FLAG_NONE,
        D3D12_RESOURCE_STATE_GENERIC_READ, nv_helpers_dx12::kUploadHeapProps);

    REQUIRE(m_sbtStorage != nullptr);
    NAME_D3D12_OBJECT(m_sbtStorage);

    m_sbtHelper.Generate(m_sbtStorage.Get(), m_rtStateObjectProperties.Get());
}

void Space::CreateTopLevelAS()
{
    nv_helpers_dx12::TopLevelASGenerator topLevelASGenerator;

    UINT instanceID = 0;
    for (const auto& mesh : m_meshes)
    {
        if (mesh->IsEnabled())
        {
            topLevelASGenerator.AddInstance(mesh->GetBLAS().Get(), mesh->GetTransform(),
                                            instanceID++, 2 * mesh->GetMaterialIndex());
        }
    }

    UINT64 scratchSize, resultSize, instanceDescriptionSize;
    topLevelASGenerator.ComputeASBufferSizes(GetDevice().Get(), true, &scratchSize, &resultSize,
                                             &instanceDescriptionSize);

    m_topLevelASBuffers.scratch = nv_helpers_dx12::CreateBuffer(GetDevice().Get(), scratchSize,
                                                                D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS,
                                                                D3D12_RESOURCE_STATE_COMMON,
                                                                nv_helpers_dx12::kDefaultHeapProps);
    m_topLevelASBuffers.result = nv_helpers_dx12::CreateBuffer(GetDevice().Get(), resultSize,
                                                               D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS,
                                                               D3D12_RESOURCE_STATE_RAYTRACING_ACCELERATION_STRUCTURE,
                                                               nv_helpers_dx12::kDefaultHeapProps);
    m_topLevelASBuffers.instanceDesc = nv_helpers_dx12::CreateBuffer(GetDevice().Get(), instanceDescriptionSize,
                                                                     D3D12_RESOURCE_FLAG_NONE,
                                                                     D3D12_RESOURCE_STATE_GENERIC_READ,
                                                                     nv_helpers_dx12::kUploadHeapProps);

    NAME_D3D12_OBJECT(m_topLevelASBuffers.scratch);
    NAME_D3D12_OBJECT(m_topLevelASBuffers.result);
    NAME_D3D12_OBJECT(m_topLevelASBuffers.instanceDesc);

    constexpr bool updateOnly = false;

    topLevelASGenerator.Generate(m_commandGroup.commandList.Get(),
                                 m_topLevelASBuffers.scratch.Get(), m_topLevelASBuffers.result.Get(),
                                 m_topLevelASBuffers.instanceDesc.Get(),
                                 updateOnly, m_topLevelASBuffers.result.Get());
}
