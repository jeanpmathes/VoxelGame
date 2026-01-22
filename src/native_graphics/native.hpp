//  <copyright file="native.hpp" company="VoxelGame">
//      MIT License
// 	 For full license see the repository.
//  </copyright>
//  <author>jeanpmathes</author>

#pragma once

#define NATIVE extern "C" __declspec(dllexport)

using NativeCallbackFunction     = void(*)();
using NativeRenderUpdateFunction = void(*)(double, double);
using NativeLogicUpdateFunction  = void(*)(double, double);
using NativeCheckFunction        = BOOL(*)();
using NativeInputFunction        = void(*)(UINT8);
using NativeCharFunction         = void(*)(UINT16);
using NativeMouseMoveFunction    = void(*)(INT, INT);
using NativeMouseScrollFunction  = void(*)(double);
using NativeResizeFunction       = void(*)(UINT, UINT);
using NativeBoolFunction         = void(*)(BOOL);
using NativeWStringFunction      = void(*)(LPCWSTR);
using NativeErrorFunction        = void(*)(HRESULT, char const*);

enum class ConfigurationOptions : UINT
{
    NONE          = 0,
    ALLOW_TEARING = 1 << 0,
    SUPPORT_PIX   = 1 << 1,
    USE_GBV       = 1 << 2,
};

DEFINE_ENUM_FLAG_OPERATORS(ConfigurationOptions)

struct Configuration
{
    NativeRenderUpdateFunction onRenderUpdate;
    NativeLogicUpdateFunction  onLogicUpdate;

    NativeCallbackFunction onInit;
    NativeCallbackFunction onDestroy;

    NativeCheckFunction canClose;

    NativeInputFunction       onKeyDown;
    NativeInputFunction       onKeyUp;
    NativeCharFunction        onChar;
    NativeMouseMoveFunction   onMouseMove;
    NativeMouseScrollFunction onMouseScroll;

    NativeResizeFunction onResize;
    NativeBoolFunction   onActiveStateChange;

    D3D12MessageFunc onDebug;

    UINT32 width;
    UINT32 height;
    LPWSTR title;
    HICON  icon;

    LPWSTR applicationName;
    LPWSTR applicationVersion;

    INT64 baseLogicUpdatesPerSecond;

    FLOAT renderScale;

    ConfigurationOptions options;
};

#define TRY try
#define CATCH() \
    catch (const HResultException& e) { onError(e.Error(), e.Info()); exit(1); } \
    catch (const NativeException& e) { onError(E_FAIL, e.what()); exit(1); } \
    catch (const std::exception& e) { onError(E_FAIL, e.what()); exit(1); } \
    catch (...) { onError(E_FAIL, "Unknown error."); exit(1); } \
    do {} while (0)
