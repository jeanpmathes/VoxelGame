//  <copyright file="DXApp.h" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#pragma once

#include "native.h"

#include "StepTimer.h"

class Uploader;

/**
 * The mouse cursor type.
 */
enum class MouseCursor : BYTE
{
    ARROW,
    I_BEAM,
    SIZE_NS,
    SIZE_WE,
    SIZE_NWSE,
    SIZE_NESW,
    SIZE_ALL,
    NO,
    WAIT,
    HAND
};

/**
 * Base class for DirectX applications.
 */
class DXApp
{
public:
    DXApp(Configuration configuration);
    virtual ~DXApp();

    DXApp(const DXApp& other) = delete;
    DXApp& operator=(const DXApp& other) = delete;
    DXApp(DXApp&& other) = delete;
    DXApp& operator=(DXApp&& other) = delete;

    /**
     * Perform a tick, which can update and render the application.
     * \param allowUpdate Whether to allow the application to update.
     */
    void Tick(bool allowUpdate);

    void Init();
    void Update(const StepTimer& timer);
    void Render(const StepTimer& timer);
    void Destroy();

    bool CanClose() const;
    
    void HandleSizeChanged(UINT width, UINT height, bool minimized);
    void HandleWindowMoved(int xPos, int yPos);
    void HandleActiveStateChange(bool active) const;

    void OnKeyDown(UINT8) const;
    void OnKeyUp(UINT8) const;
    void OnChar(UINT16) const;
    void OnMouseMove(int, int);
    void OnMouseWheel(double) const;

    virtual void OnDisplayChanged()
    {
    }

    [[nodiscard]] UINT GetWidth() const { return m_width; }
    [[nodiscard]] UINT GetHeight() const { return m_height; }
    [[nodiscard]] const WCHAR* GetTitle() const { return m_title.c_str(); }

    void SetWindowBounds(int left, int top, int right, int bottom);
    void UpdateForSizeChange(UINT clientWidth, UINT clientHeight);
    void SetMouseCursor(MouseCursor cursor) const;

    [[nodiscard]] float GetAspectRatio() const;
    [[nodiscard]] POINT GetMousePosition() const { return {m_xMousePosition, m_yMousePosition}; }

protected:
    virtual void OnInit() = 0;
    virtual void OnPostInit() = 0;
    virtual void OnUpdate(double delta) = 0;
    virtual void OnPreRender() = 0;
    virtual void OnRender(double delta) = 0;
    virtual void OnDestroy() = 0;

    virtual void OnSizeChanged(UINT width, UINT height, bool minimized) = 0;
    virtual void OnWindowMoved(int xPos, int yPos) = 0;

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
    std::wstring m_title;

    Configuration m_configuration;

    StepTimer m_updateTimer{};
    StepTimer m_renderTimer{};

    int m_xMousePosition = 0;
    int m_yMousePosition = 0;
};
