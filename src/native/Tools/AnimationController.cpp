#include "stdafx.h"

AnimationController::AnimationController(const ComPtr<IDxcBlob>& shader, const UINT space)
    : m_threadGroupDataLocation({.reg = 0, .space = space})
      , m_inputGeometryListLocation({.reg = 1, .space = space}) // SRV
      , m_outputGeometryListLocation({.reg = 0, .space = space}) // UAV
{
    TRY_DO(shader->QueryInterface<ID3DBlob>(&m_shader));

    m_threadGroupDataViewDescription.Format = DXGI_FORMAT_UNKNOWN;
    m_threadGroupDataViewDescription.ViewDimension = D3D12_SRV_DIMENSION_BUFFER;
    m_threadGroupDataViewDescription.Shader4ComponentMapping = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
    m_threadGroupDataViewDescription.Buffer.FirstElement = 0;
    m_threadGroupDataViewDescription.Buffer.NumElements = 0;
    m_threadGroupDataViewDescription.Buffer.StructureByteStride = sizeof(anim::ThreadGroup);
    m_threadGroupDataViewDescription.Buffer.Flags = D3D12_BUFFER_SRV_FLAG_NONE;
}

void AnimationController::SetupResourceLayout(ShaderResources::Description* description)
{
    const std::function<UINT(MeshObject* const&)> getIndexOfMesh = [this](auto* mesh)
    {
        REQUIRE(mesh != nullptr);
        REQUIRE(mesh->GetAnimationHandle() != Handle::INVALID);

        return static_cast<UINT>(mesh->GetAnimationHandle());
    };

    m_resourceTable = description->AddHeapDescriptorTable([this](auto& table)
    {
        m_threadGroupDataEntry = table.AddShaderResourceView(m_threadGroupDataLocation);
    });

    m_srcGeometryList = description->AddShaderResourceViewDescriptorList(m_inputGeometryListLocation,
                                                                         CreateSizeGetter(&m_meshes),
                                                                         [this](const UINT index)
                                                                         {
                                                                             return m_meshes[index]->
                                                                                 GetAnimationSourceBufferViewDescriptor();
                                                                         },
                                                                         CreateListBuilder(&m_meshes, getIndexOfMesh));

    m_dstGeometryList = description->AddUnorderedAccessViewDescriptorList(m_outputGeometryListLocation,
                                                                          CreateSizeGetter(&m_meshes),
                                                                          [this](const UINT index)
                                                                          {
                                                                              return m_meshes[index]->
                                                                                  GetAnimationDestinationBufferViewDescriptor();
                                                                          },
                                                                          CreateListBuilder(&m_meshes, getIndexOfMesh));
}

void AnimationController::Initialize(NativeClient& client, const ComPtr<ID3D12RootSignature>& rootSignature)
{
    m_client = &client;

    D3D12_COMPUTE_PIPELINE_STATE_DESC pipelineStateDescription = {};
    pipelineStateDescription.pRootSignature = rootSignature.Get();
    pipelineStateDescription.CS = CD3DX12_SHADER_BYTECODE(m_shader.Get());

    TRY_DO(m_client->GetDevice()->CreateComputePipelineState(&pipelineStateDescription, IID_PPV_ARGS(&m_pipelineState)))
    ;
}

void AnimationController::AddMesh(MeshObject& mesh)
{
    REQUIRE(mesh.GetMaterial().IsAnimated());
    REQUIRE(mesh.GetAnimationHandle() == Handle::INVALID);

    size_t index = m_meshes.Push(&mesh);
    mesh.SetAnimationHandle(static_cast<Handle>(index));

    m_changedMeshes.insert(index);
    m_removedMeshes.erase(index);
}

void AnimationController::UpdateMesh(const MeshObject& mesh)
{
    REQUIRE(mesh.GetAnimationHandle() != Handle::INVALID);
    REQUIRE(mesh.GetMaterial().IsAnimated());

    m_changedMeshes.insert(static_cast<size_t>(mesh.GetAnimationHandle()));
}

void AnimationController::RemoveMesh(MeshObject& mesh)
{
    REQUIRE(mesh.GetAnimationHandle() != Handle::INVALID);
    REQUIRE(mesh.GetMaterial().IsAnimated());

    const auto index = static_cast<size_t>(mesh.GetAnimationHandle());
    m_meshes.Pop(index);

    mesh.SetAnimationHandle(Handle::INVALID);

    m_changedMeshes.erase(index);
    m_removedMeshes.insert(index);
}

void AnimationController::Update(ShaderResources& resources, ComPtr<ID3D12GraphicsCommandList4> commandList)
{
    resources.RequestListRefresh(m_srcGeometryList, m_changedMeshes);
    resources.RequestListRefresh(m_dstGeometryList, m_changedMeshes);

    if (!m_changedMeshes.empty() || !m_removedMeshes.empty())
    {
        UpdateThreadGroupData();
        UploadThreadGroupData(resources, commandList);
    }

    m_changedMeshes.clear();
    m_removedMeshes.clear();
}

void AnimationController::Run(ComPtr<ID3D12GraphicsCommandList4> commandList)
{
    if (m_threadGroupData.empty()) return;

    std::vector<CD3DX12_RESOURCE_BARRIER> barriers;
    barriers.reserve(m_meshes.GetCount());

    barriers.clear();
    for (const auto& mesh : m_meshes)
    {
        barriers.emplace_back(CD3DX12_RESOURCE_BARRIER::Transition(mesh->GetGeometryBuffer().Get(),
                                                                   D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE,
                                                                   D3D12_RESOURCE_STATE_UNORDERED_ACCESS));
    }
    commandList->ResourceBarrier(static_cast<UINT>(barriers.size()), barriers.data());

    commandList->SetPipelineState(m_pipelineState.Get());
    commandList->Dispatch(static_cast<UINT>(m_threadGroupData.size()), 1, 1);

    barriers.clear();
    for (const auto& mesh : m_meshes)
    {
        barriers.emplace_back(CD3DX12_RESOURCE_BARRIER::Transition(mesh->GetGeometryBuffer().Get(),
                                                                   D3D12_RESOURCE_STATE_UNORDERED_ACCESS,
                                                                   D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE));
    }
    commandList->ResourceBarrier(static_cast<UINT>(barriers.size()), barriers.data());
}

void AnimationController::CreateBLAS(ComPtr<ID3D12GraphicsCommandList4> commandList, std::vector<ID3D12Resource*>* uavs)
{
    for (const auto& mesh : m_meshes)
    {
        constexpr bool isForAnimation = true;
        mesh->CreateBLAS(commandList, uavs, isForAnimation);
    }
}

void AnimationController::UpdateThreadGroupData()
{
    m_threadGroupData.clear();
    UINT currentSubmissionIndex = anim::SUBMISSIONS_PER_THREAD_GROUP;
    auto addSubmission = [this, &currentSubmissionIndex](const UINT meshIndex, const UINT instanceIndex,
                                                         const UINT offset, const UINT count)
    {
        if (currentSubmissionIndex >= anim::SUBMISSIONS_PER_THREAD_GROUP)
        {
            currentSubmissionIndex = 0;
            m_threadGroupData.emplace_back();
        }

        REQUIRE(count > 0);
        REQUIRE(count <= anim::MAX_ELEMENTS_PER_SUBMISSION);

        anim::Submission& submission = m_threadGroupData.back().submissions[currentSubmissionIndex++];
        submission.meshIndex = meshIndex;
        submission.instanceIndex = instanceIndex;
        submission.offset = offset;
        submission.count = count;
    };

    for (const auto& mesh : m_meshes)
    {
        const auto meshIndex = static_cast<UINT>(mesh->GetAnimationHandle());
        const auto instanceIndex = static_cast<UINT>(mesh->GetActiveIndex().value());
        const UINT elementCount = mesh->GetGeometryUnitCount();

        for (UINT offset = 0; offset < elementCount; offset += anim::MAX_ELEMENTS_PER_SUBMISSION)
        {
            const UINT count = std::min(elementCount - offset, anim::MAX_ELEMENTS_PER_SUBMISSION);
            addSubmission(meshIndex, instanceIndex, offset, count);
        }
    }
}

void AnimationController::UploadThreadGroupData(ShaderResources& resources,
                                                ComPtr<ID3D12GraphicsCommandList4> commandList)
{
    if (m_threadGroupDataMapping.GetSize() < m_threadGroupData.size())
    {
        const UINT sizeInElements = static_cast<UINT>(m_threadGroupData.size());
        const UINT sizeInBytes = static_cast<UINT>(sizeInElements * sizeof(anim::ThreadGroup));

        m_threadGroupDataBuffer = util::AllocateBuffer(*m_client, sizeInBytes,
                                                       D3D12_RESOURCE_FLAG_NONE,
                                                       D3D12_RESOURCE_STATE_COPY_DEST,
                                                       D3D12_HEAP_TYPE_DEFAULT);
        m_threadGroupDataUploadBuffer = util::AllocateBuffer(*m_client, sizeInBytes,
                                                             D3D12_RESOURCE_FLAG_NONE,
                                                             D3D12_RESOURCE_STATE_GENERIC_READ,
                                                             D3D12_HEAP_TYPE_UPLOAD);

        m_threadGroupDataViewDescription.Buffer.NumElements = sizeInElements;
        resources.CreateShaderResourceView(m_threadGroupDataEntry, 0, {
                                               m_threadGroupDataBuffer, &m_threadGroupDataViewDescription
                                           });

        TRY_DO(m_threadGroupDataUploadBuffer.Map(&m_threadGroupDataMapping, sizeInElements));
    }
    else
    {
        const std::vector barriers =
        {
            CD3DX12_RESOURCE_BARRIER::Transition(m_threadGroupDataBuffer.Get(),
                                                 D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE,
                                                 D3D12_RESOURCE_STATE_COPY_DEST)
        };
        commandList->ResourceBarrier(static_cast<UINT>(barriers.size()), barriers.data());
    }

    m_threadGroupDataMapping.WriteOrClear(m_threadGroupData.data(), m_threadGroupData.size());

    commandList->CopyBufferRegion(
        m_threadGroupDataBuffer.Get(), 0,
        m_threadGroupDataUploadBuffer.Get(), 0,
        static_cast<UINT>(m_threadGroupData.size() * sizeof(anim::ThreadGroup)));

    {
        const std::vector barriers =
        {
            CD3DX12_RESOURCE_BARRIER::Transition(m_threadGroupDataBuffer.Get(), D3D12_RESOURCE_STATE_COPY_DEST,
                                                 D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE)
        };
        commandList->ResourceBarrier(static_cast<UINT>(barriers.size()), barriers.data());
    }
}
