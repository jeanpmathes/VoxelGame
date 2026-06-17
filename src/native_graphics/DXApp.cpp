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

        for (int i = 0; i < static_cast<int>(MouseCursor::COUNT); i++) cursors[static_cast<MouseCursor>(i)] = LoadCursorFromEnum(static_cast<MouseCursor>(i));

        return cursors;
    }
}

DXApp::DXApp(::Configuration const& configuration)
    : hooks(
            {
                .onRenderUpdate = configuration.onRenderUpdate,
                .onLogicUpdate  = configuration.onLogicUpdate,

                .onInit    = configuration.onInit,
                .onDestroy = configuration.onDestroy,

                .canClose = configuration.canClose,

                .onKeyDown     = configuration.onKeyDown,
                .onKeyUp       = configuration.onKeyUp,
                .onChar        = configuration.onChar,
                .onMouseMove   = configuration.onMouseMove,
                .onMouseScroll = configuration.onMouseScroll,

                .onResize            = configuration.onResize,
                .onActiveStateChange = configuration.onActiveStateChange
            })
  , configurationOptions(configuration.options)
  , title(configuration.title)
  , icon(configuration.icon)
  , applicationName(configuration.applicationName)
  , applicationVersion(configuration.applicationVersion)
  , applicationLocale(configuration.applicationLocale)
  , baseLogicUpdatesPerSecond(configuration.baseLogicUpdatesPerSecond)
  , renderScale(configuration.renderScale)
  , width(std::max(configuration.width, Win32Application::MINIMUM_WINDOW_WIDTH))
  , height(std::max(configuration.height, Win32Application::MINIMUM_WINDOW_HEIGHT))
{
    UpdateForSizeChange(width, height);
    CheckTearingSupport();
}

DXApp::~DXApp() = default;

bool DXApp::HasFlag(CycleFlags value, CycleFlags flag) { return static_cast<bool>(static_cast<int>(value) & static_cast<int>(flag)); }

void DXApp::Update(CycleFlags const flags, bool const timer)
{
    if (inUpdate) return;
    inUpdate = true;

    if (!timer && isUpdateTimerRunning)
    {
        CheckReturn(KillTimer(Win32Application::GetWindowHandle(), IDT_UPDATE));
        isUpdateTimerRunning = false;
    }

    if (HasFlag(flags, CycleFlags::ALLOW_LOGIC_UPDATE)) logicTimer.Tick([this] { Update(logicTimer); });

    if (HasFlag(flags, CycleFlags::ALLOW_RENDER_UPDATE)) renderTimer.Tick([this] { RenderUpdate(renderTimer); });

    inUpdate = false;
}

void DXApp::Init()
{
    Require(!cycle.has_value());
    cycle = Cycle::INITIALIZATION;

    mouseCursors = LoadAllCursors();

    OnPreInitialization();

    hooks.onInit();

    OnPostInitialization();

    baseLogicUpdateTarget = 1.0 / static_cast<double>(std::max(baseLogicUpdatesPerSecond, 1LL));

    logicTimer.SetFixedTimeStep(true);
    logicTimer.SetTargetElapsedSeconds(baseLogicUpdateTarget);

    renderTimer.SetFixedTimeStep(false);

    OnInitializationComplete();

    cycle = std::nullopt;
}

void DXApp::Update(StepTimer const& timer)
{
    double const delta       = timer.GetElapsedSeconds();
    double const scaledDelta = delta * timeScale;

    Require(!cycle.has_value());
    cycle = Cycle::LOGIC_UPDATE;

    hooks.onLogicUpdate(delta, scaledDelta);
    OnLogicUpdate();

    cycle = std::nullopt;
}

void DXApp::RenderUpdate(StepTimer const& timer)
{
    if (logicTimer.GetFrameCount() == 0) return;

    double const delta       = timer.GetElapsedSeconds();
    double const scaledDelta = delta * timeScale;

    totalRealRenderUpdateTime   += delta;
    totalScaledRenderUpdateTime += scaledDelta;

    Require(!cycle.has_value());
    cycle = Cycle::RENDER_UPDATE;

    OnPreRenderUpdate();
    hooks.onRenderUpdate(delta, scaledDelta);
    OnRenderUpdate();

    cycle = std::nullopt;
}

void DXApp::Destroy()
{
    OnDestroy();

    Require(!cycle.has_value());
    cycle = Cycle::DESTROY;

    hooks.onDestroy();

    // After this call, we remain in the destroy phase.
}

bool DXApp::CanClose() const { return hooks.canClose(); }

void DXApp::HandleSizeChanged(UINT const newWidth, UINT const newHeight, bool const minimized)
{
    OnSizeChanged(newWidth, newHeight, minimized);
    hooks.onResize(newWidth, newHeight);

    if (mouseLocked) SetMouseLock(true);
}

void DXApp::HandleWindowMoved(int const xPos, int const yPos)
{
    OnWindowMoved(xPos, yPos);

    if (mouseLocked) SetMouseLock(true);
}

void DXApp::HandleActiveStateChange(bool const active)
{
    isActive = active;

    hooks.onActiveStateChange(active);
}

void DXApp::OnSizeMove(bool const enter)
{
    if (enter)
    {
        CheckReturn(SetTimer(Win32Application::GetWindowHandle(), IDT_UPDATE, logicTimer.GetTargetElapsedMilliseconds(), nullptr));
        isUpdateTimerRunning = true;
    }
    else if (isUpdateTimerRunning)
    {
        CheckReturn(KillTimer(Win32Application::GetWindowHandle(), IDT_UPDATE));
        isUpdateTimerRunning = false;
    }
}

void DXApp::OnTimer(UINT_PTR const id) { if (id == IDT_UPDATE) Update(CycleFlags::ALLOW_LOGIC_UPDATE, true); }

void DXApp::OnKeyDown(UINT8 const param) const { hooks.onKeyDown(param); }

void DXApp::OnKeyUp(UINT8 const param) const { hooks.onKeyUp(param); }

void DXApp::OnChar(UINT16 const c) const { hooks.onChar(c); }

void DXApp::OnMouseMove(int const x, int const y)
{
    xMousePosition = x;
    yMousePosition = y;

    hooks.onMouseMove(x, y);
}

void DXApp::OnMouseWheel(double const delta) const { hooks.onMouseScroll(delta); }

void DXApp::DoCursorSet() const { SetCursor(mouseCursors.at(mouseCursor)); }

void DXApp::SetWindowBounds(int const left, int const top, int const right, int const bottom)
{
    windowBounds.left   = static_cast<LONG>(left);
    windowBounds.top    = static_cast<LONG>(top);
    windowBounds.right  = static_cast<LONG>(right);
    windowBounds.bottom = static_cast<LONG>(bottom);
}

void DXApp::UpdateForSizeChange(UINT const clientWidth, UINT const clientHeight)
{
    width  = clientWidth;
    height = clientHeight;

    aspectRatio = static_cast<float>(clientWidth) / static_cast<float>(clientHeight);
}

void DXApp::SetMousePosition(POINT position)
{
    if (!isActive) return;

    xMousePosition = position.x;
    yMousePosition = position.y;

    TryDo(ClientToScreen(Win32Application::GetWindowHandle(), &position));
    TryDo(SetCursorPos(position.x, position.y));
}

void DXApp::SetMouseCursor(MouseCursor const cursor) { mouseCursor = cursor; }

void DXApp::SetMouseLock(bool const lock)
{
    if (lock)
    {
        RECT rect;
        TryDo(GetWindowRect(Win32Application::GetWindowHandle(), &rect));

        TryDo(ClipCursor(&rect));
    }
    else TryDo(ClipCursor(nullptr));

    if (mouseLocked != lock)
        // The function uses a display count, thus repeated calls would cause incorrect behavior.
        ShowCursor(!lock);

    mouseLocked = lock;
}

float DXApp::GetAspectRatio() const { return aspectRatio; }

void DXApp::SetTimeScale(double const scale)
{
    Require(scale > 0.0);

    // Because the timer takes the targeted elapsed time per update, we need to divide by the timescale.
    // For example, a timescale of 2.0 means we want to run logic updates twice as fast, thus the target elapsed time per update is halved.

    timeScale = scale;
    logicTimer.SetTargetElapsedSeconds(baseLogicUpdateTarget / timeScale);
}

std::optional<DXApp::Cycle> DXApp::GetCycle() const
{
    if (mainThreadId == std::this_thread::get_id()) return cycle;

    return Cycle::WORKER;
}

void DXApp::SetCustomWindowText(LPCWSTR const text) const
{
    std::wstring const windowText = title + L": " + text;
    SetWindowText(Win32Application::GetWindowHandle(), windowText.c_str());
}

void DXApp::CheckTearingSupport()
{
    ComPtr<IDXGIFactory6> factory;
    HRESULT               hr = CreateDXGIFactory1(IID_PPV_ARGS(&factory));

    bool allowTearing = false;
    if (SUCCEEDED(hr)) hr = factory->CheckFeatureSupport(DXGI_FEATURE_PRESENT_ALLOW_TEARING, &allowTearing, sizeof(allowTearing));

    auto const isTearingConfigured = static_cast<bool>(configurationOptions & ConfigurationOptions::ALLOW_TEARING);
    tearingSupport                 = SUCCEEDED(hr) && allowTearing && isTearingConfigured;
}
