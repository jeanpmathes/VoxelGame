#include "stdafx.h"

Resolution Resolution::operator*(float const scale) const
{
    Resolution scaled;

    scaled.width  = static_cast<UINT>(static_cast<float>(width) * scale);
    scaled.height = static_cast<UINT>(static_cast<float>(height) * scale);

    return scaled;
}

void RasterInfo::Set(ComPtr<ID3D12GraphicsCommandList4> commandList) const
{
    commandList->RSSetViewports(1, &viewport);
    commandList->RSSetScissorRects(1, &scissorRect);
}

std::wstring GetObjectName(ComPtr<ID3D12Object> const object)
{
    UINT    nameSizeInByte = 0;
    HRESULT ok             = object->GetPrivateData(WKPDID_D3DDebugObjectNameW, &nameSizeInByte, nullptr);

    if (SUCCEEDED(ok))
    {
        std::wstring name;
        name.resize(nameSizeInByte / sizeof(wchar_t));
        ok = object->GetPrivateData(WKPDID_D3DDebugObjectNameW, &nameSizeInByte, name.data());

        if (SUCCEEDED(ok)) return name;
    }

    return L"";
}

void SetObjectName(ComPtr<ID3D12Object> object, std::wstring const& name) { TryDo(object->SetName(name.c_str())); }

void CommandAllocatorGroup::Initialize(
    NativeClient const&           client,
    CommandAllocatorGroup*        group,
    D3D12_COMMAND_LIST_TYPE const type)
{
    for (UINT n = 0; n < FRAME_COUNT; n++)
        TryDo(client.GetDevice()->CreateCommandAllocator(type, IID_PPV_ARGS(&group->commandAllocators[n])));

    TryDo(
        client.GetDevice()->CreateCommandList(
            0,
            D3D12_COMMAND_LIST_TYPE_DIRECT,
            group->commandAllocators[0].Get(),
            nullptr,
            IID_PPV_ARGS(&group->commandList)));

#if defined(USE_NSIGHT_AFTERMATH)
    client.SetupCommandListForAftermath(group->commandList);
#endif

    TryDo(group->commandList->Close());
}

void CommandAllocatorGroup::Reset(UINT const frameIndex, ComPtr<ID3D12PipelineState> const pipelineState)
{
    ID3D12PipelineState* pipelineStatePtr = nullptr;
    if (pipelineState != nullptr) pipelineStatePtr = pipelineState.Get();

#if defined(NATIVE_DEBUG)
    std::wstring const commandAllocatorName = GetObjectName(commandAllocators[frameIndex]);
    std::wstring const commandListName      = GetObjectName(commandList);
#endif

    TryDo(commandAllocators[frameIndex]->Reset());
    TryDo(commandList->Reset(commandAllocators[frameIndex].Get(), pipelineStatePtr));

#if defined(NATIVE_DEBUG)
    SetObjectName(commandAllocators[frameIndex], commandAllocatorName);
    SetObjectName(commandList, commandListName);
#endif

    open = true;
}

void CommandAllocatorGroup::Close()
{
    Require(open);
    open = false;

    TryDo(commandList->Close());
}
