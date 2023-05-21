#include "stdafx.h"

std::wstring GetObjectName(const ComPtr<ID3D12Object> object)
{
    UINT nameSizeInByte = 0;
    TRY_DO(object->GetPrivateData(WKPDID_D3DDebugObjectNameW, &nameSizeInByte, nullptr));

    std::wstring name;
    name.resize(nameSizeInByte / sizeof(wchar_t));

    TRY_DO(object->GetPrivateData(WKPDID_D3DDebugObjectNameW, &nameSizeInByte, name.data()));

    return name;
}

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
}

void CommandAllocatorGroup::Reset(const UINT frameIndex, const ComPtr<ID3D12PipelineState> pipelineState) const
{
    ID3D12PipelineState* pipelineStatePtr = nullptr;
    if (pipelineState != nullptr)
    {
        pipelineStatePtr = pipelineState.Get();
    }

#if defined(_DEBUG)
    const std::wstring commandAllocatorName = GetObjectName(commandAllocators[frameIndex]);
    const std::wstring commandListName = GetObjectName(commandList);
#endif

    TRY_DO(commandAllocators[frameIndex]->Reset());
    TRY_DO(commandList->Reset(commandAllocators[frameIndex].Get(), pipelineStatePtr));

#if defined(_DEBUG)
    TRY_DO(commandAllocators[frameIndex]->SetName(commandAllocatorName.c_str()));
    TRY_DO(commandList->SetName(commandListName.c_str()));
#endif
}

void CommandAllocatorGroup::Close() const
{
    TRY_DO(commandList->Close());
}
