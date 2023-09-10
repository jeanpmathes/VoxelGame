//  <copyright file="DXApp.hpp" company="Microsoft">
//      Copyright (c) Microsoft. All rights reserved.
//      MIT License
//  </copyright>
//  <author>Microsoft, jeanpmathes</author>

#pragma once

#include "native.hpp"

#include "StepTimer.hpp"

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

    enum CycleFlags
    {
        ALLOW_UPDATE = 1 << 0,
        ALLOW_RENDER = 1 << 1,
        ALLOW_BOTH = ALLOW_UPDATE | ALLOW_RENDER,
    };

    /**
     * Perform a tick, which can update and render the application.
     * \param flags The flags to control which cycles are allowed.
     */
    void Tick(CycleFlags flags);

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

    /**
     * Whether to configure features in a way that are more friendly to PIX.
     */
    [[nodiscard]] BOOL SupportPIX() const { return m_configuration.supportPIX; }

    void SetWindowBounds(int left, int top, int right, int bottom);
    void UpdateForSizeChange(UINT clientWidth, UINT clientHeight);
    void SetMouseCursor(MouseCursor cursor) const;

    [[nodiscard]] float GetAspectRatio() const;
    [[nodiscard]] POINT GetMousePosition() const { return {m_xMousePosition, m_yMousePosition}; }

    enum class Cycle
    {
        /**
         * The thread is in the update cycle.
         */
        UPDATE,

        /**
         * The thread is in the render cycle.
         */
        RENDER,

        /**
         * The thread is a worker thread.
         */
        WORKER,
    };

    /**
     * Get the current cycle the calling thread is in.
     */
    [[nodiscard]] std::optional<Cycle> GetCycle() const;

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

    [[nodiscard]] FLOAT GetRenderScale() const { return m_configuration.renderScale; }

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

    std::optional<Cycle> m_cycle = std::nullopt;
    std::thread::id m_mainThreadId;
};

#define CALL_IN_UPDATE(client) ((client)->GetCycle() == DXApp::Cycle::UPDATE)
#define CALL_IN_RENDER(client) ((client)->GetCycle() == DXApp::Cycle::RENDER)
#define CALL_IN_WORKER(client) ((client)->GetCycle() == DXApp::Cycle::WORKER)
#define CALL_OUTSIDE_CYCLE(client) (!(client)->GetCycle().has_value())
#define CALL_INSIDE_CYCLE(client) ((client)->GetCycle().has_value() && (client)->GetCycle().value() != DXApp::Cycle::WORKER)
#define CALL_ON_MAIN_THREAD(client) (!(client)->GetCycle().has_value() || (client)->GetCycle().value() != DXApp::Cycle::WORKER)