#include "stdafx.h"

Uploader::Uploader(NativeClient& client, ComPtr<ID3D12GraphicsCommandList> const& optionalCommandList)
    : client(&client)
  , commandList(optionalCommandList)
  , ownsCommandList(optionalCommandList == nullptr)
{
    if (ownsCommandList)
    {
        TryDo(GetDevice()->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&commandAllocator)));
        NAME_D3D12_OBJECT(commandAllocator);

        TryDo(GetDevice()->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_DIRECT, commandAllocator.Get(), nullptr, IID_PPV_ARGS(&commandList)));
        NAME_D3D12_OBJECT(commandList);

#if defined(USE_NSIGHT_AFTERMATH)
        client.SetUpCommandListForAftermath(commandList);
#endif
    }
}

void Uploader::UploadTexture(std::byte** data, TextureDescription const& description, Allocation<ID3D12Resource> const& destination)
{
    UINT const   subresources     = description.levels;
    UINT64 const uploadBufferSize = GetRequiredIntermediateSize(destination.Get(), 0, subresources);

    Allocation<ID3D12Resource> const textureUploadBuffer = util::AllocateBuffer(
        GetClient(),
        uploadBufferSize,
        D3D12_RESOURCE_FLAG_NONE,
        D3D12_RESOURCE_STATE_GENERIC_READ,
        D3D12_HEAP_TYPE_UPLOAD);
    NAME_D3D12_OBJECT(textureUploadBuffer);

    uploadBuffers.push_back(textureUploadBuffer);

    std::vector<D3D12_SUBRESOURCE_DATA> uploadDescription(subresources);
    for (UINT layer = 0; layer < 1; layer++)
    {
        UINT width  = description.width;
        UINT height = description.height;

        for (UINT mip = 0; mip < description.levels; mip++)
        {
            UINT const subresource = mip + layer * description.levels;

            uploadDescription[subresource].pData      = data[subresource];
            uploadDescription[subresource].RowPitch   = static_cast<LONG_PTR>(width) * 4;
            uploadDescription[subresource].SlicePitch = uploadDescription[subresource].RowPitch * height;

            width  = std::max(1u, width / 2);
            height = std::max(1u, height / 2);
        }
    }

    UpdateSubresources(commandList.Get(), destination.Get(), textureUploadBuffer.Get(), 0, 0, subresources, uploadDescription.data());

    if (ownsCommandList) Texture::CreateUsabilityBarrier(commandList, destination);
}

void Uploader::UploadBuffer(std::byte const* data, UINT const size, Allocation<ID3D12Resource> const& destination)
{
    Allocation<ID3D12Resource> const normalUploadBuffer = util::AllocateBuffer(
        GetClient(),
        size,
        D3D12_RESOURCE_FLAG_NONE,
        D3D12_RESOURCE_STATE_GENERIC_READ,
        D3D12_HEAP_TYPE_UPLOAD);
    NAME_D3D12_OBJECT(normalUploadBuffer);

    uploadBuffers.push_back(normalUploadBuffer);

    TryDo(util::MapAndWrite(normalUploadBuffer, data, size));

    auto transition = CD3DX12_RESOURCE_BARRIER::Transition(destination.Get(), D3D12_RESOURCE_STATE_COMMON, D3D12_RESOURCE_STATE_COPY_DEST);
    commandList->ResourceBarrier(1, &transition);

    commandList->CopyBufferRegion(destination.Get(), 0, normalUploadBuffer.Get(), 0, size);

    transition = CD3DX12_RESOURCE_BARRIER::Transition(destination.Get(), D3D12_RESOURCE_STATE_COPY_DEST, D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER);
    commandList->ResourceBarrier(1, &transition);
}

void Uploader::ExecuteUploads(ComPtr<ID3D12CommandQueue> const& commandQueue) const
{
    TryDo(commandList->Close());
    std::array<ID3D12CommandList*, 1> const commandLists = {commandList.Get()};
    commandQueue->ExecuteCommandLists(static_cast<UINT>(commandLists.size()), commandLists.data());
}

ComPtr<ID3D12Device4> Uploader::GetDevice() const { return client->GetDevice(); }

NativeClient& Uploader::GetClient() const { return *client; }

bool Uploader::IsUploadingBeforeAnyUse() const { return ownsCommandList; }
