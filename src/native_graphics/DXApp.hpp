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
    HAND,

    COUNT
};

/**
 * Base class for DirectX applications.
 */
class DXApp
{
public:
    explicit DXApp(Configuration const& configuration);
    virtual  ~DXApp();

    DXApp(DXApp const& other)            = delete;
    DXApp& operator=(DXApp const& other) = delete;
    DXApp(DXApp&& other)                 = delete;
    DXApp& operator=(DXApp&& other)      = delete;

    enum class CycleFlags
    {
        ALLOW_UPDATE = 1 << 0,
        ALLOW_RENDER = 1 << 1,
        ALLOW_BOTH   = ALLOW_UPDATE | ALLOW_RENDER,
    };

    static bool HasFlag(CycleFlags value, CycleFlags flag);

    /**
     * Perform a tick, which can update and render the application.
     * \param flags The flags to control which cycles are allowed.
     * \param timer Whether the tick is being called from a timer.
     */
    void Tick(CycleFlags flags, bool timer = false);

    void Init();
    void Update(StepTimer const& timer);
    void Render(StepTimer const& timer);
    void Destroy();

    [[nodiscard]] bool CanClose() const;

    void HandleSizeChanged(UINT width, UINT height, bool minimized);
    void HandleWindowMoved(int xPos, int yPos);
    void HandleActiveStateChange(bool active);

    void OnSizeMove(bool enter);
    void OnTimer(UINT_PTR id);

    void OnKeyDown(UINT8) const;
    void OnKeyUp(UINT8) const;
    void OnChar(UINT16) const;
    void OnMouseMove(int, int);
    void OnMouseWheel(double) const;

    void DoCursorSet() const;

    [[nodiscard]] UINT         GetWidth() const { return m_width; }
    [[nodiscard]] UINT         GetHeight() const { return m_height; }
    [[nodiscard]] WCHAR const* GetTitle() const { return m_title.c_str(); }
    [[nodiscard]] HICON        GetIcon() const { return m_icon; }

    [[nodiscard]] bool IsTearingSupportEnabled() const { return m_tearingSupport; }

    [[nodiscard]] bool SupportPIX() const
    {
        return static_cast<bool>(m_configuration.options & ConfigurationOptions::SUPPORT_PIX);
    }

    [[nodiscard]] bool UseGBV() const
    {
        return static_cast<bool>(m_configuration.options & ConfigurationOptions::USE_GBV);
    }

    void SetWindowBounds(int left, int top, int right, int bottom);
    void UpdateForSizeChange(UINT clientWidth, UINT clientHeight);

    /**
     * Set the mouse position in client coordinates.
     */
    void SetMousePosition(POINT position);

    void SetMouseCursor(MouseCursor cursor);
    void SetMouseLock(bool lock);

    [[nodiscard]] float GetAspectRatio() const;
    [[nodiscard]] POINT GetMousePosition() const { return {m_xMousePosition, m_yMousePosition}; }

    [[nodiscard]] double GetTotalUpdateTime() const { return m_totalUpdateTime; }
    [[nodiscard]] double GetTotalRenderTime() const { return m_totalRenderTime; }

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
    virtual void OnPreInit() = 0;
    virtual void OnPostInit() = 0;
    virtual void OnInitComplete() = 0;
    virtual void OnUpdate(double delta) = 0;
    virtual void OnPreRender() = 0;
    virtual void OnRender(double delta) = 0;
    virtual void OnDestroy() = 0;

    virtual void OnSizeChanged(UINT width, UINT height, bool minimized) = 0;
    virtual void OnWindowMoved(int xPos, int yPos) = 0;

    static ComPtr<IDXGIAdapter1> GetHardwareAdapter(
        ComPtr<IDXGIFactory4> const&       dxgiFactory,
        ComPtr<ID3D12DeviceFactory> const& deviceFactory,
        bool                               requestHighPerformanceAdapter = false);

    void SetCustomWindowText(LPCWSTR text) const;
    void CheckTearingSupport();

    [[nodiscard]] FLOAT GetRenderScale() const { return m_configuration.renderScale; }

private:
    std::wstring m_title;
    HICON        m_icon;

    Configuration m_configuration;

    StepTimer m_updateTimer{};
    StepTimer m_renderTimer{};

    double m_totalUpdateTime = 0.0;
    double m_totalRenderTime = 0.0;

    UINT  m_width;
    UINT  m_height;
    float m_aspectRatio  = 0.0f;
    RECT  m_windowBounds = {0, 0, 0, 0};

    bool m_tearingSupport = false;

    int  m_xMousePosition = 0;
    int  m_yMousePosition = 0;
    bool m_mouseLocked    = false;

    MouseCursor                    m_mouseCursor = MouseCursor::ARROW;
    std::map<MouseCursor, HCURSOR> m_mouseCursors;

    std::optional<Cycle> m_cycle        = std::nullopt;
    std::thread::id      m_mainThreadId = std::this_thread::get_id();

    bool m_inTick = false;

    enum TimerID : UINT_PTR
    {
        IDT_UPDATE = 1,
    };

    bool m_isUpdateTimerRunning = false;
    bool m_isActive             = false;
};

#define CALL_IN_UPDATE(client) ((client)->GetCycle() == DXApp::Cycle::UPDATE)
#define CALL_IN_RENDER(client) ((client)->GetCycle() == DXApp::Cycle::RENDER)
#define CALL_IN_WORKER(client) ((client)->GetCycle() == DXApp::Cycle::WORKER)
#define CALL_OUTSIDE_CYCLE(client) (!(client)->GetCycle().has_value())
#define CALL_IN_UPDATE_OR_EVENT(client) (CALL_IN_UPDATE(client) || CALL_OUTSIDE_CYCLE(client))
#define CALL_INSIDE_CYCLE(client) ((client)->GetCycle().has_value() && (client)->GetCycle().value() != DXApp::Cycle::WORKER)
#define CALL_ON_MAIN_THREAD(client) (!(client)->GetCycle().has_value() || (client)->GetCycle().value() != DXApp::Cycle::WORKER)
