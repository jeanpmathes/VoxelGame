//  <copyright file="NativeClient.h" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#pragma once

#include "DXApp.h"

#include "Common.h"
#include "Space.h"

#include "Interfaces/Draw2D.h"

struct TextureDescription;
using Microsoft::WRL::ComPtr;

class RasterPipeline;
class Texture;

class NativeClient final : public DXApp
{
public:
    NativeClient(UINT width, UINT height, std::wstring name, Configuration configuration);

    [[nodiscard]] ComPtr<ID3D12Device5> GetDevice() const;
    [[nodiscard]] UINT GetRtvHeapIncrement() const;
    [[nodiscard]] UINT GetCbvSrvUavHeapIncrement() const;

    void OnInit() override;
    void OnPostInit() override;
    void OnUpdate(double delta) override;
    void OnRender(double delta) override;
    void OnDestroy() override;

    void OnSizeChanged(UINT width, UINT height, bool minimized) override;
    void OnWindowMoved(int xPos, int yPos) override;

    /**
     * Set the resolution of the space viewport. This has no effect on the window size.
     */
    void SetResolution(UINT32 width, UINT32 height);
    /**
     * Toggle fullscreen mode.
     */
    void ToggleFullscreen() const;

    /**
     * Load a texture from a file.
     */
    Texture* LoadTexture(std::byte* data, const TextureDescription& description);
    
    /**
     * Set the mouse position in client coordinates.
     */
    void SetMousePosition(POINT position) const;

    /**
     * Get the space that is being rendered.
     */
    Space* GetSpace();

    /**
     * Add a raster pipeline to the client.
     */
    void AddRasterPipeline(std::unique_ptr<RasterPipeline> pipeline);

    /**
     * Set the pipeline that will be used for post processing.
     */
    void SetPostProcessingPipeline(RasterPipeline* pipeline);

    /**
     * Add a draw 2D pipeline to the client.
     * The associated callback will be called every frame, after the post processing pipeline.
     */
    void AddDraw2DPipeline(RasterPipeline* pipeline, draw2d::Callback callback);

    void WaitForGPU();
    void MoveToNextFrame();

   private:
    static const float CLEAR_COLOR[4];
    static const float LETTERBOX_COLOR[4];
    
    struct PostVertex
    {
        DirectX::XMFLOAT4 position;
        DirectX::XMFLOAT2 uv;
    };

    Resolution m_resolution;

    D3D12MessageFunc m_debugCallback;
    DWORD m_callbackCookie{};

    std::unique_ptr<Uploader> m_uploader = nullptr;
    std::vector<std::unique_ptr<Texture>> m_textures = {};

    CD3DX12_VIEWPORT m_spaceViewport;
    CD3DX12_RECT m_spaceScissorRect;

    Space m_space;
    bool m_spaceEnabled;

    std::vector<std::unique_ptr<RasterPipeline>> m_rasterPipelines = {};
    RasterPipeline* m_postProcessingPipeline = nullptr;
    std::vector<draw2d::Pipeline> m_draw2DPipelines = {};

    CD3DX12_VIEWPORT m_postViewport;
    CD3DX12_RECT m_postScissorRect;
    ComPtr<ID3D12Resource> m_postVertexBuffer;
    D3D12_VERTEX_BUFFER_VIEW m_postVertexBufferView{};

    ComPtr<IDXGISwapChain3> m_swapChain;
    ComPtr<ID3D12Device5> m_device;
    ComPtr<ID3D12InfoQueue1> m_infoQueue;
    ComPtr<ID3D12CommandQueue> m_commandQueue;

    ComPtr<ID3D12Resource> m_renderTargets[FRAME_COUNT];
    ComPtr<ID3D12Resource> m_intermediateRenderTarget;
    ComPtr<ID3D12DescriptorHeap> m_rtvHeap;
 
    UINT m_rtvDescriptorSize;
    UINT m_srvDescriptorSize;

    ComPtr<ID3D12DescriptorHeap> m_dsvHeap;
    ComPtr<ID3D12Resource> m_depthStencilBuffer;

    UINT m_frameIndex;
    HANDLE m_fenceEvent{};
    ComPtr<ID3D12Fence> m_fence;
    UINT64 m_fenceValues[FRAME_COUNT];

    bool m_windowVisible;
    bool m_windowedMode;

    void CheckRaytracingSupport() const;
    void PopulateSpaceCommandList();
    void PopulatePostProcessingCommandList() const;
    void PopulateDraw2DCommandList(size_t index);

    void LoadDevice();
    void LoadRasterPipeline();
    void LoadRaytracingPipeline();
    void CreateDepthBuffer();
    void SetupSizeDependentResources();
    void SetupSpaceResolutionDependentResources();
    void PopulateCommandLists();
    void UpdatePostViewAndScissor();
};
