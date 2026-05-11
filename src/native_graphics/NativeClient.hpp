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

class Context;
class RasterPipeline;
class Texture;

using ScreenshotFunc = void(*)(std::byte*, UINT, UINT);

class NativeClient final : public DXApp
{
public:
    explicit NativeClient(Configuration const& configuration);

    [[nodiscard]] Context& GetContext() const;

protected:
    void OnPreInitialization() override;
    void OnPostInitialization() override;
    void OnInitializationComplete() override;
    void OnLogicUpdate() override;
    void OnPreRenderUpdate() override;
    void OnRenderUpdate() override;
    void OnDestroy() override;

    void OnSizeChanged(UINT newWidth, UINT newHeight, bool minimized) override;
    void OnWindowMoved(int xPos, int yPos) override;

public:
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

    void CreatePostProcessingShaderResourceViews() const;

    using ObjectHandle = size_t;

    ObjectHandle StoreObject(std::unique_ptr<Object> object);
    void         DeleteObject(ObjectHandle handle);

    void WaitForGPU();
    void MoveToNextFrame();

    [[nodiscard]] std::wstring GetDRED() const;

private:
    static std::array<float, 4> const CLEAR_COLOR;
    static std::array<float, 4> const LETTERBOX_COLOR;

    struct PostVertex
    {
        DirectX::XMFLOAT4 position;
        DirectX::XMFLOAT2 uv;
    };

    Configuration            configuration;
    std::unique_ptr<Context> context;
    Resolution               resolution;

    std::unique_ptr<Uploader>    uploader = nullptr;
    Bag<std::unique_ptr<Object>> objects  = {};

    RasterInfo spaceViewport  = {};
    RasterInfo postViewport   = {};
    RasterInfo draw2dViewport = {};

    std::unique_ptr<Space> space            = nullptr;
    bool                   spaceInitialized = false;

    Allocation<ID3D12Resource> postVertexBuffer;
    D3D12_VERTEX_BUFFER_VIEW   postVertexBufferView{};

    struct Draw2dPipeline
    {
        draw2d::Pipeline pipeline;
        INT              priority;
    };

    std::vector<std::unique_ptr<RasterPipeline>>        rasterPipelines        = {};
    RasterPipeline*                                     postProcessingPipeline = nullptr;
    std::list<Draw2dPipeline>                           draw2dPipelines        = {};
    std::map<UINT, decltype(draw2dPipelines)::iterator> draw2dPipelineIDs      = {};
    UINT                                                nextDraw2dPipelineID   = 0;

    CommandAllocatorGroup uploadGroup;
    CommandAllocatorGroup draw2dGroup;

    Allocation<ID3D12Resource> intermediateRenderTarget;
    bool                       intermediateRenderTargetInitialized = false;

    DescriptorHeap                                      dsvHeap;
    std::array<Allocation<ID3D12Resource>, FRAME_COUNT> finalDepthStencilBuffers;
    bool                                                finalDepthStencilBuffersInitialized = false;
    Allocation<ID3D12Resource>                          intermediateDepthStencilBuffer;
    bool                                                intermediateDepthStencilBufferInitialized = false;

    std::array<Allocation<ID3D12Resource>, FRAME_COUNT> screenshotBuffers;
    bool                                                screenshotBuffersInitialized = false;
    std::optional<ScreenshotFunc>                       screenshotFunc               = std::nullopt;

    bool windowVisible = true;
    bool windowedMode  = true;

    void PopulateSpaceCommandList() const;
    void PopulatePostProcessingCommandList() const;
    void PopulateScreenshotCommandList() const;

    void LoadRasterPipeline();
    void CreateFinalDepthBuffers();
    void EnsureValidDepthBuffers(ComPtr<ID3D12GraphicsCommandList4> commandList);
    void CreateScreenShotBuffers();
    void EnsureValidScreenShotBuffer(ComPtr<ID3D12GraphicsCommandList4> commandList);
    void SetUpSizeDependentResources();
    void SetUpSpaceResolutionDependentResources();
    void EnsureValidIntermediateRenderTarget(ComPtr<ID3D12GraphicsCommandList4> commandList);
    void PopulateCommandLists();
    void UpdatePostViewAndScissor();

    void HandleScreenshot();
};
