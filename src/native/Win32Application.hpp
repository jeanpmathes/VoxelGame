//  <copyright file="Win32Application.hpp" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#pragma once

#include "DXApp.hpp"

class DXApp;

class Win32Application
{
public:
    static int Run(DXApp* app, HINSTANCE instance, int cmdShow);

    static void ToggleFullscreenWindow(ComPtr<IDXGISwapChain> swapChain);
    static void SetWindowOrderToTopMost(bool setToTopMost);

    static HWND GetHwnd() { return m_hwnd; }
    static bool IsFullscreen() { return m_fullscreenMode; }
    static bool IsRunning(const DXApp* pApp) { return m_app == pApp; }

    static void ShowErrorMessage(LPCWSTR message, LPCWSTR title);

    static void EnterErrorMode();
    static void ExitErrorMode();
    static bool IsInErrorMode();

    static constexpr UINT MINIMUM_WINDOW_WIDTH = 150;
    static constexpr UINT MINIMUM_WINDOW_HEIGHT = 150;

protected:
    static LRESULT CALLBACK WindowProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);

private:
    static void* m_app;
    static HWND m_hwnd;
    static bool m_fullscreenMode;
    
    static constexpr UINT WINDOW_STYLE = WS_OVERLAPPEDWINDOW;
    static constexpr UINT WINDOW_FULLSCREEN_STYLE = WS_POPUP | WS_SYSMENU | WS_VISIBLE;
    
    static RECT m_windowRect;
    static size_t m_errorModeDepth;
};
