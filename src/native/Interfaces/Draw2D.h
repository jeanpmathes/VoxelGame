// <copyright file="Draw2D.h" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
    using DrawBuffer = void(*)(const Vertex* vertices, UINT vertexCount, UINT textureIndex, BOOL useTexture,
                               Pipeline* ctx);

    struct Drawer
    {
        InitializeTextures initializeTextures;
        DrawBuffer drawBuffer;
        Pipeline* ctx;
    };

    using Callback = void(*)(Drawer);

    /**
     * A pipeline wrapper for drawing 2D elements.
     */
    class Pipeline final
    {
    public:
        Pipeline(NativeClient& client, RasterPipeline* raster, Callback callback);

        /**
         * Populate the command list with setup calls.
         */
        void PopulateCommandListSetup(ComPtr<ID3D12GraphicsCommandList4> commandList) const;

        /**
         * Populate the command list with draw calls.
         */
        void PopulateCommandListDrawing(ComPtr<ID3D12GraphicsCommandList4> commandList);

    private:
        RasterPipeline* m_raster;
        Callback m_callback;
        ComPtr<ID3D12Device> m_device;

        std::vector<ComPtr<ID3D12Resource>> m_cbuffers = {};
        std::vector<D3D12_CONSTANT_BUFFER_VIEW_DESC> m_constantBufferViews = {};
        std::vector<std::tuple<ComPtr<ID3D12Resource>, D3D12_SHADER_RESOURCE_VIEW_DESC>> m_textures = {};

        std::vector<ComPtr<ID3D12Resource>> m_vertexBuffers = {};

        UINT m_currentTextureIndex = 0;
        BOOL m_currentUseTexture = FALSE;
        bool m_initialized = false;
        ComPtr<ID3D12GraphicsCommandList4> m_currentCommandList = nullptr;
    };
}
