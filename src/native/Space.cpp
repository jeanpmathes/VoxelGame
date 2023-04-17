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
    assert(m_meshes.empty());

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

void Space::PerformInitialSetupStepTwo(const ShaderPaths& paths)
{
    CreateShaderResourceHeap();
    CreateRaytracingPipeline(paths);

    CreateGlobalConstBuffer();
    CreateShaderBindingTable();
}

SequencedMeshObject& Space::CreateSequencedMeshObject()
{
    auto object = std::make_unique<SequencedMeshObject>(m_nativeClient);
    auto& sequencedMeshObject = *object;

    m_meshes.push_back(std::move(object));

    return sequencedMeshObject;
}

IndexedMeshObject& Space::CreateIndexedMeshObject()
{
    auto object = std::make_unique<IndexedMeshObject>(m_nativeClient);
    auto& indexedMeshObject = *object;

    m_meshes.push_back(std::move(object));

    return indexedMeshObject;
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
    m_globalConstantBufferData.lightPosition = m_light.GetPosition();

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

void Space::CreateRaytracingPipeline(const ShaderPaths& paths)
{
    nv_helpers_dx12::RayTracingPipelineGenerator pipeline(GetDevice().Get());

    // todo: use Material abstraction, and allow passing arbitrary count of shader paths + symbols
    // todo: the raytracing pipeline should use an abstraction similar to shader buffer
    // (but not the same as raytracing does not need a descriptor heap per buffer, on c# side they could be the same)
    // or on c# side raytracing the mesh object integrates the generic data buffer for raytracing

    m_rayGenLibrary = nv_helpers_dx12::CompileShaderLibrary(paths.rayGenShader.c_str());
    m_missLibrary = nv_helpers_dx12::CompileShaderLibrary(paths.missShader.c_str());
    m_hitLibrary = nv_helpers_dx12::CompileShaderLibrary(paths.hitShader.c_str());
    m_shadowLibrary = nv_helpers_dx12::CompileShaderLibrary(paths.shadowShader.c_str());

    pipeline.AddLibrary(m_rayGenLibrary.Get(), {L"RayGen"});
    pipeline.AddLibrary(m_missLibrary.Get(), {L"Miss"});
    pipeline.AddLibrary(m_hitLibrary.Get(), {L"IndexedClosestHit", L"SequencedClosestHit"});
    pipeline.AddLibrary(m_shadowLibrary.Get(),
                        {L"IndexedShadowClosestHit", L"SequencedShadowClosestHit", L"ShadowMiss"});

    m_rayGenSignature = CreateRayGenSignature();
    m_missSignature = CreateMissSignature();
    m_hitSignatureSequenced = SequencedMeshObject::CreateRootSignature(GetDevice());
    m_shadowSignatureSequenced = SequencedMeshObject::CreateRootSignature(GetDevice());
    m_hitSignatureIndexed = IndexedMeshObject::CreateRootSignature(GetDevice());
    m_shadowSignatureIndexed = IndexedMeshObject::CreateRootSignature(GetDevice());

    NAME_D3D12_OBJECT(m_rayGenSignature);
    NAME_D3D12_OBJECT(m_missSignature);
    NAME_D3D12_OBJECT(m_hitSignatureSequenced);
    NAME_D3D12_OBJECT(m_shadowSignatureSequenced);
    NAME_D3D12_OBJECT(m_hitSignatureIndexed);
    NAME_D3D12_OBJECT(m_shadowSignatureIndexed);

    pipeline.AddHitGroup(L"IndexedHitGroup", L"IndexedClosestHit");
    pipeline.AddHitGroup(L"IndexedShadowHitGroup", L"IndexedShadowClosestHit");
    pipeline.AddHitGroup(L"SequencedHitGroup", L"SequencedClosestHit");
    pipeline.AddHitGroup(L"SequencedShadowHitGroup", L"SequencedShadowClosestHit");

    pipeline.AddRootSignatureAssociation(m_rayGenSignature.Get(), {L"RayGen"});
    pipeline.AddRootSignatureAssociation(m_missSignature.Get(), {L"Miss", L"ShadowMiss"});
    pipeline.AddRootSignatureAssociation(m_hitSignatureIndexed.Get(), {L"IndexedHitGroup"});
    pipeline.AddRootSignatureAssociation(m_hitSignatureSequenced.Get(), {L"SequencedHitGroup"});
    pipeline.AddRootSignatureAssociation(m_shadowSignatureSequenced.Get(), {L"SequencedShadowHitGroup"});
    pipeline.AddRootSignatureAssociation(m_shadowSignatureIndexed.Get(), {L"IndexedShadowHitGroup"});

    pipeline.SetMaxPayloadSize(4 * sizeof(float));
    pipeline.SetMaxAttributeSize(2 * sizeof(float));
    pipeline.SetMaxRecursionDepth(2);

    m_rtStateObject = pipeline.Generate();
    NAME_D3D12_OBJECT(m_rtStateObject);

    TRY_DO(m_rtStateObject->QueryInterface(IID_PPV_ARGS(&m_rtStateObjectProperties)));
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
        {0, 1, 0, D3D12_DESCRIPTOR_RANGE_TYPE_UAV, 0},
        {0, 1, 0, D3D12_DESCRIPTOR_RANGE_TYPE_SRV, 1},
        {0, 1, 0, D3D12_DESCRIPTOR_RANGE_TYPE_CBV, 2}
    });

    return rsc.Generate(GetDevice().Get(), true);
}

ComPtr<ID3D12RootSignature> Space::CreateMissSignature() const
{
    nv_helpers_dx12::RootSignatureGenerator rsc;
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
        mesh->FillArguments(arguments);
        mesh->SetupHitGroup(m_sbtHelper, arguments);
    }

    // todo: add a proxy hit group if we have no meshes to make PIX happy 

    const uint32_t sbtSize = m_sbtHelper.ComputeSBTSize();

    m_sbtStorage = nv_helpers_dx12::CreateBuffer(
        GetDevice().Get(), sbtSize, D3D12_RESOURCE_FLAG_NONE,
        D3D12_RESOURCE_STATE_GENERIC_READ, nv_helpers_dx12::kUploadHeapProps);

    assert(m_sbtStorage != nullptr);
    NAME_D3D12_OBJECT(m_sbtStorage);

    m_sbtHelper.Generate(m_sbtStorage.Get(), m_rtStateObjectProperties.Get());
}

void Space::CreateTopLevelAS()
{
    nv_helpers_dx12::TopLevelASGenerator topLevelASGenerator;

    for (size_t index = 0; index < m_meshes.size(); index++)
    {
        MeshObject& mesh = *m_meshes[index];
        topLevelASGenerator.AddInstance(mesh.GetBLAS().Get(), mesh.GetTransform(), static_cast<UINT>(index),
                                        2 * static_cast<UINT>(index));
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
