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
    std::byte** data, UINT subresources,
    const TextureDescription& description, const Allocation<ID3D12Resource> destination)
{
    const UINT64 uploadBufferSize = GetRequiredIntermediateSize(destination.Get(), 0, subresources);

    const Allocation<ID3D12Resource> textureUploadBuffer = util::AllocateBuffer(
        GetClient(),
        uploadBufferSize,
        D3D12_RESOURCE_FLAG_NONE,
        D3D12_RESOURCE_STATE_GENERIC_READ,
        D3D12_HEAP_TYPE_UPLOAD);
    NAME_D3D12_OBJECT(textureUploadBuffer);

    m_uploadBuffers.push_back(textureUploadBuffer);

    std::vector<D3D12_SUBRESOURCE_DATA> uploadDescription(subresources);
    for (UINT subresource = 0; subresource < subresources; subresource++)
    {
        uploadDescription[subresource].pData = data[subresource];
        uploadDescription[subresource].RowPitch = static_cast<LONG_PTR>(description.width) * 4;
        uploadDescription[subresource].SlicePitch = uploadDescription[subresource].RowPitch * description.height;
    }

    UpdateSubresources(m_commandList.Get(), destination.Get(), textureUploadBuffer.Get(),
                       0, 0, subresources,
                       uploadDescription.data());
    
    if (m_ownsCommandList) Texture::CreateUsabilityBarrier(m_commandList, destination);
}

void Uploader::UploadBuffer(const std::byte* data, const UINT size, const Allocation<ID3D12Resource> destination)
{
    const Allocation<ID3D12Resource> normalUploadBuffer = util::AllocateBuffer(
        GetClient(),
        size,
        D3D12_RESOURCE_FLAG_NONE,
        D3D12_RESOURCE_STATE_GENERIC_READ,
        D3D12_HEAP_TYPE_UPLOAD);
    NAME_D3D12_OBJECT(normalUploadBuffer);

    m_uploadBuffers.push_back(normalUploadBuffer);

    TRY_DO(util::MapAndWrite(normalUploadBuffer, data, size));

    auto transition = CD3DX12_RESOURCE_BARRIER::Transition(destination.Get(),
                                                           D3D12_RESOURCE_STATE_COMMON,
                                                           D3D12_RESOURCE_STATE_COPY_DEST);
    m_commandList->ResourceBarrier(1, &transition);

    m_commandList->CopyBufferRegion(destination.Get(), 0, normalUploadBuffer.Get(), 0, size);

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
