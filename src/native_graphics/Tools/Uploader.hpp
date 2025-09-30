// <copyright file="Uploader.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

struct TextureDescription;

/**
 * Help uploading data to the GPU.
 */
class Uploader
{
public:
    /**
     * Create a new uploader for a client.
     * Optionally, a command list can be provided instead of creating a new internal one.
     */
    Uploader(NativeClient& client, ComPtr<ID3D12GraphicsCommandList> const& optionalCommandList);

    /**
     * Upload a texture to the GPU.
     */
    void UploadTexture(
        std::byte**                       data,
        TextureDescription const&         description,
        Allocation<ID3D12Resource> const& destination);

    /**
     * Upload a buffer to the GPU.
     */
    void UploadBuffer(std::byte const* data, UINT size, Allocation<ID3D12Resource> const& destination);

    /**
     * Execute the uploads.
     */
    void ExecuteUploads(ComPtr<ID3D12CommandQueue> const& commandQueue) const;

    [[nodiscard]] ComPtr<ID3D12Device4> GetDevice() const;
    [[nodiscard]] NativeClient&         GetClient() const;

    /**
     * Whether the uploader is uploading before any uses, meaning that the command list is only used for uploading.
     */
    [[nodiscard]] bool IsUploadingBeforeAnyUse() const;

private:
    NativeClient* m_client;

    ComPtr<ID3D12CommandAllocator>    m_commandAllocator = {};
    ComPtr<ID3D12GraphicsCommandList> m_commandList      = {};

    std::vector<Allocation<ID3D12Resource>> m_uploadBuffers = {};

    bool m_ownsCommandList;
};
