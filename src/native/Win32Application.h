//  <copyright file="Win32Application.h" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#pragma once

#include "DXApp.h"

class DXApp;

class Win32Application
{
public:
    static int Run(DXApp* pApp, HINSTANCE hInstance, int nCmdShow);

    static void ToggleFullscreenWindow(IDXGISwapChain* pSwapChain = nullptr);
    static void SetWindowOrderToTopMost(bool setToTopMost);

    static HWND GetHwnd() { return m_hwnd; }
    static bool IsFullscreen() { return m_fullscreenMode; }
    static bool IsRunning(const DXApp* pApp) { return m_app == pApp; }

protected:
    static LRESULT CALLBACK WindowProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam);

private:
    static void* m_app;
    static HWND m_hwnd;
    static bool m_fullscreenMode;
    static constexpr UINT WINDOW_STYLE = WS_OVERLAPPEDWINDOW;
    static RECT m_windowRect;
};
