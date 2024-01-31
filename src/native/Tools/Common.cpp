#include "stdafx.h"

Resolution Resolution::operator*(const float scale) const
{
    Resolution scaled;

    scaled.width = static_cast<UINT>(static_cast<float>(width) * scale);
    scaled.height = static_cast<UINT>(static_cast<float>(height) * scale);

    return scaled;
}

void RasterInfo::Set(ComPtr<ID3D12GraphicsCommandList4> commandList) const
{
    commandList->RSSetViewports(1, &viewport);
    commandList->RSSetScissorRects(1, &scissorRect);
}

bool operator==(const Resolution& lhs, const Resolution& rhs)
{
    return lhs.width == rhs.width && lhs.height == rhs.height;
}

bool operator!=(const Resolution& lhs, const Resolution& rhs)
{
    return !(lhs == rhs);
}

std::wstring GetObjectName(const ComPtr<ID3D12Object> object)
{
    UINT nameSizeInByte = 0;
    HRESULT ok = object->GetPrivateData(WKPDID_D3DDebugObjectNameW, &nameSizeInByte, nullptr);

    if (SUCCEEDED(ok))
    {
        std::wstring name;
        name.resize(nameSizeInByte / sizeof(wchar_t));
        ok = object->GetPrivateData(WKPDID_D3DDebugObjectNameW, &nameSizeInByte, name.data());

        if (SUCCEEDED(ok))
        {
            return name;
        }
    }

    return L"";
}

void SetObjectName(ComPtr<ID3D12Object> object, const std::wstring& name)
{
    TRY_DO(object->SetName(name.c_str()));
}

void CommandAllocatorGroup::Initialize(
    ComPtr<ID3D12Device> device,
    CommandAllocatorGroup* group,
    const D3D12_COMMAND_LIST_TYPE type)
{
    for (UINT n = 0; n < FRAME_COUNT; n++)
    {
        TRY_DO(device->CreateCommandAllocator(type, IID_PPV_ARGS(&group->commandAllocators[n])));
    }

    TRY_DO(device->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_DIRECT,
        group->commandAllocators[0].Get(), nullptr, IID_PPV_ARGS(&group->commandList)));

#if defined(USE_NSIGHT_AFTERMATH)
    DXApp::SetupCommandListForAftermath(group->commandList);
#endif

    TRY_DO(group->commandList->Close());
}

void CommandAllocatorGroup::Reset(const UINT frameIndex, const ComPtr<ID3D12PipelineState> pipelineState)
{
    ID3D12PipelineState* pipelineStatePtr = nullptr;
    if (pipelineState != nullptr)
    {
        pipelineStatePtr = pipelineState.Get();
    }

#if defined(NATIVE_DEBUG)
    const std::wstring commandAllocatorName = GetObjectName(commandAllocators[frameIndex]);
    const std::wstring commandListName = GetObjectName(commandList);
#endif

    TRY_DO(commandAllocators[frameIndex]->Reset());
    TRY_DO(commandList->Reset(commandAllocators[frameIndex].Get(), pipelineStatePtr));

#if defined(NATIVE_DEBUG)
    SetObjectName(commandAllocators[frameIndex], commandAllocatorName);
    SetObjectName(commandList, commandListName);
#endif

    m_open = true;
}

void CommandAllocatorGroup::Close()
{
    REQUIRE(m_open);
    m_open = false;
    
    TRY_DO(commandList->Close());
}
