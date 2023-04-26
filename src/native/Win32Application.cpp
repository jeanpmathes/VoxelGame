//  <copyright file="Win32Application.cpp" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#include "stdafx.h"

HWND Win32Application::m_hwnd = nullptr;
bool Win32Application::m_fullscreenMode = false;
RECT Win32Application::m_windowRect;

// ReSharper disable once CppParameterMayBeConst
int Win32Application::Run(DXApp* pApp, HINSTANCE hInstance, const int nCmdShow)
{
    WNDCLASSEX windowClass = {0};
    windowClass.cbSize = sizeof(WNDCLASSEX);
    windowClass.style = CS_HREDRAW | CS_VREDRAW;
    windowClass.lpfnWndProc = WindowProc;
    windowClass.hInstance = hInstance;
    windowClass.hCursor = nullptr;
    windowClass.lpszClassName = L"DXApp";
    RegisterClassEx(&windowClass);

    RECT windowRect = {0, 0, static_cast<LONG>(pApp->GetWidth()), static_cast<LONG>(pApp->GetHeight())};
    TRY_DO(AdjustWindowRect(&windowRect, WS_OVERLAPPEDWINDOW, FALSE));

    m_hwnd = CreateWindow(
        windowClass.lpszClassName,
        pApp->GetTitle(),
        WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT,
        CW_USEDEFAULT,
        windowRect.right - windowRect.left,
        windowRect.bottom - windowRect.top,
        nullptr,
        nullptr,
        hInstance,
        pApp);

    pApp->Init();

    ShowWindow(m_hwnd, nCmdShow);

    MSG msg = {};
    while (msg.message != WM_QUIT)
    {
        if (PeekMessage(&msg, nullptr, 0, 0, PM_REMOVE))
        {
            TranslateMessage(&msg);
            DispatchMessage(&msg);
        }
        else
        {
            pApp->Tick();
        }
    }

    pApp->Destroy();

    return static_cast<char>(msg.wParam);
}

void Win32Application::ToggleFullscreenWindow(IDXGISwapChain* pSwapChain)
{
    if (m_fullscreenMode)
    {
        SetWindowLong(m_hwnd, GWL_STYLE, WINDOW_STYLE);

        TRY_DO(SetWindowPos(
            m_hwnd,
            HWND_NOTOPMOST,
            m_windowRect.left,
            m_windowRect.top,
            m_windowRect.right - m_windowRect.left,
            m_windowRect.bottom - m_windowRect.top,
            SWP_FRAMECHANGED | SWP_NOACTIVATE));

        ShowWindow(m_hwnd, SW_NORMAL);
    }
    else
    {
        TRY_DO(GetWindowRect(m_hwnd, &m_windowRect));

        SetWindowLong(m_hwnd, GWL_STYLE,
                      WINDOW_STYLE & ~(WS_CAPTION | WS_MAXIMIZEBOX | WS_MINIMIZEBOX | WS_SYSMENU | WS_THICKFRAME));

        RECT fullscreenWindowRect;
        try
        {
            if (pSwapChain)
            {
                ComPtr<IDXGIOutput> pOutput;
                TRY_DO(pSwapChain->GetContainingOutput(&pOutput));
                DXGI_OUTPUT_DESC desc;
                TRY_DO(pOutput->GetDesc(&desc));
                fullscreenWindowRect = desc.DesktopCoordinates;
            }
            else
            {
                throw HResultException(S_FALSE, "pSwapChain is null");
            }
        }
        catch (HResultException& e)
        {
            UNREFERENCED_PARAMETER(e);

            DEVMODE devMode = {};
            devMode.dmSize = sizeof(DEVMODE);
            EnumDisplaySettings(nullptr, ENUM_CURRENT_SETTINGS, &devMode);

            fullscreenWindowRect = {
                devMode.dmPosition.x,
                devMode.dmPosition.y,
                devMode.dmPosition.x + static_cast<LONG>(devMode.dmPelsWidth),
                devMode.dmPosition.y + static_cast<LONG>(devMode.dmPelsHeight)
            };
        }

        TRY_DO(SetWindowPos(
            m_hwnd,
            HWND_TOPMOST,
            fullscreenWindowRect.left,
            fullscreenWindowRect.top,
            fullscreenWindowRect.right,
            fullscreenWindowRect.bottom,
            SWP_FRAMECHANGED | SWP_NOACTIVATE));


        ShowWindow(m_hwnd, SW_MAXIMIZE);
    }

    m_fullscreenMode = !m_fullscreenMode;
}

void Win32Application::SetWindowOrderToTopMost(bool setToTopMost)
{
    RECT windowRect;
    TRY_DO(GetWindowRect(m_hwnd, &windowRect));

    TRY_DO(SetWindowPos(
        m_hwnd,
        (setToTopMost) ? HWND_TOPMOST : HWND_NOTOPMOST,
        windowRect.left,
        windowRect.top,
        windowRect.right - windowRect.left,
        windowRect.bottom - windowRect.top,
        SWP_FRAMECHANGED | SWP_NOACTIVATE));
}

// ReSharper disable once CppParameterMayBeConst
LRESULT CALLBACK Win32Application::WindowProc(HWND hWnd, const UINT message, const WPARAM wParam, const LPARAM lParam)
{
    const auto app = reinterpret_cast<DXApp*>(GetWindowLongPtr(hWnd, GWLP_USERDATA));

    switch (message)
    {
    case WM_CREATE:
        {
            const auto pCreateStruct = reinterpret_cast<LPCREATESTRUCT>(lParam);
            SetWindowLongPtr(hWnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(pCreateStruct->lpCreateParams));
        }
        return 0;

    case WM_ACTIVATE:
        {
            const bool active = LOWORD(wParam) != WA_INACTIVE;
            if (app)
            {
                app->HandleActiveStateChange(active);
            }
        }
        return 0;

    case WM_PAINT:
        {
            if (app)
            {
                app->Tick();
            }
        }
        return 0;

    case WM_KEYDOWN:
        if (app)
        {
            app->OnKeyDown(static_cast<UINT8>(wParam));
        }
        return 0;

    case WM_KEYUP:
        if (app)
        {
            app->OnKeyUp(static_cast<UINT8>(wParam));
        }
        return 0;

    case WM_LBUTTONDOWN:
        if (app)
        {
            app->OnKeyDown(VK_LBUTTON);
        }
        return 0;

    case WM_LBUTTONUP:
        if (app)
        {
            app->OnKeyUp(VK_LBUTTON);
        }
        return 0;

    case WM_RBUTTONDOWN:
        if (app)
        {
            app->OnKeyDown(VK_RBUTTON);
        }
        return 0;

    case WM_RBUTTONUP:
        if (app)
        {
            app->OnKeyUp(VK_RBUTTON);
        }
        return 0;

    case WM_MBUTTONDOWN:
        if (app)
        {
            app->OnKeyDown(VK_MBUTTON);
        }
        return 0;

    case WM_MBUTTONUP:
        if (app)
        {
            app->OnKeyUp(VK_MBUTTON);
        }
        return 0;

    case WM_XBUTTONDOWN:
        if (app)
        {
            const UINT button = GET_XBUTTON_WPARAM(wParam);
            if (button == XBUTTON1)
            {
                app->OnKeyDown(VK_XBUTTON1);
            }
            else if (button == XBUTTON2)
            {
                app->OnKeyDown(VK_XBUTTON2);
            }
        }
        return TRUE; // see https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-xbuttondown#return-value

    case WM_XBUTTONUP:
        if (app)
        {
            const UINT button = GET_XBUTTON_WPARAM(wParam);
            if (button == XBUTTON1)
            {
                app->OnKeyUp(VK_XBUTTON1);
            }
            else if (button == XBUTTON2)
            {
                app->OnKeyUp(VK_XBUTTON2);
            }
        }
        return TRUE; // see https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-xbuttonup#return-value

    case WM_CHAR:
        if (app)
        {
            app->OnChar(static_cast<UINT16>(wParam));
        }
        return 0;

    case WM_MOUSEWHEEL:
        if (app)
        {
            const double delta = GET_WHEEL_DELTA_WPARAM(wParam);
            const double zDelta = delta / WHEEL_DELTA;
            app->OnMouseWheel(zDelta);
        }
        return 0;

    case WM_MOUSEMOVE:
        if (app)
        {
            const int xPos = GET_X_LPARAM(lParam);
            const int yPos = GET_Y_LPARAM(lParam);
            app->OnMouseMove(xPos, yPos);
        }
        return 0;

    case WM_SIZE:
        if (app)
        {
            RECT windowRect = {};
            TRY_DO(GetWindowRect(hWnd, &windowRect));
            app->SetWindowBounds(windowRect.left, windowRect.top, windowRect.right, windowRect.bottom);

            RECT clientRect = {};
            TRY_DO(GetClientRect(hWnd, &clientRect));
            app->HandleSizeChanged(clientRect.right - clientRect.left, clientRect.bottom - clientRect.top,
                                   wParam == SIZE_MINIMIZED);
        }
        return 0;

    case WM_MOVE:
        if (app)
        {
            RECT windowRect = {};
            TRY_DO(GetWindowRect(hWnd, &windowRect));
            app->SetWindowBounds(windowRect.left, windowRect.top, windowRect.right, windowRect.bottom);

            const int xPos = static_cast<short>(LOWORD(lParam));
            const int yPos = static_cast<short>(HIWORD(lParam));
            app->HandleWindowMoved(xPos, yPos);
        }
        return 0;

    case WM_DISPLAYCHANGE:
        if (app)
        {
            app->OnDisplayChanged();
        }
        return 0;

    case WM_DESTROY:
        PostQuitMessage(0);
        return 0;

    default: return DefWindowProc(hWnd, message, wParam, lParam);
    }
}
