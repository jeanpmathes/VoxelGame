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
     * \brief A pipeline wrapper for drawing 2D elements.
     */
    class Pipeline final
    {
    public:
        Pipeline(NativeClient& client, RasterPipeline* raster, UINT id, Callback callback);
        
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

        RasterPipeline* m_raster;
        Callback m_callback;
        NativeClient* m_client;

        std::wstring m_name;

        std::vector<Allocation<ID3D12Resource>> m_cbuffers = {};
        std::vector<ShaderResources::ConstantBufferViewDescriptor> m_constantBufferViews = {};
        std::vector<ShaderResources::ShaderResourceViewDescriptor> m_textures = {};

        Allocation<ID3D12Resource> m_vertexBuffer = {};
        Allocation<ID3D12Resource> m_uploadBuffer = {};
        UINT m_vertexCount = 0;

        D3D12_VERTEX_BUFFER_VIEW m_vertexBufferView = {};
        bool m_vertexBufferBound = false;

        UINT m_currentTextureIndex = 0;
        BOOL m_currentUseTexture = FALSE;
        bool m_initialized = false;
        ComPtr<ID3D12GraphicsCommandList4> m_currentCommandList = nullptr;
    };
}
