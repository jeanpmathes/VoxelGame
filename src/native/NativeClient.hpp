//  <copyright file="NativeClient.hpp" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#pragma once

#include "DXApp.hpp"

#include "Common.hpp"
#include "Space.hpp"

#include "Interfaces/Draw2D.hpp"

struct TextureDescription;
using Microsoft::WRL::ComPtr;

class RasterPipeline;
class Texture;

class NativeClient final : public DXApp
{
public:
    explicit NativeClient(Configuration configuration);

    [[nodiscard]] ComPtr<ID3D12Device5> GetDevice() const;
    [[nodiscard]] ComPtr<D3D12MA::Allocator> GetAllocator() const;

    void OnInit() override;
    void OnPostInit() override;
    void OnUpdate(double delta) override;
    void OnPreRender() override;
    void OnRender(double delta) override;
    void OnDestroy() override;

    void OnSizeChanged(UINT width, UINT height, bool minimized) override;
    void OnWindowMoved(int xPos, int yPos) override;

    void InitRaytracingPipeline(const SpacePipeline& pipeline);

    /**
     * Toggle fullscreen mode.
     */
    void ToggleFullscreen() const;

    /**
     * Load a texture from a file.
     */
    Texture* LoadTexture(std::byte** data, const TextureDescription& description) const;

    /**
     * Set the mouse position in client coordinates.
     */
    void SetMousePosition(POINT position) const;

    /**
     * Get the space that is being rendered.
     */
    Space* GetSpace() const;

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

    using ObjectHandle = size_t;

    ObjectHandle StoreObject(std::unique_ptr<Object> object);
    void DeleteObject(ObjectHandle handle);

    void WaitForGPU();
    void MoveToNextFrame();

    std::wstring GetDRED() const;

private:
    static const float CLEAR_COLOR[4];
    static const float LETTERBOX_COLOR[4];

    struct PostVertex
    {
        DirectX::XMFLOAT4 position;
        DirectX::XMFLOAT2 uv;
    };

    ComPtr<ID3D12Device5> m_device;
    ComPtr<D3D12MA::Allocator> m_allocator;
    ComPtr<IDXGISwapChain3> m_swapChain;
    ComPtr<ID3D12InfoQueue1> m_infoQueue;
    ComPtr<ID3D12CommandQueue> m_commandQueue;

    Resolution m_resolution;

    D3D12MessageFunc m_debugCallback;
    DWORD m_callbackCookie{};

    std::unique_ptr<Uploader> m_uploader = nullptr;
    GappedList<std::unique_ptr<Object>> m_objects = {};

    CD3DX12_VIEWPORT m_spaceViewport;
    CD3DX12_RECT m_spaceScissorRect;

    std::unique_ptr<Space> m_space = nullptr;
    bool m_spaceInitialized = false;

    CD3DX12_VIEWPORT m_postViewport;
    CD3DX12_RECT m_postScissorRect;
    Allocation<ID3D12Resource> m_postVertexBuffer;
    D3D12_VERTEX_BUFFER_VIEW m_postVertexBufferView{};

    CD3DX12_VIEWPORT m_draw2DViewport;
    CD3DX12_RECT m_draw2DScissorRect;

    std::vector<std::unique_ptr<RasterPipeline>> m_rasterPipelines = {};
    RasterPipeline* m_postProcessingPipeline = nullptr;
    std::vector<draw2d::Pipeline> m_draw2DPipelines = {};

    CommandAllocatorGroup m_uploadGroup;
    CommandAllocatorGroup m_2dGroup;

    ComPtr<ID3D12Resource> m_renderTargets[FRAME_COUNT];
    Allocation<ID3D12Resource> m_intermediateRenderTarget;
    bool m_intermediateRenderTargetInitialized = false;
    CD3DX12_CPU_DESCRIPTOR_HANDLE m_intermediateRenderTargetView = {};
    DescriptorHeap m_rtvHeap;

    DescriptorHeap m_dsvHeap;
    Allocation<ID3D12Resource> m_depthStencilBuffer;
    bool m_depthStencilBufferInitialized = false;

    UINT m_frameIndex;
    HANDLE m_fenceEvent{};
    ComPtr<ID3D12Fence> m_fence;
    UINT64 m_fenceValues[FRAME_COUNT];

    bool m_windowVisible;
    bool m_windowedMode;

    void CheckRaytracingSupport() const;
    void PopulateSpaceCommandList() const;
    void PopulatePostProcessingCommandList() const;
    void PopulateDraw2DCommandList(size_t index);

    void LoadDevice();
    void LoadRasterPipeline();
    void CreateDepthBuffer();
    void EnsureValidDepthBuffer(ComPtr<ID3D12GraphicsCommandList4> commandList);
    void SetupSizeDependentResources();
    void SetupSpaceResolutionDependentResources();
    void EnsureValidIntermediateRenderTarget(ComPtr<ID3D12GraphicsCommandList4> commandList);
    void PopulateCommandLists();
    void UpdatePostViewAndScissor();
};