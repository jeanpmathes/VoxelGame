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
    HCURSOR LoadCursorFromEnum(MouseCursor const cursor)
    {
        TCHAR const* name;
        switch (cursor)
        {
            using enum MouseCursor;

        case ARROW:
            name = IDC_ARROW;
            break;
        case I_BEAM:
            name = IDC_IBEAM;
            break;
        case SIZE_NS:
            name = IDC_SIZENS;
            break;
        case SIZE_WE:
            name = IDC_SIZEWE;
            break;
        case SIZE_NWSE:
            name = IDC_SIZENWSE;
            break;
        case SIZE_NESW:
            name = IDC_SIZENESW;
            break;
        case SIZE_ALL:
            name = IDC_SIZEALL;
            break;
        case NO:
            name = IDC_NO;
            break;
        case WAIT:
            name = IDC_WAIT;
            break;
        case HAND:
            name = IDC_HAND;
            break;
        default:
            throw NativeException("Cursor not implemented.");
        }

        return CheckReturn(LoadCursor(nullptr, name));
    }

    std::map<MouseCursor, HCURSOR> LoadAllCursors()
    {
        std::map<MouseCursor, HCURSOR> cursors;

        for (int i                               = 0; i < static_cast<int>(MouseCursor::COUNT); i++)
            cursors[static_cast<MouseCursor>(i)] = LoadCursorFromEnum(static_cast<MouseCursor>(i));

        return cursors;
    }
}

DXApp::DXApp(Configuration const& configuration)
    : m_title(configuration.title)
  , m_icon(configuration.icon)
  , m_configuration(configuration)
  , m_width(std::max(configuration.width, Win32Application::MINIMUM_WINDOW_WIDTH))
  , m_height(std::max(configuration.height, Win32Application::MINIMUM_WINDOW_HEIGHT))
{
    UpdateForSizeChange(m_width, m_height);
    CheckTearingSupport();
}

DXApp::~DXApp() = default;

bool DXApp::HasFlag(CycleFlags value, CycleFlags flag)
{
    return static_cast<bool>(static_cast<int>(value) & static_cast<int>(flag));
}

void DXApp::Tick(CycleFlags const flags, bool const timer)
{
    if (m_inTick) return;
    m_inTick = true;

    if (!timer && m_isUpdateTimerRunning)
    {
        CheckReturn(KillTimer(Win32Application::GetHwnd(), IDT_UPDATE));
        m_isUpdateTimerRunning = false;
    }

    if (HasFlag(flags, CycleFlags::ALLOW_UPDATE)) m_updateTimer.Tick([this] { Update(m_updateTimer); });

    if (HasFlag(flags, CycleFlags::ALLOW_RENDER)) m_renderTimer.Tick([this] { Render(m_renderTimer); });

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

void DXApp::Update(StepTimer const& timer)
{
    double const delta = timer.GetElapsedSeconds();
    m_totalRenderTime += delta;

    m_cycle = Cycle::UPDATE;

    m_configuration.onUpdate(delta);
    OnUpdate(delta);

    m_cycle = std::nullopt;
}

void DXApp::Render(StepTimer const& timer)
{
    if (m_updateTimer.GetFrameCount() == 0) return;
    
    double const delta = timer.GetElapsedSeconds();
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

bool DXApp::CanClose() const { return m_configuration.canClose(); }

void DXApp::HandleSizeChanged(UINT const width, UINT const height, bool const minimized)
{
    OnSizeChanged(width, height, minimized);
    m_configuration.onResize(width, height);

    if (m_mouseLocked) SetMouseLock(true);
}

void DXApp::HandleWindowMoved(int const xPos, int const yPos)
{
    OnWindowMoved(xPos, yPos);

    if (m_mouseLocked) SetMouseLock(true);
}

void DXApp::HandleActiveStateChange(bool const active)
{
    m_isActive = active;

    m_configuration.onActiveStateChange(active);
}

void DXApp::OnSizeMove(bool const enter)
{
    if (enter)
    {
        CheckReturn(
            SetTimer(Win32Application::GetHwnd(), IDT_UPDATE, m_updateTimer.GetTargetElapsedMilliseconds(), nullptr));
        m_isUpdateTimerRunning = true;
    }
    else if (m_isUpdateTimerRunning)
    {
        CheckReturn(KillTimer(Win32Application::GetHwnd(), IDT_UPDATE));
        m_isUpdateTimerRunning = false;
    }
}

void DXApp::OnTimer(UINT_PTR const id) { if (id == IDT_UPDATE) Tick(CycleFlags::ALLOW_UPDATE, true); }

void DXApp::OnKeyDown(UINT8 const param) const { m_configuration.onKeyDown(param); }

void DXApp::OnKeyUp(UINT8 const param) const { m_configuration.onKeyUp(param); }

void DXApp::OnChar(UINT16 const c) const { m_configuration.onChar(c); }

void DXApp::OnMouseMove(int const x, int const y)
{
    m_xMousePosition = x;
    m_yMousePosition = y;

    m_configuration.onMouseMove(x, y);
}

void DXApp::OnMouseWheel(double const delta) const { m_configuration.onMouseScroll(delta); }

void DXApp::DoCursorSet() const { SetCursor(m_mouseCursors.at(m_mouseCursor)); }

void DXApp::SetWindowBounds(int const left, int const top, int const right, int const bottom)
{
    m_windowBounds.left   = static_cast<LONG>(left);
    m_windowBounds.top    = static_cast<LONG>(top);
    m_windowBounds.right  = static_cast<LONG>(right);
    m_windowBounds.bottom = static_cast<LONG>(bottom);
}

void DXApp::UpdateForSizeChange(UINT const clientWidth, UINT const clientHeight)
{
    m_width  = clientWidth;
    m_height = clientHeight;

    m_aspectRatio = static_cast<float>(clientWidth) / static_cast<float>(clientHeight);
}

void DXApp::SetMousePosition(POINT position)
{
    if (!m_isActive) return;

    m_xMousePosition = position.x;
    m_yMousePosition = position.y;
    
    TryDo(ClientToScreen(Win32Application::GetHwnd(), &position));
    TryDo(SetCursorPos(position.x, position.y));
}

void DXApp::SetMouseCursor(MouseCursor const cursor) { m_mouseCursor = cursor; }

void DXApp::SetMouseLock(bool const lock)
{
    if (lock)
    {
        RECT rect;
        TryDo(GetWindowRect(Win32Application::GetHwnd(), &rect));

        TryDo(ClipCursor(&rect));
    }
    else TryDo(ClipCursor(nullptr));

    if (m_mouseLocked != lock)
        // The function uses a display count, thus repeated calls would cause incorrect behavior.
        ShowCursor(!lock);

    m_mouseLocked = lock;
}

float DXApp::GetAspectRatio() const { return m_aspectRatio; }

std::optional<DXApp::Cycle> DXApp::GetCycle() const
{
    if (m_mainThreadId == std::this_thread::get_id()) return m_cycle;

    return Cycle::WORKER;
}

ComPtr<IDXGIAdapter1> DXApp::GetHardwareAdapter(
    ComPtr<IDXGIFactory4> const&       dxgiFactory,
    ComPtr<ID3D12DeviceFactory> const& deviceFactory,
    bool const                         requestHighPerformanceAdapter)
{
    ComPtr<IDXGIAdapter1> adapter;

    ComPtr<IDXGIFactory6> factory6;
    if (SUCCEEDED(dxgiFactory->QueryInterface(IID_PPV_ARGS(&factory6))))
        for (UINT adapterIndex = 0; SUCCEEDED(
                 factory6->EnumAdapterByGpuPreference( adapterIndex, requestHighPerformanceAdapter == true ?
                     DXGI_GPU_PREFERENCE_HIGH_PERFORMANCE : DXGI_GPU_PREFERENCE_UNSPECIFIED, IID_PPV_ARGS(&adapter)));
             ++adapterIndex)
        {
            DXGI_ADAPTER_DESC1 desc;
            TryDo(adapter->GetDesc1(&desc));

            if (desc.Flags & DXGI_ADAPTER_FLAG_SOFTWARE) continue;
            
            ComPtr<ID3D12Device> uselessDevice;
            if (SUCCEEDED(
                deviceFactory->CreateDevice(adapter.Get(), D3D_FEATURE_LEVEL_12_2, __uuidof(ID3D12Device), nullptr)))
                break ;
        }

    if (adapter.Get() == nullptr)
        for (UINT adapterIndex = 0; SUCCEEDED(dxgiFactory->EnumAdapters1(adapterIndex, &adapter)); ++adapterIndex)
        {
            DXGI_ADAPTER_DESC1 desc;
            TryDo(adapter->GetDesc1(&desc));

            if (desc.Flags & DXGI_ADAPTER_FLAG_SOFTWARE) continue;

            ComPtr<ID3D12Device> uselessDevice;
            if (SUCCEEDED(
                deviceFactory->CreateDevice(adapter.Get(), D3D_FEATURE_LEVEL_12_2, __uuidof(ID3D12Device), nullptr)))
                break ;
        }

    return adapter;
}

void DXApp::SetCustomWindowText(LPCWSTR const text) const
{
    std::wstring const windowText = m_title + L": " + text;
    SetWindowText(Win32Application::GetHwnd(), windowText.c_str());
}

void DXApp::CheckTearingSupport()
{
    ComPtr<IDXGIFactory6> factory;
    HRESULT               hr = CreateDXGIFactory1(IID_PPV_ARGS(&factory));

    bool allowTearing = false;
    if (SUCCEEDED(hr))
        hr = factory->CheckFeatureSupport(DXGI_FEATURE_PRESENT_ALLOW_TEARING, &allowTearing, sizeof(allowTearing));

    auto const isTearingConfigured = static_cast<bool>(m_configuration.options & ConfigurationOptions::ALLOW_TEARING);
    m_tearingSupport               = SUCCEEDED(hr) && allowTearing && isTearingConfigured;
}
