#include "stdafx.h"

AnimationController::AnimationController(ComPtr<IDxcBlob> const& shader, UINT const space)
    : m_threadGroupDataLocation({.reg = 0, .space = space})
  , m_inputGeometryListLocation({.reg = 0, .space = space})  // SRV
  , m_outputGeometryListLocation({.reg = 0, .space = space}) // UAV
{
    TryDo(shader->QueryInterface<ID3DBlob>(&m_shader));
}

void AnimationController::SetUpResourceLayout(ShaderResources::Description* description)
{
    std::function<UINT(Mesh* const&)> const getIndexOfMesh = [this](auto* mesh)
    {
        Require(mesh != nullptr);
        Require(mesh->GetAnimationHandle() != Handle::INVALID);

        return static_cast<UINT>(mesh->GetAnimationHandle());
    };

    m_workIndexConstant = description->AddRootConstant([this] { return this->m_workIndex; }, {.reg = 0, .space = 1});
    m_workSizeConstant  = description->AddRootConstant([this] { return this->m_workSize; }, {.reg = 1, .space = 1});

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

void AnimationController::Update(ShaderResources& resources)
{
    // ReSharper disable once CppTemplateArgumentsCanBeDeduced
    IntegerSet<size_t> const changed(m_changedMeshes);

    resources.RequestListRefresh(m_srcGeometryList, changed);
    resources.RequestListRefresh(m_dstGeometryList, changed);

    m_changedMeshes.Clear();
    m_removedMeshes.Clear();
}

void AnimationController::Run(ShaderResources const& resources, ComPtr<ID3D12GraphicsCommandList4> const& commandList)
{
    if (m_meshes.IsEmpty()) return;

    CreateBarriers();

    commandList->SetPipelineState(m_pipelineState.Get());

    commandList->ResourceBarrier(static_cast<UINT>(m_entryBarriers.size()), m_entryBarriers.data());

    constexpr UINT threadGroupSize = 32;

    m_meshes.ForEach(
        [this, &resources, &commandList](Mesh* const& mesh)
        {
            UINT const meshSize         = mesh->GetGeometryUnitCount();
            UINT const threadGroupCount = (meshSize + threadGroupSize - 1) / threadGroupSize;

            m_workIndex.uInteger = static_cast<UINT>(mesh->GetAnimationHandle());
            resources.UpdateConstant(m_workIndexConstant, commandList);

            m_workSize.uInteger = meshSize;
            resources.UpdateConstant(m_workSizeConstant, commandList);

            commandList->Dispatch(threadGroupCount, 1, 1);
        });

    commandList->ResourceBarrier(static_cast<UINT>(m_exitBarriers.size()), m_exitBarriers.data());
}

void AnimationController::CreateBLAS(
    ComPtr<ID3D12GraphicsCommandList4> const& commandList,
    std::vector<ID3D12Resource*>*             uavs)
{
    PIXScopedEvent(commandList.Get(), PIX_COLOR_DEFAULT, L"Animation BLAS Update");

    m_meshes.ForEach(
        [&commandList, uavs](Mesh* const& mesh)
        {
            constexpr bool isForAnimation = true;
            mesh->CreateBLAS(commandList, uavs, isForAnimation);
        });
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
