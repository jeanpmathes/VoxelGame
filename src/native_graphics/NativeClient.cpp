//  <copyright file="NativeClient.cpp" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#include "stdafx.h"

constexpr std::array<float, 4> NativeClient::CLEAR_COLOR     = {1.0f, 1.0f, 1.0f, 1.0f};
constexpr std::array<float, 4> NativeClient::LETTERBOX_COLOR = {0.0f, 0.0f, 0.0f, 1.0f};

UINT const   NativeClient::AGILITY_SDK_VERSION = 614;
LPCSTR const NativeClient::AGILITY_SDK_PATH    = ".\\D3D12\\";

NativeClient::NativeClient(Configuration const& configuration)
    : DXApp(configuration)
  , resolution(Resolution{configuration.width, configuration.height} * configuration.renderScale)
#if defined(NATIVE_DEBUG)
  , debugCallback(configuration.onDebug)
#endif
  , space(std::make_unique<Space>(*this))
#if defined(USE_NSIGHT_AFTERMATH)
, gpuCrashTracker(markerMap, shaderDatabase, GpuCrashTracker::Description::Create(configuration.applicationName, configuration.applicationVersion))
#endif
{
    if (SupportPIX() && !PIXIsAttachedForGpuCapture()) PIXLoadLatestWinPixGpuCapturerLibrary();
}

ComPtr<ID3D12Device5> NativeClient::GetDevice() const { return device; }

ComPtr<D3D12MA::Allocator> NativeClient::GetAllocator() const { return allocator; }

void NativeClient::OnPreInitialization()
{
    LoadDevice();
    InitializeFences();

    space->PerformInitialSetupStepOne(commandQueue);

    SetUpSizeDependentResources();
    SetUpSpaceResolutionDependentResources();

    uploader = std::make_unique<Uploader>(*this, nullptr);

    LoadRasterPipeline();
}

void NativeClient::OnPostInitialization()
{
    if (!spaceInitialized) space = nullptr;

    uploader->ExecuteUploads(commandQueue);

    WaitForGPU();
    uploader = nullptr;
}

void NativeClient::OnInitializationComplete() { if (space) space->SpoolUp(); }

void NativeClient::LoadDevice()
{
#if defined(NATIVE_DEBUG)
    constexpr UINT dxgiFactoryFlags = DXGI_CREATE_FACTORY_DEBUG;
#else
    constexpr UINT dxgiFactoryFlags = 0;
#endif

    ComPtr<IDXGIFactory4> dxgiFactory;
    TryDo(CreateDXGIFactory2(dxgiFactoryFlags, IID_PPV_ARGS(&dxgiFactory)));

    ComPtr<ID3D12SDKConfiguration1> sdk;
    TryDo(D3D12GetInterface(CLSID_D3D12SDKConfiguration, IID_PPV_ARGS(&sdk)));

    ComPtr<ID3D12DeviceFactory> deviceFactory;
    TryDo(sdk->CreateDeviceFactory(AGILITY_SDK_VERSION, AGILITY_SDK_PATH, IID_PPV_ARGS(&deviceFactory)));

#if defined(NATIVE_DEBUG)
    ComPtr<ID3D12Debug5> debug;
    if (SUCCEEDED(deviceFactory->GetConfigurationInterface(CLSID_D3D12Debug, IID_PPV_ARGS(&debug))))
    {
        debug->EnableDebugLayer();
        debug->SetEnableAutoName(TRUE);

        if (!SupportPIX() && UseGBV()) debug->SetEnableGPUBasedValidation(TRUE);
    }
    ComPtr<ID3D12DeviceRemovedExtendedDataSettings1> dredSettings;
    if (SUCCEEDED(deviceFactory->GetConfigurationInterface(CLSID_D3D12DeviceRemovedExtendedData, IID_PPV_ARGS(&dredSettings))))
    {
        dredSettings->SetAutoBreadcrumbsEnablement(D3D12_DRED_ENABLEMENT_FORCED_ON);
        dredSettings->SetPageFaultEnablement(D3D12_DRED_ENABLEMENT_FORCED_ON);
        dredSettings->SetBreadcrumbContextEnablement(D3D12_DRED_ENABLEMENT_FORCED_ON);
    }
#endif

    ComPtr<IDXGIAdapter1> const hardwareAdapter = GetHardwareAdapter(dxgiFactory, deviceFactory);

#if defined(USE_NSIGHT_AFTERMATH)
    if (!SupportPIX()) gpuCrashTracker.Initialize();
#endif

    TryDo(deviceFactory->CreateDevice(hardwareAdapter.Get(), D3D_FEATURE_LEVEL_12_2, IID_PPV_ARGS(&device)));
    NAME_D3D12_OBJECT(device);

#if defined(USE_NSIGHT_AFTERMATH)
    if (!SupportPIX())
    {
        constexpr uint32_t aftermathFlags = GFSDK_Aftermath_FeatureFlags_EnableMarkers | GFSDK_Aftermath_FeatureFlags_EnableResourceTracking |
        GFSDK_Aftermath_FeatureFlags_CallStackCapturing | GFSDK_Aftermath_FeatureFlags_GenerateShaderDebugInfo;

        AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_DX12_Initialize(GFSDK_Aftermath_Version_API, aftermathFlags, device.Get()));
    }
#endif

#if defined(NATIVE_DEBUG)
    auto callback = [](D3D12_MESSAGE_CATEGORY const category, D3D12_MESSAGE_SEVERITY const severity, D3D12_MESSAGE_ID const id, LPCSTR const description, void* context) -> void
    {
        auto const self = static_cast<NativeClient*>(context);

        Win32Application::EnterErrorMode();
        self->debugCallback(category, severity, id, description, nullptr);
        Win32Application::ExitErrorMode();
    };
    HRESULT const infoQueueResult = device->QueryInterface(IID_PPV_ARGS(&infoQueue));
    if (SUCCEEDED(infoQueueResult))
    {
        TryDo(device->QueryInterface(IID_PPV_ARGS(&infoQueue)));
        TryDo(infoQueue->RegisterMessageCallback(callback, D3D12_MESSAGE_CALLBACK_FLAG_NONE, this, &callbackCookie));

        TryDo(infoQueue->AddApplicationMessage(D3D12_MESSAGE_SEVERITY_MESSAGE, "Installed debug callback"));

        if (PIXIsAttachedForGpuCapture() && !SupportPIX()) TryDo(
            infoQueue->AddApplicationMessage(D3D12_MESSAGE_SEVERITY_WARNING, "PIX detected, consider using the --pix command line argument"));
    }
    else debugCallback(D3D12_MESSAGE_CATEGORY_APPLICATION_DEFINED, D3D12_MESSAGE_SEVERITY_WARNING, D3D12_MESSAGE_ID_UNKNOWN, "Failed to install debug callback", nullptr);

#endif

    D3D12MA::ALLOCATOR_DESC allocatorDesc = {};
    allocatorDesc.pDevice                 = device.Get();
    allocatorDesc.pAdapter                = hardwareAdapter.Get();

    TryDo(CreateAllocator(&allocatorDesc, &allocator));

    CheckRaytracingSupport();

    D3D12_COMMAND_QUEUE_DESC queueDesc = {};
    queueDesc.Flags                    = D3D12_COMMAND_QUEUE_FLAG_NONE;
    queueDesc.Type                     = D3D12_COMMAND_LIST_TYPE_DIRECT;

    TryDo(device->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&commandQueue)));
    NAME_D3D12_OBJECT(commandQueue);

    DXGI_SWAP_CHAIN_DESC1 swapChainDesc = {};
    swapChainDesc.BufferCount           = FRAME_COUNT;
    swapChainDesc.Width                 = GetWidth();
    swapChainDesc.Height                = GetHeight();
    swapChainDesc.Format                = DXGI_FORMAT_B8G8R8A8_UNORM;
    swapChainDesc.BufferUsage           = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    swapChainDesc.SwapEffect            = DXGI_SWAP_EFFECT_FLIP_DISCARD;
    swapChainDesc.SampleDesc.Count      = 1;

    swapChainDesc.Flags = IsTearingSupportEnabled() ? DXGI_SWAP_CHAIN_FLAG_ALLOW_TEARING : 0;

    ComPtr<IDXGISwapChain1> swapChain1;
    TryDo(dxgiFactory->CreateSwapChainForHwnd(commandQueue.Get(), Win32Application::GetWindowHandle(), &swapChainDesc, nullptr, nullptr, &swapChain1));

    TryDo(dxgiFactory->MakeWindowAssociation(Win32Application::GetWindowHandle(), DXGI_MWA_NO_ALT_ENTER));

    TryDo(swapChain1.As(&swapChain));
    frameIndex = swapChain->GetCurrentBackBufferIndex();

    rtvHeap.Create(device, FRAME_COUNT + 1, D3D12_DESCRIPTOR_HEAP_TYPE_RTV, false);
    NAME_D3D12_OBJECT(rtvHeap);

    dsvHeap.Create(device, FRAME_COUNT + 1, D3D12_DESCRIPTOR_HEAP_TYPE_DSV, false);
    NAME_D3D12_OBJECT(dsvHeap);
}

void NativeClient::InitializeFences()
{
    TryDo(device->CreateFence(fenceValues[frameIndex], D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&fence)));
    NAME_D3D12_OBJECT(fence);

    fenceValues[frameIndex]++;

    fenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
    if (fenceEvent == nullptr) TryDo(HRESULT_FROM_WIN32(GetLastError()));
}

void NativeClient::LoadRasterPipeline()
{
    constexpr std::array quadVertices = {
        PostVertex{{-1.0f, 1.0f, 0.0f, 1.0f}, {0.0f, 0.0f}},
        PostVertex{{1.0f, 1.0f, 0.0f, 1.0f}, {1.0f, 0.0f}},
        PostVertex{{-1.0f, -1.0f, 0.0f, 1.0f}, {0.0f, 1.0f}},
        PostVertex{{1.0f, -1.0f, 0.0f, 1.0f}, {1.0f, 1.0f}}
    };

    constexpr UINT vertexBufferSize = sizeof quadVertices;
    postVertexBuffer                = util::AllocateBuffer(*this, vertexBufferSize, D3D12_RESOURCE_FLAG_NONE, D3D12_RESOURCE_STATE_COMMON, D3D12_HEAP_TYPE_DEFAULT);
    NAME_D3D12_OBJECT(postVertexBuffer);

    uploader->UploadBuffer(static_cast<std::byte const*>(static_cast<void const*>(quadVertices.data())), vertexBufferSize, postVertexBuffer);

    postVertexBufferView.BufferLocation = postVertexBuffer.GetGPUVirtualAddress();
    postVertexBufferView.StrideInBytes  = sizeof(PostVertex);
    postVertexBufferView.SizeInBytes    = vertexBufferSize;

    INITIALIZE_COMMAND_ALLOCATOR_GROUP(*this, &uploadGroup, D3D12_COMMAND_LIST_TYPE_DIRECT);
    INITIALIZE_COMMAND_ALLOCATOR_GROUP(*this, &draw2dGroup, D3D12_COMMAND_LIST_TYPE_DIRECT);
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
        NAME_D3D12_OBJECT_INDEXED(finalDepthStencilBuffers, frame);
    }

    D3D12_DEPTH_STENCIL_VIEW_DESC dsvDesc = {};
    dsvDesc.Format                        = DXGI_FORMAT_D32_FLOAT;
    dsvDesc.ViewDimension                 = D3D12_DSV_DIMENSION_TEXTURE2D;
    dsvDesc.Flags                         = D3D12_DSV_FLAG_NONE;

    for (UINT frame = 0; frame < FRAME_COUNT; frame++) device->CreateDepthStencilView(finalDepthStencilBuffers[frame].Get(), &dsvDesc, dsvHeap.GetDescriptorHandleCPU(frame));
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
        UINT64 const        size = GetRequiredIntermediateSize(finalRenderTargets[frame].Get(), 0, 1);
        D3D12_RESOURCE_DESC desc = CD3DX12_RESOURCE_DESC::Buffer(size);

        screenshotBuffers[frame] = util::AllocateResource<ID3D12Resource>(*this, desc, D3D12_HEAP_TYPE_READBACK, D3D12_RESOURCE_STATE_COPY_DEST, nullptr);
        NAME_D3D12_OBJECT_INDEXED(screenshotBuffers, frame);
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

    for (UINT frame = 0; frame < FRAME_COUNT; frame++)
    {
        TryDo(swapChain->GetBuffer(frame, IID_PPV_ARGS(&finalRenderTargets[frame])));
        device->CreateRenderTargetView(finalRenderTargets[frame].Get(), nullptr, rtvHeap.GetDescriptorHandleCPU(frame));

        NAME_D3D12_OBJECT_INDEXED(finalRenderTargets, frame);
    }

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

    D3D12_RESOURCE_DESC const   swapChainDesc = finalRenderTargets[frameIndex]->GetDesc();
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
    NAME_D3D12_OBJECT(intermediateRenderTarget);

    intermediateRenderTargetInitialized = false;

    device->CreateRenderTargetView(intermediateRenderTarget.Get(), nullptr, rtvHeap.GetDescriptorHandleCPU(FRAME_COUNT));

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
    NAME_D3D12_OBJECT(intermediateDepthStencilBuffer);

    D3D12_DEPTH_STENCIL_VIEW_DESC dsvDesc = {};
    dsvDesc.Format                        = DXGI_FORMAT_D32_FLOAT;
    dsvDesc.ViewDimension                 = D3D12_DSV_DIMENSION_TEXTURE2D;
    dsvDesc.Flags                         = D3D12_DSV_FLAG_NONE;

    device->CreateDepthStencilView(intermediateDepthStencilBuffer.Get(), &dsvDesc, dsvHeap.GetDescriptorHandleCPU(FRAME_COUNT));

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

    uploadGroup.Reset(frameIndex);
    uploader = std::make_unique<Uploader>(*this, uploadGroup.commandList);
}

void NativeClient::OnRenderUpdate()
{
    if (!windowVisible) return;

    {
        PIXScopedEvent(commandQueue.Get(), PIX_COLOR_DEFAULT, L"Render");

        uploadGroup.Close();

        PopulateCommandLists();

        std::vector<ID3D12CommandList*> commandLists;
        commandLists.reserve(3);

        commandLists.push_back(uploadGroup.commandList.Get());
        if (space && space->IsRendered()) commandLists.push_back(space->GetCommandList().Get());
        commandLists.push_back(draw2dGroup.commandList.Get());

        commandQueue->ExecuteCommandLists(static_cast<UINT>(commandLists.size()), commandLists.data());
    }

    UINT const                        syncInterval      = IsTearingSupportEnabled() && windowedMode ? 0 : 1;
    UINT const                        presentFlags      = IsTearingSupportEnabled() && windowedMode ? DXGI_PRESENT_ALLOW_TEARING : 0;
    constexpr DXGI_PRESENT_PARAMETERS presentParameters = {};
    HRESULT const                     present           = swapChain->Present1(syncInterval, presentFlags, &presentParameters);

#if defined(USE_NSIGHT_AFTERMATH)
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

    WaitForGPU();

    if (space && space->IsRendered()) space->CleanupRender();

    HandleScreenshot();

    MoveToNextFrame();
}

void NativeClient::OnDestroy()
{
    WaitForGPU();
    CloseHandle(fenceEvent);
}

void NativeClient::OnSizeChanged(UINT const newWidth, UINT const newHeight, bool const minimized)
{
    if ((newWidth != GetWidth() || newHeight != GetHeight()) && !minimized)
    {
        WaitForGPU();

        for (UINT frame = 0; frame < FRAME_COUNT; frame++) // NOLINT(modernize-loop-convert)
        {
            finalRenderTargets[frame].Reset();
            fenceValues[frame] = fenceValues[frameIndex];
        }

        DXGI_SWAP_CHAIN_DESC desc = {};
        TryDo(swapChain->GetDesc(&desc));
        TryDo(swapChain->ResizeBuffers(FRAME_COUNT, newWidth, newHeight, desc.BufferDesc.Format, desc.Flags));

        BOOL fullscreenState;
        TryDo(swapChain->GetFullscreenState(&fullscreenState, nullptr));
        windowedMode = !static_cast<bool>(fullscreenState);

        frameIndex = swapChain->GetCurrentBackBufferIndex();

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
void NativeClient::ToggleFullscreen() const { Win32Application::ToggleFullscreenWindow(swapChain.Get()); }

void NativeClient::TakeScreenshot(ScreenshotFunc func)
{
    if (screenshotFunc.has_value()) return;
    screenshotFunc = std::move(func);
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

void NativeClient::WaitForGPU()
{
    TryDo(commandQueue->Signal(fence.Get(), fenceValues[frameIndex]));

    TryDo(fence->SetEventOnCompletion(fenceValues[frameIndex], fenceEvent));
    WaitForSingleObjectEx(fenceEvent, INFINITE, FALSE);

    fenceValues[frameIndex]++;
}

void NativeClient::MoveToNextFrame()
{
    TryDo(commandQueue->Signal(fence.Get(), fenceValues[frameIndex]));

    UINT64 const currentFenceValue = fenceValues[static_cast<UINT64>(frameIndex)];
    frameIndex                     = swapChain->GetCurrentBackBufferIndex();

    if (fence->GetCompletedValue() < fenceValues[frameIndex])
    {
        TryDo(fence->SetEventOnCompletion(fenceValues[frameIndex], fenceEvent));
        WaitForSingleObjectEx(fenceEvent, INFINITE, FALSE);
    }

    fenceValues[frameIndex] = currentFenceValue + 1;
}

std::wstring NativeClient::GetDRED() const
{
    ComPtr<ID3D12DeviceRemovedExtendedData2> dred;
    TryDo(device->QueryInterface(IID_PPV_ARGS(&dred)));

    D3D12_DRED_AUTO_BREADCRUMBS_OUTPUT1 dredAutoBreadcrumbsOutput = {};
    TryDo(dred->GetAutoBreadcrumbsOutput1(&dredAutoBreadcrumbsOutput));

    D3D12_DRED_PAGE_FAULT_OUTPUT2 dredPageFaultOutput = {};
    TryDo(dred->GetPageFaultAllocationOutput2(&dredPageFaultOutput));

    return util::FormatDRED(dredAutoBreadcrumbsOutput, dredPageFaultOutput, dred->GetDeviceState());
}
#if defined(USE_NSIGHT_AFTERMATH)
void NativeClient::SetUpCommandListForAftermath(ComPtr<ID3D12GraphicsCommandList> const& commandList) const
{
    if (SupportPIX()) return;

    GFSDK_Aftermath_ContextHandle contextHandle;
    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_DX12_CreateContextHandle(commandList.Get(), &contextHandle));
}void NativeClient::SetUpShaderForAftermath(ComPtr<IDxcResult> const& result)
{
    if (SupportPIX()) return;

    ComPtr<IDxcBlob> objectBlob;
    TryDo(result->GetOutput(DXC_OUT_OBJECT, IID_PPV_ARGS(&objectBlob), nullptr));
    std::vector<uint8_t> binary(objectBlob->GetBufferSize());
    std::memcpy(binary.data(), objectBlob->GetBufferPointer(), objectBlob->GetBufferSize());

    ComPtr<IDxcBlob> pdbBlob;
    TryDo(result->GetOutput(DXC_OUT_PDB, IID_PPV_ARGS(&pdbBlob), nullptr));
    std::vector<uint8_t> pdb(pdbBlob->GetBufferSize());
    std::memcpy(pdb.data(), pdbBlob->GetBufferPointer(), pdbBlob->GetBufferSize());

    shaderDatabase.AddShader(std::move(binary), std::move(pdb));
}
#endif

void NativeClient::CheckRaytracingSupport() const
{
    D3D12_FEATURE_DATA_D3D12_OPTIONS5 options5 = {};
    TryDo(device->CheckFeatureSupport(D3D12_FEATURE_D3D12_OPTIONS5, &options5, sizeof(options5)));

    if (options5.RaytracingTier < D3D12_RAYTRACING_TIER_1_1) throw NativeException("Raytracing not supported on device.");
}

void NativeClient::PopulateSpaceCommandList() const
{
    Require(space != nullptr);

    D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle = rtvHeap.GetDescriptorHandleCPU(FRAME_COUNT);
    D3D12_CPU_DESCRIPTOR_HANDLE dsvHandle = dsvHeap.GetDescriptorHandleCPU(FRAME_COUNT);

    space->Reset(frameIndex);
    space->Render(intermediateRenderTarget, intermediateDepthStencilBuffer, {.rtv = &rtvHandle, .dsv = &dsvHandle, .viewport = &spaceViewport});
}

void NativeClient::PopulatePostProcessingCommandList() const
{
    if (space == nullptr || !space->IsRendered()) return; // Nothing to post-process.

    PIXScopedEvent(draw2dGroup.commandList.Get(), PIX_COLOR_DEFAULT, postProcessingPipeline->GetName());

    postProcessingPipeline->SetPipeline(draw2dGroup.commandList);
    postProcessingPipeline->BindResources(draw2dGroup.commandList);

    postViewport.Set(draw2dGroup.commandList);

    draw2dGroup.commandList->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP);
    draw2dGroup.commandList->IASetVertexBuffers(0, 1, &postVertexBufferView);
    draw2dGroup.commandList->DrawInstanced(4, 1, 0, 0);
}

void NativeClient::PopulateScreenshotCommandList() const
{
    if (!screenshotFunc.has_value()) return;

    PIXScopedEvent(draw2dGroup.commandList.Get(), PIX_COLOR_DEFAULT, L"Screenshot");

    D3D12_RESOURCE_BARRIER const entry = CD3DX12_RESOURCE_BARRIER::Transition(
        finalRenderTargets[frameIndex].Get(),
        D3D12_RESOURCE_STATE_RENDER_TARGET,
        D3D12_RESOURCE_STATE_COPY_SOURCE);
    draw2dGroup.commandList->ResourceBarrier(1, &entry);

    D3D12_PLACED_SUBRESOURCE_FOOTPRINT footprint = {};
    footprint.Footprint.Format                   = DXGI_FORMAT_B8G8R8A8_UNORM;
    footprint.Footprint.Width                    = GetWidth();
    footprint.Footprint.Height                   = GetHeight();
    footprint.Footprint.Depth                    = 1;
    footprint.Footprint.RowPitch                 = GetWidth() * 4;

    auto const dst = CD3DX12_TEXTURE_COPY_LOCATION(screenshotBuffers[frameIndex].Get(), footprint);
    auto const src = CD3DX12_TEXTURE_COPY_LOCATION(finalRenderTargets[frameIndex].Get(), 0);
    draw2dGroup.commandList->CopyTextureRegion(&dst, 0, 0, 0, &src, nullptr);

    D3D12_RESOURCE_BARRIER const exit = CD3DX12_RESOURCE_BARRIER::Transition(
        finalRenderTargets[frameIndex].Get(),
        D3D12_RESOURCE_STATE_COPY_SOURCE,
        D3D12_RESOURCE_STATE_RENDER_TARGET);
    draw2dGroup.commandList->ResourceBarrier(1, &exit);
}

void NativeClient::PopulateCommandLists()
{
    draw2dGroup.Reset(frameIndex);

    EnsureValidDepthBuffers(draw2dGroup.commandList);
    EnsureValidIntermediateRenderTarget(draw2dGroup.commandList);

    std::array<D3D12_RESOURCE_BARRIER, 3> barriers = {
        CD3DX12_RESOURCE_BARRIER::Transition(finalRenderTargets[frameIndex].Get(), D3D12_RESOURCE_STATE_PRESENT, D3D12_RESOURCE_STATE_RENDER_TARGET),
        CD3DX12_RESOURCE_BARRIER::Transition(intermediateRenderTarget.Get(), D3D12_RESOURCE_STATE_RENDER_TARGET, D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE),
        CD3DX12_RESOURCE_BARRIER::Transition(intermediateDepthStencilBuffer.Get(), D3D12_RESOURCE_STATE_DEPTH_WRITE, D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE)
    };

    draw2dGroup.commandList->ResourceBarrier(static_cast<UINT>(barriers.size()), barriers.data());

    if (space && space->IsRendered()) PopulateSpaceCommandList();

    auto const rtvHandle = rtvHeap.GetDescriptorHandleCPU(frameIndex);
    auto const dsvHandle = dsvHeap.GetDescriptorHandleCPU(frameIndex);

    draw2dGroup.commandList->OMSetRenderTargets(1, &rtvHandle, FALSE, &dsvHandle);
    draw2dGroup.commandList->ClearRenderTargetView(rtvHandle, LETTERBOX_COLOR.data(), 0, nullptr);
    draw2dGroup.commandList->ClearDepthStencilView(dsvHandle, D3D12_CLEAR_FLAG_DEPTH, 1.0f, 0, 0, nullptr);

    if (postProcessingPipeline != nullptr) PopulatePostProcessingCommandList();

    draw2dViewport.Set(draw2dGroup.commandList);

    for (auto& [pipeline, priority] : draw2dPipelines)
    {
        PIXScopedEvent(draw2dGroup.commandList.Get(), PIX_COLOR_DEFAULT, pipeline.GetName());
        pipeline.PopulateCommandList(draw2dGroup.commandList);
    }

    PopulateScreenshotCommandList();

    barriers[0].Transition.StateBefore = D3D12_RESOURCE_STATE_RENDER_TARGET;
    barriers[0].Transition.StateAfter  = D3D12_RESOURCE_STATE_PRESENT;

    barriers[1].Transition.StateBefore = D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
    barriers[1].Transition.StateAfter  = D3D12_RESOURCE_STATE_RENDER_TARGET;

    barriers[2].Transition.StateBefore = D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
    barriers[2].Transition.StateAfter  = D3D12_RESOURCE_STATE_DEPTH_WRITE;

    draw2dGroup.commandList->ResourceBarrier(static_cast<UINT>(barriers.size()), barriers.data());
    draw2dGroup.Close();
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

void NativeClient::HandleScreenshot()
{
    if (!screenshotFunc.has_value()) return;
    auto* func = screenshotFunc.value();

    UINT const size = GetWidth() * GetHeight() * 4;
    auto const data = std::make_unique<std::byte[]>(size);

    TryDo(util::MapAndRead(screenshotBuffers[frameIndex], data.get(), size));

    func(data.get(), GetWidth(), GetHeight());
    screenshotFunc = std::nullopt;
}
