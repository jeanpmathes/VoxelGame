//  <copyright file="DXApp.cpp" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#include "stdafx.h"

#include <filesystem>

using namespace Microsoft::WRL;

namespace
{
    HCURSOR LoadCursorFromEnum(const MouseCursor cursor)
    {
        const TCHAR* name;
        switch (cursor)
        {
        case MouseCursor::ARROW:
            name = IDC_ARROW;
            break;
        case MouseCursor::I_BEAM:
            name = IDC_IBEAM;
            break;
        case MouseCursor::SIZE_NS:
            name = IDC_SIZENS;
            break;
        case MouseCursor::SIZE_WE:
            name = IDC_SIZEWE;
            break;
        case MouseCursor::SIZE_NWSE:
            name = IDC_SIZENWSE;
            break;
        case MouseCursor::SIZE_NESW:
            name = IDC_SIZENESW;
            break;
        case MouseCursor::SIZE_ALL:
            name = IDC_SIZEALL;
            break;
        case MouseCursor::NO:
            name = IDC_NO;
            break;
        case MouseCursor::WAIT:
            name = IDC_WAIT;
            break;
        case MouseCursor::HAND:
            name = IDC_HAND;
            break;
        default:
            throw NativeException("Cursor not implemented.");
        }

        const HCURSOR handle = LoadCursor(nullptr, name);
        CHECK_RETURN(handle);

        return handle;
    }

    std::map<MouseCursor, HCURSOR> LoadAllCursors()
    {
        std::map<MouseCursor, HCURSOR> cursors;

        for (int i = 0; i < static_cast<int>(MouseCursor::COUNT); i++)
        {
            cursors[static_cast<MouseCursor>(i)] = LoadCursorFromEnum(static_cast<MouseCursor>(i));
        }

        return cursors;
    }
}

DXApp::DXApp(const Configuration& configuration) :
    m_title(configuration.title),
    m_icon(configuration.icon),
    m_configuration(configuration),
    m_width(std::max(configuration.width, Win32Application::MINIMUM_WINDOW_WIDTH)),
    m_height(std::max(configuration.height, Win32Application::MINIMUM_WINDOW_HEIGHT)),
    m_aspectRatio(0.0f),
    m_windowBounds{0, 0, 0, 0},
    m_tearingSupport(false),
    m_mainThreadId(std::this_thread::get_id())
{
    UpdateForSizeChange(m_width, m_height);
    CheckTearingSupport();
}

DXApp::~DXApp() = default;

void DXApp::Tick(const CycleFlags flags, const bool timer)
{
    if (m_inTick) return;
    m_inTick = true;

    if (!timer && m_isUpdateTimerRunning)
    {
        CHECK_RETURN(KillTimer(Win32Application::GetHwnd(), IDT_UPDATE));
        m_isUpdateTimerRunning = false;
    }
    
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

    m_inTick = false;
}

void DXApp::Init()
{
    m_mouseCursors = LoadAllCursors();
    
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
    m_totalRenderTime += delta;

    m_cycle = Cycle::UPDATE;

    m_configuration.onUpdate(delta);
    OnUpdate(delta);

    m_cycle = std::nullopt;
}

void DXApp::Render(const StepTimer& timer)
{
    if (m_updateTimer.GetFrameCount() == 0) return;

    const double delta = timer.GetElapsedSeconds();
    m_totalRenderTime += delta;

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

    if (m_mouseLocked)
    {
        SetMouseLock(true);
    }
}

void DXApp::HandleWindowMoved(const int xPos, const int yPos)
{
    OnWindowMoved(xPos, yPos);

    if (m_mouseLocked)
    {
        SetMouseLock(true);
    }
}

void DXApp::HandleActiveStateChange(const bool active) const
{
    m_configuration.onActiveStateChange(active);
}

void DXApp::OnSizeMove(const bool enter)
{
    if (enter)
    {
        CHECK_RETURN(SetTimer(
            Win32Application::GetHwnd(),
            IDT_UPDATE,
            m_updateTimer.GetTargetElapsedMilliseconds(),
            nullptr));
        m_isUpdateTimerRunning = true;
    }
    else if (m_isUpdateTimerRunning)
    {
        CHECK_RETURN(KillTimer(Win32Application::GetHwnd(), IDT_UPDATE));
        m_isUpdateTimerRunning = false;
    }
}

void DXApp::OnTimer(const UINT_PTR id)
{
    if (id == IDT_UPDATE)
    {
        Tick(ALLOW_UPDATE, true);
    }
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

void DXApp::DoCursorSet() const
{
    SetCursor(m_mouseCursors.at(m_mouseCursor));
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

void DXApp::SetMouseCursor(const MouseCursor cursor)
{
    m_mouseCursor = cursor;
}

void DXApp::SetMouseLock(const bool lock)
{
    if (lock)
    {
        RECT rect;
        TRY_DO(GetWindowRect(Win32Application::GetHwnd(), &rect));

        TRY_DO(ClipCursor(&rect));
    }
    else
    {
        TRY_DO(ClipCursor(nullptr));
    }

    if (m_mouseLocked != lock)
    {
        // The function uses a display count, thus repeated calls would cause incorrect behavior.
        ShowCursor(!lock);
    }

    m_mouseLocked = lock;
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

ComPtr<IDXGIAdapter1> DXApp::GetHardwareAdapter(
    const ComPtr<IDXGIFactory4>& dxgiFactory,
    const ComPtr<ID3D12DeviceFactory>& deviceFactory,
    const bool requestHighPerformanceAdapter)
{
    ComPtr<IDXGIAdapter1> adapter;

    ComPtr<IDXGIFactory6> factory6;
    if (SUCCEEDED(dxgiFactory->QueryInterface(IID_PPV_ARGS(&factory6))))
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
            TRY_DO(adapter->GetDesc1(&desc));

            if (desc.Flags & DXGI_ADAPTER_FLAG_SOFTWARE)
            {
                continue;
            }

            // Instead of passing the useless device, nullptr and __uuidof(ID3D12Device) should be passed.
            // The current version of PIX (2312.08) does not support this.

            ComPtr<ID3D12Device> uselessDevice;
            if (SUCCEEDED(
                deviceFactory->CreateDevice(adapter.Get(), D3D_FEATURE_LEVEL_12_2, IID_PPV_ARGS(&uselessDevice))))
            {
                break;
            }
        }
    }
    
    if (adapter.Get() == nullptr)
    {
        for (UINT adapterIndex = 0; SUCCEEDED(dxgiFactory->EnumAdapters1(adapterIndex, &adapter)); ++adapterIndex)
        {
            DXGI_ADAPTER_DESC1 desc;
            TRY_DO(adapter->GetDesc1(&desc));

            if (desc.Flags & DXGI_ADAPTER_FLAG_SOFTWARE)
            {
                continue;
            }

            // Instead of passing the useless device, nullptr and __uuidof(ID3D12Device) should be passed.
            // The current version of PIX (2312.08) does not support this.

            ComPtr<ID3D12Device> uselessDevice;
            if (SUCCEEDED(
                deviceFactory->CreateDevice(adapter.Get(), D3D_FEATURE_LEVEL_12_2, IID_PPV_ARGS(&uselessDevice))))
            {
                break;
            }
        }
    }

    return adapter;
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

    const bool isTearingConfigured = static_cast<bool>(m_configuration.options & ConfigurationOptions::ALLOW_TEARING);
    m_tearingSupport = SUCCEEDED(hr) && allowTearing && isTearingConfigured;
}
