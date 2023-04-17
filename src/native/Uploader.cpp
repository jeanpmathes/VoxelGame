#include "stdafx.h"

Uploader::Uploader(NativeClient& client, const ComPtr<ID3D12GraphicsCommandList> optionalCommandList)
    : m_client(client), m_commandList(optionalCommandList), m_ownsCommandList(optionalCommandList == nullptr)
{
    if (m_ownsCommandList)
    {
        TRY_DO(GetDevice()->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&m_commandAllocator)));
        NAME_D3D12_OBJECT(m_commandAllocator);

        TRY_DO(GetDevice()->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_DIRECT,
            m_commandAllocator.Get(),nullptr, IID_PPV_ARGS(&m_commandList)));
        NAME_D3D12_OBJECT(m_commandList);
    }
}

void Uploader::UploadTexture(
    const std::byte* data, const UINT subresource, const UINT subresourceCount,
    const TextureDescription& description, const ComPtr<ID3D12Resource> destination)
{
    const UINT64 uploadBufferSize = GetRequiredIntermediateSize(destination.Get(), subresource, subresourceCount);

    const ComPtr<ID3D12Resource> uploadBuffer = nv_helpers_dx12::CreateBuffer(
        GetDevice().Get(),
        uploadBufferSize,
        D3D12_RESOURCE_FLAG_NONE,
        D3D12_RESOURCE_STATE_GENERIC_READ,
        nv_helpers_dx12::kUploadHeapProps);

    m_uploadBuffers.push_back(uploadBuffer);

    D3D12_SUBRESOURCE_DATA textureData;
    textureData.pData = data;
    textureData.RowPitch = static_cast<LONG_PTR>(description.width) * 4;
    textureData.SlicePitch = textureData.RowPitch * description.height;

    UpdateSubresources(m_commandList.Get(), destination.Get(), uploadBuffer.Get(), 0, subresource, subresourceCount,
                       &textureData);

    if (m_ownsCommandList) Texture::CreateUsabilityBarrier(m_commandList, destination);
}

void Uploader::UploadBuffer(const std::byte* data, const UINT size, const ComPtr<ID3D12Resource> destination)
{
    const ComPtr<ID3D12Resource> uploadBuffer = nv_helpers_dx12::CreateBuffer(
        GetDevice().Get(),
        size,
        D3D12_RESOURCE_FLAG_NONE,
        D3D12_RESOURCE_STATE_GENERIC_READ,
        nv_helpers_dx12::kUploadHeapProps);

    m_uploadBuffers.push_back(uploadBuffer);

    std::byte* pData;
    TRY_DO(uploadBuffer->Map(0, nullptr, reinterpret_cast<void**>(&pData)));
    memcpy(pData, data, size);
    uploadBuffer->Unmap(0, nullptr);

    auto transition = CD3DX12_RESOURCE_BARRIER::Transition(destination.Get(),
                                                           D3D12_RESOURCE_STATE_COMMON,
                                                           D3D12_RESOURCE_STATE_COPY_DEST);
    m_commandList->ResourceBarrier(1, &transition);

    m_commandList->CopyBufferRegion(destination.Get(), 0, uploadBuffer.Get(), 0, size);

    transition = CD3DX12_RESOURCE_BARRIER::Transition(destination.Get(),
                                                      D3D12_RESOURCE_STATE_COPY_DEST,
                                                      D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER);
    m_commandList->ResourceBarrier(1, &transition);
}

void Uploader::ExecuteUploads(ComPtr<ID3D12CommandQueue> commandQueue) const
{
    TRY_DO(m_commandList->Close());
    ID3D12CommandList* ppCommandLists[] = {m_commandList.Get()};
    commandQueue->ExecuteCommandLists(_countof(ppCommandLists), ppCommandLists);
}

ComPtr<ID3D12Device4> Uploader::GetDevice() const
{
    return m_client.GetDevice();
}

NativeClient& Uploader::GetClient() const
{
    return m_client;
}

bool Uploader::IsUploadingIndividually() const
{
    return m_ownsCommandList;
}
