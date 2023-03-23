//  <copyright file="DXApp.cpp" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#include "stdafx.h"

using namespace Microsoft::WRL;

DXApp::DXApp(const UINT width, const UINT height, const std::wstring name, const Configuration configuration) :
    m_width(width),
    m_height(height),
    m_aspectRatio(0.0f),
    m_windowBounds{0, 0, 0, 0},
    m_tearingSupport(false),
    m_title(name),
    m_configuration(configuration)
{
    WCHAR assetsPath[512];
    GetAssetsPath(assetsPath, _countof(assetsPath));
    m_assetsPath = assetsPath;
    m_assetsPath += L"Shaders\\";

    UpdateForSizeChange(width, height);
    CheckTearingSupport();
}

DXApp::~DXApp() = default;

void DXApp::Tick()
{
    m_updateTimer.Tick([&]
    {
        Update(m_updateTimer);
    });

    m_renderTimer.Tick([&]
    {
        Render(m_renderTimer);
    });
}

void DXApp::Init()
{
    OnInit();
    m_configuration.onInit();

    m_updateTimer.SetFixedTimeStep(true);
    m_updateTimer.SetTargetElapsedSeconds(1.0 / 60.0);

    m_renderTimer.SetFixedTimeStep(false);
}

void DXApp::Update(const StepTimer& timer)
{
    const double delta = timer.GetElapsedSeconds();

    m_configuration.onUpdate(delta);
    OnUpdate(delta);
}

void DXApp::Render(const StepTimer& timer)
{
    if (m_updateTimer.GetFrameCount() == 0) return;

    const double delta = timer.GetElapsedSeconds();

    m_configuration.onRender(delta);
    OnRender(delta);
}

void DXApp::Destroy()
{
    OnDestroy();
    m_configuration.onDestroy();
}

void DXApp::HandleSizeChanged(const UINT width, const UINT height, const bool minimized)
{
    OnSizeChanged(width, height, minimized);
}

void DXApp::HandleWindowMoved(const int xPos, const int yPos)
{
    OnWindowMoved(xPos, yPos);
}

void DXApp::OnKeyDown(const UINT8 param) const
{
    m_configuration.onKeyDown(param);
}

void DXApp::OnKeyUp(const UINT8 param) const
{
    m_configuration.onKeyUp(param);
}

void DXApp::OnMouseMove(const int x, const int y)
{
    m_xMousePosition = x;
    m_yMousePosition = y;
}

void DXApp::SetWindowBounds(const int left, const int top, const int right, const int bottom)
{
    m_windowBounds.left = static_cast<LONG>(left);
    m_windowBounds.top = static_cast<LONG>(top);
    m_windowBounds.right = static_cast<LONG>(right);
    m_windowBounds.bottom = static_cast<LONG>(bottom);
}

void DXApp::UpdateForSizeChange(const UINT clientWidth, const UINT clientHeight)
{
    m_width = clientWidth;
    m_height = clientHeight;
    m_aspectRatio = static_cast<float>(clientWidth) / static_cast<float>(clientHeight);
}

float DXApp::GetAspectRatio() const
{
    return m_aspectRatio;
}

std::wstring DXApp::GetAssetFullPath(const LPCWSTR assetName) const
{
    return m_assetsPath + assetName;
}

_Use_decl_annotations_

void DXApp::GetHardwareAdapter(
    IDXGIFactory1* pFactory,
    IDXGIAdapter1** ppAdapter,
    const bool requestHighPerformanceAdapter) const
{
    *ppAdapter = nullptr;

    ComPtr<IDXGIAdapter1> adapter;

    ComPtr<IDXGIFactory6> factory6;
    if (SUCCEEDED(pFactory->QueryInterface(IID_PPV_ARGS(&factory6))))
    {
        for (
            UINT adapterIndex = 0;
            SUCCEEDED(factory6->EnumAdapterByGpuPreference(
                adapterIndex,
                requestHighPerformanceAdapter == true ? DXGI_GPU_PREFERENCE_HIGH_PERFORMANCE :
                DXGI_GPU_PREFERENCE_UNSPECIFIED,
                IID_PPV_ARGS(&adapter)));
            ++adapterIndex)
        {
            DXGI_ADAPTER_DESC1 desc;
            adapter->GetDesc1(&desc);

            if (desc.Flags & DXGI_ADAPTER_FLAG_SOFTWARE)
            {
                continue;
            }

            if (SUCCEEDED(D3D12CreateDevice(adapter.Get(), D3D_FEATURE_LEVEL_12_2, _uuidof(ID3D12Device), nullptr)))
            {
                break;
            }
        }
    }

    if (adapter.Get() == nullptr)
    {
        for (UINT adapterIndex = 0; SUCCEEDED(pFactory->EnumAdapters1(adapterIndex, &adapter)); ++adapterIndex)
        {
            DXGI_ADAPTER_DESC1 desc;
            adapter->GetDesc1(&desc);

            if (desc.Flags & DXGI_ADAPTER_FLAG_SOFTWARE)
            {
                continue;
            }

            if (SUCCEEDED(D3D12CreateDevice(adapter.Get(), D3D_FEATURE_LEVEL_12_2, _uuidof(ID3D12Device), nullptr)))
            {
                break;
            }
        }
    }

    *ppAdapter = adapter.Detach();
}

void DXApp::SetCustomWindowText(const LPCWSTR text) const
{
    const std::wstring windowText = m_title + L": " + text;
    SetWindowText(Win32Application::GetHwnd(), windowText.c_str());
}

void DXApp::CheckTearingSupport()
{
    ComPtr<IDXGIFactory6> factory;
    HRESULT hr = CreateDXGIFactory1(IID_PPV_ARGS(&factory));

    BOOL allowTearing = FALSE;
    if (SUCCEEDED(hr))
    {
        hr = factory->CheckFeatureSupport(DXGI_FEATURE_PRESENT_ALLOW_TEARING, &allowTearing, sizeof(allowTearing));
    }

    m_tearingSupport = SUCCEEDED(hr) && allowTearing && m_configuration.allowTearing;
}
