//  <copyright file="NativeClient.cpp" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#include "stdafx.h"

constexpr std::array<float, 4> NativeClient::CLEAR_COLOR     = {1.0f, 1.0f, 1.0f, 1.0f};
constexpr std::array<float, 4> NativeClient::LETTERBOX_COLOR = {0.0f, 0.0f, 0.0f, 1.0f};

UINT const   NativeClient::AGILITY_SDK_VERSION = 611;
LPCSTR const NativeClient::AGILITY_SDK_PATH    = ".\\D3D12\\";

NativeClient::NativeClient(Configuration const& configuration)
    : DXApp(configuration)
  , m_resolution(Resolution{configuration.width, configuration.height} * configuration.renderScale)
  , m_debugCallback(configuration.onDebug)
  , m_space(std::make_unique<Space>(*this))
#if defined(USE_NSIGHT_AFTERMATH)
  , m_gpuCrashTracker(
        m_markerMap,
        m_shaderDatabase,
        GpuCrashTracker::Description::Create(configuration.applicationName, configuration.applicationVersion))
#endif
{
    if (SupportPIX() && !PIXIsAttachedForGpuCapture()) PIXLoadLatestWinPixGpuCapturerLibrary();
}

ComPtr<ID3D12Device5> NativeClient::GetDevice() const { return m_device; }

ComPtr<D3D12MA::Allocator> NativeClient::GetAllocator() const { return m_allocator; }

void NativeClient::OnInit()
{
    LoadDevice();
    InitializeFences();

    m_space->PerformInitialSetupStepOne(m_commandQueue);

    SetupSizeDependentResources();
    SetupSpaceResolutionDependentResources();

    m_uploader = std::make_unique<Uploader>(*this, nullptr);

    LoadRasterPipeline();
}

void NativeClient::OnPostInit()
{
    if (!m_spaceInitialized) m_space = nullptr;

    m_uploader->ExecuteUploads(m_commandQueue);

    WaitForGPU();
    m_uploader = nullptr;
}

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
    if (SUCCEEDED(
        deviceFactory->GetConfigurationInterface(CLSID_D3D12DeviceRemovedExtendedData, IID_PPV_ARGS(&dredSettings))))
    {
        dredSettings->SetAutoBreadcrumbsEnablement(D3D12_DRED_ENABLEMENT_FORCED_ON);
        dredSettings->SetPageFaultEnablement(D3D12_DRED_ENABLEMENT_FORCED_ON);
        dredSettings->SetBreadcrumbContextEnablement(D3D12_DRED_ENABLEMENT_FORCED_ON);
    }
#endif

    ComPtr<IDXGIAdapter1> const hardwareAdapter = GetHardwareAdapter(dxgiFactory, deviceFactory);

#if defined(USE_NSIGHT_AFTERMATH)
    if (!SupportPIX()) m_gpuCrashTracker.Initialize();
#endif

    TryDo(deviceFactory->CreateDevice(hardwareAdapter.Get(), D3D_FEATURE_LEVEL_12_2, IID_PPV_ARGS(&m_device)));
    NAME_D3D12_OBJECT(m_device);

#if defined(USE_NSIGHT_AFTERMATH)
    if (!SupportPIX())
    {
        constexpr uint32_t aftermathFlags = GFSDK_Aftermath_FeatureFlags_EnableMarkers |
        GFSDK_Aftermath_FeatureFlags_EnableResourceTracking | GFSDK_Aftermath_FeatureFlags_CallStackCapturing |
        GFSDK_Aftermath_FeatureFlags_GenerateShaderDebugInfo;

        AFTERMATH_CHECK_ERROR(
        GFSDK_Aftermath_DX12_Initialize( GFSDK_Aftermath_Version_API, aftermathFlags, m_device.Get()));
    }
#endif

#if defined(NATIVE_DEBUG)
    auto                             callback = [](
        D3D12_MESSAGE_CATEGORY const category,
        D3D12_MESSAGE_SEVERITY const severity,
        D3D12_MESSAGE_ID const       id,
        LPCSTR const                 description,
        void*                        context) -> void
    {
        auto const self = static_cast<NativeClient*>(context);

        Win32Application::EnterErrorMode();
        self->m_debugCallback(category, severity, id, description, nullptr);
        Win32Application::ExitErrorMode();
    };

    HRESULT const infoQueueResult = m_device->QueryInterface(IID_PPV_ARGS(&m_infoQueue));
    if (SUCCEEDED(infoQueueResult))
    {
        TryDo(m_device->QueryInterface(IID_PPV_ARGS(&m_infoQueue)));
        TryDo(
            m_infoQueue->RegisterMessageCallback(callback, D3D12_MESSAGE_CALLBACK_FLAG_NONE, this, &m_callbackCookie));

        TryDo(m_infoQueue->AddApplicationMessage(D3D12_MESSAGE_SEVERITY_MESSAGE, "Installed debug callback"));

        if (PIXIsAttachedForGpuCapture() && !SupportPIX())
            TryDo(
                m_infoQueue->AddApplicationMessage(
                    D3D12_MESSAGE_SEVERITY_WARNING,
                    "PIX detected, consider using the --pix command line argument"));
    }
    else
        m_debugCallback(
            D3D12_MESSAGE_CATEGORY_APPLICATION_DEFINED,
            D3D12_MESSAGE_SEVERITY_WARNING,
            D3D12_MESSAGE_ID_UNKNOWN,
            "Failed to install debug callback",
            nullptr);

#endif

    D3D12MA::ALLOCATOR_DESC allocatorDesc = {};
    allocatorDesc.pDevice                 = m_device.Get();
    allocatorDesc.pAdapter                = hardwareAdapter.Get();

    TryDo(CreateAllocator(&allocatorDesc, &m_allocator));

    CheckRaytracingSupport();

    D3D12_COMMAND_QUEUE_DESC queueDesc = {};
    queueDesc.Flags                    = D3D12_COMMAND_QUEUE_FLAG_NONE;
    queueDesc.Type                     = D3D12_COMMAND_LIST_TYPE_DIRECT;

    TryDo(m_device->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&m_commandQueue)));
    NAME_D3D12_OBJECT(m_commandQueue);

    DXGI_SWAP_CHAIN_DESC1 swapChainDesc = {};
    swapChainDesc.BufferCount           = FRAME_COUNT;
    swapChainDesc.Width                 = GetWidth();
    swapChainDesc.Height                = GetHeight();
    swapChainDesc.Format                = DXGI_FORMAT_B8G8R8A8_UNORM;
    swapChainDesc.BufferUsage           = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    swapChainDesc.SwapEffect            = DXGI_SWAP_EFFECT_FLIP_DISCARD;
    swapChainDesc.SampleDesc.Count      = 1;

    swapChainDesc.Flags = IsTearingSupportEnabled() ? DXGI_SWAP_CHAIN_FLAG_ALLOW_TEARING : 0;

    ComPtr<IDXGISwapChain1> swapChain;
    TryDo(
        dxgiFactory->CreateSwapChainForHwnd(
            m_commandQueue.Get(),
            Win32Application::GetHwnd(),
            &swapChainDesc,
            nullptr,
            nullptr,
            &swapChain));

    TryDo(dxgiFactory->MakeWindowAssociation(Win32Application::GetHwnd(), DXGI_MWA_NO_ALT_ENTER));

    TryDo(swapChain.As(&m_swapChain));
    m_frameIndex = m_swapChain->GetCurrentBackBufferIndex();

    m_rtvHeap.Create(m_device, FRAME_COUNT + 1, D3D12_DESCRIPTOR_HEAP_TYPE_RTV, false);
    NAME_D3D12_OBJECT(m_rtvHeap);

    m_dsvHeap.Create(m_device, FRAME_COUNT + 1, D3D12_DESCRIPTOR_HEAP_TYPE_DSV, false);
    NAME_D3D12_OBJECT(m_dsvHeap);
}

void NativeClient::InitializeFences()
{
    TryDo(m_device->CreateFence(m_fenceValues[m_frameIndex], D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&m_fence)));
    NAME_D3D12_OBJECT(m_fence);

    m_fenceValues[m_frameIndex]++;

    m_fenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
    if (m_fenceEvent == nullptr) TryDo(HRESULT_FROM_WIN32(GetLastError()));
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
    m_postVertexBuffer              = util::AllocateBuffer(
        *this,
        vertexBufferSize,
        D3D12_RESOURCE_FLAG_NONE,
        D3D12_RESOURCE_STATE_COMMON,
        D3D12_HEAP_TYPE_DEFAULT);
    NAME_D3D12_OBJECT(m_postVertexBuffer);

    m_uploader->UploadBuffer(
        static_cast<std::byte const*>(static_cast<void const*>(quadVertices.data())),
        vertexBufferSize,
        m_postVertexBuffer);

    m_postVertexBufferView.BufferLocation = m_postVertexBuffer.GetGPUVirtualAddress();
    m_postVertexBufferView.StrideInBytes  = sizeof(PostVertex);
    m_postVertexBufferView.SizeInBytes    = vertexBufferSize;

    INITIALIZE_COMMAND_ALLOCATOR_GROUP(*this, &m_uploadGroup, D3D12_COMMAND_LIST_TYPE_DIRECT);
    INITIALIZE_COMMAND_ALLOCATOR_GROUP(*this, &m_2dGroup, D3D12_COMMAND_LIST_TYPE_DIRECT);
}

void NativeClient::CreateFinalDepthBuffers()
{
    m_finalDepthStencilBuffersInitialized = false;

    D3D12_RESOURCE_DESC depthResourceDesc = CD3DX12_RESOURCE_DESC::Tex2D(
        DXGI_FORMAT_D32_FLOAT,
        GetWidth(),
        GetHeight(),
        1,
        1);
    depthResourceDesc.Flags |= D3D12_RESOURCE_FLAG_ALLOW_DEPTH_STENCIL;

    CD3DX12_CLEAR_VALUE const depthOptimizedClearValue(DXGI_FORMAT_D32_FLOAT, 1.0f, 0);

    for (UINT frameIndex = 0; frameIndex < FRAME_COUNT; frameIndex++)
    {
        m_finalDepthStencilBuffers[frameIndex] = util::AllocateResource<ID3D12Resource>(
            *this,
            depthResourceDesc,
            D3D12_HEAP_TYPE_DEFAULT,
            D3D12_RESOURCE_STATE_DEPTH_WRITE,
            &depthOptimizedClearValue);
        NAME_D3D12_OBJECT_INDEXED(m_finalDepthStencilBuffers, frameIndex);
    }

    D3D12_DEPTH_STENCIL_VIEW_DESC dsvDesc = {};
    dsvDesc.Format                        = DXGI_FORMAT_D32_FLOAT;
    dsvDesc.ViewDimension                 = D3D12_DSV_DIMENSION_TEXTURE2D;
    dsvDesc.Flags                         = D3D12_DSV_FLAG_NONE;

    for (UINT frameIndex = 0; frameIndex < FRAME_COUNT; frameIndex++)
        m_device->CreateDepthStencilView(
            m_finalDepthStencilBuffers[frameIndex].Get(),
            &dsvDesc,
            m_dsvHeap.GetDescriptorHandleCPU(frameIndex));
}

void NativeClient::EnsureValidDepthBuffers(ComPtr<ID3D12GraphicsCommandList4> const commandList)
{
    if (!m_finalDepthStencilBuffersInitialized)
    {
        for (auto const& buffer : m_finalDepthStencilBuffers) commandList->DiscardResource(buffer.Get(), nullptr);

        m_finalDepthStencilBuffersInitialized = true;
    }

    if (!m_intermediateDepthStencilBufferInitialized)
    {
        commandList->DiscardResource(m_intermediateDepthStencilBuffer.Get(), nullptr);
        m_intermediateDepthStencilBufferInitialized = true;
    }
}

void NativeClient::CreateScreenShotBuffers()
{
    m_screenshotBuffersInitialized = false;

    for (UINT frameIndex = 0; frameIndex < FRAME_COUNT; frameIndex++)
    {
        UINT64 const        size = GetRequiredIntermediateSize(m_finalRenderTargets[frameIndex].Get(), 0, 1);
        D3D12_RESOURCE_DESC desc = CD3DX12_RESOURCE_DESC::Buffer(size);

        m_screenshotBuffers[frameIndex] = util::AllocateResource<ID3D12Resource>(
            *this,
            desc,
            D3D12_HEAP_TYPE_READBACK,
            D3D12_RESOURCE_STATE_COPY_DEST,
            nullptr);
        NAME_D3D12_OBJECT_INDEXED(m_screenshotBuffers, frameIndex);
    }
}

void NativeClient::EnsureValidScreenShotBuffer(ComPtr<ID3D12GraphicsCommandList4> commandList)
{
    m_screenshotBuffersInitialized = true;

    for (auto const& buffer : m_screenshotBuffers) commandList->DiscardResource(buffer.Get(), nullptr);
}

void NativeClient::SetupSizeDependentResources()
{
    UpdatePostViewAndScissor();

    m_draw2dViewport.viewport.Width  = static_cast<float>(GetWidth());
    m_draw2dViewport.viewport.Height = static_cast<float>(GetHeight());

    m_draw2dViewport.scissorRect.right  = static_cast<LONG>(GetWidth());
    m_draw2dViewport.scissorRect.bottom = static_cast<LONG>(GetHeight());

    for (UINT n = 0; n < FRAME_COUNT; n++)
    {
        TryDo(m_swapChain->GetBuffer(n, IID_PPV_ARGS(&m_finalRenderTargets[n])));
        m_device->CreateRenderTargetView(m_finalRenderTargets[n].Get(), nullptr, m_rtvHeap.GetDescriptorHandleCPU(n));

        NAME_D3D12_OBJECT_INDEXED(m_finalRenderTargets, n);
    }

    CreateFinalDepthBuffers();
    CreateScreenShotBuffers();
}

void NativeClient::SetupSpaceResolutionDependentResources()
{
    m_spaceViewport.viewport.Width  = static_cast<float>(m_resolution.width);
    m_spaceViewport.viewport.Height = static_cast<float>(m_resolution.height);

    m_spaceViewport.scissorRect.right  = static_cast<LONG>(m_resolution.width);
    m_spaceViewport.scissorRect.bottom = static_cast<LONG>(m_resolution.height);

    UpdatePostViewAndScissor();

    D3D12_RESOURCE_DESC const   swapChainDesc = m_finalRenderTargets[m_frameIndex]->GetDesc();
    CD3DX12_CLEAR_VALUE const   clearValue(swapChainDesc.Format, CLEAR_COLOR.data());
    CD3DX12_RESOURCE_DESC const renderTargetDesc = CD3DX12_RESOURCE_DESC::Tex2D(
        swapChainDesc.Format,
        m_resolution.width,
        m_resolution.height,
        1,
        1,
        swapChainDesc.SampleDesc.Count,
        swapChainDesc.SampleDesc.Quality,
        D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET,
        D3D12_TEXTURE_LAYOUT_UNKNOWN,
        0u);

    m_intermediateRenderTarget = util::AllocateResource<ID3D12Resource>(
        *this,
        renderTargetDesc,
        D3D12_HEAP_TYPE_DEFAULT,
        D3D12_RESOURCE_STATE_RENDER_TARGET,
        &clearValue);
    NAME_D3D12_OBJECT(m_intermediateRenderTarget);

    m_intermediateRenderTargetInitialized = false;

    m_device->CreateRenderTargetView(
        m_intermediateRenderTarget.Get(),
        nullptr,
        m_rtvHeap.GetDescriptorHandleCPU(FRAME_COUNT));

    if (m_space) m_space->PerformResolutionDependentSetup(m_resolution);

    D3D12_RESOURCE_DESC depthResourceDesc = CD3DX12_RESOURCE_DESC::Tex2D(
            DXGI_FORMAT_D32_FLOAT,
            m_resolution.width,
            m_resolution.height,
            1,
            1);
    depthResourceDesc.Flags |= D3D12_RESOURCE_FLAG_ALLOW_DEPTH_STENCIL;

    CD3DX12_CLEAR_VALUE const depthOptimizedClearValue(DXGI_FORMAT_D32_FLOAT, 1.0f, 0);

    m_intermediateDepthStencilBuffer = util::AllocateResource<ID3D12Resource>(
        *this,
        depthResourceDesc,
        D3D12_HEAP_TYPE_DEFAULT,
        D3D12_RESOURCE_STATE_DEPTH_WRITE,
        &depthOptimizedClearValue);
    NAME_D3D12_OBJECT(m_intermediateDepthStencilBuffer);

    D3D12_DEPTH_STENCIL_VIEW_DESC dsvDesc = {};
    dsvDesc.Format                        = DXGI_FORMAT_D32_FLOAT;
    dsvDesc.ViewDimension                 = D3D12_DSV_DIMENSION_TEXTURE2D;
    dsvDesc.Flags                         = D3D12_DSV_FLAG_NONE;

    m_device->CreateDepthStencilView(
        m_intermediateDepthStencilBuffer.Get(),
        &dsvDesc,
        m_dsvHeap.GetDescriptorHandleCPU(FRAME_COUNT));

    if (m_postProcessingPipeline != nullptr)
        m_postProcessingPipeline->CreateShaderResourceView(
            m_postProcessingPipeline->GetBindings().PostProcessing().input,
            0,
            {m_intermediateRenderTarget});
}

void NativeClient::EnsureValidIntermediateRenderTarget(ComPtr<ID3D12GraphicsCommandList4> const commandList)
{
    if (m_intermediateRenderTargetInitialized) return;
    m_intermediateRenderTargetInitialized = true;

    commandList->DiscardResource(m_intermediateRenderTarget.Get(), nullptr);
}

void NativeClient::OnUpdate(double const delta) { if (m_space) m_space->Update(delta); }

void NativeClient::OnPreRender()
{
    if (!m_windowVisible) return;

    m_uploadGroup.Reset(m_frameIndex);
    m_uploader = std::make_unique<Uploader>(*this, m_uploadGroup.commandList);
}

void NativeClient::OnRender(double const)
{
    if (!m_windowVisible) return;
    
    {
        PIXScopedEvent(m_commandQueue.Get(), PIX_COLOR_DEFAULT, L"Render");

        m_uploadGroup.Close();

        PopulateCommandLists();
        
        std::vector<ID3D12CommandList*> commandLists;
        commandLists.reserve(3);

        commandLists.push_back(m_uploadGroup.commandList.Get());
        if (m_space) commandLists.push_back(m_space->GetCommandList().Get());
        commandLists.push_back(m_2dGroup.commandList.Get());

        m_commandQueue->ExecuteCommandLists(static_cast<UINT>(commandLists.size()), commandLists.data());
    }

    UINT const syncInterval = IsTearingSupportEnabled() && m_windowedMode ? 0 : 1;
    UINT const presentFlags = IsTearingSupportEnabled() && m_windowedMode ? DXGI_PRESENT_ALLOW_TEARING : 0;
    constexpr DXGI_PRESENT_PARAMETERS presentParameters = {};
    HRESULT const present = m_swapChain->Present1(syncInterval, presentFlags, &presentParameters);

#if defined(USE_NSIGHT_AFTERMATH)
    if (FAILED(present))
    {
        if (SupportPIX())
            throw std::runtime_error("Present failed");
        
        constexpr auto tdrTerminationTimeout = std::chrono::seconds(3);
        auto const     tStart                = std::chrono::steady_clock::now();
        auto           tElapsed              = std::chrono::milliseconds::zero();

        GFSDK_Aftermath_CrashDump_Status status = GFSDK_Aftermath_CrashDump_Status_Unknown;
        AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GetCrashDumpStatus(&status));

        while (status != GFSDK_Aftermath_CrashDump_Status_CollectingDataFailed && status !=
            GFSDK_Aftermath_CrashDump_Status_Finished && tElapsed < tdrTerminationTimeout)
        {
            std::this_thread::sleep_for(std::chrono::milliseconds(50));
            AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_GetCrashDumpStatus(&status));

            auto tEnd = std::chrono::steady_clock::now();
            tElapsed  = std::chrono::duration_cast<std::chrono::milliseconds>(tEnd - tStart);
        }

        if (status != GFSDK_Aftermath_CrashDump_Status_Finished)
        {
            std::stringstream errMsg;
            errMsg << "Unexpected crash dump status: " << status;
            MessageBoxA(nullptr, errMsg.str().c_str(), "Aftermath Error", MB_OK);
        }

        throw std::runtime_error("Present failed");
    }
#else
    TryDo(present);
#endif

    WaitForGPU();

    if (m_space) m_space->CleanupRender();

    HandleScreenshot();

    MoveToNextFrame();
}

void NativeClient::OnDestroy()
{
    WaitForGPU();
    CloseHandle(m_fenceEvent);
}

void NativeClient::OnSizeChanged(UINT const width, UINT const height, bool const minimized)
{
    if ((width != GetWidth() || height != GetHeight()) && !minimized)
    {
        WaitForGPU();

        for (UINT n = 0; n < FRAME_COUNT; n++) // NOLINT(modernize-loop-convert)
        {
            m_finalRenderTargets[n].Reset();
            m_fenceValues[n] = m_fenceValues[m_frameIndex];
        }

        DXGI_SWAP_CHAIN_DESC desc = {};
        TryDo(m_swapChain->GetDesc(&desc));
        TryDo(m_swapChain->ResizeBuffers(FRAME_COUNT, width, height, desc.BufferDesc.Format, desc.Flags));

        BOOL fullscreenState;
        TryDo(m_swapChain->GetFullscreenState(&fullscreenState, nullptr));
        m_windowedMode = !static_cast<bool>(fullscreenState);

        m_frameIndex = m_swapChain->GetCurrentBackBufferIndex();

        UpdateForSizeChange(width, height);
        SetupSizeDependentResources();

        if (Resolution const newResolution = Resolution{width, height} * GetRenderScale();
            newResolution != m_resolution)
        {
            m_resolution = newResolution;
            SetupSpaceResolutionDependentResources();
        }
    }

    m_windowVisible = !minimized;
}

void NativeClient::OnWindowMoved(int, int)
{
    // Nothing to do, here for symmetry with other message handlers.
}

void NativeClient::InitRaytracingPipeline(SpacePipelineDescription const& pipeline)
{
    if (m_space->PerformInitialSetupStepTwo(pipeline)) m_spaceInitialized = true;
    else m_space                                                          = nullptr;
}

// ReSharper disable once CppMemberFunctionMayBeStatic
void NativeClient::ToggleFullscreen() const { Win32Application::ToggleFullscreenWindow(m_swapChain.Get()); }

void NativeClient::TakeScreenshot(ScreenshotFunc func)
{
    if (m_screenshotFunc.has_value()) return;
    m_screenshotFunc = std::move(func);
}

Texture* NativeClient::LoadTexture(std::byte** data, TextureDescription const& description) const
{
    Require(m_uploader != nullptr);

    return Texture::Create(*m_uploader, data, description);
}

Space* NativeClient::GetSpace() const { return m_space.get(); }

void NativeClient::AddRasterPipeline(std::unique_ptr<RasterPipeline> pipeline)
{
    m_rasterPipelines.push_back(std::move(pipeline));
}

void NativeClient::SetPostProcessingPipeline(RasterPipeline* pipeline)
{
    m_postProcessingPipeline = pipeline;
    m_postProcessingPipeline->CreateShaderResourceView(
        m_postProcessingPipeline->GetBindings().PostProcessing().input,
        0,
        {m_intermediateRenderTarget});
}

UINT NativeClient::AddDraw2DPipeline(RasterPipeline* pipeline, INT const priority, draw2d::Callback const callback)
{
    // INT_MIN and INT_MAX should always place the pipeline at the front and back of the list, respectively.
    // Thus, all entries in the list should be in the range (INT_MIN, INT_MAX) - both exclusive.
    auto clampedPriority = static_cast<UINT>(std::clamp(priority, INT_MIN + 1, INT_MAX - 1));

    decltype(m_draw2dPipelines)::iterator iterator;

    UINT const id = m_nextDraw2dPipelineID;

    if (m_draw2dPipelines.empty() || priority < m_draw2dPipelines.front().priority)
    {
        m_draw2dPipelines.emplace_front(draw2d::Pipeline{*this, pipeline, id, callback}, clampedPriority);
        iterator = m_draw2dPipelines.begin();
    }
    else if (priority > m_draw2dPipelines.back().priority)
    {
        m_draw2dPipelines.emplace_back(draw2d::Pipeline{*this, pipeline, id, callback}, clampedPriority);
        iterator = std::prev(m_draw2dPipelines.end());
    }
    else
        for (auto it = m_draw2dPipelines.begin(); it != m_draw2dPipelines.end(); ++it)
            // Goal: insert after the first element with priority lower than the new one.
            if (priority > it->priority)
            {
                iterator = m_draw2dPipelines.emplace(
                    --it,
                    draw2d::Pipeline(*this, pipeline, id, callback),
                    clampedPriority);
                break;
            }

    m_draw2dPipelineIDs[m_nextDraw2dPipelineID] = iterator;
    m_nextDraw2dPipelineID++;

    return id;
}

void NativeClient::RemoveDraw2DPipeline(UINT const id)
{
    auto const iterator = m_draw2dPipelineIDs[id];

    m_draw2dPipelines.erase(iterator);
    m_draw2dPipelineIDs.erase(id);
}

NativeClient::ObjectHandle NativeClient::StoreObject(std::unique_ptr<Object> object)
{
    return m_objects.Push(std::move(object));
}

void NativeClient::DeleteObject(ObjectHandle const handle) { m_objects.Pop(handle); }

void NativeClient::WaitForGPU()
{
    TryDo(m_commandQueue->Signal(m_fence.Get(), m_fenceValues[m_frameIndex]));

    TryDo(m_fence->SetEventOnCompletion(m_fenceValues[m_frameIndex], m_fenceEvent));
    WaitForSingleObjectEx(m_fenceEvent, INFINITE, FALSE);

    m_fenceValues[m_frameIndex]++;
}

void NativeClient::MoveToNextFrame()
{
    TryDo(m_commandQueue->Signal(m_fence.Get(), m_fenceValues[m_frameIndex]));

    UINT64 const currentFenceValue = m_fenceValues[static_cast<UINT64>(m_frameIndex)];
    m_frameIndex                   = m_swapChain->GetCurrentBackBufferIndex();

    if (m_fence->GetCompletedValue() < m_fenceValues[m_frameIndex])
    {
        TryDo(m_fence->SetEventOnCompletion(m_fenceValues[m_frameIndex], m_fenceEvent));
        WaitForSingleObjectEx(m_fenceEvent, INFINITE, FALSE);
    }

    m_fenceValues[m_frameIndex] = currentFenceValue + 1;
}

std::wstring NativeClient::GetDRED() const
{
    ComPtr<ID3D12DeviceRemovedExtendedData2> dred;
    TryDo(m_device->QueryInterface(IID_PPV_ARGS(&dred)));

    D3D12_DRED_AUTO_BREADCRUMBS_OUTPUT1 dredAutoBreadcrumbsOutput = {};
    TryDo(dred->GetAutoBreadcrumbsOutput1(&dredAutoBreadcrumbsOutput));

    D3D12_DRED_PAGE_FAULT_OUTPUT2 dredPageFaultOutput = {};
    TryDo(dred->GetPageFaultAllocationOutput2(&dredPageFaultOutput));

    return util::FormatDRED(dredAutoBreadcrumbsOutput, dredPageFaultOutput, dred->GetDeviceState());
}
#if defined(USE_NSIGHT_AFTERMATH)
void NativeClient::SetupCommandListForAftermath(ComPtr<ID3D12GraphicsCommandList> const& commandList) const
{
    if (SupportPIX())
        return;
    
    GFSDK_Aftermath_ContextHandle contextHandle;
    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_DX12_CreateContextHandle(commandList.Get(), &contextHandle));
}

void NativeClient::SetupShaderForAftermath(ComPtr<IDxcResult> const& result)
{
    if (SupportPIX())
        return;
    
    ComPtr<IDxcBlob> objectBlob;
    TryDo(result->GetOutput(DXC_OUT_OBJECT, IID_PPV_ARGS(&objectBlob), nullptr));
    std::vector<uint8_t> binary(objectBlob->GetBufferSize());
    std::memcpy(binary.data(), objectBlob->GetBufferPointer(), objectBlob->GetBufferSize());

    ComPtr<IDxcBlob> pdbBlob;
    TryDo(result->GetOutput(DXC_OUT_PDB, IID_PPV_ARGS(&pdbBlob), nullptr));
    std::vector<uint8_t> pdb(pdbBlob->GetBufferSize());
    std::memcpy(pdb.data(), pdbBlob->GetBufferPointer(), pdbBlob->GetBufferSize());

    m_shaderDatabase.AddShader(std::move(binary), std::move(pdb));
}
#endif

void NativeClient::CheckRaytracingSupport() const
{
    D3D12_FEATURE_DATA_D3D12_OPTIONS5 options5 = {};
    TryDo(m_device->CheckFeatureSupport(D3D12_FEATURE_D3D12_OPTIONS5, &options5, sizeof(options5)));

    if (options5.RaytracingTier < D3D12_RAYTRACING_TIER_1_1)
        throw NativeException("Raytracing not supported on device.");
}

void NativeClient::PopulateSpaceCommandList() const
{
    Require(m_space != nullptr);

    D3D12_CPU_DESCRIPTOR_HANDLE rtvHandle = m_rtvHeap.GetDescriptorHandleCPU(FRAME_COUNT);
    D3D12_CPU_DESCRIPTOR_HANDLE dsvHandle = m_dsvHeap.GetDescriptorHandleCPU(FRAME_COUNT);

    m_space->Reset(m_frameIndex);
    m_space->Render(
        m_intermediateRenderTarget,
        m_intermediateDepthStencilBuffer,
        {.rtv = &rtvHandle, .dsv = &dsvHandle, .viewport = &m_spaceViewport});
}

void NativeClient::PopulatePostProcessingCommandList() const
{
    if (m_space == nullptr) return; // Nothing to post-process.

    PIXScopedEvent(m_2dGroup.commandList.Get(), PIX_COLOR_DEFAULT, m_postProcessingPipeline->GetName());

    m_postProcessingPipeline->SetPipeline(m_2dGroup.commandList);
    m_postProcessingPipeline->BindResources(m_2dGroup.commandList);

    m_postViewport.Set(m_2dGroup.commandList);

    m_2dGroup.commandList->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP);
    m_2dGroup.commandList->IASetVertexBuffers(0, 1, &m_postVertexBufferView);
    m_2dGroup.commandList->DrawInstanced(4, 1, 0, 0);
}

void NativeClient::PopulateScreenshotCommandList() const
{
    if (!m_screenshotFunc.has_value()) return;

    PIXScopedEvent(m_2dGroup.commandList.Get(), PIX_COLOR_DEFAULT, L"Screenshot");

    D3D12_RESOURCE_BARRIER const entry = CD3DX12_RESOURCE_BARRIER::Transition(
        m_finalRenderTargets[m_frameIndex].Get(),
        D3D12_RESOURCE_STATE_RENDER_TARGET,
        D3D12_RESOURCE_STATE_COPY_SOURCE);
    m_2dGroup.commandList->ResourceBarrier(1, &entry);

    D3D12_PLACED_SUBRESOURCE_FOOTPRINT footprint = {};
    footprint.Footprint.Format                   = DXGI_FORMAT_B8G8R8A8_UNORM;
    footprint.Footprint.Width                    = GetWidth();
    footprint.Footprint.Height                   = GetHeight();
    footprint.Footprint.Depth                    = 1;
    footprint.Footprint.RowPitch                 = GetWidth() * 4;

    auto const dst = CD3DX12_TEXTURE_COPY_LOCATION(m_screenshotBuffers[m_frameIndex].Get(), footprint);
    auto const src = CD3DX12_TEXTURE_COPY_LOCATION(m_finalRenderTargets[m_frameIndex].Get(), 0);
    m_2dGroup.commandList->CopyTextureRegion(&dst, 0, 0, 0, &src, nullptr);

    D3D12_RESOURCE_BARRIER const exit = CD3DX12_RESOURCE_BARRIER::Transition(
        m_finalRenderTargets[m_frameIndex].Get(),
        D3D12_RESOURCE_STATE_COPY_SOURCE,
        D3D12_RESOURCE_STATE_RENDER_TARGET);
    m_2dGroup.commandList->ResourceBarrier(1, &exit);
}

void NativeClient::PopulateCommandLists()
{
    m_2dGroup.Reset(m_frameIndex);

    EnsureValidDepthBuffers(m_2dGroup.commandList);
    EnsureValidIntermediateRenderTarget(m_2dGroup.commandList);

    std::array<D3D12_RESOURCE_BARRIER, 2> barriers = {
        CD3DX12_RESOURCE_BARRIER::Transition(
            m_finalRenderTargets[m_frameIndex].Get(),
            D3D12_RESOURCE_STATE_PRESENT,
            D3D12_RESOURCE_STATE_RENDER_TARGET),
        CD3DX12_RESOURCE_BARRIER::Transition(
            m_intermediateRenderTarget.Get(),
            D3D12_RESOURCE_STATE_RENDER_TARGET,
            D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE)
    };

    m_2dGroup.commandList->ResourceBarrier(static_cast<UINT>(barriers.size()), barriers.data());

    if (m_space) PopulateSpaceCommandList();

    auto const rtvHandle = m_rtvHeap.GetDescriptorHandleCPU(m_frameIndex);
    auto const dsvHandle = m_dsvHeap.GetDescriptorHandleCPU(m_frameIndex);

    m_2dGroup.commandList->OMSetRenderTargets(1, &rtvHandle, FALSE, &dsvHandle);
    m_2dGroup.commandList->ClearRenderTargetView(rtvHandle, LETTERBOX_COLOR.data(), 0, nullptr);
    m_2dGroup.commandList->ClearDepthStencilView(dsvHandle, D3D12_CLEAR_FLAG_DEPTH, 1.0f, 0, 0, nullptr);

    if (m_postProcessingPipeline != nullptr) PopulatePostProcessingCommandList();

    m_draw2dViewport.Set(m_2dGroup.commandList);

    for (auto& [pipeline, priority] : m_draw2dPipelines)
    {
        PIXScopedEvent(m_2dGroup.commandList.Get(), PIX_COLOR_DEFAULT, pipeline.GetName());
        pipeline.PopulateCommandList(m_2dGroup.commandList);
    }

    PopulateScreenshotCommandList();

    barriers[0].Transition.StateBefore = D3D12_RESOURCE_STATE_RENDER_TARGET;
    barriers[0].Transition.StateAfter  = D3D12_RESOURCE_STATE_PRESENT;
    barriers[1].Transition.StateBefore = D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
    barriers[1].Transition.StateAfter  = D3D12_RESOURCE_STATE_RENDER_TARGET;

    m_2dGroup.commandList->ResourceBarrier(static_cast<UINT>(barriers.size()), barriers.data());
    m_2dGroup.Close();
}

void NativeClient::UpdatePostViewAndScissor()
{
    auto const width  = static_cast<float>(GetWidth());
    auto const height = static_cast<float>(GetHeight());

    float const viewWidthRatio  = static_cast<float>(m_resolution.width) / width;
    float const viewHeightRatio = static_cast<float>(m_resolution.height) / height;

    float x = 1.0f;
    float y = 1.0f;

    if (viewWidthRatio < viewHeightRatio) x = viewWidthRatio / viewHeightRatio;
    else y                                  = viewHeightRatio / viewWidthRatio;

    m_postViewport.viewport.TopLeftX = width * (1.0f - x) / 2.0f;
    m_postViewport.viewport.TopLeftY = height * (1.0f - y) / 2.0f;
    m_postViewport.viewport.Width    = x * width;
    m_postViewport.viewport.Height   = y * height;

    m_postViewport.scissorRect.left  = static_cast<LONG>(m_postViewport.viewport.TopLeftX);
    m_postViewport.scissorRect.right = static_cast<LONG>(m_postViewport.viewport.TopLeftX + m_postViewport.viewport.
        Width);
    m_postViewport.scissorRect.top    = static_cast<LONG>(m_postViewport.viewport.TopLeftY);
    m_postViewport.scissorRect.bottom = static_cast<LONG>(m_postViewport.viewport.TopLeftY + m_postViewport.viewport.
        Height);
}

void NativeClient::HandleScreenshot()
{
    if (!m_screenshotFunc.has_value()) return;
    auto* func = m_screenshotFunc.value();

    UINT const size = GetWidth() * GetHeight() * 4;
    auto const data = std::make_unique<std::byte[]>(size);

    TryDo(util::MapAndRead(m_screenshotBuffers[m_frameIndex], data.get(), size));

    func(data.get(), GetWidth(), GetHeight());
    m_screenshotFunc = std::nullopt;
}
