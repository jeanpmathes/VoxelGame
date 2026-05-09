#include "stdafx.h"

AnimationController::AnimationController(ComPtr<IDxcBlob> const& shaderBlob, UINT const space)
    : threadGroupDataLocation({.reg = 0, .space = space})
  , inputGeometryListLocation({.reg = 0, .space = space})  // SRV
  , outputGeometryListLocation({.reg = 0, .space = space}) // UAV
{
    TryDo(shaderBlob->QueryInterface<ID3DBlob>(&shader));
}

void AnimationController::SetUpResourceLayout(ShaderResources::Description* description)
{
    std::function<UINT(Mesh* const&)> const getIndexOfMesh = [this](auto* mesh)
    {
        Require(mesh != nullptr);
        Require(mesh->GetAnimationHandle() != Handle::INVALID);

        return static_cast<UINT>(mesh->GetAnimationHandle());
    };

    workIndexConstant = description->AddRootConstant([this] { return this->workIndex; }, {.reg = 0, .space = 1});
    workSizeConstant  = description->AddRootConstant([this] { return this->workSize; }, {.reg = 1, .space = 1});

    auto getSourceDescriptor = [this](UINT const index)
    {
        return meshes[static_cast<Handle>(index)]->GetAnimationSourceBufferViewDescriptor();
    };

    auto getDestinationDescriptor = [this](UINT const index)
    {
        return meshes[static_cast<Handle>(index)]->GetAnimationDestinationBufferViewDescriptor();
    };

    srcGeometryList = description->AddShaderResourceViewDescriptorList(
        inputGeometryListLocation,
        CreateSizeGetter(&meshes),
        getSourceDescriptor,
        CreateBagBuilder(&meshes, getIndexOfMesh));

    dstGeometryList = description->AddUnorderedAccessViewDescriptorList(
        outputGeometryListLocation,
        CreateSizeGetter(&meshes),
        getDestinationDescriptor,
        CreateBagBuilder(&meshes, getIndexOfMesh));
}

void AnimationController::Initialize(NativeClient& usedClient, ComPtr<ID3D12RootSignature> const& rootSignature)
{
    client = &usedClient;

    D3D12_COMPUTE_PIPELINE_STATE_DESC pipelineStateDescription = {};
    pipelineStateDescription.pRootSignature                    = rootSignature.Get();
    pipelineStateDescription.CS                                = CD3DX12_SHADER_BYTECODE(shader.Get());

    TryDo(client->GetDevice()->CreateComputePipelineState(&pipelineStateDescription, IID_PPV_ARGS(&pipelineState)));
}

void AnimationController::AddMesh(Mesh& mesh)
{
    Require(mesh.GetMaterial().IsAnimated());
    Require(mesh.GetAnimationHandle() == Handle::INVALID);

    Handle const handle = meshes.Push(&mesh);
    mesh.SetAnimationHandle(handle);

    changedMeshes.Insert(handle);
    removedMeshes.Erase(handle);
}

void AnimationController::UpdateMesh(Mesh const& mesh)
{
    Require(mesh.GetAnimationHandle() != Handle::INVALID);
    Require(mesh.GetMaterial().IsAnimated());

    changedMeshes.Insert(mesh.GetAnimationHandle());
}

void AnimationController::RemoveMesh(Mesh& mesh)
{
    Require(mesh.GetAnimationHandle() != Handle::INVALID);
    Require(mesh.GetMaterial().IsAnimated());

    Handle const handle = mesh.GetAnimationHandle();
    mesh.SetAnimationHandle(Handle::INVALID);

    meshes.Pop(handle);

    changedMeshes.Erase(handle);
    removedMeshes.Insert(handle);
}

void AnimationController::Update(ShaderResources& resources)
{
    // ReSharper disable once CppTemplateArgumentsCanBeDeduced
    IntegerSet<size_t> const changed(changedMeshes);

    resources.RequestListRefresh(srcGeometryList, changed);
    resources.RequestListRefresh(dstGeometryList, changed);

    changedMeshes.Clear();
    removedMeshes.Clear();
}

void AnimationController::Run(ShaderResources const& resources, ComPtr<ID3D12GraphicsCommandList4> const& commandList)
{
    if (meshes.IsEmpty()) return;

    CreateBarriers();

    commandList->SetPipelineState(pipelineState.Get());

    commandList->ResourceBarrier(static_cast<UINT>(entryBarriers.size()), entryBarriers.data());

    constexpr UINT threadGroupSize = 32;

    meshes.ForEach(
        [this, &resources, &commandList](Mesh* const& mesh)
        {
            UINT const meshSize         = mesh->GetGeometryUnitCount();
            UINT const threadGroupCount = (meshSize + threadGroupSize - 1) / threadGroupSize;

            workIndex.uInteger = static_cast<UINT>(mesh->GetAnimationHandle());
            resources.UpdateConstant(workIndexConstant, commandList);

            workSize.uInteger = meshSize;
            resources.UpdateConstant(workSizeConstant, commandList);

            commandList->Dispatch(threadGroupCount, 1, 1);
        });

    commandList->ResourceBarrier(static_cast<UINT>(exitBarriers.size()), exitBarriers.data());
}

void AnimationController::CreateBLAS(ComPtr<ID3D12GraphicsCommandList4> const& commandList, std::vector<ID3D12Resource*>* uavs)
{
    PIXScopedEvent(commandList.Get(), PIX_COLOR_DEFAULT, L"Animation BLAS Update");

    meshes.ForEach(
        [&commandList, uavs](Mesh* const& mesh)
        {
            constexpr bool isForAnimation = true;
            mesh->CreateBLAS(commandList, uavs, isForAnimation);
        });
}

void AnimationController::CreateBarriers()
{
    entryBarriers.clear();
    entryBarriers.reserve(meshes.GetCount());

    exitBarriers.clear();
    exitBarriers.reserve(meshes.GetCount());

    meshes.ForEach(
        [this](Mesh* const& mesh)
        {
            entryBarriers.emplace_back(
                CD3DX12_RESOURCE_BARRIER::Transition(mesh->GetGeometryBuffer().Get(), D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE, D3D12_RESOURCE_STATE_UNORDERED_ACCESS));

            exitBarriers.emplace_back(
                CD3DX12_RESOURCE_BARRIER::Transition(mesh->GetGeometryBuffer().Get(), D3D12_RESOURCE_STATE_UNORDERED_ACCESS, D3D12_RESOURCE_STATE_NON_PIXEL_SHADER_RESOURCE));
        });
}
