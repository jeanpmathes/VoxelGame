#include "stdafx.h"

AnimationController::AnimationController(ComPtr<IDxcBlob> const& shader, UINT const space)
    : m_threadGroupDataLocation({.reg = 0, .space = space})
  , m_inputGeometryListLocation({.reg = 1, .space = space})  // SRV
  , m_outputGeometryListLocation({.reg = 0, .space = space}) // UAV
{
    TryDo(shader->QueryInterface<ID3DBlob>(&m_shader));

    m_threadGroupDataViewDescription.Format                     = DXGI_FORMAT_UNKNOWN;
    m_threadGroupDataViewDescription.ViewDimension              = D3D12_SRV_DIMENSION_BUFFER;
    m_threadGroupDataViewDescription.Shader4ComponentMapping    = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
    m_threadGroupDataViewDescription.Buffer.FirstElement        = 0;
    m_threadGroupDataViewDescription.Buffer.NumElements         = 0;
    m_threadGroupDataViewDescription.Buffer.StructureByteStride = sizeof(anim::ThreadGroup);
    m_threadGroupDataViewDescription.Buffer.Flags               = D3D12_BUFFER_SRV_FLAG_NONE;
}

void AnimationController::SetupResourceLayout(ShaderResources::Description* description)
{
    std::function<UINT(Mesh* const&)> const getIndexOfMesh = [this](auto* mesh)
    {
        Require(mesh != nullptr);
        Require(mesh->GetAnimationHandle() != Handle::INVALID);

        return static_cast<UINT>(mesh->GetAnimationHandle());
    };

    m_resourceTable = description->AddHeapDescriptorTable(
        [this](auto& table) { m_threadGroupDataEntry = table.AddShaderResourceView(m_threadGroupDataLocation); });

    auto getSourceDescriptor = [this](UINT const index)
    {
        return m_meshes[static_cast<Handle>(index)]->GetAnimationSourceBufferViewDescriptor();
    };

    auto getDestinationDescriptor = [this](UINT const index)
    {
        return m_meshes[static_cast<Handle>(index)]->GetAnimationDestinationBufferViewDescriptor();
    };

    m_srcGeometryList = description->AddShaderResourceViewDescriptorList(
        m_inputGeometryListLocation,
        CreateSizeGetter(&m_meshes),
        getSourceDescriptor,
        CreateBagBuilder(&m_meshes, getIndexOfMesh));

    m_dstGeometryList = description->AddUnorderedAccessViewDescriptorList(
        m_outputGeometryListLocation,
        CreateSizeGetter(&m_meshes),
        getDestinationDescriptor,
        CreateBagBuilder(&m_meshes, getIndexOfMesh));
}

void AnimationController::Initialize(NativeClient& client, ComPtr<ID3D12RootSignature> const& rootSignature)
{
    m_client = &client;

    D3D12_COMPUTE_PIPELINE_STATE_DESC pipelineStateDescription = {};
    pipelineStateDescription.pRootSignature                    = rootSignature.Get();
    pipelineStateDescription.CS                                = CD3DX12_SHADER_BYTECODE(m_shader.Get());

    TryDo(m_client->GetDevice()->CreateComputePipelineState(&pipelineStateDescription, IID_PPV_ARGS(&m_pipelineState)));
}

void AnimationController::AddMesh(Mesh& mesh)
{
    Require(mesh.GetMaterial().IsAnimated());
    Require(mesh.GetAnimationHandle() == Handle::INVALID);

    Handle const handle = m_meshes.Push(&mesh);
    mesh.SetAnimationHandle(handle);

    m_changedMeshes.Insert(handle);
    m_removedMeshes.Erase(handle);
}

void AnimationController::UpdateMesh(Mesh const& mesh)
{
    Require(mesh.GetAnimationHandle() != Handle::INVALID);
    Require(mesh.GetMaterial().IsAnimated());

    m_changedMeshes.Insert(mesh.GetAnimationHandle());
}

void AnimationController::RemoveMesh(Mesh& mesh)
{
    Require(mesh.GetAnimationHandle() != Handle::INVALID);
    Require(mesh.GetMaterial().IsAnimated());

    Handle const handle = mesh.GetAnimationHandle();
    mesh.SetAnimationHandle(Handle::INVALID);

    m_meshes.Pop(handle);

    m_changedMeshes.Erase(handle);
    m_removedMeshes.Insert(handle);
}

void AnimationController::Update(ShaderResources& resources, ComPtr<ID3D12GraphicsCommandList4> const& commandList)
{
    // ReSharper disable once CppTemplateArgumentsCanBeDeduced
    IntegerSet<size_t> const changed(m_changedMeshes);

    resources.RequestListRefresh(m_srcGeometryList, changed);
    resources.RequestListRefresh(m_dstGeometryList, changed);

    if (!m_changedMeshes.IsEmpty() || !m_removedMeshes.IsEmpty())
    {
        UpdateThreadGroupData();
        UploadThreadGroupData(resources, commandList);
    }

    m_changedMeshes.Clear();
    m_removedMeshes.Clear();
}

void AnimationController::Run(ComPtr<ID3D12GraphicsCommandList4> const& commandList)
{
    if (m_threadGroupData.empty()) return;

    CreateBarriers();

    commandList->ResourceBarrier(static_cast<UINT>(m_entryBarriers.size()), m_entryBarriers.data());

    commandList->SetPipelineState(m_pipelineState.Get());
    commandList->Dispatch(static_cast<UINT>(m_threadGroupData.size()), 1, 1);

    commandList->ResourceBarrier(static_cast<UINT>(m_exitBarriers.size()), m_exitBarriers.data());
}

void AnimationController::CreateBLAS(
    ComPtr<ID3D12GraphicsCommandList4> const& commandList,
    std::vector<ID3D12Resource*>*             uavs)
{
    m_meshes.ForEach(
        [&commandList, uavs](Mesh* const& mesh)
    {
        constexpr bool isForAnimation = true;
        mesh->CreateBLAS(commandList, uavs, isForAnimation);
    });
}

void AnimationController::UpdateThreadGroupData()
{
    m_threadGroupData.clear();
    UINT           currentSubmissionIndex = anim::SUBMISSIONS_PER_THREAD_GROUP;
    auto           addSubmission          = [this, &currentSubmissionIndex](
        UINT const meshIndex,
        UINT const instanceIndex,
        UINT const offset,
        UINT const count)
    {
        if (currentSubmissionIndex >= anim::SUBMISSIONS_PER_THREAD_GROUP)
        {
            currentSubmissionIndex = 0;
            m_threadGroupData.emplace_back();
        }

        Require(count > 0);
        Require(count <= anim::MAX_ELEMENTS_PER_SUBMISSION);

        anim::Submission& submission = m_threadGroupData.back().submissions[currentSubmissionIndex++];
        submission.meshIndex         = meshIndex;
        submission.instanceIndex     = instanceIndex;
        submission.offset            = offset;
        submission.count             = count;
    };

    m_meshes.ForEach(
        [&addSubmission](Mesh* const& mesh)
    {
        auto const meshIndex     = static_cast<UINT>(mesh->GetAnimationHandle());
        auto const instanceIndex = static_cast<UINT>(mesh->GetActiveIndex().value());
        UINT const elementCount  = mesh->GetGeometryUnitCount();

        for (UINT offset = 0; offset < elementCount; offset += anim::MAX_ELEMENTS_PER_SUBMISSION)
        {
            UINT const count = std::min(elementCount - offset, anim::MAX_ELEMENTS_PER_SUBMISSION);
            addSubmission(meshIndex, instanceIndex, offset, count);
        }
    });
}

void AnimationController::UploadThreadGroupData(
    ShaderResources const&                    resources,
    ComPtr<ID3D12GraphicsCommandList4> const& commandList)
{
    if (m_threadGroupDataMapping.GetSize() < m_threadGroupData.size())
    {
        auto const sizeInElements = static_cast<UINT>(m_threadGroupData.size());
        auto const sizeInBytes    = static_cast<UINT>(sizeInElements * sizeof(anim::ThreadGroup));

        util::ReAllocateBuffer(
            &m_threadGroupDataBuffer,
            *m_client,
            sizeInBytes,
            D3D12_RESOURCE_FLAG_NONE,
            D3D12_RESOURCE_STATE_COPY_DEST,
            D3D12_HEAP_TYPE_DEFAULT);
        util::ReAllocateBuffer(
            //
            &m_threadGroupDataUploadBuffer,
            *m_client,
            sizeInBytes,
            D3D12_RESOURCE_FLAG_NONE,
            D3D12_RESOURCE_STATE_GENERIC_READ,
            D3D12_HEAP_TYPE_UPLOAD);

        m_threadGroupDataViewDescription.Buffer.NumElements = sizeInElements;
        resources.CreateShaderResourceView(
            m_threadGroupDataEntry,
            0,
            {m_threadGroupDataBuffer, &m_threadGroupDataViewDescription});

        TryDo(m_threadGroupDataUploadBuffer.Map(&m_threadGroupDataMapping, sizeInElements));
    }
    else
    {
        std::vector const barriers = {
            CD3DX12_RESOURCE_BARRIER::Transition(
                m_threadGroupDataBuffer.Get(),
                D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE,
                D3D12_RESOURCE_STATE_COPY_DEST)
        };
        commandList->ResourceBarrier(static_cast<UINT>(barriers.size()), barriers.data());
    }

    m_threadGroupDataMapping.WriteOrClear(m_threadGroupData.data(), m_threadGroupData.size());

    commandList->CopyBufferRegion(
        m_threadGroupDataBuffer.Get(),
        0,
        m_threadGroupDataUploadBuffer.Get(),
        0,
        static_cast<UINT>(m_threadGroupData.size() * sizeof(anim::ThreadGroup)));

    std::vector const barriers = {
        CD3DX12_RESOURCE_BARRIER::Transition(
            m_threadGroupDataBuffer.Get(),
            D3D12_RESOURCE_STATE_COPY_DEST,
            D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE)
    };
    commandList->ResourceBarrier(static_cast<UINT>(barriers.size()), barriers.data());
}

void AnimationController::CreateBarriers()
{
    m_entryBarriers.clear();
    m_entryBarriers.reserve(m_meshes.GetCount());

    m_exitBarriers.clear();
    m_exitBarriers.reserve(m_meshes.GetCount());

    m_meshes.ForEach(
        [this](Mesh* const& mesh)
        {
            m_entryBarriers.emplace_back(
                CD3DX12_RESOURCE_BARRIER::Transition(
                    mesh->GetGeometryBuffer().Get(),
                    D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE,
                    D3D12_RESOURCE_STATE_UNORDERED_ACCESS));

            m_exitBarriers.emplace_back(
                CD3DX12_RESOURCE_BARRIER::Transition(
                    mesh->GetGeometryBuffer().Get(),
                    D3D12_RESOURCE_STATE_UNORDERED_ACCESS,
                    D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE));
        });
}
