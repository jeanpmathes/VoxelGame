//  <copyright file="Context.cpp" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#include "stdafx.h"

namespace
{
    ComPtr<IDXGIAdapter1> GetHardwareAdapter(
        ComPtr<IDXGIFactory4> const&       dxgiFactory,
        ComPtr<ID3D12DeviceFactory> const& deviceFactory,
        bool const                         requestHighPerformanceAdapter = false)
    {
        ComPtr<IDXGIAdapter1> adapter;

        ComPtr<IDXGIFactory6> factory;
        if (SUCCEEDED(dxgiFactory.As(&factory)))
            for (UINT adapterIndex = 0; SUCCEEDED(
                                                  factory->EnumAdapterByGpuPreference( adapterIndex, requestHighPerformanceAdapter == true ? DXGI_GPU_PREFERENCE_HIGH_PERFORMANCE :
                                                      DXGI_GPU_PREFERENCE_UNSPECIFIED, IID_PPV_ARGS(&adapter))); ++adapterIndex)
            {
                DXGI_ADAPTER_DESC1 description;
                TryDo(adapter->GetDesc1(&description));

                if (description.Flags & DXGI_ADAPTER_FLAG_SOFTWARE) continue;

                if (SUCCEEDED(deviceFactory->CreateDevice(adapter.Get(), D3D_FEATURE_LEVEL_12_2, __uuidof(ID3D12Device), nullptr))) break;
            }

        if (adapter.Get() == nullptr)
            for (UINT adapterIndex = 0; SUCCEEDED(dxgiFactory->EnumAdapters1(adapterIndex, &adapter)); ++adapterIndex)
            {
                DXGI_ADAPTER_DESC1 description;
                TryDo(adapter->GetDesc1(&description));

                if (description.Flags & DXGI_ADAPTER_FLAG_SOFTWARE) continue;

                if (SUCCEEDED(deviceFactory->CreateDevice(adapter.Get(), D3D_FEATURE_LEVEL_12_2, __uuidof(ID3D12Device), nullptr))) break;
            }

        return adapter;
    }
}

UINT const   Context::AGILITY_SDK_VERSION = 619;
LPCSTR const Context::AGILITY_SDK_PATH    = ".\\D3D12\\";

Context::Context(NativeClient& client, WCHAR const* applicationName, WCHAR const* applicationVersion, D3D12MessageFunc const onDebug, UINT const width, UINT const height)
    : client(&client)
  , debugLayer(onDebug)
#ifdef USE_NSIGHT_AFTERMATH
, gpuCrashTracker(markerMap, shaderDatabase, GpuCrashTracker::Description::Create(applicationName, applicationVersion))
#endif
{
    (void)applicationName;
    (void)applicationVersion;

    CreateDevice();
    CreateSwapChain(width, height);
    CreateFences();

    userInterfaceContext = std::make_unique<ui::Context>(*this);

    CreateSizeDependentResources();
}

Context::~Context()
{
    if (fenceEvent != nullptr) CloseHandle(fenceEvent);
}

void Context::OnResize(UINT const width, UINT const height)
{
    WaitForGPU();

    userInterfaceContext->ReleaseRenderTargets();

    WaitForGPU();

    {
        for (UINT frame = 0; frame < FRAME_COUNT; frame++)
        {
            finalRenderTargets[frame].Reset();
            fenceValues[frame] = fenceValues[frameIndex];
        }

        DXGI_SWAP_CHAIN_DESC description = {};
        TryDo(swapChain->GetDesc(&description));
        TryDo(swapChain->ResizeBuffers(FRAME_COUNT, width, height, description.BufferDesc.Format, description.Flags));

        frameIndex = swapChain->GetCurrentBackBufferIndex();
    }

    CreateSizeDependentResources();
}

void Context::MoveToNextFrame()
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

void Context::WaitForGPU()
{
    TryDo(commandQueue->Signal(fence.Get(), fenceValues[frameIndex]));

    TryDo(fence->SetEventOnCompletion(fenceValues[frameIndex], fenceEvent));
    WaitForSingleObjectEx(fenceEvent, INFINITE, FALSE);

    fenceValues[frameIndex]++;
}

NativeClient& Context::GetClient() const { return *client; }

DebugLayer& Context::GetDebugLayer()
{
    return debugLayer;
}

UINT Context::GetFrameIndex() const { return frameIndex; }

ComPtr<ID3D12Device5> Context::GetD3D12Device() const { return device; }

ComPtr<D3D12MA::Allocator> Context::GetAllocator() const { return allocator; }

ComPtr<ID3D12CommandQueue> Context::GetCommandQueue() const { return commandQueue; }

ComPtr<IDXGISwapChain3> Context::GetSwapChain() const { return swapChain; }

ComPtr<ID3D12Resource> Context::GetFinalRenderTarget(UINT const frame) const { return finalRenderTargets[frame]; }

D3D12_CPU_DESCRIPTOR_HANDLE Context::GetFinalRenderTargetView(UINT const frame) const { return rtvHeap.GetDescriptorHandleCPU(frame); }

D3D12_CPU_DESCRIPTOR_HANDLE Context::GetIntermediateRenderTargetView() const { return rtvHeap.GetDescriptorHandleCPU(FRAME_COUNT); }

ui::Context& Context::GetUserInterfaceContext() const
{
    return *userInterfaceContext;
}

ComPtr<ID3D11On12Device> Context::GetD3D11On12Device() const { return direct3D11On12Device; }

ComPtr<ID3D11Device> Context::GetD3D11Device() const { return direct3D11Device; }

ComPtr<ID3D11DeviceContext> Context::GetD3D11DeviceContext() const { return direct3D11DeviceContext; }

#ifdef USE_NSIGHT_AFTERMATH
void Context::InitializeGpuCrashTracker()
{
    if (client->SupportPIX()) return;

    gpuCrashTracker.Initialize();
}

void Context::InitializeAftermath() const
{
    if (client->SupportPIX()) return;

    constexpr uint32_t aftermathFlags = GFSDK_Aftermath_FeatureFlags_EnableMarkers | GFSDK_Aftermath_FeatureFlags_EnableResourceTracking |
                                        GFSDK_Aftermath_FeatureFlags_CallStackCapturing | GFSDK_Aftermath_FeatureFlags_GenerateShaderDebugInfo;

    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_DX12_Initialize(GFSDK_Aftermath_Version_API, aftermathFlags, device.Get()));
}

void Context::SetUpCommandListForAftermath(ComPtr<ID3D12GraphicsCommandList> const& commandList) const
{
    if (client->SupportPIX()) return;

    GFSDK_Aftermath_ContextHandle contextHandle;
    AFTERMATH_CHECK_ERROR(GFSDK_Aftermath_DX12_CreateContextHandle(commandList.Get(), &contextHandle));
}

void Context::SetUpShaderForAftermath(ComPtr<IDxcResult> const& result)
{
    if (client->SupportPIX()) return;

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

void Context::CreateDevice()
{
    UINT const dxgiFactoryFlags = debugLayer.GetDXGIFactoryFlags();

    ComPtr<IDXGIFactory4> dxgiFactory;
    TryDo(CreateDXGIFactory2(dxgiFactoryFlags, IID_PPV_ARGS(&dxgiFactory)));

    ComPtr<ID3D12SDKConfiguration1> sdk;
    TryDo(D3D12GetInterface(CLSID_D3D12SDKConfiguration, IID_PPV_ARGS(&sdk)));

    ComPtr<ID3D12DeviceFactory> deviceFactory;
    TryDo(sdk->CreateDeviceFactory(AGILITY_SDK_VERSION, AGILITY_SDK_PATH, IID_PPV_ARGS(&deviceFactory)));

    debugLayer.ConfigureDeviceFactory(deviceFactory, *client);

    ComPtr<IDXGIAdapter1> const hardwareAdapter = GetHardwareAdapter(dxgiFactory, deviceFactory);

#ifdef USE_NSIGHT_AFTERMATH
    InitializeGpuCrashTracker();
#endif

    TryDo(deviceFactory->CreateDevice(hardwareAdapter.Get(), D3D_FEATURE_LEVEL_12_2, IID_PPV_ARGS(&device)));
    NAME_DIRECT_OBJECT(device);

#ifdef USE_NSIGHT_AFTERMATH
    InitializeAftermath();
#endif

    debugLayer.ConfigureDevice(device, *client);

    D3D12MA::ALLOCATOR_DESC allocatorDesc = {};
    allocatorDesc.pDevice                 = device.Get();
    allocatorDesc.pAdapter                = hardwareAdapter.Get();

    TryDo(CreateAllocator(&allocatorDesc, &allocator));

    CheckRaytracingSupport();

    D3D12_COMMAND_QUEUE_DESC queueDesc = {};
    queueDesc.Flags                    = D3D12_COMMAND_QUEUE_FLAG_NONE;
    queueDesc.Type                     = D3D12_COMMAND_LIST_TYPE_DIRECT;

    TryDo(device->CreateCommandQueue(&queueDesc, IID_PPV_ARGS(&commandQueue)));
    NAME_DIRECT_OBJECT(commandQueue);

    rtvHeap.Create(device, FRAME_COUNT + 1, D3D12_DESCRIPTOR_HEAP_TYPE_RTV, false);
    NAME_DIRECT_OBJECT(rtvHeap);

    CreateD3D11On12Device();
}

void Context::CreateSwapChain(UINT const width, UINT const height)
{
    UINT const dxgiFactoryFlags = debugLayer.GetDXGIFactoryFlags();

    ComPtr<IDXGIFactory4> dxgiFactory;
    TryDo(CreateDXGIFactory2(dxgiFactoryFlags, IID_PPV_ARGS(&dxgiFactory)));

    DXGI_SWAP_CHAIN_DESC1 swapChainDesc = {};
    swapChainDesc.BufferCount           = FRAME_COUNT;
    swapChainDesc.Width                 = width;
    swapChainDesc.Height                = height;
    swapChainDesc.Format                = DXGI_FORMAT_B8G8R8A8_UNORM;
    swapChainDesc.BufferUsage           = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    swapChainDesc.SwapEffect            = DXGI_SWAP_EFFECT_FLIP_DISCARD;
    swapChainDesc.SampleDesc.Count      = 1;

    swapChainDesc.Flags = client->IsTearingSupportEnabled() ? DXGI_SWAP_CHAIN_FLAG_ALLOW_TEARING : 0;

    ComPtr<IDXGISwapChain1> swapChain1;
    TryDo(dxgiFactory->CreateSwapChainForHwnd(commandQueue.Get(), Win32Application::GetWindowHandle(), &swapChainDesc, nullptr, nullptr, &swapChain1));

    TryDo(dxgiFactory->MakeWindowAssociation(Win32Application::GetWindowHandle(), DXGI_MWA_NO_ALT_ENTER));

    TryDo(swapChain1.As(&swapChain));
    frameIndex = swapChain->GetCurrentBackBufferIndex();
}

void Context::CreateSizeDependentResources()
{
    for (UINT frame = 0; frame < FRAME_COUNT; frame++)
    {
        TryDo(swapChain->GetBuffer(frame, IID_PPV_ARGS(&finalRenderTargets[frame])));
        device->CreateRenderTargetView(finalRenderTargets[frame].Get(), nullptr, rtvHeap.GetDescriptorHandleCPU(frame));

        NAME_DIRECT_OBJECT_INDEXED(finalRenderTargets, frame);
    }

    userInterfaceContext->CreateRenderTargets();
}

void Context::CreateFences()
{
    TryDo(device->CreateFence(fenceValues[frameIndex], D3D12_FENCE_FLAG_NONE, IID_PPV_ARGS(&fence)));
    NAME_DIRECT_OBJECT(fence);

    fenceValues[frameIndex]++;

    fenceEvent = CreateEvent(nullptr, FALSE, FALSE, nullptr);
    if (fenceEvent == nullptr) TryDo(HRESULT_FROM_WIN32(GetLastError()));
}

void Context::CreateD3D11On12Device()
{
    std::array<IUnknown*, 1> const commandQueues = {commandQueue.Get()};

    TryDo(
          D3D11On12CreateDevice(
                                device.Get(),
                                D3D11_CREATE_DEVICE_BGRA_SUPPORT,
                                nullptr,
                                0,
                                commandQueues.data(),
                                static_cast<UINT>(commandQueues.size()),
                                0,
                                &direct3D11Device,
                                &direct3D11DeviceContext,
                                nullptr));

    NAME_DIRECT_OBJECT(direct3D11DeviceContext);

    TryDo(direct3D11Device.As(&direct3D11On12Device));
}

void Context::CheckRaytracingSupport() const
{
    D3D12_FEATURE_DATA_D3D12_OPTIONS5 options5 = {};
    TryDo(device->CheckFeatureSupport(D3D12_FEATURE_D3D12_OPTIONS5, &options5, sizeof(options5)));

    if (options5.RaytracingTier < D3D12_RAYTRACING_TIER_1_1) throw NativeException("Raytracing not supported on device.");
}
