//  <copyright file="Win32Application.cpp" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#include "stdafx.h"

namespace
{
    [[nodiscard]] WORD GetKeyScanCode(LPARAM const flags)
    {
        WORD const keyFlags = HIWORD(flags);
        WORD       scanCode = LOBYTE(keyFlags);

        if ((keyFlags & KF_EXTENDED) == KF_EXTENDED) scanCode = MAKEWORD(scanCode, 0xE0);

        return scanCode;
    }

    ModifierKeys GetCurrentModifierKeys()
    {
        auto modifiers = ModifierKeys::NONE;

        if (GetKeyState(VK_CONTROL) < 0) modifiers |= ModifierKeys::CONTROL;
        if (GetKeyState(VK_MENU) < 0) modifiers |= ModifierKeys::ALT;
        if (GetKeyState(VK_SHIFT) < 0) modifiers |= ModifierKeys::SHIFT;

        return modifiers;
    }

    DXApp* GetApp(HWND const hWnd)
    {
        return reinterpret_cast<DXApp*>(GetWindowLongPtr(hWnd, GWLP_USERDATA)); // NOLINT(performance-no-int-to-ptr)
    }

    bool ProcessKeyMessage(MSG const& message)
    {
        DXApp const* app = GetApp(message.hwnd);
        if (!app) return false;

        auto       vkCode   = LOWORD(message.wParam);
        auto const keyFlags = HIWORD(message.lParam);

        if (vkCode == VK_LWIN || vkCode == VK_RWIN) return false;

        WORD const scanCode = GetKeyScanCode(message.lParam);

        bool const up     = (keyFlags & KF_UP) == KF_UP;
        bool const repeat = (keyFlags & KF_REPEAT) == KF_REPEAT;

        switch (vkCode)
        {
        case VK_SHIFT:
        case VK_CONTROL:
        case VK_MENU:
            vkCode = LOWORD(MapVirtualKeyW(scanCode, MAPVK_VSC_TO_VK_EX));
            break;
        default:
            break;
        }

        auto const         vk        = static_cast<UINT8>(vkCode);
        ModifierKeys const modifiers = GetCurrentModifierKeys();

        return app->OnKey(vk, !up, repeat, modifiers);
    }
}

HWND               Win32Application::hWindow        = nullptr;
bool               Win32Application::fullscreenMode = false;
RECT               Win32Application::windowRectangle;
size_t             Win32Application::errorModeDepth             = 0;
std::exception_ptr Win32Application::pendingWindowProcException = nullptr;

// ReSharper disable once CppParameterMayBeConst
int Win32Application::Run(DXApp* app, HINSTANCE instance, int const cmdShow)
{
    WNDCLASSEX windowClass    = {};
    windowClass.cbSize        = sizeof(WNDCLASSEX);
    windowClass.style         = CS_HREDRAW | CS_VREDRAW;
    windowClass.lpfnWndProc   = WindowProc;
    windowClass.hInstance     = instance;
    windowClass.hIcon         = app->GetIcon();
    windowClass.hCursor       = nullptr;
    windowClass.lpszClassName = L"NATIVE";

    TryDo(RegisterClassEx(&windowClass) != 0);

    RECT initialWindowRectangle = {.left = 0, .top = 0, .right = static_cast<LONG>(app->GetWidth()), .bottom = static_cast<LONG>(app->GetHeight())};
    TryDo(AdjustWindowRect(&initialWindowRectangle, WS_OVERLAPPEDWINDOW, FALSE));

    hWindow = CreateWindow(
                           windowClass.lpszClassName,
                           app->GetTitle(),
                           WINDOW_STYLE,
                           CW_USEDEFAULT,
                           CW_USEDEFAULT,
                           initialWindowRectangle.right - initialWindowRectangle.left,
                           initialWindowRectangle.bottom - initialWindowRectangle.top,
                           nullptr,
                           nullptr,
                           instance,
                           app);

    app->Init();
    app->Update(DXApp::CycleFlags::ALLOW_INPUT_UPDATE);
    app->Update(DXApp::CycleFlags::ALLOW_LOGIC_UPDATE);
    app->Update(DXApp::CycleFlags::ALLOW_RENDER_UPDATE);

    ShowWindow(hWindow, cmdShow);
    RethrowPendingWindowProcException();

    app->Update(DXApp::CycleFlags::ALLOW_INPUT_AND_RENDER_UPDATE);

    MSG message = {};
    for (;;)
        if (PeekMessage(&message, nullptr, 0, 0, PM_REMOVE))
        {
            if (message.message == WM_QUIT) break;

            bool const isKeyMessage = message.message == WM_KEYDOWN
                                      || message.message == WM_KEYUP
                                      || message.message == WM_SYSKEYDOWN
                                      || message.message == WM_SYSKEYUP;

            bool const isHandled = isKeyMessage && ProcessKeyMessage(message);

            // The key messages would be translated to char messages, even if already handled.
            // This would mean two different handlers would respond to the same event.

            if (!isHandled)
            {
                TranslateMessage(&message);
                DispatchMessage(&message);
            }

            RethrowPendingWindowProcException();
        }
        else app->Update(DXApp::CycleFlags::ALLOW_ALL_UPDATES);

    app->Destroy();

    TryDo(UnregisterClass(windowClass.lpszClassName, instance));

    hWindow = nullptr;

    return static_cast<int>(message.wParam);
}

void Win32Application::ToggleFullscreenWindow(ComPtr<IDXGISwapChain> swapChain)
{
    if (fullscreenMode)
    {
        SetWindowLongPtr(hWindow, GWL_STYLE, WINDOW_STYLE);

        TryDo(
              SetWindowPos(
                           hWindow,
                           HWND_NOTOPMOST,
                           windowRectangle.left,
                           windowRectangle.top,
                           windowRectangle.right - windowRectangle.left,
                           windowRectangle.bottom - windowRectangle.top,
                           SWP_FRAMECHANGED | SWP_NOACTIVATE));

        ShowWindow(hWindow, SW_NORMAL);
    }
    else
    {
        TryDo(GetWindowRect(hWindow, &windowRectangle));

        SetWindowLongPtr(hWindow, GWL_STYLE, WINDOW_FULLSCREEN_STYLE);

        RECT fullscreenWindowRect;
        try
        {
            ComPtr<IDXGIOutput> pOutput;
            TryDo(swapChain->GetContainingOutput(&pOutput), false);

            DXGI_OUTPUT_DESC desc;
            TryDo(pOutput->GetDesc(&desc), false);

            fullscreenWindowRect = desc.DesktopCoordinates;
        }
        catch (HResultException& e)
        {
            UNREFERENCED_PARAMETER(e);

            DEVMODE devMode = {};
            devMode.dmSize  = sizeof(DEVMODE);
            EnumDisplaySettings(nullptr, ENUM_CURRENT_SETTINGS, &devMode);

            fullscreenWindowRect = {
                .left   = devMode.dmPosition.x,
                .top    = devMode.dmPosition.y,
                .right  = devMode.dmPosition.x + static_cast<LONG>(devMode.dmPelsWidth),
                .bottom = devMode.dmPosition.y + static_cast<LONG>(devMode.dmPelsHeight)
            };
        }

        TryDo(
              SetWindowPos(
                           hWindow,
                           HWND_TOPMOST,
                           fullscreenWindowRect.left,
                           fullscreenWindowRect.top,
                           fullscreenWindowRect.right - fullscreenWindowRect.left,
                           fullscreenWindowRect.bottom - fullscreenWindowRect.top,
                           SWP_FRAMECHANGED | SWP_NOACTIVATE));

        ShowWindow(hWindow, SW_MAXIMIZE);
    }

    fullscreenMode = !fullscreenMode;
}

void Win32Application::SetWindowOrderToTopMost(bool const setToTopMost)
{
    RECT windowRect;
    TryDo(GetWindowRect(hWindow, &windowRect));

    TryDo(
          SetWindowPos(
                       hWindow,
                       setToTopMost ? HWND_TOPMOST : HWND_NOTOPMOST,
                       windowRect.left,
                       windowRect.top,
                       windowRect.right - windowRect.left,
                       windowRect.bottom - windowRect.top,
                       SWP_FRAMECHANGED | SWP_NOACTIVATE));
}

void Win32Application::ShowErrorMessage(LPCWSTR const message, LPCWSTR const title)
{
    EnterErrorMode();
    MessageBoxW(hWindow, message, title, MB_OK | MB_ICONERROR | MB_SETFOREGROUND);
    ExitErrorMode();
}

void Win32Application::EnterErrorMode() { ++errorModeDepth; }

void Win32Application::ExitErrorMode() { --errorModeDepth; }

bool Win32Application::IsInErrorMode() { return errorModeDepth > 0; }

LRESULT CALLBACK Win32Application::WindowProc(HWND const hWnd, UINT const message, WPARAM const wParam, LPARAM const lParam)
{
    // On modern Windows systems, exceptions thrown in WindowProc are not guaranteed to propagate well through it.
    // As such, we catch the first exceptions and store it to rethrow later.
    // See: https://learn.microsoft.com/en-us/windows/win32/api/winuser/nc-winuser-wndproc

    try
    {
        return WindowProcImplementation(hWnd, message, wParam, lParam);
    }
    catch (...)
    {
        if (!pendingWindowProcException) pendingWindowProcException = std::current_exception();

        PostQuitMessage(1);

        return 0;
    }
}

LRESULT Win32Application::WindowProcImplementation(HWND const hWnd, UINT const message, WPARAM const wParam, LPARAM const lParam)
{
    auto const app = GetApp(hWnd);
    auto       def = [&] { return DefWindowProc(hWnd, message, wParam, lParam); };

    if (IsInErrorMode()) return def();

    switch (message)
    {
    case WM_CREATE:
    {
        auto const pCreateStruct = reinterpret_cast<LPCREATESTRUCT>(lParam);
        SetWindowLongPtr(hWnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(pCreateStruct->lpCreateParams));

        return 0;
    }

    case WM_MOUSEACTIVATE:
        return MA_ACTIVATEANDEAT;

    case WM_ACTIVATE:
    {
        bool const active = LOWORD(wParam) != WA_INACTIVE;
        if (app) app->HandleActiveStateChange(active);

        return 0;
    }

    case WM_SETFOCUS:
        if (app)
        {
            app->HandleKeyboardFocusChange(true);
            return 0;
        }
        return def();

    case WM_KILLFOCUS:
        if (app)
        {
            app->HandleKeyboardFocusChange(false);
            return 0;
        }
        return def();

    case WM_PAINT:
    {
        if (app)
        {
            app->Update(DXApp::CycleFlags::ALLOW_INPUT_AND_RENDER_UPDATE);
            ValidateRect(hWnd, nullptr);
        }

        return 0;
    }

    case WM_KEYDOWN:
    case WM_KEYUP:
    case WM_SYSKEYDOWN:
    case WM_SYSKEYUP:
        // This is already handled by ProcessKeyMessage in the message loop.
        return def();

    case WM_LBUTTONDOWN:
        if (app)
        {
            INT32 const x = GET_X_LPARAM(lParam);
            INT32 const y = GET_Y_LPARAM(lParam);

            ModifierKeys const modifiers = GetCurrentModifierKeys();

            if (app->OnMouseButton(VK_LBUTTON, true, x, y, modifiers)) return 0;
        }
        return def();

    case WM_LBUTTONUP:
        if (app)
        {
            INT32 const x = GET_X_LPARAM(lParam);
            INT32 const y = GET_Y_LPARAM(lParam);

            ModifierKeys const modifiers = GetCurrentModifierKeys();

            if (app->OnMouseButton(VK_LBUTTON, false, x, y, modifiers)) return 0;
        }
        return def();

    case WM_RBUTTONDOWN:
        if (app)
        {
            INT32 const x = GET_X_LPARAM(lParam);
            INT32 const y = GET_Y_LPARAM(lParam);

            ModifierKeys const modifiers = GetCurrentModifierKeys();

            if (app->OnMouseButton(VK_RBUTTON, true, x, y, modifiers)) return 0;
        }
        return def();

    case WM_RBUTTONUP:
        if (app)
        {
            INT32 const x = GET_X_LPARAM(lParam);
            INT32 const y = GET_Y_LPARAM(lParam);

            ModifierKeys const modifiers = GetCurrentModifierKeys();

            if (app->OnMouseButton(VK_RBUTTON, false, x, y, modifiers)) return 0;
        }
        return def();

    case WM_MBUTTONDOWN:
        if (app)
        {
            INT32 const x = GET_X_LPARAM(lParam);
            INT32 const y = GET_Y_LPARAM(lParam);

            ModifierKeys const modifiers = GetCurrentModifierKeys();

            if (app->OnMouseButton(VK_MBUTTON, true, x, y, modifiers)) return 0;
        }
        return def();

    case WM_MBUTTONUP:
        if (app)
        {
            INT32 const x = GET_X_LPARAM(lParam);
            INT32 const y = GET_Y_LPARAM(lParam);

            ModifierKeys const modifiers = GetCurrentModifierKeys();

            if (app->OnMouseButton(VK_MBUTTON, false, x, y, modifiers)) return 0;
        }
        return def();

    case WM_XBUTTONDOWN:
        if (app)
        {
            INT32 const x = GET_X_LPARAM(lParam);
            INT32 const y = GET_Y_LPARAM(lParam);

            ModifierKeys const modifiers = GetCurrentModifierKeys();

            UINT const button = GET_XBUTTON_WPARAM(wParam);

            // See https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-xbuttondown#return-value

            if (button == XBUTTON1 && app->OnMouseButton(VK_XBUTTON1, true, x, y, modifiers)) return TRUE;
            if (button == XBUTTON2 && app->OnMouseButton(VK_XBUTTON2, true, x, y, modifiers)) return TRUE;
        }
        return def();

    case WM_XBUTTONUP:
        if (app)
        {
            INT32 const x = GET_X_LPARAM(lParam);
            INT32 const y = GET_Y_LPARAM(lParam);

            ModifierKeys const modifiers = GetCurrentModifierKeys();

            UINT const button = GET_XBUTTON_WPARAM(wParam);

            // See https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-xbuttonup#return-value

            if (button == XBUTTON1 && app->OnMouseButton(VK_XBUTTON1, false, x, y, modifiers)) return TRUE;
            if (button == XBUTTON2 && app->OnMouseButton(VK_XBUTTON2, false, x, y, modifiers)) return TRUE;
        }
        return def();

    case WM_CHAR:
        if (app && app->OnChar(static_cast<UINT16>(wParam))) return 0;
        return def();

    case WM_SYSCHAR:
        return def();

    case WM_MOUSEWHEEL:
        if (app)
        {
            double const zDelta = GET_WHEEL_DELTA_WPARAM(wParam);
            double const delta  = zDelta / WHEEL_DELTA;

            POINT point
            {
                .x = GET_X_LPARAM(lParam),
                .y = GET_Y_LPARAM(lParam)
            };

            ScreenToClient(hWnd, &point);

            if (app->OnMouseWheel(point.x, point.y, 0.0, delta)) return 0;
        }
        return def();

    case WM_MOUSEHWHEEL:
        if (app)
        {
            double const zDelta = GET_WHEEL_DELTA_WPARAM(wParam);
            double const delta  = zDelta / WHEEL_DELTA;

            POINT point
            {
                .x = GET_X_LPARAM(lParam),
                .y = GET_Y_LPARAM(lParam)
            };

            ScreenToClient(hWnd, &point);

            if (app->OnMouseWheel(point.x, point.y, delta, 0.0)) return 0;
        }
        return def();

    case WM_MOUSEMOVE:
        if (app)
        {
            auto const xPos = GET_X_LPARAM(lParam);
            auto const yPos = GET_Y_LPARAM(lParam);
            if (app->OnMouseMove(xPos, yPos)) return 0;
        }
        return def();

    case WM_SETCURSOR:
        if (app && LOWORD(lParam) == HTCLIENT)
        {
            app->DoCursorSet();
            return TRUE;
        }
        return def();

    case WM_ENTERSIZEMOVE:
    case WM_ENTERMENULOOP:
        if (app) app->OnSizeMoveMenu(true);
        return 0;

    case WM_EXITSIZEMOVE:
    case WM_EXITMENULOOP:
        if (app) app->OnSizeMoveMenu(false);
        return 0;

    case WM_SIZE:
        if (app)
        {
            RECT windowRect = {};
            TryDo(GetWindowRect(hWnd, &windowRect));
            app->SetWindowBounds(windowRect.left, windowRect.top, windowRect.right, windowRect.bottom);

            RECT clientRect = {};
            TryDo(GetClientRect(hWnd, &clientRect));
            app->HandleSizeChanged(clientRect.right - clientRect.left, clientRect.bottom - clientRect.top, wParam == SIZE_MINIMIZED);
        }
        return 0;

    case WM_MOVE:
        if (app)
        {
            RECT windowRect = {};
            TryDo(GetWindowRect(hWnd, &windowRect));
            app->SetWindowBounds(windowRect.left, windowRect.top, windowRect.right, windowRect.bottom);

            int const xPos = static_cast<short>(LOWORD(lParam));
            int const yPos = static_cast<short>(HIWORD(lParam));
            app->HandleWindowMoved(xPos, yPos);
        }
        return 0;

    case WM_TIMER:
        if (app)
        {
            app->OnTimer(wParam);
            return 0;
        }
        return def();

    case WM_GETMINMAXINFO:
    {
        auto const minmaxInfo        = reinterpret_cast<LPMINMAXINFO>(lParam);
        minmaxInfo->ptMinTrackSize.x = MINIMUM_WINDOW_WIDTH;
        minmaxInfo->ptMinTrackSize.y = MINIMUM_WINDOW_HEIGHT;

        return 0;
    }

    case WM_CLOSE:
        if (app && app->CanClose()) TryDo(DestroyWindow(hWnd));
        return 0;

    case WM_DESTROY:
        PostQuitMessage(0);
        return 0;

    default:
        return def();
    }
}

void Win32Application::RethrowPendingWindowProcException()
{
    if (std::exception_ptr const pending = std::exchange(pendingWindowProcException, nullptr)) std::rethrow_exception(pending);
}
