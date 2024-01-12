// <copyright file="Texture.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

enum ColorFormat : byte
{
    RGBA,
    BGRA
};

struct TextureDescription
{
    UINT width;
    UINT height;
    UINT mipLevels;
    ColorFormat format;
};

/**
 * A texture.
 */
class Texture final : public Object
{
    DECLARE_OBJECT_SUBCLASS(Texture)

public:
    /**
     * Create a texture from given data, in RGBA format.
     * The texture is stored in the client that is associated with the uploader.
     */
    static Texture* Create(Uploader& uploader, std::byte** data, TextureDescription description);

    Texture(
        NativeClient& client,
        Allocation<ID3D12Resource> resource,
        DirectX::XMUINT2 size,
        D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc);

    /**
     * Free this texture. This will detach the texture from the client, causing it to be destroyed.
     */
    void Free() const;

    /**
     * Get the resource in which the texture is stored.
     */
    [[nodiscard]] Allocation<ID3D12Resource> GetResource() const;

    /**
     * Get the shader resource view description.
     */
    [[nodiscard]] const D3D12_SHADER_RESOURCE_VIEW_DESC& GetView() const;

    /**
     * Get the size of the texture.
     */
    [[nodiscard]] DirectX::XMUINT2 GetSize() const;

    /**
     * Create a transition to the usable state for fresh textures.
     * This is a no-op for usable textures.
     */
    void TransitionToUsable(ComPtr<ID3D12GraphicsCommandList> commandList);
    
    static void CreateUsabilityBarrier(ComPtr<ID3D12GraphicsCommandList> commandList,
                                       Allocation<ID3D12Resource> resource);

private:
    Allocation<ID3D12Resource> m_resource;
    D3D12_SHADER_RESOURCE_VIEW_DESC m_srvDesc;
    DirectX::XMUINT2 m_size;

    bool m_usable;
    NativeClient::ObjectHandle m_handle{};
};
