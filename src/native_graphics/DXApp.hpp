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
    explicit DXApp(::Configuration const& configuration);
    virtual  ~DXApp();

    DXApp(DXApp const& other)            = delete;
    DXApp& operator=(DXApp const& other) = delete;
    DXApp(DXApp&& other)                 = delete;
    DXApp& operator=(DXApp&& other)      = delete;

    enum class CycleFlags : uint8_t
    {
        ALLOW_INPUT_UPDATE            = 1 << 0,
        ALLOW_LOGIC_UPDATE            = 1 << 1,
        ALLOW_RENDER_UPDATE           = 1 << 2,
        ALLOW_INPUT_AND_RENDER_UPDATE = ALLOW_INPUT_UPDATE | ALLOW_RENDER_UPDATE,
        ALLOW_ALL_UPDATES             = ALLOW_INPUT_UPDATE | ALLOW_LOGIC_UPDATE | ALLOW_RENDER_UPDATE,
    };

    static bool HasFlag(CycleFlags value, CycleFlags flag);

    /**
     * Perform an outer update with the requested input, logic, and render phases.
     * \param flags The flags to control which cycles are allowed.
     * \param timer Whether the update is being called from a timer.
     */
    void Update(CycleFlags flags, bool timer = false);

    void Init();
    void InputUpdate(StepTimer const& timer);
    void LogicUpdate(StepTimer const& timer);
    void RenderUpdate(StepTimer const& timer);
    void Destroy();

    [[nodiscard]] bool CanClose() const;

    void HandleSizeChanged(UINT newWidth, UINT newHeight, bool minimized);
    void HandleWindowMoved(int xPos, int yPos);
    void HandleActiveStateChange(bool active);
    void HandleKeyboardFocusChange(bool focused) const;

    void OnSizeMoveMenu(bool enter);
    void OnTimer(UINT_PTR id);

    [[nodiscard]] bool OnKey(UINT8 key, BOOL isDown, BOOL isRepeat, ModifierKeys modifiers) const;
    [[nodiscard]] bool OnChar(UINT16 c) const;
    [[nodiscard]] bool OnMouseButton(UINT8 button, BOOL isDown, INT32 x, INT32 y, ModifierKeys modifiers) const;
    [[nodiscard]] bool OnMouseMove(INT32 x, INT32 y);
    [[nodiscard]] bool OnMouseWheel(INT32 x, INT32 y, double sx, double sy) const;

    void DoCursorSet() const;

    [[nodiscard]] UINT         GetWidth() const { return width; }
    [[nodiscard]] UINT         GetHeight() const { return height; }
    [[nodiscard]] WCHAR const* GetTitle() const { return title.c_str(); }
    [[nodiscard]] HICON        GetIcon() const { return icon; }
    [[nodiscard]] WCHAR const* GetApplicationName() const { return applicationName.c_str(); }
    [[nodiscard]] WCHAR const* GetApplicationVersion() const { return applicationVersion.c_str(); }
    [[nodiscard]] WCHAR const* GetApplicationLocale() const { return applicationLocale.c_str(); }

    [[nodiscard]] bool IsTearingSupportEnabled() const { return tearingSupport; }

    [[nodiscard]] bool SupportPIX() const { return static_cast<bool>(configurationOptions & ConfigurationOptions::SUPPORT_PIX); }

    [[nodiscard]] bool UseGBV() const { return static_cast<bool>(configurationOptions & ConfigurationOptions::USE_GBV); }

    void SetWindowBounds(int left, int top, int right, int bottom);
    void UpdateForSizeChange(UINT clientWidth, UINT clientHeight);

    /**
     * Set the mouse position in client coordinates.
     */
    void SetMousePosition(POINT position);

    void SetMouseCursor(MouseCursor cursor);
    void SetMouseLock(bool lock);

    [[nodiscard]] float GetAspectRatio() const;
    [[nodiscard]] POINT GetMousePosition() const { return {xMousePosition, yMousePosition}; }

    [[nodiscard]] double GetTotalRealRenderUpdateTime() const { return totalRealRenderUpdateTime; }
    [[nodiscard]] double GetTotalScaledRenderUpdateTime() const { return totalScaledRenderUpdateTime; }

    void SetTimeScale(double scale);

    enum class Cycle
    {
        /**
         * The thread is performing the initialization of the client.
         * This occurs exactly once and only on the main thread.
         */
        INITIALIZATION,

        /**
         * The thread is performing the final destruction of the client.
         * This occurs exactly once and only on the main thread.
         */
        DESTROY,

        /**
         * The thread is in the variable-rate input update cycle.
         */
        INPUT_UPDATE,

        /**
         * The thread is in the fixed-rate logic update cycle.
         */
        LOGIC_UPDATE,

        /**
         * The thread is in the render update cycle.
         */
        RENDER_UPDATE,

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
    virtual void OnPreInitialization() = 0;
    virtual void OnPostInitialization() = 0;
    virtual void OnInitializationComplete() = 0;
    virtual void OnInputUpdate() = 0;
    virtual void OnLogicUpdate() = 0;
    virtual void OnPreRenderUpdate() = 0;
    virtual void OnRenderUpdate() = 0;
    virtual void OnDestroy() = 0;

    virtual void OnSizeChanged(UINT width, UINT height, bool minimized) = 0;
    virtual void OnWindowMoved(int xPos, int yPos) = 0;

    void SetCustomWindowText(LPCWSTR text) const;
    void CheckTearingSupport();

    [[nodiscard]] FLOAT GetRenderScale() const { return renderScale; }

private:
    struct Hooks
    {
        NativeInputUpdateFunction  onInputUpdate;
        NativeRenderUpdateFunction onRenderUpdate;
        NativeLogicUpdateFunction  onLogicUpdate;

        NativeCallbackFunction onInit;
        NativeCallbackFunction onDestroy;

        NativeCheckFunction canClose;

        NativeKeyFunction         onKey;
        NativeCharFunction        onChar;
        NativeMouseButtonFunction onMouseButton;
        NativeMouseMoveFunction   onMouseMove;
        NativeMouseScrollFunction onMouseScroll;

        NativeResizeFunction onResize;
        NativeBoolFunction   onActiveStateChange;
        NativeBoolFunction   onSizeMoveMenu;
        NativeBoolFunction   onKeyboardFocusChange;
    };

    Hooks                hooks;
    ConfigurationOptions configurationOptions;

    std::wstring title;
    HICON        icon;

    std::wstring applicationName;
    std::wstring applicationVersion;
    std::wstring applicationLocale;

    INT64 baseLogicUpdatesPerSecond;

    FLOAT renderScale;

    StepTimer inputTimer{};
    StepTimer logicTimer{};
    StepTimer renderTimer{};

    double baseLogicUpdateTarget = 1.0;
    double timeScale             = 1.0;

    double totalRealRenderUpdateTime   = 0.0;
    double totalScaledRenderUpdateTime = 0.0;

    UINT  width;
    UINT  height;
    float aspectRatio  = 0.0f;
    RECT  windowBounds = {0, 0, 0, 0};

    bool tearingSupport = false;

    INT32 xMousePosition = 0;
    INT32 yMousePosition = 0;
    bool  mouseLocked    = false;

    MouseCursor                    mouseCursor = MouseCursor::ARROW;
    std::map<MouseCursor, HCURSOR> mouseCursors;

    std::optional<Cycle> cycle        = std::nullopt;
    std::thread::id      mainThreadId = std::this_thread::get_id();

    bool inUpdate = false;

    enum TimerID : UINT_PTR
    {
        IDT_UPDATE = 1,
    };

    bool isUpdateTimerRunning = false;
    bool isActive             = false;
};

#define CALL_IN_INITIALIZATION(client) ((client)->GetCycle() == DXApp::Cycle::INITIALIZATION)
#define CALL_IN_DESTROY(client) ((client)->GetCycle() == DXApp::Cycle::DESTROY)
#define CALL_IN_INPUT(client) ((client)->GetCycle() == DXApp::Cycle::INPUT_UPDATE)
#define CALL_IN_LOGIC(client) ((client)->GetCycle() == DXApp::Cycle::LOGIC_UPDATE)
#define CALL_IN_RENDER(client) ((client)->GetCycle() == DXApp::Cycle::RENDER_UPDATE)
#define CALL_IN_WORKER(client) ((client)->GetCycle() == DXApp::Cycle::WORKER)
#define CALL_IN_EVENT_OR_OTHER(client) (!(client)->GetCycle().has_value())
#define CALL_IN_INPUT_OR_LOGIC(client) (CALL_IN_INPUT(client) || CALL_IN_LOGIC(client))
#define CALL_IN_INPUT_LOGIC_OR_RENDER(client) (CALL_IN_INPUT_OR_LOGIC(client) || CALL_IN_RENDER(client))
#define CALL_IN_INITIALIZATION_OR_DESTROY(client) (CALL_IN_INITIALIZATION(client) || CALL_IN_DESTROY(client))
#define CALL_IN_INPUT_LOGIC_OR_EVENT(client) (CALL_IN_INPUT(client) || CALL_IN_LOGIC(client) || CALL_IN_EVENT_OR_OTHER(client))
#define CALL_ON_MAIN_THREAD(client) (!(client)->GetCycle().has_value() || (client)->GetCycle().value() != DXApp::Cycle::WORKER)
