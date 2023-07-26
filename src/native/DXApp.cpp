//  <copyright file="DXApp.cpp" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#include "stdafx.h"

#include <filesystem>

using namespace Microsoft::WRL;

DXApp::DXApp(const Configuration configuration) :
    m_width(max(configuration.width, Win32Application::MINIMUM_WINDOW_WIDTH)),
    m_height(max(configuration.height, Win32Application::MINIMUM_WINDOW_HEIGHT)),
    m_aspectRatio(0.0f),
    m_windowBounds{0, 0, 0, 0},
    m_tearingSupport(false),
    m_title(configuration.title),
    m_configuration(configuration),
    m_mainThreadId(std::this_thread::get_id())
{
    UpdateForSizeChange(m_width, m_height);
    CheckTearingSupport();
}

DXApp::~DXApp() = default;

void DXApp::Tick(const CycleFlags flags)
{
    if (flags & ALLOW_UPDATE)
    {
        m_updateTimer.Tick([&]
        {
            Update(m_updateTimer);
        });
    }

    if (flags & ALLOW_RENDER)
    {
        m_renderTimer.Tick([&]
        {
            Render(m_renderTimer);
        });
    }
}

void DXApp::Init()
{
    OnInit();

    m_configuration.onInit();

    OnPostInit();

    m_updateTimer.SetFixedTimeStep(true);
    m_updateTimer.SetTargetElapsedSeconds(1.0 / 60.0);

    m_renderTimer.SetFixedTimeStep(false);
}

void DXApp::Update(const StepTimer& timer)
{
    const double delta = timer.GetElapsedSeconds();

    m_cycle = Cycle::UPDATE;

    m_configuration.onUpdate(delta);
    OnUpdate(delta);

    m_cycle = std::nullopt;
}

void DXApp::Render(const StepTimer& timer)
{
    if (m_updateTimer.GetFrameCount() == 0) return;

    const double delta = timer.GetElapsedSeconds();

    m_cycle = Cycle::RENDER;

    OnPreRender();
    m_configuration.onRender(delta);
    OnRender(delta);

    m_cycle = std::nullopt;
}

void DXApp::Destroy()
{
    OnDestroy();
    m_configuration.onDestroy();
}

bool DXApp::CanClose() const
{
    return m_configuration.canClose();
}

void DXApp::HandleSizeChanged(const UINT width, const UINT height, const bool minimized)
{
    OnSizeChanged(width, height, minimized);
    m_configuration.onResize(width, height);
}

void DXApp::HandleWindowMoved(const int xPos, const int yPos)
{
    OnWindowMoved(xPos, yPos);
}

void DXApp::HandleActiveStateChange(const bool active) const
{
    m_configuration.onActiveStateChange(active);
}

void DXApp::OnKeyDown(const UINT8 param) const
{
    m_configuration.onKeyDown(param);
}

void DXApp::OnKeyUp(const UINT8 param) const
{
    m_configuration.onKeyUp(param);
}

void DXApp::OnChar(const UINT16 c) const
{
    m_configuration.onChar(c);
}

void DXApp::OnMouseMove(const int x, const int y)
{
    m_xMousePosition = x;
    m_yMousePosition = y;

    m_configuration.onMouseMove(x, y);
}

void DXApp::OnMouseWheel(const double delta) const
{
    m_configuration.onMouseScroll(delta);
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

void DXApp::SetMouseCursor(const MouseCursor cursor) const
{
    const TCHAR* cursorName;
    switch (cursor)
    {
    case MouseCursor::ARROW:
        cursorName = IDC_ARROW;
        break;
    case MouseCursor::I_BEAM:
        cursorName = IDC_IBEAM;
        break;
    case MouseCursor::SIZE_NS:
        cursorName = IDC_SIZENS;
        break;
    case MouseCursor::SIZE_WE:
        cursorName = IDC_SIZEWE;
        break;
    case MouseCursor::SIZE_NWSE:
        cursorName = IDC_SIZENWSE;
        break;
    case MouseCursor::SIZE_NESW:
        cursorName = IDC_SIZENESW;
        break;
    case MouseCursor::SIZE_ALL:
        cursorName = IDC_SIZEALL;
        break;
    case MouseCursor::NO:
        cursorName = IDC_NO;
        break;
    case MouseCursor::WAIT:
        cursorName = IDC_WAIT;
        break;
    case MouseCursor::HAND:
        cursorName = IDC_HAND;
        break;
    default:
        throw NativeException("Cursor not implemented.");
    }

    const HCURSOR cursorHandle = LoadCursor(nullptr, cursorName);
    CHECK_RETURN(cursorHandle);

    SetCursor(cursorHandle);
}

float DXApp::GetAspectRatio() const
{
    return m_aspectRatio;
}

std::optional<DXApp::Cycle> DXApp::GetCycle() const
{
    if (m_mainThreadId == std::this_thread::get_id()) return m_cycle;

    return Cycle::WORKER;
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
