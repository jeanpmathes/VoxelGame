//  <copyright file="Win32Application.cpp" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#include "stdafx.h"

void*  Win32Application::m_app            = nullptr;
HWND   Win32Application::m_hwnd           = nullptr;
bool   Win32Application::m_fullscreenMode = false;
RECT   Win32Application::m_windowRect;
size_t Win32Application::m_errorModeDepth = 0;

// ReSharper disable once CppParameterMayBeConst
int Win32Application::Run(DXApp* app, HINSTANCE instance, int const cmdShow)
{
    WNDCLASSEX windowClass    = {0};
    windowClass.cbSize        = sizeof(WNDCLASSEX);
    windowClass.style         = CS_HREDRAW | CS_VREDRAW;
    windowClass.lpfnWndProc   = WindowProc;
    windowClass.hInstance     = instance;
    windowClass.hIcon         = app->GetIcon();
    windowClass.hCursor       = nullptr;
    windowClass.lpszClassName = L"DXApp";
    RegisterClassEx(&windowClass);

    RECT windowRect = {0, 0, static_cast<LONG>(app->GetWidth()), static_cast<LONG>(app->GetHeight())};
    TRY_DO(AdjustWindowRect(&windowRect, WS_OVERLAPPEDWINDOW, FALSE));

    m_hwnd = CreateWindow(
        windowClass.lpszClassName,
        app->GetTitle(),
        WINDOW_STYLE,
        CW_USEDEFAULT,
        CW_USEDEFAULT,
        windowRect.right - windowRect.left,
        windowRect.bottom - windowRect.top,
        nullptr,
        nullptr,
        instance,
        app);
    m_app = app;

    app->Init();
    app->Tick(DXApp::ALLOW_UPDATE);
    app->Tick(DXApp::ALLOW_RENDER);

    ShowWindow(m_hwnd, cmdShow);

    app->Tick(DXApp::ALLOW_RENDER);

    MSG msg = {};
    while (msg.message != WM_QUIT)
        if (PeekMessage(&msg, nullptr, 0, 0, PM_REMOVE))
        {
            TranslateMessage(&msg);
            DispatchMessage(&msg);
        }
        else app->Tick(DXApp::ALLOW_BOTH);

    app->Destroy();

    return static_cast<char>(msg.wParam);
}

void Win32Application::ToggleFullscreenWindow(ComPtr<IDXGISwapChain> swapChain)
{
    if (m_fullscreenMode)
    {
        SetWindowLongPtr(m_hwnd, GWL_STYLE, WINDOW_STYLE);

        TRY_DO(
            SetWindowPos( m_hwnd, HWND_NOTOPMOST, m_windowRect.left, m_windowRect.top, m_windowRect.right - m_windowRect
                .left, m_windowRect.bottom - m_windowRect.top, SWP_FRAMECHANGED | SWP_NOACTIVATE));

        ShowWindow(m_hwnd, SW_NORMAL);
    }
    else
    {
        TRY_DO(GetWindowRect(m_hwnd, &m_windowRect));

        SetWindowLongPtr(m_hwnd, GWL_STYLE, WINDOW_FULLSCREEN_STYLE);

        RECT fullscreenWindowRect;
        try
        {
            ComPtr<IDXGIOutput> pOutput;
            TRY_DO(swapChain->GetContainingOutput(&pOutput));

            DXGI_OUTPUT_DESC desc;
            TRY_DO(pOutput->GetDesc(&desc));

            fullscreenWindowRect = desc.DesktopCoordinates;
        }
        catch (HResultException& e)
        {
            UNREFERENCED_PARAMETER(e);

            DEVMODE devMode = {};
            devMode.dmSize  = sizeof(DEVMODE);
            EnumDisplaySettings(nullptr, ENUM_CURRENT_SETTINGS, &devMode);

            fullscreenWindowRect = {
                devMode.dmPosition.x,
                devMode.dmPosition.y,
                devMode.dmPosition.x + static_cast<LONG>(devMode.dmPelsWidth),
                devMode.dmPosition.y + static_cast<LONG>(devMode.dmPelsHeight)
            };
        }

        TRY_DO(
            SetWindowPos( m_hwnd, HWND_TOPMOST, fullscreenWindowRect.left, fullscreenWindowRect.top,
                fullscreenWindowRect.right, fullscreenWindowRect.bottom, SWP_FRAMECHANGED | SWP_NOACTIVATE));

        ShowWindow(m_hwnd, SW_MAXIMIZE);
    }

    m_fullscreenMode = !m_fullscreenMode;
}

void Win32Application::SetWindowOrderToTopMost(bool const setToTopMost)
{
    RECT windowRect;
    TRY_DO(GetWindowRect(m_hwnd, &windowRect));

    TRY_DO(
        SetWindowPos( m_hwnd, (setToTopMost) ? HWND_TOPMOST : HWND_NOTOPMOST, windowRect.left, windowRect.top,
            windowRect.right - windowRect.left, windowRect.bottom - windowRect.top, SWP_FRAMECHANGED | SWP_NOACTIVATE));
}

void Win32Application::ShowErrorMessage(LPCWSTR const message, LPCWSTR const title)
{
    EnterErrorMode();
    MessageBoxW(m_hwnd, message, title, MB_OK | MB_ICONERROR | MB_SETFOREGROUND);
    ExitErrorMode();
}

void Win32Application::EnterErrorMode() { ++m_errorModeDepth; }

void Win32Application::ExitErrorMode() { --m_errorModeDepth; }

bool Win32Application::IsInErrorMode() { return m_errorModeDepth > 0; }

// ReSharper disable once CppParameterMayBeConst
LRESULT CALLBACK Win32Application::WindowProc(HWND hWnd, UINT const message, WPARAM const wParam, LPARAM const lParam)
{
    auto const app = reinterpret_cast<DXApp*>(GetWindowLongPtr(hWnd, GWLP_USERDATA));

    auto def = [&] { return DefWindowProc(hWnd, message, wParam, lParam); };

    if (IsInErrorMode()) return def();

    switch (message)
    {
    case WM_CREATE:
    {
        auto const pCreateStruct = reinterpret_cast<LPCREATESTRUCT>(lParam);
        SetWindowLongPtr(hWnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(pCreateStruct->lpCreateParams));
    }
        return 0;

    case WM_MOUSEACTIVATE: return MA_ACTIVATEANDEAT;

    case WM_ACTIVATE:
    {
        bool const active = LOWORD(wParam) != WA_INACTIVE;
        if (app) app->HandleActiveStateChange(active);
    }
        return 0;

    case WM_PAINT:
    {
        if (app)
        {
            app->Tick(DXApp::ALLOW_RENDER);
            ValidateRect(m_hwnd, nullptr);
        }
    }
        return 0;

    case WM_KEYDOWN:
    case WM_KEYUP:
    case WM_SYSKEYDOWN:
    case WM_SYSKEYUP: if (app)
        {
            WORD vkCode   = LOWORD(wParam);
            WORD keyFlags = HIWORD(lParam);

            if (vkCode == VK_LWIN || vkCode == VK_RWIN) return 0;

            WORD scanCode = LOBYTE(keyFlags);
            BOOL extended = (keyFlags & KF_EXTENDED) == KF_EXTENDED;

            BOOL up  = (keyFlags & KF_UP) == KF_UP;
            BOOL alt = (keyFlags & KF_ALTDOWN) == KF_ALTDOWN;

            switch (vkCode)
            {
            case VK_SHIFT: vkCode = LOWORD(MapVirtualKeyW(scanCode, MAPVK_VSC_TO_VK_EX));
                break;
            case VK_CONTROL: vkCode = extended ? VK_RCONTROL : VK_LCONTROL;
                break;
            case VK_MENU: vkCode = extended ? VK_RMENU : VK_LMENU;
                break;
            default: break;
            }

            auto vk = static_cast<UINT8>(vkCode);

            if (up) app->OnKeyUp(vk);
            else { if (!alt) app->OnKeyDown(vk); }
        }
        return 0;

    case WM_LBUTTONDOWN: if (app) app->OnKeyDown(VK_LBUTTON);
        return 0;

    case WM_LBUTTONUP: if (app) app->OnKeyUp(VK_LBUTTON);
        return 0;

    case WM_RBUTTONDOWN: if (app) app->OnKeyDown(VK_RBUTTON);
        return 0;

    case WM_RBUTTONUP: if (app) app->OnKeyUp(VK_RBUTTON);
        return 0;

    case WM_MBUTTONDOWN: if (app) app->OnKeyDown(VK_MBUTTON);
        return 0;

    case WM_MBUTTONUP: if (app) app->OnKeyUp(VK_MBUTTON);
        return 0;

    case WM_XBUTTONDOWN: if (app)
        {
            UINT const button = GET_XBUTTON_WPARAM(wParam);

            if (button == XBUTTON1) app->OnKeyDown(VK_XBUTTON1);
            else if (button == XBUTTON2) app->OnKeyDown(VK_XBUTTON2);
        }
        return TRUE; // see https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-xbuttondown#return-value

    case WM_XBUTTONUP: if (app)
        {
            UINT const button = GET_XBUTTON_WPARAM(wParam);

            if (button == XBUTTON1) app->OnKeyUp(VK_XBUTTON1);
            else if (button == XBUTTON2) app->OnKeyUp(VK_XBUTTON2);
        }
        return TRUE; // see https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-xbuttonup#return-value

    case WM_CHAR: if (app) app->OnChar(static_cast<UINT16>(wParam));
        return 0;

    case WM_MOUSEWHEEL: if (app)
        {
            double const delta  = GET_WHEEL_DELTA_WPARAM(wParam);
            double const zDelta = delta / WHEEL_DELTA;
            app->OnMouseWheel(zDelta);
        }
        return 0;

    case WM_MOUSEMOVE: if (app)
        {
            int const xPos = GET_X_LPARAM(lParam);
            int const yPos = GET_Y_LPARAM(lParam);
            app->OnMouseMove(xPos, yPos);
        }
        return 0;

    case WM_SETCURSOR: if (app)
            if (LOWORD(lParam) == HTCLIENT)
            {
                app->DoCursorSet();
                return TRUE;
            }
        return def();

    case WM_ENTERSIZEMOVE: if (app) app->OnSizeMove(true);
        return 0;

    case WM_EXITSIZEMOVE: if (app) app->OnSizeMove(false);
        return 0;

    case WM_SIZE: if (app)
        {
            RECT windowRect = {};
            TRY_DO(GetWindowRect(hWnd, &windowRect));
            app->SetWindowBounds(windowRect.left, windowRect.top, windowRect.right, windowRect.bottom);

            RECT clientRect = {};
            TRY_DO(GetClientRect(hWnd, &clientRect));
            app->HandleSizeChanged(
                clientRect.right - clientRect.left,
                clientRect.bottom - clientRect.top,
                wParam == SIZE_MINIMIZED);
        }
        return 0;

    case WM_MOVE: if (app)
        {
            RECT windowRect = {};
            TRY_DO(GetWindowRect(hWnd, &windowRect));
            app->SetWindowBounds(windowRect.left, windowRect.top, windowRect.right, windowRect.bottom);

            int const xPos = static_cast<short>(LOWORD(lParam));
            int const yPos = static_cast<short>(HIWORD(lParam));
            app->HandleWindowMoved(xPos, yPos);
        }
        return 0;

    case WM_TIMER: if (app)
        {
            app->OnTimer(static_cast<UINT_PTR>(wParam));
            return 0;
        }
        return def();

    case WM_GETMINMAXINFO:
    {
        auto const minmaxInfo        = reinterpret_cast<LPMINMAXINFO>(lParam);
        minmaxInfo->ptMinTrackSize.x = MINIMUM_WINDOW_WIDTH;
        minmaxInfo->ptMinTrackSize.y = MINIMUM_WINDOW_HEIGHT;
    }
        return 0;

    case WM_DISPLAYCHANGE: if (app) app->OnDisplayChanged();
        return 0;

    case WM_CLOSE: if (app && app->CanClose())
            TRY_DO(DestroyWindow(hWnd));
        return 0;

    case WM_DESTROY: PostQuitMessage(0);
        return 0;

    default: return def();
    }
}
