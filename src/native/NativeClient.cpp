//  <copyright file="NativeClient.cpp" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#include "stdafx.h"

constexpr float NativeClient::CLEAR_COLOR[4] = {0.0f, 0.2f, 0.4f, 1.0f};
constexpr float NativeClient::LETTERBOX_COLOR[4] = {0.0f, 0.0f, 0.0f, 1.0f};

NativeClient::NativeClient(const UINT width, const UINT height, const std::wstring name,
                           const Configuration configuration) :
    DXApp(width, height, name, configuration),
    m_resolution{width, height},
    m_debugCallback(configuration.onDebug),
    m_spaceViewport(0.0f, 0.0f, 0.0f, 0.0f),
    m_spaceScissorRect(0, 0, 0, 0),
    m_space(*this),
    m_postViewport(0.0f, 0.0f, 0.0f, 0.0f),
    m_postScissorRect(0, 0, 0, 0),
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

void NativeClient::OnInit()
{
    LoadDevice();
    LoadPipeline();
}

void NativeClient::LoadDevice()
{
    UINT dxgiFactoryFlags = 0;

#if defined(_DEBUG)
    {
        ComPtr<ID3D12Debug> debugController;
        if (SUCCEEDED(D3D12GetDebugInterface(IID_PPV_ARGS(&debugController))))
        {
            debugController->EnableDebugLayer();
            dxgiFactoryFlags |= DXGI_CREATE_FACTORY_DEBUG;
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
    TRY_DO(m_device->QueryInterface(__uuidof(ID3D12InfoQueue1), &m_infoQueue));
    TRY_DO(
        m_infoQueue->RegisterMessageCallback(m_debugCallback, D3D12_MESSAGE_CALLBACK_FLAG_NONE, nullptr, &
            m_callbackCookie));
    TRY_DO(m_infoQueue->AddApplicationMessage(D3D12_MESSAGE_SEVERITY_MESSAGE, "Installed debug callback"));
#endif

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
        D3D12_DESCRIPTOR_HEAP_DESC rtvHeapDesc = {};
        rtvHeapDesc.NumDescriptors = FRAME_COUNT + 1;
        rtvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_RTV;
        rtvHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;
        TRY_DO(m_device->CreateDescriptorHeap(&rtvHeapDesc, IID_PPV_ARGS(&m_rtvHeap)));

        D3D12_DESCRIPTOR_HEAP_DESC srvHeapDesc = {};
        srvHeapDesc.NumDescriptors = FRAME_COUNT + 1;
        srvHeapDesc.Type = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
        srvHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
        TRY_DO(m_device->CreateDescriptorHeap(&srvHeapDesc, IID_PPV_ARGS(&m_srvHeap)));

        m_rtvDescriptorSize = m_device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_RTV);
        m_srvDescriptorSize = m_device->GetDescriptorHandleIncrementSize(D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV);
    }

    for (UINT n = 0; n < FRAME_COUNT; n++)
    {
        TRY_DO(
            m_device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&m_spaceCommandAllocators[n])
            ));
        NAME_D3D12_OBJECT_INDEXED(m_spaceCommandAllocators, n);

        TRY_DO(
            m_device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&m_postCommandAllocators[n])))
        ;
        NAME_D3D12_OBJECT_INDEXED(m_postCommandAllocators, n);
    }
}

void NativeClient::LoadPipeline()
{
    {
        CD3DX12_DESCRIPTOR_RANGE1 ranges[1] = {};
        CD3DX12_ROOT_PARAMETER1 rootParameters[1] = {};

        ranges[0].Init(D3D12_DESCRIPTOR_RANGE_TYPE_CBV, 1, 0);
        rootParameters[0].InitAsDescriptorTable(1, &ranges[0], D3D12_SHADER_VISIBILITY_ALL);

        CD3DX12_VERSIONED_ROOT_SIGNATURE_DESC rootSignatureDesc;
        rootSignatureDesc.Init_1_1(_countof(rootParameters), rootParameters, 0, nullptr,
                                   D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT);

        ComPtr<ID3DBlob> signature;
        ComPtr<ID3DBlob> error;
        TRY_DO(
            D3DX12SerializeVersionedRootSignature(&rootSignatureDesc, D3D_ROOT_SIGNATURE_VERSION_1_1, &signature, &error
            ));
        TRY_DO(m_device->CreateRootSignature(0,
            signature->GetBufferPointer(), signature->GetBufferSize(), IID_PPV_ARGS(&m_spaceRootSignature)));
        NAME_D3D12_OBJECT(m_spaceRootSignature);
    }

    {
        CD3DX12_DESCRIPTOR_RANGE1 ranges[1];
        CD3DX12_ROOT_PARAMETER1 rootParameters[1];

        ranges[0].Init(D3D12_DESCRIPTOR_RANGE_TYPE_SRV, 1, 0);
        rootParameters[0].InitAsDescriptorTable(1, &ranges[0], D3D12_SHADER_VISIBILITY_PIXEL);

        D3D12_ROOT_SIGNATURE_FLAGS rootSignatureFlags =
            D3D12_ROOT_SIGNATURE_FLAG_ALLOW_INPUT_ASSEMBLER_INPUT_LAYOUT;

        D3D12_STATIC_SAMPLER_DESC sampler = {};
        sampler.Filter = D3D12_FILTER_MIN_MAG_MIP_LINEAR;
        sampler.AddressU = D3D12_TEXTURE_ADDRESS_MODE_BORDER;
        sampler.AddressV = D3D12_TEXTURE_ADDRESS_MODE_BORDER;
        sampler.AddressW = D3D12_TEXTURE_ADDRESS_MODE_BORDER;
        sampler.MipLODBias = 0;
        sampler.MaxAnisotropy = 0;
        sampler.ComparisonFunc = D3D12_COMPARISON_FUNC_NEVER;
        sampler.BorderColor = D3D12_STATIC_BORDER_COLOR_TRANSPARENT_BLACK;
        sampler.MinLOD = 0.0f;
        sampler.MaxLOD = D3D12_FLOAT32_MAX;
        sampler.ShaderRegister = 0;
        sampler.RegisterSpace = 0;
        sampler.ShaderVisibility = D3D12_SHADER_VISIBILITY_PIXEL;

        CD3DX12_VERSIONED_ROOT_SIGNATURE_DESC rootSignatureDesc;
        rootSignatureDesc.Init_1_1(_countof(rootParameters), rootParameters, 1, &sampler, rootSignatureFlags);

        ComPtr<ID3DBlob> signature;
        ComPtr<ID3DBlob> error;
        TRY_DO(
            D3DX12SerializeVersionedRootSignature(&rootSignatureDesc, D3D_ROOT_SIGNATURE_VERSION_1_1, &signature, &error
            ));
        TRY_DO(m_device->CreateRootSignature(0,
            signature->GetBufferPointer(), signature->GetBufferSize(), IID_PPV_ARGS(&m_postRootSignature)));
        NAME_D3D12_OBJECT(m_postRootSignature);
    }

    {
        ComPtr<ID3DBlob> sceneVertexShader;
        ComPtr<ID3DBlob> scenePixelShader;
        ComPtr<ID3DBlob> postVertexShader;
        ComPtr<ID3DBlob> postPixelShader;
        ComPtr<ID3DBlob> error;

#if defined(_DEBUG)
        UINT compileFlags = D3DCOMPILE_DEBUG | D3DCOMPILE_SKIP_OPTIMIZATION;
#else
        UINT compileFlags = 0;
#endif

        TRY_DO(D3DCompileFromFile(GetAssetFullPath(L"Space.hlsl").c_str(),
            nullptr, nullptr, "VSMain", "vs_5_0", compileFlags, 0, &sceneVertexShader, &error));
        TRY_DO(D3DCompileFromFile(GetAssetFullPath(L"Space.hlsl").c_str(),
            nullptr, nullptr, "PSMain", "ps_5_0", compileFlags, 0, &scenePixelShader, &error));

        TRY_DO(D3DCompileFromFile(GetAssetFullPath(L"Post.hlsl").c_str(),
            nullptr, nullptr, "VSMain", "vs_5_0", compileFlags, 0, &postVertexShader, &error));
        TRY_DO(D3DCompileFromFile(GetAssetFullPath(L"Post.hlsl").c_str(),
            nullptr, nullptr, "PSMain", "ps_5_0", compileFlags, 0, &postPixelShader, &error));

        D3D12_INPUT_ELEMENT_DESC inputElementDescription[] =
        {
            {
                "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT,
                0, 0, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0
            },
            {
                "COLOR", 0, DXGI_FORMAT_R32G32B32A32_FLOAT,
                0, 12, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0
            }
        };
        D3D12_INPUT_ELEMENT_DESC scaleInputElementDescription[] =
        {
            {
                "POSITION", 0, DXGI_FORMAT_R32G32B32A32_FLOAT,
                0, 0, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0
            },
            {
                "TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT,
                0, D3D12_APPEND_ALIGNED_ELEMENT, D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA, 0
            }
        };

        D3D12_GRAPHICS_PIPELINE_STATE_DESC psoDesc = {};
        psoDesc.InputLayout = {inputElementDescription, _countof(inputElementDescription)};
        psoDesc.pRootSignature = m_spaceRootSignature.Get();
        psoDesc.VS = CD3DX12_SHADER_BYTECODE(sceneVertexShader.Get());
        psoDesc.PS = CD3DX12_SHADER_BYTECODE(scenePixelShader.Get());
        psoDesc.RasterizerState = CD3DX12_RASTERIZER_DESC(D3D12_DEFAULT);
        psoDesc.BlendState = CD3DX12_BLEND_DESC(D3D12_DEFAULT);
        psoDesc.DepthStencilState = CD3DX12_DEPTH_STENCIL_DESC(D3D12_DEFAULT);
        psoDesc.DSVFormat = DXGI_FORMAT_D32_FLOAT;
        psoDesc.DepthStencilState.StencilEnable = FALSE;
        psoDesc.SampleMask = UINT_MAX;
        psoDesc.PrimitiveTopologyType = D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE;
        psoDesc.NumRenderTargets = 1;
        psoDesc.RTVFormats[0] = DXGI_FORMAT_R8G8B8A8_UNORM;
        psoDesc.SampleDesc.Count = 1;

        TRY_DO(m_device->CreateGraphicsPipelineState(&psoDesc, IID_PPV_ARGS(&m_spacePipelineState)));
        NAME_D3D12_OBJECT(m_spacePipelineState);

        psoDesc.InputLayout = {scaleInputElementDescription, _countof(scaleInputElementDescription)};
        psoDesc.pRootSignature = m_postRootSignature.Get();
        psoDesc.VS = CD3DX12_SHADER_BYTECODE(postVertexShader.Get());
        psoDesc.PS = CD3DX12_SHADER_BYTECODE(postPixelShader.Get());

        TRY_DO(m_device->CreateGraphicsPipelineState(&psoDesc, IID_PPV_ARGS(&m_postPipelineState)));
        NAME_D3D12_OBJECT(m_postPipelineState);
    }

    {
        TRY_DO(m_device->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_DIRECT,
            m_spaceCommandAllocators[m_frameIndex].Get(), m_spacePipelineState.Get(), IID_PPV_ARGS(&m_spaceCommandList)
        ));
        NAME_D3D12_OBJECT(m_spaceCommandList);

        TRY_DO(m_device->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_DIRECT,
            m_postCommandAllocators[m_frameIndex].Get(), m_postPipelineState.Get(), IID_PPV_ARGS(&m_postCommandList)));
        NAME_D3D12_OBJECT(m_postCommandList);

        TRY_DO(m_spaceCommandList->Close());
        TRY_DO(m_postCommandList->Close());
    }

    ComPtr<ID3D12CommandAllocator> commandAllocator;
    ComPtr<ID3D12GraphicsCommandList> commandList;

    TRY_DO(m_device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(&commandAllocator)));
    NAME_D3D12_OBJECT(commandAllocator);

    TRY_DO(m_device->CreateCommandList(0, D3D12_COMMAND_LIST_TYPE_DIRECT,
        commandAllocator.Get(),nullptr, IID_PPV_ARGS(&commandList)));
    NAME_D3D12_OBJECT(commandList);

    ComPtr<ID3D12Resource> postVertexBufferUpload;
    {
        PostVertex quadVertices[] =
        {
            {{-1.0f, -1.0f, 0.0f, 1.0f}, {0.0f, 0.0f}},
            {{-1.0f, 1.0f, 0.0f, 1.0f}, {0.0f, 1.0f}},
            {{1.0f, -1.0f, 0.0f, 1.0f}, {1.0f, 0.0f}},
            {{1.0f, 1.0f, 0.0f, 1.0f}, {1.0f, 1.0f}}
        };

        constexpr UINT vertexBufferSize = sizeof(quadVertices);

        auto vertexBufferHeapProps = CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_DEFAULT);
        auto vertexBufferDesc = CD3DX12_RESOURCE_DESC::Buffer(vertexBufferSize);
        TRY_DO(m_device->CreateCommittedResource(
            &vertexBufferHeapProps,
            D3D12_HEAP_FLAG_NONE,
            &vertexBufferDesc,
            D3D12_RESOURCE_STATE_COMMON,
            nullptr,
            IID_PPV_ARGS(&m_postVertexBuffer)));

        auto vertexBufferUploadHeapProps = CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_UPLOAD);
        auto vertexBufferUploadDesc = CD3DX12_RESOURCE_DESC::Buffer(vertexBufferSize);
        TRY_DO(m_device->CreateCommittedResource(
            &vertexBufferUploadHeapProps,
            D3D12_HEAP_FLAG_NONE,
            &vertexBufferUploadDesc,
            D3D12_RESOURCE_STATE_GENERIC_READ,
            nullptr,
            IID_PPV_ARGS(&postVertexBufferUpload)));

        NAME_D3D12_OBJECT(m_postVertexBuffer);

        UINT8* pVertexDataBegin;
        CD3DX12_RANGE readRange(0, 0);
        TRY_DO(postVertexBufferUpload->Map(0, &readRange, reinterpret_cast<void**>(&pVertexDataBegin)));
        memcpy(pVertexDataBegin, quadVertices, sizeof(quadVertices));
        postVertexBufferUpload->Unmap(0, nullptr);

        auto transitionCommonToCopyDest = CD3DX12_RESOURCE_BARRIER::Transition(m_postVertexBuffer.Get(),
                                                                               D3D12_RESOURCE_STATE_COMMON,
                                                                               D3D12_RESOURCE_STATE_COPY_DEST);
        commandList->ResourceBarrier(1, &transitionCommonToCopyDest);

        commandList->CopyBufferRegion(m_postVertexBuffer.Get(), 0, postVertexBufferUpload.Get(), 0, vertexBufferSize);

        auto transitionCopyDestToBuffer = CD3DX12_RESOURCE_BARRIER::Transition(m_postVertexBuffer.Get(),
                                                                               D3D12_RESOURCE_STATE_COPY_DEST,
                                                                               D3D12_RESOURCE_STATE_VERTEX_AND_CONSTANT_BUFFER);
        commandList->ResourceBarrier(1, &transitionCopyDestToBuffer);

        m_postVertexBufferView.BufferLocation = m_postVertexBuffer->GetGPUVirtualAddress();
        m_postVertexBufferView.StrideInBytes = sizeof(PostVertex);
        m_postVertexBufferView.SizeInBytes = vertexBufferSize;
    }

    TRY_DO(commandList->Close());
    ID3D12CommandList* ppCommandLists[] = {commandList.Get()};
    m_commandQueue->ExecuteCommandLists(_countof(ppCommandLists), ppCommandLists);

    {
        TRY_DO(m_device->CreateFence(m_fenceValues[m_frameIndex], D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&m_fence)));
        m_fenceValues[m_frameIndex]++;

        m_fenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
        if (m_fenceEvent == nullptr)
        {
            TRY_DO(HRESULT_FROM_WIN32(GetLastError()));
        }
    }

    {
        m_space.PerformInitialSetupStepOne(m_commandQueue);

        SetupSizeDependentResources();
        SetupSpaceResolutionDependentResources();

        ShaderPaths shaderPaths;
        shaderPaths.rayGenShader = GetAssetFullPath(L"RayGen.hlsl");
        shaderPaths.missShader = GetAssetFullPath(L"Miss.hlsl");
        shaderPaths.hitShader = GetAssetFullPath(L"Hit.hlsl");
        shaderPaths.shadowShader = GetAssetFullPath(L"Shadow.hlsl");

        m_space.PerformInitialSetupStepTwo(shaderPaths);
    }

    WaitForGPU();
}

void NativeClient::CreateDepthBuffer()
{
    m_dsvHeap = nv_helpers_dx12::CreateDescriptorHeap(m_device.Get(), 1, D3D12_DESCRIPTOR_HEAP_TYPE_DSV, false);

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
}

void NativeClient::SetupSizeDependentResources()
{
    UpdatePostViewAndScissor();

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
        const CD3DX12_CLEAR_VALUE clearValue(swapChainDesc.Format, CLEAR_COLOR);
        const CD3DX12_RESOURCE_DESC renderTargetDesc = CD3DX12_RESOURCE_DESC::Tex2D(
            swapChainDesc.Format,
            m_resolution.width,
            m_resolution.height,
            1u, 1u,
            swapChainDesc.SampleDesc.Count,
            swapChainDesc.SampleDesc.Quality,
            D3D12_RESOURCE_FLAG_ALLOW_RENDER_TARGET,
            D3D12_TEXTURE_LAYOUT_UNKNOWN, 0u);

        const CD3DX12_CPU_DESCRIPTOR_HANDLE rtvHandle(m_rtvHeap->GetCPUDescriptorHandleForHeapStart(), FRAME_COUNT,
                                                      m_rtvDescriptorSize);

        const auto intermediateRendertargetHeapProps = CD3DX12_HEAP_PROPERTIES(D3D12_HEAP_TYPE_DEFAULT);
        TRY_DO(m_device->CreateCommittedResource(
            &intermediateRendertargetHeapProps,
            D3D12_HEAP_FLAG_NONE,
            &renderTargetDesc,
            D3D12_RESOURCE_STATE_RENDER_TARGET,
            &clearValue,
            IID_PPV_ARGS(&m_intermediateRenderTarget)));
        m_device->CreateRenderTargetView(m_intermediateRenderTarget.Get(), nullptr, rtvHandle);
        NAME_D3D12_OBJECT(m_intermediateRenderTarget);

        m_space.PerformResolutionDependentSetup(m_resolution);
    }

    m_device->CreateShaderResourceView(m_intermediateRenderTarget.Get(), nullptr,
                                       m_srvHeap->GetCPUDescriptorHandleForHeapStart());
}

void NativeClient::OnUpdate(const double delta)
{
    m_space.Update(delta);
}

void NativeClient::OnRender(double)
{
    if (!m_windowVisible) return;

    {
        PIXScopedEvent(m_commandQueue.Get(), 0, L"Render");

        PopulateCommandLists();

        ID3D12CommandList* ppCommandLists[] = {m_spaceCommandList.Get(), m_postCommandList.Get()};
        m_commandQueue->ExecuteCommandLists(_countof(ppCommandLists), ppCommandLists);
    }

    const UINT presentFlags = (m_tearingSupport && m_windowedMode) ? DXGI_PRESENT_ALLOW_TEARING : 0;
    TRY_DO(m_swapChain->Present(0, presentFlags));

    WaitForGPU(); // There is a possibility that the fences are incorrectly set. This is a workaround for that.

    m_space.CleanupRenderSetup();

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

void NativeClient::SetMousePosition(POINT position) const
{
    TRY_DO(ClientToScreen(Win32Application::GetHwnd(), &position));
    TRY_DO(SetCursorPos(position.x, position.y));
}

Space* NativeClient::GetSpace()
{
    return &m_space;
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

void NativeClient::PopulateCommandLists()
{
    TRY_DO(m_spaceCommandAllocators[m_frameIndex]->Reset());
    TRY_DO(m_postCommandAllocators[m_frameIndex]->Reset());

    TRY_DO(m_spaceCommandList->Reset(m_spaceCommandAllocators[m_frameIndex].Get(), m_spacePipelineState.Get()));
    TRY_DO(m_postCommandList->Reset(m_postCommandAllocators[m_frameIndex].Get(), m_postPipelineState.Get()));

    {
        PIXScopedEvent(m_spaceCommandList.Get(), PIX_COLOR_DEFAULT, L"Space");

        m_space.EnqueueRenderSetup(m_spaceCommandList);
        m_space.DispatchRays(m_spaceCommandList);
        m_space.CopyOutputToBuffer(m_intermediateRenderTarget, m_spaceCommandList);
    }

    TRY_DO(m_spaceCommandList->Close());

    {
        PIXScopedEvent(m_postCommandList.Get(), 0, L"Post Processing");

        m_postCommandList->SetGraphicsRootSignature(m_postRootSignature.Get());

        ID3D12DescriptorHeap* ppHeaps[] = {m_srvHeap.Get()};
        m_postCommandList->SetDescriptorHeaps(_countof(ppHeaps), ppHeaps);

        D3D12_RESOURCE_BARRIER barriers[] = {
            CD3DX12_RESOURCE_BARRIER::Transition(m_renderTargets[m_frameIndex].Get(),
                                                 D3D12_RESOURCE_STATE_PRESENT, D3D12_RESOURCE_STATE_RENDER_TARGET),
            CD3DX12_RESOURCE_BARRIER::Transition(m_intermediateRenderTarget.Get(),
                                                 D3D12_RESOURCE_STATE_RENDER_TARGET,
                                                 D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE)
        };

        m_postCommandList->ResourceBarrier(_countof(barriers), barriers);

        m_postCommandList->SetGraphicsRootDescriptorTable(0, m_srvHeap->GetGPUDescriptorHandleForHeapStart());
        m_postCommandList->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
        m_postCommandList->RSSetViewports(1, &m_postViewport);
        m_postCommandList->RSSetScissorRects(1, &m_postScissorRect);

        const CD3DX12_CPU_DESCRIPTOR_HANDLE rtvHandle(m_rtvHeap->GetCPUDescriptorHandleForHeapStart(), m_frameIndex,
                                                      m_rtvDescriptorSize);
        const CD3DX12_CPU_DESCRIPTOR_HANDLE dsvHandle(m_dsvHeap->GetCPUDescriptorHandleForHeapStart());
        m_postCommandList->OMSetRenderTargets(1, &rtvHandle, FALSE, &dsvHandle);

        m_postCommandList->ClearRenderTargetView(rtvHandle, LETTERBOX_COLOR, 0, nullptr);
        m_postCommandList->ClearDepthStencilView(dsvHandle, D3D12_CLEAR_FLAG_DEPTH, 1.0f, 0, 0, nullptr);

        m_postCommandList->IASetPrimitiveTopology(D3D_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP);
        m_postCommandList->IASetVertexBuffers(0, 1, &m_postVertexBufferView);

        m_postCommandList->DrawInstanced(4, 1, 0, 0);

        barriers[0].Transition.StateBefore = D3D12_RESOURCE_STATE_RENDER_TARGET;
        barriers[0].Transition.StateAfter = D3D12_RESOURCE_STATE_PRESENT;
        barriers[1].Transition.StateBefore = D3D12_RESOURCE_STATE_PIXEL_SHADER_RESOURCE;
        barriers[1].Transition.StateAfter = D3D12_RESOURCE_STATE_RENDER_TARGET;

        m_postCommandList->ResourceBarrier(_countof(barriers), barriers);
    }

    TRY_DO(m_postCommandList->Close());
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