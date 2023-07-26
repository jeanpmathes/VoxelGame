// <copyright file="Draw2D.hpp" company="VoxelGame">
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
    using UploadBuffer = void(*)(const Vertex* vertices, UINT vertexCount, Pipeline* ctx);
    using DrawBuffer = void(*)(UINT firstVertex, UINT vertexCount, UINT textureIndex, BOOL useTexture,
                               Pipeline* ctx);

    struct Drawer
    {
        InitializeTextures initializeTextures;
        UploadBuffer uploadBuffer;
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
        static void Initialize(Pipeline* ctx);

        RasterPipeline* m_raster;
        Callback m_callback;
        NativeClient& m_client;

        std::vector<Allocation<ID3D12Resource>> m_cbuffers = {};
        std::vector<D3D12_CONSTANT_BUFFER_VIEW_DESC> m_constantBufferViews = {};
        std::vector<std::tuple<Allocation<ID3D12Resource>, D3D12_SHADER_RESOURCE_VIEW_DESC>> m_textures = {};

        Allocation<ID3D12Resource> m_vertexBuffer = {};
        Allocation<ID3D12Resource> m_uploadBuffer = {};

        UINT m_currentTextureIndex = 0;
        BOOL m_currentUseTexture = FALSE;
        bool m_initialized = false;
        ComPtr<ID3D12GraphicsCommandList4> m_currentCommandList = nullptr;
    };
}
