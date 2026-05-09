// <copyright file="Draw2D.hpp" company="VoxelGame">
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

class Texture;
class RasterPipeline;

namespace draw2d
{
    class Pipeline;

#pragma pack(push, 1)
    struct Vertex
    {
        DirectX::XMFLOAT2 position;
        DirectX::XMFLOAT2 uv;
        DirectX::XMFLOAT4 color;
    };
#pragma pack(pop)

    using InitializeTextures = void(*)(Texture** textures, UINT textureCount, Pipeline* ctx);
    using UploadBuffer       = void(*)(Vertex const* vertices, UINT vertexCount, Pipeline* ctx);
    using DrawBuffer         = void(*)(UINT firstVertex, UINT vertexCount, UINT textureIndex, BOOL useTexture, Pipeline* ctx);

    struct Drawer
    {
        InitializeTextures initializeTextures;
        UploadBuffer       uploadBuffer;
        DrawBuffer         drawBuffer;
        Pipeline*          ctx;
    };

    using Callback = void(*)(Drawer);

    /**
     * \brief A pipeline wrapper for drawing 2D elements.
     */
    class Pipeline final
    {
    public:
        Pipeline(NativeClient& hostClient, RasterPipeline* raster, UINT id, Callback callback);

        /**
         * \brief Populate the command list with all necessary commands to draw the 2D elements.
         * \param commandList The command list to populate.
         */
        void PopulateCommandList(ComPtr<ID3D12GraphicsCommandList4> commandList);

        [[nodiscard]] LPCWSTR GetName() const;

    private:
        static void Initialize(Pipeline* ctx);

        void BindBoolean() const;
        void BindTextures() const;

        void BindVertexBuffer();

        RasterPipeline* raster;
        Callback        callback;
        NativeClient*   client;

        std::wstring name;

        std::vector<Allocation<ID3D12Resource>>                    cbuffers            = {};
        std::vector<ShaderResources::ConstantBufferViewDescriptor> constantBufferViews = {};
        std::vector<ShaderResources::ShaderResourceViewDescriptor> textures            = {};

        Allocation<ID3D12Resource> vertexBuffer = {};
        Allocation<ID3D12Resource> uploadBuffer = {};
        UINT                       vertexCount  = 0;

        D3D12_VERTEX_BUFFER_VIEW vertexBufferView  = {};
        bool                     vertexBufferBound = false;

        UINT                               currentTextureIndex = 0;
        BOOL                               currentUseTexture   = FALSE;
        bool                               initialized         = false;
        ComPtr<ID3D12GraphicsCommandList4> currentCommandList  = nullptr;
    };
}
