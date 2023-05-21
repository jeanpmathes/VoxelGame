//  <copyright file="NativeClient.cpp" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#include "stdafx.h"

constexpr float NativeClient::CLEAR_COLOR[4] = {1.0f, 1.0f, 1.0f, 1.0f};
constexpr float NativeClient::LETTERBOX_COLOR[4] = {0.0f, 0.0f, 0.0f, 1.0f};

NativeClient::NativeClient(const Configuration configuration) :
    DXApp(configuration),
    m_resolution{configuration.width, configuration.height},
    m_debugCallback(configuration.onDebug),
    m_spaceViewport(0.0f, 0.0f, 0.0f, 0.0f),
    m_spaceScissorRect(0, 0, 0, 0),
    m_space(std::make_unique<Space>(*this)),
    m_postViewport(0.0f, 0.0f, 0.0f, 0.0f),
    m_postScissorRect(0, 0, 0, 0),
    m_draw2DViewport(0.0f, 0.0f, 0.0f, 0.0f),
    m_draw2DScissorRect(0, 0, 0, 0),
    m_rtvDescriptorSize(0),
    m_srvDescriptorSize(0),
    m_frameIndex(0),
    m_fenceValues{},
    m_windowVisible(true),
    m_windowedMode(true)
{
}

ComPtr<ID3D12Device5> NativeClient::GetDevice() const
{
    return m_device;
}

ComPtr<D3D12MA::Allocator> NativeClient::GetAllocator() const
{
    return m_allocator;
}

UINT NativeClient::GetRtvHeapIncrement() const
{
    return m_rtvDescriptorSize;
}

UINT NativeClient::GetCbvSrvUavHeapIncrement() const
{
    return m_srvDescriptorSize;
}

void NativeClient::OnInit()
{
    LoadDevice();

    {
        TRY_DO(m_device->CreateFence(m_fenceValues[m_frameIndex], D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&m_fence)));
        NAME_D3D12_OBJECT(m_fence);
        
        m_fenceValues[m_frameIndex]++;

        m_fenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
        if (m_fenceEvent == nullptr)
        {
            TRY_DO(HRESULT_FROM_WIN32(GetLastError()));
        }
    }

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
    m_uploader.reset();
}

void NativeClient::LoadDevice()
{
    UINT dxgiFactoryFlags = 0;

#if defined(_DEBUG)
    {
        ComPtr<ID3D12Debug5> debugController;
        if (SUCCEEDED(D3D12GetDebugInterface(IID_PPV_ARGS(&debugController))))
        {
            debugController->EnableDebugLayer();
            debugController->SetEnableAutoName(TRUE);
            
            dxgiFactoryFlags |= DXGI_CREATE_FACTORY_DEBUG;
        }

        ComPtr<ID3D12DeviceRemovedExtendedDataSettings1> dredSettings;
        if (SUCCEEDED(D3D12GetDebugInterface(IID_PPV_ARGS(&dredSettings))))
        {
            dredSettings->SetAutoBreadcrumbsEnablement(D3D12_DRED_ENABLEMENT_FORCED_ON);
            dredSettings->SetPageFaultEnablement(D3D12_DRED_ENABLEMENT_FORCED_ON);
            dredSettings->SetBreadcrumbContextEnablement(D3D12_DRED_ENABLEMENT_FORCED_ON);
        }
    }
#endif

    ComPtr<IDXGIFactory4> factory;
    TRY_DO(CreateDXGIFactory2(dxgiFactoryFlags, IID_PPV_ARGS(&factory)));

    ComPtr<IDXGIAdapter1> hardwareAdapter;
    GetHardwareAdapter(factory.Get(), &hardwareAdapter);

    TRY_DO(D3D12CreateDevice(
        hardwareAdapter.Get(),
        D3D_FEATURE_LEVEL_12_2,
        IID_PPV_ARGS(&m_device)
    ));
    NAME_D3D12_OBJECT(m_device);

#if defined(_DEBUG)
    auto callback = [](D3D12_MESSAGE_CATEGORY category, D3D12_MESSAGE_SEVERITY severity, D3D12_MESSAGE_ID id,
                       LPCSTR description, void* context)
    {
        const auto* self = static_cast<NativeClient*>(context);

        std::wstring messageStore;

        if (id == D3D12_MESSAGE_ID_DEVICE_REMOVAL_PROCESS_AT_FAULT ||
            id == D3D12_MESSAGE_ID_DEVICE_REMOVAL_PROCESS_POSSIBLY_AT_FAULT ||
            id == D3D12_MESSAGE_ID_DEVICE_REMOVAL_PROCESS_NOT_AT_FAULT)
        {
            ComPtr<ID3D12DeviceRemovedExtendedData2> dred;
            TRY_DO(self->m_device->QueryInterface(IID_PPV_ARGS(&dred)));

            D3D12_DRED_AUTO_BREADCRUMBS_OUTPUT1 dredAutoBreadcrumbsOutput = {};
            TRY_DO(dred->GetAutoBreadcrumbsOutput1(&dredAutoBreadcrumbsOutput));

            D3D12_DRED_PAGE_FAULT_OUTPUT2 dredPageFaultOutput = {};
            TRY_DO(dred->GetPageFaultAllocationOutput2(&dredPageFaultOutput));
            
            messageStore = util::FormatDRED(dredAutoBreadcrumbsOutput, dredPageFaultOutput, dred->GetDeviceState());
        }

        void* newContext = messageStore.empty() ? nullptr : const_cast<wchar_t*>(messageStore.c_str());
        self->m_debugCallback(category, severity, id, description, newContext);
    };

    TRY_DO(m_device->QueryInterface(__uuidof(ID3D12InfoQueue1), &m_infoQueue));
    TRY_DO(m_infoQueue->RegisterMessageCallback(
        callback,
        D3D12_MESSAGE_CALLBACK_FLAG_NONE,
        this,
        &m_callbackCookie));
    TRY_DO(m_infoQueue->AddApplicationMessage(D3D12_MESSAGE_SEVERITY_MESSAGE, "Installed debug callback"));
#endif

    D3D12MA::ALLOCATOR_DESC allocatorDesc = {};
    allocatorDesc.pDevice = m_device.Get();
    allocatorDesc.pAdapter = hardwareAdapter.Get();

    TRY_DO(D3D12MA::CreateAllocator(&allocatorDesc, &m_allocator));

    CheckRaytracingSupport();

    D3D12_COMMAND_QUEUE_DESC queueDesc = {};
    queueDesc.Flags = D3D12_COMMAND_QUEUE_FLAG_NONE;
    queueDesc.Type = D3D12_COMMAND_LIST_TYPE_DIRECT;

    TRY_DO(m_device->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&m_commandQueue)));
    NAME_D3D12_OBJECT(m_commandQueue);

    DXGI_SWAP_CHAIN_DESC1 swapChainDesc = {};
    swapChainDesc.BufferCount = FRAME_COUNT;
    swapChainDesc.Width = m_width;
    swapChainDesc.Height = m_height;
    swapChainDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    swapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;
    swapChainDesc.SampleDesc.Count = 1;

    swapChainDesc.Flags = m_tearingSupport ? DXGI_SWAP_CHAIN_FLAG_ALLOW_TEARING : 0;

    ComPtr<IDXGISwapChain1> swapChain;
    TRY_DO(factory->CreateSwapChainForHwnd(
        m_commandQueue.Get(),
        Win32Application::GetHwnd(),
        &swapChainDesc,
        nullptr,
        nullptr,
        &swapChain
    ));

    TRY_DO(factory->MakeWindowAssociation(Win32Application::GetHwnd(), DXGI_MWA_NO_ALT_ENTER));

    TRY_DO(swapChain.As(&m_swapChain));
    m_frameIndex = m_swapChain->GetCurrentBackBufferIndex();

    {
        m_rtvHeap = CreateDescriptorHeap(m_device.Get(), FRAME_COUNT + 1,
                                         D3D12_DESCRIPTOR_HEAP_TYPE_RTV, false);

        NAME_D3D12_OBJECT(m_rtvHeap);

        m_rtvDescriptorSize = m_device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_RTV);
        m_srvDescriptorSize = m_device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
    }
}

void NativeClient::LoadRasterPipeline()
{
    constexpr PostVertex quadVertices[] =
    {
        {{-1.0f, -1.0f, 0.0f, 1.0f}, {0.0f, 0.0f}},
        {{-1.0f, 1.0f, 0.0f, 1.0f}, {0.0f, 1.0f}},
        {{1.0f, -1.0f, 0.0f, 1.0f}, {1.0f, 0.0f}},
        {{1.0f, 1.0f, 0.0f, 1.0f}, {1.0f, 1.0f}}
    };

    constexpr UINT vertexBufferSize = sizeof quadVertices;
    m_postVertexBuffer = util::AllocateBuffer(*this, vertexBufferSize, D3D12_RESOURCE_FLAG_NONE,
                                              D3D12_RESOURCE_STATE_COMMON, D3D12_HEAP_TYPE_DEFAULT);
    NAME_D3D12_OBJECT(m_postVertexBuffer);

    m_uploader->UploadBuffer(reinterpret_cast<const std::byte*>(&quadVertices), vertexBufferSize,
                             m_postVertexBuffer);

    m_postVertexBufferView.BufferLocation = m_postVertexBuffer.resource->GetGPUVirtualAddress();
    m_postVertexBufferView.StrideInBytes = sizeof(PostVertex);
    m_postVertexBufferView.SizeInBytes = vertexBufferSize;

    INITIALIZE_COMMAND_ALLOCATOR_GROUP(m_device, &m_uploadGroup, D3D12_COMMAND_LIST_TYPE_DIRECT);
    INITIALIZE_COMMAND_ALLOCATOR_GROUP(m_device, &m_2dGroup, D3D12_COMMAND_LIST_TYPE_DIRECT);
}

void NativeClient::CreateDepthBuffer()
{
    m_dsvHeap = CreateDescriptorHeap(m_device.Get(), 1, D3D12_DESCRIPTOR_HEAP_TYPE_DSV, false);
    NAME_D3D12_OBJECT(m_dsvHeap);

    const D3D12_HEAP_PROPERTIES depthHeapProperties = CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_DEFAULT);
    D3D12_RESOURCE_DESC depthResourceDesc =
        CD3DX12_RESOURCE_DESC::Tex2D(DXGI_FORMAT_D32_FLOAT, m_width, m_height, 1, 1);
    depthResourceDesc.Flags |= D3D12_RESOURCE_FLAG_ALLOW_DEPTH_STENCIL;
    const CD3DX12_CLEAR_VALUE depthOptimizedClearValue(DXGI_FORMAT_D32_FLOAT, 1.0f, 0);

    TRY_DO(
        m_device->CreateCommittedResource( &depthHeapProperties, D3D12_HEAP_FLAG_NONE, &depthResourceDesc,
            D3D12_RESOURCE_STATE_DEPTH_WRITE, &depthOptimizedClearValue, IID_PPV_ARGS(&m_depthStencilBuffer)));

    D3D12_DEPTH_STENCIL_VIEW_DESC dsvDesc = {};
    dsvDesc.Format = DXGI_FORMAT_D32_FLOAT;
    dsvDesc.ViewDimension = D3D12_DSV_DIMENSION_TEXTURE2D;
    dsvDesc.Flags = D3D12_DSV_FLAG_NONE;

    m_device->CreateDepthStencilView(m_depthStencilBuffer.Get(), &dsvDesc,
                                     m_dsvHeap->GetCPUDescriptorHandleForHeapStart());
    NAME_D3D12_OBJECT(m_depthStencilBuffer);
}

void NativeClient::SetupSizeDependentResources()
{
    UpdatePostViewAndScissor();

    {
        m_draw2DViewport.Width = static_cast<float>(m_width);
        m_draw2DViewport.Height = static_cast<float>(m_height);

        m_draw2DScissorRect.right = static_cast<LONG>(m_width);
        m_draw2DScissorRect.bottom = static_cast<LONG>(m_height);
    }

    {
        CD3DX12_CPU_DESCRIPTOR_HANDLE rtvHandle(m_rtvHeap->GetCPUDescriptorHandleForHeapStart());

        for (UINT n = 0; n < FRAME_COUNT; n++)
        {
            TRY_DO(m_swapChain->GetBuffer(n, IID_PPV_ARGS(&m_renderTargets[n])));
            m_device->CreateRenderTargetView(m_renderTargets[n].Get(), nullptr, rtvHandle);
            rtvHandle.Offset(1, m_rtvDescriptorSize);

            NAME_D3D12_OBJECT_INDEXED(m_renderTargets, n);
        }
    }

    CreateDepthBuffer();
}

void NativeClient::SetupSpaceResolutionDependentResources()
{
    {
        m_spaceViewport.Width = static_cast<float>(m_resolution.width);
        m_spaceViewport.Height = static_cast<float>(m_resolution.height);

        m_spaceScissorRect.right = static_cast<LONG>(m_resolution.width);
        m_spaceScissorRect.bottom = static_cast<LONG>(m_resolution.height);
    }

    UpdatePostViewAndScissor();

    {
        const D3D12_RESOURCE_DESC swapChainDesc = m_renderTargets[m_frameIndex]->GetDesc();
        const CD3DX12_RESOURCE_DESC renderTargetDesc = CD3DX12_RESOURCE_DESC::Tex2D(
            swapChainDesc.Format,
            m_resolution.width,
            m_resolution.height,
            1u, 1u,
            swapChainDesc.SampleDesc.Count,
            swapChainDesc.SampleDesc.Quality,
            D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET,
            D3D12_TEXTURE_LAYOUT_UNKNOWN, 0u);

        m_intermediateRenderTargetView = CD3DX12_CPU_DESCRIPTOR_HANDLE(m_rtvHeap->GetCPUDescriptorHandleForHeapStart(),
                                                                       FRAME_COUNT,
                                                                       m_rtvDescriptorSize);

        m_intermediateRenderTarget = util::AllocateResource<ID3D12Resource>(
            *this,
            renderTargetDesc,
            D3D12_HEAP_TYPE_DEFAULT,
            D3D12_RESOURCE_STATE_RENDER_TARGET);
        NAME_D3D12_OBJECT(m_intermediateRenderTarget);

        m_device->CreateRenderTargetView(
            m_intermediateRenderTarget.Get(),
            nullptr,
            m_intermediateRenderTargetView);

        if (m_space) m_space->PerformResolutionDependentSetup(m_resolution);
    }

    if (m_postProcessingPipeline != nullptr)
        m_postProcessingPipeline->CreateResourceView(m_intermediateRenderTarget);
}

void NativeClient::OnUpdate(const double delta)
{
    if (m_space) m_space->Update(delta);
}

void NativeClient::OnPreRender()
{
    if (!m_windowVisible) return;

    m_uploadGroup.Reset(m_frameIndex);
    m_uploader = std::make_unique<Uploader>(*this, m_uploadGroup.commandList);
}

void NativeClient::OnRender(double)
{
    if (!m_windowVisible) return;

    {
        PIXScopedEvent(m_commandQueue.Get(), 0, L"Render");

        m_uploadGroup.Close();

        PopulateCommandLists();

        std::vector<ID3D12CommandList*> commandLists;
        commandLists.reserve(3);

        commandLists.push_back(m_uploadGroup.commandList.Get());
        if (m_space) commandLists.push_back(m_space->GetCommandList().Get());
        commandLists.push_back(m_2dGroup.commandList.Get());

        m_commandQueue->ExecuteCommandLists(static_cast<UINT>(commandLists.size()), commandLists.data());
    }

    const UINT presentFlags = (m_tearingSupport && m_windowedMode) ? DXGI_PRESENT_ALLOW_TEARING : 0;
    TRY_DO(m_swapChain->Present(0, presentFlags));

    WaitForGPU();

    if (m_space) m_space->CleanupRenderSetup();

    MoveToNextFrame();
}

void NativeClient::OnDestroy()
{
    WaitForGPU();
    CloseHandle(m_fenceEvent);
}

void NativeClient::OnSizeChanged(UINT width, UINT height, bool minimized)
{
    if ((width != m_width || height != m_height) && !minimized)
    {
        WaitForGPU();

        for (UINT n = 0; n < FRAME_COUNT; n++) // NOLINT(modernize-loop-convert)
        {
            m_renderTargets[n].Reset();
            m_fenceValues[n] = m_fenceValues[m_frameIndex];
        }

        DXGI_SWAP_CHAIN_DESC desc = {};
        m_swapChain->GetDesc(&desc);
        TRY_DO(m_swapChain->ResizeBuffers(FRAME_COUNT, width, height, desc.BufferDesc.Format, desc.Flags));

        BOOL fullscreenState;
        TRY_DO(m_swapChain->GetFullscreenState(&fullscreenState, nullptr));
        m_windowedMode = !fullscreenState;

        m_frameIndex = m_swapChain->GetCurrentBackBufferIndex();

        UpdateForSizeChange(width, height);

        SetupSizeDependentResources();
    }

    m_windowVisible = !minimized;
}

void NativeClient::OnWindowMoved(int, int)
{
}

void NativeClient::InitRaytracingPipeline(const SpacePipeline& pipeline)
{
    if (m_space->PerformInitialSetupStepTwo(pipeline))
    {
        m_spaceInitialized = true;
    }
    else
    {
        m_space = nullptr;
    }
}

void NativeClient::SetResolution(UINT width, UINT height)
{
    m_resolution.width = width;
    m_resolution.height = height;

    WaitForGPU();
    SetupSpaceResolutionDependentResources();
}

// ReSharper disable once CppMemberFunctionMayBeStatic
void NativeClient::ToggleFullscreen() const
{
    Win32Application::ToggleFullscreenWindow();
}

Texture* NativeClient::LoadTexture(std::byte** data, const TextureDescription& description) const
{
    REQUIRE(m_uploader != nullptr);

    return Texture::Create(*m_uploader, data, description);
}

void NativeClient::SetMousePosition(POINT position) const
{
    TRY_DO(ClientToScreen(Win32Application::GetHwnd(), &position));
    TRY_DO(SetCursorPos(position.x, position.y));
}

Space* NativeClient::GetSpace() const
{
    return m_space.get();
}

void NativeClient::AddRasterPipeline(std::unique_ptr<RasterPipeline> pipeline)
{
    m_rasterPipelines.push_back(std::move(pipeline));
}

void NativeClient::SetPostProcessingPipeline(RasterPipeline* pipeline)
{
    m_postProcessingPipeline = pipeline;
    m_postProcessingPipeline->CreateResourceView(m_intermediateRenderTarget);
}

void NativeClient::AddDraw2DPipeline(RasterPipeline* pipeline, draw2d::Callback callback)
{
    m_draw2DPipelines.push_back({*this, pipeline, callback});
}

NativeClient::ObjectHandle NativeClient::StoreObject(std::unique_ptr<Object> object)
{
    return m_objects.insert(m_objects.end(), std::move(object));
}

void NativeClient::DeleteObject(const ObjectHandle handle)
{
    m_objects.erase(handle);
}

void NativeClient::WaitForGPU()
{
    TRY_DO(m_commandQueue->Signal(m_fence.Get(), m_fenceValues[m_frameIndex]));

    TRY_DO(m_fence->SetEventOnCompletion(m_fenceValues[m_frameIndex], m_fenceEvent));
    WaitForSingleObjectEx(m_fenceEvent, INFINITE, FALSE);

    m_fenceValues[m_frameIndex]++;
}

void NativeClient::MoveToNextFrame()
{
    const UINT64 currentFenceValue = m_fenceValues[m_frameIndex];
    TRY_DO(m_commandQueue->Signal(m_fence.Get(), currentFenceValue));

    m_frameIndex = m_swapChain->GetCurrentBackBufferIndex();

    if (m_fence->GetCompletedValue() < m_fenceValues[m_frameIndex])
    {
        TRY_DO(m_fence->SetEventOnCompletion(m_fenceValues[m_frameIndex], m_fenceEvent));
        WaitForSingleObjectEx(m_fenceEvent, INFINITE, FALSE);
    }

    m_fenceValues[m_frameIndex] = currentFenceValue + 1;
}

void NativeClient::CheckRaytracingSupport() const
{
    D3D12_FEATURE_DATA_D3D12_OPTIONS5 options5 = {};
    TRY_DO(m_device->CheckFeatureSupport(D3D12_FEATURE_D3D12_OPTIONS5, &options5, sizeof(options5)));

    if (options5.RaytracingTier < D3D12_RAYTRACING_TIER_1_0)
        throw NativeException(
            "Raytracing not supported on device.");
}

void NativeClient::PopulateSpaceCommandList() const
{
    REQUIRE(m_space != nullptr);

    m_space->Reset(m_frameIndex);

    {
        PIXScopedEvent(m_space->GetCommandList().Get(), PIX_COLOR_DEFAULT, L"Space");

        m_space->EnqueueRenderSetup();
        m_space->DispatchRays();
        m_space->CopyOutputToBuffer(m_intermediateRenderTarget);
    }

    TRY_DO(m_space->GetCommandList()->Close());
}

void NativeClient::PopulatePostProcessingCommandList() const
{
    if (m_space == nullptr) return; // Nothing to post-process.

    {
        PIXScopedEvent(m_2dGroup.commandList.Get(), 0, L"Post Processing");

        m_postProcessingPipeline->SetPipeline(m_2dGroup.commandList);
        m_2dGroup.commandList->SetGraphicsRootSignature(m_postProcessingPipeline->GetRootSignature().Get());

        m_postProcessingPipeline->SetupHeaps(m_2dGroup.commandList);
        m_postProcessingPipeline->SetupRootDescriptorTable(m_2dGroup.commandList);

        m_2dGroup.commandList->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
        m_2dGroup.commandList->RSSetViewports(1, &m_postViewport);
        m_2dGroup.commandList->RSSetScissorRects(1, &m_postScissorRect);

        const CD3DX12_CPU_DESCRIPTOR_HANDLE rtvHandle(m_rtvHeap->GetCPUDescriptorHandleForHeapStart(), m_frameIndex,
                                                      m_rtvDescriptorSize);
        const CD3DX12_CPU_DESCRIPTOR_HANDLE dsvHandle(m_dsvHeap->GetCPUDescriptorHandleForHeapStart());
        m_2dGroup.commandList->OMSetRenderTargets(1, &rtvHandle, FALSE, &dsvHandle);

        m_2dGroup.commandList->ClearRenderTargetView(rtvHandle, LETTERBOX_COLOR, 0, nullptr);
        m_2dGroup.commandList->ClearDepthStencilView(dsvHandle, D3D12_CLEAR_FLAG_DEPTH, 1.0f, 0, 0, nullptr);

        m_2dGroup.commandList->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP);
        m_2dGroup.commandList->IASetVertexBuffers(0, 1, &m_postVertexBufferView);
        m_2dGroup.commandList->DrawInstanced(4, 1, 0, 0);
    }
}

void NativeClient::PopulateDraw2DCommandList(const size_t index)
{
    {
        auto& pipeline = m_draw2DPipelines[index];
        PIXScopedEvent(m_2dGroup.commandList.Get(), 0, L"Draw2D");

        pipeline.PopulateCommandListSetup(m_2dGroup.commandList);

        m_2dGroup.commandList->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
        m_2dGroup.commandList->RSSetViewports(1, &m_draw2DViewport);
        m_2dGroup.commandList->RSSetScissorRects(1, &m_draw2DScissorRect);

        const CD3DX12_CPU_DESCRIPTOR_HANDLE rtvHandle(m_rtvHeap->GetCPUDescriptorHandleForHeapStart(), m_frameIndex,
                                                      m_rtvDescriptorSize);
        const CD3DX12_CPU_DESCRIPTOR_HANDLE dsvHandle(m_dsvHeap->GetCPUDescriptorHandleForHeapStart());
        m_2dGroup.commandList->OMSetRenderTargets(1, &rtvHandle, FALSE, &dsvHandle);

        pipeline.PopulateCommandListDrawing(m_2dGroup.commandList);
    }
}

void NativeClient::PopulateCommandLists()
{
    m_2dGroup.Reset(m_frameIndex);

    D3D12_RESOURCE_BARRIER barriers[] = {
        CD3DX12_RESOURCE_BARRIER::Transition(m_renderTargets[m_frameIndex].Get(),
                                             D3D12_RESOURCE_STATE_PRESENT, D3D12_RESOURCE_STATE_RENDER_TARGET),
        CD3DX12_RESOURCE_BARRIER::Transition(m_intermediateRenderTarget.Get(),
                                             D3D12_RESOURCE_STATE_RENDER_TARGET,
                                             D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE)
    };

    m_2dGroup.commandList->ResourceBarrier(_countof(barriers), barriers);

    if (m_space)
    {
        PopulateSpaceCommandList();

        if (m_postProcessingPipeline != nullptr)
        {
            PopulatePostProcessingCommandList();
        }
    }

    for (size_t i = 0; i < m_draw2DPipelines.size(); ++i)
    {
        PopulateDraw2DCommandList(i);
    }

    barriers[0].Transition.StateBefore = D3D12_RESOURCE_STATE_RENDER_TARGET;
    barriers[0].Transition.StateAfter = D3D12_RESOURCE_STATE_PRESENT;
    barriers[1].Transition.StateBefore = D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
    barriers[1].Transition.StateAfter = D3D12_RESOURCE_STATE_RENDER_TARGET;

    m_2dGroup.commandList->ResourceBarrier(_countof(barriers), barriers);
    m_2dGroup.Close();
}

void NativeClient::UpdatePostViewAndScissor()
{
    const auto width = static_cast<float>(m_width);
    const auto height = static_cast<float>(m_height);

    const float viewWidthRatio = static_cast<float>(m_resolution.width) / width;
    const float viewHeightRatio = static_cast<float>(m_resolution.height) / height;

    float x = 1.0f;
    float y = 1.0f;

    if (viewWidthRatio < viewHeightRatio)
    {
        x = viewWidthRatio / viewHeightRatio;
    }
    else
    {
        y = viewHeightRatio / viewWidthRatio;
    }

    m_postViewport.TopLeftX = width * (1.0f - x) / 2.0f;
    m_postViewport.TopLeftY = height * (1.0f - y) / 2.0f;
    m_postViewport.Width = x * width;
    m_postViewport.Height = y * height;

    m_postScissorRect.left = static_cast<LONG>(m_postViewport.TopLeftX);
    m_postScissorRect.right = static_cast<LONG>(m_postViewport.TopLeftX + m_postViewport.Width);
    m_postScissorRect.top = static_cast<LONG>(m_postViewport.TopLeftY);
    m_postScissorRect.bottom = static_cast<LONG>(m_postViewport.TopLeftY + m_postViewport.Height);
}
