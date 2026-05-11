//  <copyright file="Context.hpp" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#pragma once

class NativeClient;

class Context final
{
public:
    Context(NativeClient& client, LPWSTR applicationName, LPWSTR applicationVersion);
    ~Context();

    Context(Context const&)            = delete;
    Context(Context&&)                 = delete;
    Context& operator=(Context const&) = delete;
    Context& operator=(Context&&)      = delete;

    void LoadDevice(Configuration const& configuration);
    void CreateSwapChain(UINT width, UINT height);
    void CreateSizeDependentResources();
    void ResizeSwapChain(UINT width, UINT height);
    void MoveToNextFrame();
    void WaitForGPU();

    [[nodiscard]] NativeClient& GetClient() const;
    [[nodiscard]] UINT          GetFrameIndex() const;

    [[nodiscard]] ComPtr<ID3D12Device5>       GetD3D12Device() const;
    [[nodiscard]] ComPtr<D3D12MA::Allocator>  GetAllocator() const;
    [[nodiscard]] ComPtr<ID3D12CommandQueue>  GetCommandQueue() const;
    [[nodiscard]] ComPtr<IDXGISwapChain3>     GetSwapChain() const;
    [[nodiscard]] ComPtr<ID3D12Resource>      GetFinalRenderTarget(UINT frame) const;
    [[nodiscard]] D3D12_CPU_DESCRIPTOR_HANDLE GetFinalRenderTargetView(UINT frame) const;
    [[nodiscard]] D3D12_CPU_DESCRIPTOR_HANDLE GetIntermediateRenderTargetView() const;

    [[nodiscard]] ComPtr<ID3D11On12Device>    GetD3D11On12Device() const;
    [[nodiscard]] ComPtr<ID3D11Device>        GetD3D11Device() const;
    [[nodiscard]] ComPtr<ID3D11DeviceContext> GetD3D11DeviceContext() const;

private:
    static UINT const   AGILITY_SDK_VERSION;
    static LPCSTR const AGILITY_SDK_PATH;

    NativeClient* client;

    ComPtr<ID3D12Device5>      device;
    ComPtr<D3D12MA::Allocator> allocator;
    ComPtr<ID3D12InfoQueue1>   infoQueue;
    ComPtr<ID3D12CommandQueue> commandQueue;
    ComPtr<IDXGISwapChain3>    swapChain;

    DescriptorHeap                                  rtvHeap;
    std::array<ComPtr<ID3D12Resource>, FRAME_COUNT> finalRenderTargets;

    ComPtr<ID3D11On12Device>    d3d11On12Device;
    ComPtr<ID3D11Device>        d3d11Device;
    ComPtr<ID3D11DeviceContext> d3d11DeviceContext;

    UINT                            frameIndex = 0;
    HANDLE                          fenceEvent = {};
    ComPtr<ID3D12Fence>             fence;
    std::array<UINT64, FRAME_COUNT> fenceValues = {0};

#ifdef NATIVE_DEBUG
    D3D12MessageFunc debugCallback; DWORD callbackCookie{};
#endif

#ifdef USE_NSIGHT_AFTERMATH
    GpuCrashTracker::MarkerMap markerMap      = {};
    ShaderDatabase             shaderDatabase = {};
    GpuCrashTracker            gpuCrashTracker;

public:
    void InitializeGpuCrashTracker();
    void InitializeAftermath() const;
    void SetUpCommandListForAftermath(ComPtr<ID3D12GraphicsCommandList> const& commandList) const;
    void SetUpShaderForAftermath(ComPtr<IDxcResult> const& result);

private:
#endif

    void InitializeFences();
    void CreateD3D11On12Device();
    void CheckRaytracingSupport() const;
};

#ifdef USE_NSIGHT_AFTERMATH
#define VG_SHADER_REGISTRY(client) [&client](ComPtr<IDxcResult> result){(client).GetContext().SetUpShaderForAftermath(result);} // NOLINT(bugprone-macro-parentheses)
#else
#define VG_SHADER_REGISTRY(client) [&client](ComPtr<IDxcResult>){(void)(client);} // NOLINT(bugprone-macro-parentheses)
#endif
