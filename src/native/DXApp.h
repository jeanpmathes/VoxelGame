//  <copyright file="DXApp.h" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#pragma once

#include "native.h"

#include "StepTimer.h"

/**
 * Base class for DirectX applications.
 */
class DXApp
{
public:
    DXApp(UINT width, UINT height, std::wstring name, Configuration configuration);
    virtual ~DXApp();

    DXApp(const DXApp& other) = delete;
    DXApp& operator=(const DXApp& other) = delete;
    DXApp(DXApp&& other) = delete;
    DXApp& operator=(DXApp&& other) = delete;

    /**
     * Perform a tick, which can update and render the application.
     */
    void Tick();

    void Init();
    void Update(const StepTimer& timer);
    void Render(const StepTimer& timer);
    void Destroy();

    void HandleSizeChanged(UINT width, UINT height, bool minimized);
    void HandleWindowMoved(int xPos, int yPos);

    void OnKeyDown(UINT8) const;
    void OnKeyUp(UINT8) const;
    void OnMouseMove(int, int);

    virtual void OnDisplayChanged()
    {
    }

    [[nodiscard]] UINT GetWidth() const { return m_width; }
    [[nodiscard]] UINT GetHeight() const { return m_height; }
    [[nodiscard]] const WCHAR* GetTitle() const { return m_title.c_str(); }

    void SetWindowBounds(int left, int top, int right, int bottom);
    void UpdateForSizeChange(UINT clientWidth, UINT clientHeight);

    [[nodiscard]] float GetAspectRatio() const;
    [[nodiscard]] POINT GetMousePosition() const { return {m_xMousePosition, m_yMousePosition}; }

protected:
    virtual void OnInit() = 0;
    virtual void OnUpdate(double delta) = 0;
    virtual void OnRender(double delta) = 0;
    virtual void OnDestroy() = 0;

    virtual void OnSizeChanged(UINT width, UINT height, bool minimized) = 0;
    virtual void OnWindowMoved(int xPos, int yPos) = 0;

    std::wstring GetAssetFullPath(LPCWSTR assetName) const;

    void GetHardwareAdapter(
        _In_ IDXGIFactory1* pFactory,
        _Outptr_result_maybenull_ IDXGIAdapter1** ppAdapter,
        bool requestHighPerformanceAdapter = false) const;

    void SetCustomWindowText(LPCWSTR text) const;
    void CheckTearingSupport();

    UINT m_width;
    UINT m_height;
    float m_aspectRatio;

    RECT m_windowBounds;

    bool m_tearingSupport;

private:
    std::wstring m_assetsPath;
    std::wstring m_title;

    Configuration m_configuration;

    StepTimer m_updateTimer{};
    StepTimer m_renderTimer{};

    int m_xMousePosition = 0;
    int m_yMousePosition = 0;
};
