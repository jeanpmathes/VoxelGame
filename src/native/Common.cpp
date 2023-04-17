#include "stdafx.h"

void CommandAllocatorGroup::Initialize(
    const ComPtr<ID3D12Device> device,
    CommandAllocatorGroup* group,
    const D3D12_COMMAND_LIST_TYPE type)
{
    for (UINT n = 0; n < FRAME_COUNT; n++)
    {
        TRY_DO(device->CreateCommandAllocator(type, IID_PPV_ARGS(&group->commandAllocators[n])));
    }

    TRY_DO(device->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_DIRECT,
        group->commandAllocators[0].Get(), nullptr, IID_PPV_ARGS(&group->commandList)));

    group->Close();
}

void CommandAllocatorGroup::Reset(const UINT frameIndex, const ComPtr<ID3D12PipelineState> pipelineState) const
{
    ID3D12PipelineState* pipelineStatePtr = nullptr;
    if (pipelineState != nullptr)
    {
        pipelineStatePtr = pipelineState.Get();
    }

    TRY_DO(commandAllocators[frameIndex]->Reset());
    TRY_DO(commandList->Reset(commandAllocators[frameIndex].Get(), pipelineStatePtr));
}

void CommandAllocatorGroup::Close() const
{
    TRY_DO(commandList->Close());
}
