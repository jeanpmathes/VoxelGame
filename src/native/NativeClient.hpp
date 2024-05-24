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

#if defined(USE_NSIGHT_AFTERMATH)
#include "nv_aftermath/NsightAftermathGpuCrashTracker.hpp"
#endif

struct TextureDescription;
using Microsoft::WRL::ComPtr;

class RasterPipeline;
class Texture;

using ScreenshotFunc = void(*)(std::byte*, UINT, UINT);

class NativeClient final : public DXApp
{
public:
    explicit NativeClient(Configuration const& configuration);
    
    [[nodiscard]] ComPtr<ID3D12Device5>      GetDevice() const;
    [[nodiscard]] ComPtr<D3D12MA::Allocator> GetAllocator() const;

    void OnInit() override;
    void OnPostInit() override;
    void OnUpdate(double delta) override;
    void OnPreRender() override;
    void OnRender(double delta) override;
    void OnDestroy() override;

    void OnSizeChanged(UINT width, UINT height, bool minimized) override;
    void OnWindowMoved(int xPos, int yPos) override;

    void InitRaytracingPipeline(SpacePipelineDescription const& pipeline);

    /**
     * Toggle fullscreen mode.
     */
    void ToggleFullscreen() const;

    /**
     * \brief Take a screenshot of the next frame.
     * \param func The function that will be called when the screenshot is ready.
     */
    void TakeScreenshot(ScreenshotFunc func);

    /**
     * Load a texture from a file.
     */
    Texture* LoadTexture(std::byte** data, TextureDescription const& description) const;

    /**
     * Get the space that is being rendered.
     */
    [[nodiscard]] Space* GetSpace() const;

    /**
     * Add a raster pipeline to the client.
     */
    void AddRasterPipeline(std::unique_ptr<RasterPipeline> pipeline);

    /**
     * Set the pipeline that will be used for post processing.
     */
    void SetPostProcessingPipeline(RasterPipeline* pipeline);

    /**
     * \brief Add a draw2d pipeline to the client.
     * \param pipeline The pipeline to add. Must use the DRAW_2D preset.
     * \param priority The priority of the pipeline. Higher priorities are drawn later, and thus on top of lower priorities.
     * \param callback The associated callback will be called every frame, after the post processing pipeline.
     * \return The ID of the pipeline. Can be used to remove it later.
     */
    UINT AddDraw2DPipeline(RasterPipeline* pipeline, INT priority, draw2d::Callback callback);

    /**
     * \brief Remove a draw2d pipeline from the client.
     * \param id The ID of the pipeline to remove.
     */
    void RemoveDraw2DPipeline(UINT id);

    using ObjectHandle = size_t;

    ObjectHandle StoreObject(std::unique_ptr<Object> object);
    void         DeleteObject(ObjectHandle handle);

    void WaitForGPU();
    void MoveToNextFrame();

    [[nodiscard]] std::wstring GetDRED() const;

private:
    static std::array<float, 4> const CLEAR_COLOR;
    static std::array<float, 4> const LETTERBOX_COLOR;

    static UINT const   AGILITY_SDK_VERSION;
    static LPCSTR const AGILITY_SDK_PATH;

    struct PostVertex
    {
        DirectX::XMFLOAT4 position;
        DirectX::XMFLOAT2 uv;
    };

    ComPtr<ID3D12Device5>      m_device;
    ComPtr<D3D12MA::Allocator> m_allocator;
    ComPtr<IDXGISwapChain3>    m_swapChain;
    ComPtr<ID3D12InfoQueue1>   m_infoQueue;
    ComPtr<ID3D12CommandQueue> m_commandQueue;

    Resolution m_resolution;

    D3D12MessageFunc m_debugCallback;
    DWORD            m_callbackCookie{};

    std::unique_ptr<Uploader>    m_uploader = nullptr;
    Bag<std::unique_ptr<Object>> m_objects  = {};

    RasterInfo m_spaceViewport  = {};
    RasterInfo m_postViewport   = {};
    RasterInfo m_draw2dViewport = {};

    std::unique_ptr<Space> m_space            = nullptr;
    bool                   m_spaceInitialized = false;

    Allocation<ID3D12Resource> m_postVertexBuffer;
    D3D12_VERTEX_BUFFER_VIEW   m_postVertexBufferView{};

    struct Draw2dPipeline
    {
        draw2d::Pipeline pipeline;
        INT              priority;
    };

    std::vector<std::unique_ptr<RasterPipeline>>          m_rasterPipelines        = {};
    RasterPipeline*                                       m_postProcessingPipeline = nullptr;
    std::list<Draw2dPipeline>                             m_draw2dPipelines        = {};
    std::map<UINT, decltype(m_draw2dPipelines)::iterator> m_draw2dPipelineIDs      = {};
    UINT                                                  m_nextDraw2dPipelineID   = 0;

    CommandAllocatorGroup m_uploadGroup;
    CommandAllocatorGroup m_2dGroup;

    DescriptorHeap                                  m_rtvHeap;
    std::array<ComPtr<ID3D12Resource>, FRAME_COUNT> m_finalRenderTargets;
    Allocation<ID3D12Resource>                      m_intermediateRenderTarget;
    bool                                            m_intermediateRenderTargetInitialized = false;

    DescriptorHeap                                      m_dsvHeap;
    std::array<Allocation<ID3D12Resource>, FRAME_COUNT> m_finalDepthStencilBuffers;
    bool                                                m_finalDepthStencilBuffersInitialized = false;
    Allocation<ID3D12Resource>                          m_intermediateDepthStencilBuffer;
    bool                                                m_intermediateDepthStencilBufferInitialized = false;

    std::array<Allocation<ID3D12Resource>, FRAME_COUNT> m_screenshotBuffers;
    bool                                                m_screenshotBuffersInitialized = false;
    std::optional<ScreenshotFunc>                       m_screenshotFunc               = std::nullopt;

    UINT                            m_frameIndex = 0;
    HANDLE                          m_fenceEvent = {};
    ComPtr<ID3D12Fence>             m_fence;
    std::array<UINT64, FRAME_COUNT> m_fenceValues = {0};

    bool m_windowVisible = true;
    bool m_windowedMode  = true;

#if defined(USE_NSIGHT_AFTERMATH)
    GpuCrashTracker::MarkerMap m_markerMap      = {};
    ShaderDatabase             m_shaderDatabase = {};
    GpuCrashTracker            m_gpuCrashTracker;

public:
    void SetupCommandListForAftermath(ComPtr<ID3D12GraphicsCommandList> const& commandList) const;
    void SetupShaderForAftermath(ComPtr<IDxcResult> const& result);

private:
#endif

    void CheckRaytracingSupport() const;
    void PopulateSpaceCommandList() const;
    void PopulatePostProcessingCommandList() const;
    void PopulateScreenshotCommandList() const;

    void LoadDevice();
    void InitializeFences();
    void LoadRasterPipeline();
    void CreateFinalDepthBuffers();
    void EnsureValidDepthBuffers(ComPtr<ID3D12GraphicsCommandList4> commandList);
    void CreateScreenShotBuffers();
    void EnsureValidScreenShotBuffer(ComPtr<ID3D12GraphicsCommandList4> commandList);
    void SetupSizeDependentResources();
    void SetupSpaceResolutionDependentResources();
    void EnsureValidIntermediateRenderTarget(ComPtr<ID3D12GraphicsCommandList4> commandList);
    void PopulateCommandLists();
    void UpdatePostViewAndScissor();

    void HandleScreenshot();
};

#if defined(USE_NSIGHT_AFTERMATH)
#define VG_SHADER_REGISTRY(client) [&client](ComPtr<IDxcResult> result){(client).SetupShaderForAftermath(result);} // NOLINT(bugprone-macro-parentheses)
#else
#define VG_SHADER_REGISTRY(client) [&client](ComPtr<IDxcResult>){(void)(client);} // NOLINT(bugprone-macro-parentheses)
#endif
