//  <copyright file="NativeClient.cpp" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#include "stdafx.h"
#include "UserInterface/Objects/Renderer.hpp"

constexpr std::array<float, 4> NativeClient::CLEAR_COLOR     = {1.0f, 1.0f, 1.0f, 1.0f};
constexpr std::array<float, 4> NativeClient::LETTERBOX_COLOR = {0.0f, 0.0f, 0.0f, 1.0f};

NativeClient::NativeClient(Configuration const& configuration)
    : DXApp(configuration)
  , configuration(configuration)
  , resolution(Resolution{.width = configuration.width, .height = configuration.height} * configuration.renderScale)
  , space(std::make_unique<Space>(*this))
{
    if (SupportPIX() && !PIXIsAttachedForGpuCapture()) PIXLoadLatestWinPixGpuCapturerLibrary();
}

Context& NativeClient::GetContext() const { return *context; }

void NativeClient::OnPreInitialization()
{
    context = std::make_unique<Context>(*this, configuration, GetWidth(), GetHeight());

    space->PerformInitialSetupStepOne(context->GetCommandQueue());

    dsvHeap.Create(context->GetD3D12Device(), FRAME_COUNT + 1, D3D12_DESCRIPTOR_HEAP_TYPE_DSV, false);
    NAME_DIRECT_OBJECT(dsvHeap);

    SetUpSizeDependentResources();
    SetUpSpaceResolutionDependentResources();

    uploader = std::make_unique<Uploader>(*this, nullptr);

    LoadRasterPipeline();
}

void NativeClient::OnPostInitialization()
{
    if (!spaceInitialized) space = nullptr;

    uploader->ExecuteUploads(context->GetCommandQueue());

    context->WaitForGPU();
    uploader = nullptr;
}

void NativeClient::OnInitializationComplete() { if (space) space->SpoolUp(); }

void NativeClient::LoadRasterPipeline()
{
    constexpr std::array quadVertices = {
        PostVertex{.position = {-1.0f, 1.0f, 0.0f, 1.0f}, .uv = {0.0f, 0.0f}},
        PostVertex{.position = {1.0f, 1.0f, 0.0f, 1.0f}, .uv = {1.0f, 0.0f}},
        PostVertex{.position = {-1.0f, -1.0f, 0.0f, 1.0f}, .uv = {0.0f, 1.0f}},
        PostVertex{.position = {1.0f, -1.0f, 0.0f, 1.0f}, .uv = {1.0f, 1.0f}}
    };

    constexpr UINT vertexBufferSize = sizeof quadVertices;
    postVertexBuffer                = util::AllocateBuffer(*this, vertexBufferSize, D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_COMMON, D3D12_HEAP_TYPE_DEFAULT);
    NAME_DIRECT_OBJECT(postVertexBuffer);

    uploader->UploadBuffer(static_cast<std::byte const*>(static_cast<void const*>(quadVertices.data())), vertexBufferSize, postVertexBuffer);

    postVertexBufferView.BufferLocation = postVertexBuffer.GetGPUVirtualAddress();
    postVertexBufferView.StrideInBytes  = sizeof(PostVertex);
    postVertexBufferView.SizeInBytes    = vertexBufferSize;

    INITIALIZE_COMMAND_ALLOCATOR_GROUP(*this, &uploadGroup, D3D12_COMMAND_LIST_TYPE_DIRECT);
    INITIALIZE_COMMAND_ALLOCATOR_GROUP(*this, &draw2DGroup, D3D12_COMMAND_LIST_TYPE_DIRECT);
    INITIALIZE_COMMAND_ALLOCATOR_GROUP(*this, &screenshotGroup, D3D12_COMMAND_LIST_TYPE_DIRECT);
}

void NativeClient::CreateFinalDepthBuffers()
{
    finalDepthStencilBuffersInitialized = false;

    D3D12_RESOURCE_DESC depthResourceDesc = CD3DX12_RESOURCE_DESC::Tex2D(DXGI_FORMAT_D32_FLOAT, GetWidth(), GetHeight(), 1, 1);
    depthResourceDesc.Flags               |= D3D12_RESOURCE_FLAG_ALLOW_DEPTH_STENCIL;

    CD3DX12_CLEAR_VALUE const depthOptimizedClearValue(DXGI_FORMAT_D32_FLOAT, 1.0f, 0);

    for (UINT frame = 0; frame < FRAME_COUNT; frame++)
    {
        finalDepthStencilBuffers[frame] = util::AllocateResource<ID3D12Resource>(
                                                                                 *this,
                                                                                 depthResourceDesc,
                                                                                 D3D12_HEAP_TYPE_DEFAULT,
                                                                                 D3D12_RESOURCE_STATE_DEPTH_WRITE,
                                                                                 &depthOptimizedClearValue);
        NAME_DIRECT_OBJECT_INDEXED(finalDepthStencilBuffers, frame);
    }

    D3D12_DEPTH_STENCIL_VIEW_DESC dsvDesc = {};
    dsvDesc.Format                        = DXGI_FORMAT_D32_FLOAT;
    dsvDesc.ViewDimension                 = D3D12_DSV_DIMENSION_TEXTURE2D;
    dsvDesc.Flags                         = D3D12_DSV_FLAG_NONE;

    for (UINT frame = 0; frame < FRAME_COUNT; frame++) context->GetD3D12Device()->CreateDepthStencilView(
                                                                                                         finalDepthStencilBuffers[frame].Get(),
                                                                                                         &dsvDesc,
                                                                                                         dsvHeap.GetDescriptorHandleCPU(frame));
}

void NativeClient::EnsureValidDepthBuffers(ComPtr<ID3D12GraphicsCommandList4> const commandList)
{
    if (!finalDepthStencilBuffersInitialized)
    {
        for (auto const& buffer : finalDepthStencilBuffers) commandList->DiscardResource(buffer.Get(), nullptr);

        finalDepthStencilBuffersInitialized = true;
    }

    if (!intermediateDepthStencilBufferInitialized)
    {
        commandList->DiscardResource(intermediateDepthStencilBuffer.Get(), nullptr);
        intermediateDepthStencilBufferInitialized = true;
    }
}

void NativeClient::CreateScreenShotBuffers()
{
    screenshotBuffersInitialized = false;

    for (UINT frame = 0; frame < FRAME_COUNT; frame++)
    {
        UINT64 const        size = GetRequiredIntermediateSize(context->GetFinalRenderTarget(frame).Get(), 0, 1);
        D3D12_RESOURCE_DESC desc = CD3DX12_RESOURCE_DESC::Buffer(size);

        screenshotBuffers[frame] = util::AllocateResource<ID3D12Resource>(*this, desc, D3D12_HEAP_TYPE_READBACK, D3D12_RESOURCE_STATE_COPY_DEST, nullptr);
        NAME_DIRECT_OBJECT_INDEXED(screenshotBuffers, frame);
    }
}

void NativeClient::EnsureValidScreenShotBuffer(ComPtr<ID3D12GraphicsCommandList4> commandList)
{
    screenshotBuffersInitialized = true;

    for (auto const& buffer : screenshotBuffers) commandList->DiscardResource(buffer.Get(), nullptr);
}

void NativeClient::SetUpSizeDependentResources()
{
    UpdatePostViewAndScissor();

    draw2dViewport.viewport.Width  = static_cast<float>(GetWidth());
    draw2dViewport.viewport.Height = static_cast<float>(GetHeight());

    draw2dViewport.scissorRect.right  = static_cast<LONG>(GetWidth());
    draw2dViewport.scissorRect.bottom = static_cast<LONG>(GetHeight());

    CreateFinalDepthBuffers();
    CreateScreenShotBuffers();
}

void NativeClient::SetUpSpaceResolutionDependentResources()
{
    spaceViewport.viewport.Width  = static_cast<float>(resolution.width);
    spaceViewport.viewport.Height = static_cast<float>(resolution.height);

    spaceViewport.scissorRect.right  = static_cast<LONG>(resolution.width);
    spaceViewport.scissorRect.bottom = static_cast<LONG>(resolution.height);

    UpdatePostViewAndScissor();

    D3D12_RESOURCE_DESC const   swapChainDesc = context->GetFinalRenderTarget(context->GetFrameIndex())->GetDesc();
    CD3DX12_CLEAR_VALUE const   clearValue(swapChainDesc.Format, CLEAR_COLOR.data());
    CD3DX12_RESOURCE_DESC const renderTargetDesc = CD3DX12_RESOURCE_DESC::Tex2D(
                                                                                swapChainDesc.Format,
                                                                                resolution.width,
                                                                                resolution.height,
                                                                                1,
                                                                                1,
                                                                                swapChainDesc.SampleDesc.Count,
                                                                                swapChainDesc.SampleDesc.Quality,
                                                                                D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET,
                                                                                D3D12_TEXTURE_LAYOUT_UNKNOWN,
                                                                                0u);

    intermediateRenderTarget = util::AllocateResource<ID3D12Resource>(*this, renderTargetDesc, D3D12_HEAP_TYPE_DEFAULT, D3D12_RESOURCE_STATE_RENDER_TARGET, &clearValue);
    NAME_DIRECT_OBJECT(intermediateRenderTarget);

    intermediateRenderTargetInitialized = false;

    context->GetD3D12Device()->CreateRenderTargetView(intermediateRenderTarget.Get(), nullptr, context->GetIntermediateRenderTargetView());

    if (space) space->PerformResolutionDependentSetup(resolution);

    D3D12_RESOURCE_DESC depthResourceDesc = CD3DX12_RESOURCE_DESC::Tex2D(DXGI_FORMAT_R32_TYPELESS, resolution.width, resolution.height, 1, 1);
    depthResourceDesc.Flags               |= D3D12_RESOURCE_FLAG_ALLOW_DEPTH_STENCIL;

    CD3DX12_CLEAR_VALUE const depthOptimizedClearValue(DXGI_FORMAT_D32_FLOAT, 1.0f, 0);

    intermediateDepthStencilBuffer = util::AllocateResource<ID3D12Resource>(
                                                                            *this,
                                                                            depthResourceDesc,
                                                                            D3D12_HEAP_TYPE_DEFAULT,
                                                                            D3D12_RESOURCE_STATE_DEPTH_WRITE,
                                                                            &depthOptimizedClearValue);
    NAME_DIRECT_OBJECT(intermediateDepthStencilBuffer);

    D3D12_DEPTH_STENCIL_VIEW_DESC dsvDesc = {};
    dsvDesc.Format                        = DXGI_FORMAT_D32_FLOAT;
    dsvDesc.ViewDimension                 = D3D12_DSV_DIMENSION_TEXTURE2D;
    dsvDesc.Flags                         = D3D12_DSV_FLAG_NONE;

    context->GetD3D12Device()->CreateDepthStencilView(intermediateDepthStencilBuffer.Get(), &dsvDesc, dsvHeap.GetDescriptorHandleCPU(FRAME_COUNT));

    if (postProcessingPipeline != nullptr) CreatePostProcessingShaderResourceViews();
}

void NativeClient::EnsureValidIntermediateRenderTarget(ComPtr<ID3D12GraphicsCommandList4> const commandList)
{
    if (intermediateRenderTargetInitialized) return;
    intermediateRenderTargetInitialized = true;

    commandList->DiscardResource(intermediateRenderTarget.Get(), nullptr);
}

void NativeClient::OnLogicUpdate() { if (space) space->Update(); }

void NativeClient::OnPreRenderUpdate()
{
    if (!windowVisible) return;

    uploadGroup.Reset(context->GetFrameIndex());
    uploader = std::make_unique<Uploader>(*this, uploadGroup.commandList);
}

void NativeClient::OnRenderUpdate()
{
    if (!windowVisible) return;

    {
        PIXScopedEvent(context->GetCommandQueue().Get(), PIX_COLOR_DEFAULT, L"Render");

        uploadGroup.Close();

        PopulateRenderingCommandLists();

        std::vector<ID3D12CommandList*> commandLists;
        commandLists.reserve(3);

        commandLists.push_back(uploadGroup.commandList.Get());
        if (space && space->IsRendered()) commandLists.push_back(space->GetCommandList().Get());
        commandLists.push_back(draw2DGroup.commandList.Get());

        context->GetCommandQueue()->ExecuteCommandLists(static_cast<UINT>(commandLists.size()), commandLists.data());
    }

    if (!userInterfaces.empty())
    {
        PIXScopedEvent(context->GetCommandQueue().Get(), PIX_COLOR_DEFAULT, L"UI");

        context->GetUserInterfaceContext().BeginFrame(context->GetFrameIndex());

        RenderUserInterfaces();

        context->GetUserInterfaceContext().EndFrame(context->GetFrameIndex());
    }

    if (screenshotFunction.has_value())
    {
        PIXScopedEvent(context->GetCommandQueue().Get(), PIX_COLOR_DEFAULT, L"Screenshot");

        PopulateScreenshotCommandList();

        std::vector<ID3D12CommandList*> commandLists;
        commandLists.reserve(1);

        commandLists.push_back(screenshotGroup.commandList.Get());

        context->GetCommandQueue()->ExecuteCommandLists(static_cast<UINT>(commandLists.size()), commandLists.data());
    }

    UINT const                        syncInterval      = IsTearingSupportEnabled() && windowedMode ? 0 : 1;
    UINT const                        presentFlags      = IsTearingSupportEnabled() && windowedMode ? DXGI_PRESENT_ALLOW_TEARING : 0;
    constexpr DXGI_PRESENT_PARAMETERS presentParameters = {};
    HRESULT const                     present           = context->GetSwapChain()->Present1(syncInterval, presentFlags, &presentParameters);

#ifdef USE_NSIGHT_AFTERMATH
    if (FAILED(present))
    {
        if (SupportPIX()) throw std::runtime_error("Present failed");

        constexpr auto tdrTerminationTimeout = std::chrono::seconds(3);
        auto const     tStart                = std::chrono::steady_clock::now();
        auto           tElapsed              = std::chrono::milliseconds::zero();

        GFSDK_Aftermath_CrashDump_Status status = GFSDK_Aftermath_CrashDump_Status_Unknown;
        AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GetCrashDumpStatus(&status));

        while (status != GFSDK_Aftermath_CrashDump_Status_CollectingDataFailed && status != GFSDK_Aftermath_CrashDump_Status_Finished && tElapsed < tdrTerminationTimeout)
        {
            std::this_thread::sleep_for(std::chrono::milliseconds(50));
            AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GetCrashDumpStatus(&status));

            auto tEnd = std::chrono::steady_clock::now();
            tElapsed  = std::chrono::duration_cast<std::chrono::milliseconds>(tEnd - tStart);
        }

        if (status != GFSDK_Aftermath_CrashDump_Status_Finished)
        {
            std::stringstream errorMessage;
            errorMessage << "Unexpected crash dump status: " << status;
            MessageBoxA(nullptr, errorMessage.str().c_str(), "Aftermath Error", MB_OK);
        }

        throw std::runtime_error("Present failed");
    }
#else
    TryDo(present);
#endif

    context->WaitForGPU();

    if (space && space->IsRendered()) space->CleanupRender();

    HandleScreenshot();

    context->MoveToNextFrame();
}

void NativeClient::OnDestroy()
{
    context->WaitForGPU();
}

void NativeClient::OnSizeChanged(UINT const newWidth, UINT const newHeight, bool const minimized)
{
    if ((newWidth != GetWidth() || newHeight != GetHeight()) && !minimized)
    {
        context->OnResize(newWidth, newHeight);

        BOOL fullscreenState;
        TryDo(context->GetSwapChain()->GetFullscreenState(&fullscreenState, nullptr));
        windowedMode = !static_cast<bool>(fullscreenState);

        UpdateForSizeChange(newWidth, newHeight);
        SetUpSizeDependentResources();

        if (Resolution const newResolution = Resolution{.width = newWidth, .height = newHeight} * GetRenderScale();
            newResolution != resolution)
        {
            resolution = newResolution;
            SetUpSpaceResolutionDependentResources();
        }
    }

    windowVisible = !minimized;
}

void NativeClient::OnWindowMoved(int, int)
{
    // Nothing to do, here for symmetry with other message handlers.
}

void NativeClient::InitRaytracingPipeline(SpacePipelineDescription const& pipeline)
{
    if (space->PerformInitialSetupStepTwo(pipeline)) spaceInitialized = true;
    else space                                                        = nullptr;
}

// ReSharper disable once CppMemberFunctionMayBeStatic
void NativeClient::ToggleFullscreen() const { Win32Application::ToggleFullscreenWindow(context->GetSwapChain().Get()); }

void NativeClient::TakeScreenshot(ScreenshotFunc func)
{
    if (screenshotFunction.has_value()) return;
    screenshotFunction = std::move(func);
}

Texture* NativeClient::LoadTexture(std::byte** data, TextureDescription const& description) const
{
    Require(uploader != nullptr);

    return Texture::Create(*uploader, data, description);
}

Space* NativeClient::GetSpace() const { return space.get(); }

void NativeClient::AddRasterPipeline(std::unique_ptr<RasterPipeline> pipeline) { rasterPipelines.push_back(std::move(pipeline)); }

void NativeClient::SetPostProcessingPipeline(RasterPipeline* pipeline)
{
    postProcessingPipeline = pipeline;
    CreatePostProcessingShaderResourceViews();
}

UINT NativeClient::AddDraw2DPipeline(RasterPipeline* pipeline, INT const priority, draw2d::Callback const callback)
{
    // INT_MIN and INT_MAX should always place the pipeline at the front and back of the list, respectively.
    // Thus, all entries in the list should be in the range (INT_MIN, INT_MAX) - both exclusive.
    auto clampedPriority = static_cast<UINT>(std::clamp(priority, INT_MIN + 1, INT_MAX - 1));

    decltype(draw2dPipelines)::iterator iterator;

    UINT const id = nextDraw2dPipelineID;

    if (draw2dPipelines.empty() || priority < draw2dPipelines.front().priority)
    {
        draw2dPipelines.emplace_front(draw2d::Pipeline{*this, pipeline, id, callback}, clampedPriority);
        iterator = draw2dPipelines.begin();
    }
    else if (priority > draw2dPipelines.back().priority)
    {
        draw2dPipelines.emplace_back(draw2d::Pipeline{*this, pipeline, id, callback}, clampedPriority);
        iterator = std::prev(draw2dPipelines.end());
    }
    else
        for (auto it = draw2dPipelines.begin(); it != draw2dPipelines.end(); ++it)
            // Goal: insert after the first element with priority lower than the new one.
            if (priority > it->priority)
            {
                iterator = draw2dPipelines.emplace(--it, draw2d::Pipeline(*this, pipeline, id, callback), clampedPriority);
                break;
            }

    draw2dPipelineIDs[nextDraw2dPipelineID] = iterator;
    nextDraw2dPipelineID++;

    return id;
}

void NativeClient::RemoveDraw2DPipeline(UINT const id)
{
    auto const iterator = draw2dPipelineIDs[id];

    draw2dPipelines.erase(iterator);
    draw2dPipelineIDs.erase(id);
}

ui::Renderer* NativeClient::CreateUserInterface(INT priority)
{
    // todo: Create a ui::Renderer, insert it into userInterfaces sorted by priority, and return its pointer.
    // Contract: lower priority renders first; higher priority renders later and appears on top; equal priority preserves
    // insertion order.
    (void)priority;
    throw NativeException("TODO: create UI renderer.");
}

void NativeClient::FreeUserInterface(ui::Renderer* renderer)
{
    // todo: Validate active UI resources, remove the matching renderer from userInterfaces, and destroy it.
    // Contract: the passed pointer is invalid after this call.
    (void)renderer;
}

void NativeClient::CreatePostProcessingShaderResourceViews() const
{
    postProcessingPipeline->CreateShaderResourceView(postProcessingPipeline->GetBindings().PostProcessing().color, 0, {intermediateRenderTarget});

    // Because the depth buffer is created as TYPELESS, we cannot use the NULL descriptor.

    D3D12_SHADER_RESOURCE_VIEW_DESC srvDesc = {};
    srvDesc.ViewDimension                   = D3D12_SRV_DIMENSION_TEXTURE2D;
    srvDesc.Format                          = DXGI_FORMAT_R32_FLOAT;
    srvDesc.Shader4ComponentMapping         = D3D12_DEFAULT_SHADER_4_COMPONENT_MAPPING;
    srvDesc.Texture2D.MipLevels             = 1;

    postProcessingPipeline->CreateShaderResourceView(postProcessingPipeline->GetBindings().PostProcessing().depth, 0, {intermediateDepthStencilBuffer, &srvDesc});
}

NativeClient::ObjectHandle NativeClient::StoreObject(std::unique_ptr<Object> object) { return objects.Push(std::move(object)); }

void NativeClient::DeleteObject(ObjectHandle const handle) { objects.Pop(handle); }

std::wstring NativeClient::GetDRED() const
{
    ComPtr<ID3D12DeviceRemovedExtendedData2> dred;
    TryDo(context->GetD3D12Device()->QueryInterface(IID_PPV_ARGS(&dred)));

    D3D12_DRED_AUTO_BREADCRUMBS_OUTPUT1 dredAutoBreadcrumbsOutput = {};
    TryDo(dred->GetAutoBreadcrumbsOutput1(&dredAutoBreadcrumbsOutput));

    D3D12_DRED_PAGE_FAULT_OUTPUT2 dredPageFaultOutput = {};
    TryDo(dred->GetPageFaultAllocationOutput2(&dredPageFaultOutput));

    return util::FormatDRED(dredAutoBreadcrumbsOutput, dredPageFaultOutput, dred->GetDeviceState());
}

void NativeClient::PopulateSpaceCommandList() const
{
    Require(space != nullptr);

    D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle = context->GetIntermediateRenderTargetView();
    D3D12_CPU_DESCRIPTOR_HANDLE dsvHandle = dsvHeap.GetDescriptorHandleCPU(FRAME_COUNT);

    space->Reset(context->GetFrameIndex());
    space->Render(intermediateRenderTarget, intermediateDepthStencilBuffer, {.rtv = &rtvHandle, .dsv = &dsvHandle, .viewport = &spaceViewport});
}

void NativeClient::PopulatePostProcessingCommandList() const
{
    if (space == nullptr || !space->IsRendered()) return; // Nothing to post-process.

    PIXScopedEvent(draw2DGroup.commandList.Get(), PIX_COLOR_DEFAULT, postProcessingPipeline->GetName());

    postProcessingPipeline->SetPipeline(draw2DGroup.commandList);
    postProcessingPipeline->BindResources(draw2DGroup.commandList);

    postViewport.Set(draw2DGroup.commandList);

    draw2DGroup.commandList->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP);
    draw2DGroup.commandList->IASetVertexBuffers(0, 1, &postVertexBufferView);
    draw2DGroup.commandList->DrawInstanced(4, 1, 0, 0);
}

void NativeClient::PopulateScreenshotCommandList()
{
    screenshotGroup.Reset(context->GetFrameIndex());

    D3D12_RESOURCE_BARRIER const entry = CD3DX12_RESOURCE_BARRIER::Transition(
                                                                              context->GetFinalRenderTarget(context->GetFrameIndex()).Get(),
                                                                              D3D12_RESOURCE_STATE_PRESENT,
                                                                              D3D12_RESOURCE_STATE_COPY_SOURCE);
    screenshotGroup.commandList->ResourceBarrier(1, &entry);

    D3D12_PLACED_SUBRESOURCE_FOOTPRINT footprint = {};
    footprint.Footprint.Format                   = DXGI_FORMAT_B8G8R8A8_UNORM;
    footprint.Footprint.Width                    = GetWidth();
    footprint.Footprint.Height                   = GetHeight();
    footprint.Footprint.Depth                    = 1;
    footprint.Footprint.RowPitch                 = GetWidth() * 4;

    auto const dst = CD3DX12_TEXTURE_COPY_LOCATION(screenshotBuffers[context->GetFrameIndex()].Get(), footprint);
    auto const src = CD3DX12_TEXTURE_COPY_LOCATION(context->GetFinalRenderTarget(context->GetFrameIndex()).Get(), 0);
    screenshotGroup.commandList->CopyTextureRegion(&dst, 0, 0, 0, &src, nullptr);

    D3D12_RESOURCE_BARRIER const exit = CD3DX12_RESOURCE_BARRIER::Transition(
                                                                             context->GetFinalRenderTarget(context->GetFrameIndex()).Get(),
                                                                             D3D12_RESOURCE_STATE_COPY_SOURCE,
                                                                             D3D12_RESOURCE_STATE_PRESENT);
    screenshotGroup.commandList->ResourceBarrier(1, &exit);

    TryDo(screenshotGroup.commandList->Close());
}

void NativeClient::PopulateRenderingCommandLists()
{
    draw2DGroup.Reset(context->GetFrameIndex());

    EnsureValidDepthBuffers(draw2DGroup.commandList);
    EnsureValidIntermediateRenderTarget(draw2DGroup.commandList);

    std::array<D3D12_RESOURCE_BARRIER, 3> barriers = {
        CD3DX12_RESOURCE_BARRIER::Transition(intermediateRenderTarget.Get(), D3D12_RESOURCE_STATE_RENDER_TARGET, D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE),
        CD3DX12_RESOURCE_BARRIER::Transition(intermediateDepthStencilBuffer.Get(), D3D12_RESOURCE_STATE_DEPTH_WRITE, D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE),
        CD3DX12_RESOURCE_BARRIER::Transition(context->GetFinalRenderTarget(context->GetFrameIndex()).Get(), D3D12_RESOURCE_STATE_PRESENT, D3D12_RESOURCE_STATE_RENDER_TARGET),
    };

    draw2DGroup.commandList->ResourceBarrier(static_cast<UINT>(barriers.size()), barriers.data());

    if (space && space->IsRendered()) PopulateSpaceCommandList();

    auto const rtvHandle = context->GetFinalRenderTargetView(context->GetFrameIndex());
    auto const dsvHandle = dsvHeap.GetDescriptorHandleCPU(context->GetFrameIndex());

    draw2DGroup.commandList->OMSetRenderTargets(1, &rtvHandle, FALSE, &dsvHandle);
    draw2DGroup.commandList->ClearRenderTargetView(rtvHandle, LETTERBOX_COLOR.data(), 0, nullptr);
    draw2DGroup.commandList->ClearDepthStencilView(dsvHandle, D3D12_CLEAR_FLAG_DEPTH, 1.0f, 0, 0, nullptr);

    if (postProcessingPipeline != nullptr) PopulatePostProcessingCommandList();

    draw2dViewport.Set(draw2DGroup.commandList);

    for (auto& [pipeline, priority] : draw2dPipelines)
    {
        PIXScopedEvent(draw2DGroup.commandList.Get(), PIX_COLOR_DEFAULT, pipeline.GetName());
        pipeline.PopulateCommandList(draw2DGroup.commandList);
    }

    barriers[0].Transition.StateBefore = D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
    barriers[0].Transition.StateAfter  = D3D12_RESOURCE_STATE_RENDER_TARGET;

    barriers[1].Transition.StateBefore = D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
    barriers[1].Transition.StateAfter  = D3D12_RESOURCE_STATE_DEPTH_WRITE;

    barriers[2].Transition.StateBefore = D3D12_RESOURCE_STATE_RENDER_TARGET;
    barriers[2].Transition.StateAfter  = D3D12_RESOURCE_STATE_PRESENT;

    UINT numberOfBarriers = static_cast<UINT>(barriers.size());

    if (!userInterfaces.empty())
        // The UI rendering expects the final render target to remain in D3D12_RESOURCE_STATE_RENDER_TARGET.
        // It will also handle the transition to D3D12_RESOURCE_STATE_PRESENT.

        numberOfBarriers -= 1;

    draw2DGroup.commandList->ResourceBarrier(numberOfBarriers, barriers.data());
    draw2DGroup.Close();
}

void NativeClient::UpdatePostViewAndScissor()
{
    auto const widthF  = static_cast<float>(GetWidth());
    auto const heightF = static_cast<float>(GetHeight());

    float const viewWidthRatio  = static_cast<float>(resolution.width) / widthF;
    float const viewHeightRatio = static_cast<float>(resolution.height) / heightF;

    float x = 1.0f;
    float y = 1.0f;

    if (viewWidthRatio < viewHeightRatio) x = viewWidthRatio / viewHeightRatio;
    else y                                  = viewHeightRatio / viewWidthRatio;

    postViewport.viewport.TopLeftX = widthF * (1.0f - x) / 2.0f;
    postViewport.viewport.TopLeftY = heightF * (1.0f - y) / 2.0f;
    postViewport.viewport.Width    = x * widthF;
    postViewport.viewport.Height   = y * heightF;

    postViewport.scissorRect.left   = static_cast<LONG>(postViewport.viewport.TopLeftX);
    postViewport.scissorRect.right  = static_cast<LONG>(postViewport.viewport.TopLeftX + postViewport.viewport.Width);
    postViewport.scissorRect.top    = static_cast<LONG>(postViewport.viewport.TopLeftY);
    postViewport.scissorRect.bottom = static_cast<LONG>(postViewport.viewport.TopLeftY + postViewport.viewport.Height);
}

void NativeClient::RenderUserInterfaces()
{
    // todo: Render all UI renderers after Draw2D and before screenshot copying or final present transitions.
    // Contract: call ui::Renderer::Render(frameIndex) for each renderer in ordered userInterfaces.
}

void NativeClient::HandleScreenshot()
{
    if (!screenshotFunction.has_value()) return;
    auto* func = screenshotFunction.value();

    UINT const size = GetWidth() * GetHeight() * 4;
    auto const data = std::make_unique<std::byte[]>(size);

    TryDo(util::MapAndRead(screenshotBuffers[context->GetFrameIndex()], data.get(), size));

    func(data.get(), GetWidth(), GetHeight());
    screenshotFunction = std::nullopt;
}
