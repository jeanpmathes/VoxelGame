// <copyright file="Texture.h" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

struct TextureDescription
{
    UINT width;
    UINT height;
};

/**
 * A texture.
 */
class Texture final : public Object
{
    DECLARE_OBJECT_SUBCLASS(Texture)

public:
    static std::unique_ptr<Texture> Create(Uploader& uploader, std::byte* data, TextureDescription description);

    explicit Texture(NativeClient& client, ComPtr<ID3D12Resource> resource, D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc);

    /**
     * Get the resource in which the texture is stored.
     */
    [[nodiscard]] ComPtr<ID3D12Resource> GetResource() const;

    /**
     * Get the shader resource view description.
     */
    [[nodiscard]] D3D12_SHADER_RESOURCE_VIEW_DESC GetView() const;

private:
    ComPtr<ID3D12Resource> m_resource;
    D3D12_SHADER_RESOURCE_VIEW_DESC m_srvDesc;
};
