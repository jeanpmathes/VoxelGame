//  <copyright file="NativeClient.h" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#pragma once

#include "DXApp.h"

#include "Common.h"
#include "Space.h"

using Microsoft::WRL::ComPtr;

class NativeClient final : public DXApp
{
public:
    NativeClient(UINT width, UINT height, std::wstring name, Configuration configuration);

    [[nodiscard]] ComPtr<ID3D12Device5> GetDevice() const;

    void OnInit() override;
    void OnUpdate(double delta) override;
    void OnRender(double delta) override;
    void OnDestroy() override;

    void OnSizeChanged(UINT width, UINT height, bool minimized) override;
    void OnWindowMoved(int xPos, int yPos) override;

    /**
     * Set the resolution of the space viewport. This has no effect on the window size.
     */
    void SetResolution(UINT32 width, UINT32 height);
    /**
     * Toggle fullscreen mode.
     */
    void ToggleFullscreen() const;

    /**
     * Set the mouse position in client coordinates.
     */
    void SetMousePosition(POINT position) const;

    /**
     * Get the space that is being rendered.
     */
    Space* GetSpace();

    void WaitForGPU();
    void MoveToNextFrame();

private:
    static constexpr UINT FRAME_COUNT = 2;
    static const float CLEAR_COLOR[4];
    static const float LETTERBOX_COLOR[4];

    struct PostVertex
    {
        DirectX::XMFLOAT4 position;
        DirectX::XMFLOAT2 uv;
    };

    Resolution m_resolution;

    D3D12MessageFunc m_debugCallback;
    DWORD m_callbackCookie{};

    CD3DX12_VIEWPORT m_spaceViewport;
    CD3DX12_RECT m_spaceScissorRect;
    ComPtr<ID3D12CommandAllocator> m_spaceCommandAllocators[FRAME_COUNT];
    ComPtr<ID3D12RootSignature> m_spaceRootSignature;
    ComPtr<ID3D12PipelineState> m_spacePipelineState;
    ComPtr<ID3D12GraphicsCommandList4> m_spaceCommandList;

    Space m_space;

    CD3DX12_VIEWPORT m_postViewport;
    CD3DX12_RECT m_postScissorRect;
    ComPtr<ID3D12CommandAllocator> m_postCommandAllocators[FRAME_COUNT];
    ComPtr<ID3D12RootSignature> m_postRootSignature;
    ComPtr<ID3D12PipelineState> m_postPipelineState;
    ComPtr<ID3D12GraphicsCommandList4> m_postCommandList;
    ComPtr<ID3D12Resource> m_postVertexBuffer;
    D3D12_VERTEX_BUFFER_VIEW m_postVertexBufferView{};

    ComPtr<IDXGISwapChain3> m_swapChain;
    ComPtr<ID3D12Device5> m_device;
    ComPtr<ID3D12InfoQueue1> m_infoQueue;
    ComPtr<ID3D12CommandQueue> m_commandQueue;

    ComPtr<ID3D12Resource> m_renderTargets[FRAME_COUNT];
    ComPtr<ID3D12Resource> m_intermediateRenderTarget;
    ComPtr<ID3D12DescriptorHeap> m_rtvHeap;
    ComPtr<ID3D12DescriptorHeap> m_srvHeap;
    UINT m_rtvDescriptorSize;
    UINT m_srvDescriptorSize;

    ComPtr<ID3D12DescriptorHeap> m_dsvHeap;
    ComPtr<ID3D12Resource> m_depthStencilBuffer;

    UINT m_frameIndex;
    HANDLE m_fenceEvent{};
    ComPtr<ID3D12Fence> m_fence;
    UINT64 m_fenceValues[FRAME_COUNT];

    bool m_windowVisible;
    bool m_windowedMode;

    void CheckRaytracingSupport() const;

    void LoadDevice();
    void LoadPipeline();
    void CreateDepthBuffer();
    void SetupSizeDependentResources();
    void SetupSpaceResolutionDependentResources();
    void PopulateCommandLists();
    void UpdatePostViewAndScissor();
};
