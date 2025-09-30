// <copyright file="Texture.hpp" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

enum class ColorFormat : byte
{
    RGBA,
    BGRA
};

struct TextureDescription
{
    UINT        width  = 1;
    UINT        height = 1;
    UINT        levels = 1;
    ColorFormat format = ColorFormat::BGRA;
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

    /**
     * \brief Create an empty texture.
     * \param client The client to create the texture in.
     * \param description The description of the texture.
     * \return The created texture.
     */
    static Texture* Create(NativeClient& client, TextureDescription description);

    Texture(NativeClient& client, Allocation<ID3D12Resource> const& resource, DirectX::XMUINT3 size, D3D12_SHADER_RESOURCE_VIEW_DESC const& srvDesc);

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
    [[nodiscard]] D3D12_SHADER_RESOURCE_VIEW_DESC const& GetView() const;

    /**
     * Get the size of the texture, in the form of width, height and level count.
     */
    [[nodiscard]] DirectX::XMUINT3 GetSize() const;

    /**
     * Create a transition to the usable state for fresh textures.
     * This is a no-op for usable textures.
     */
    void TransitionToUsable(ComPtr<ID3D12GraphicsCommandList> commandList);

    static void CreateUsabilityBarrier(ComPtr<ID3D12GraphicsCommandList> commandList, Allocation<ID3D12Resource> resource);

private:
    Allocation<ID3D12Resource>      m_resource;
    D3D12_SHADER_RESOURCE_VIEW_DESC m_srvDesc;

    DirectX::XMUINT3 m_size;

    bool                       m_usable = false;
    NativeClient::ObjectHandle m_handle{};
};
