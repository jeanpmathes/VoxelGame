// <copyright file="Texture.hpp" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
