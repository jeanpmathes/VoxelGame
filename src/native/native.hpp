//  <copyright file="native.hpp" company="VoxelGame">
//      MIT License
// 	 For full license see the repository.
//  </copyright>
//  <author>jeanpmathes</author>

#pragma once

#define NATIVE extern "C" __declspec(dllexport)

using NativeCallbackFunc = void(*)();
using NativeStepFunc = void(*)(double);
using NativeCheckFunc = BOOL(*)();
using NativeInputFunc = void(*)(UINT8);
using NativeCharFunc = void(*)(UINT16);
using NativeMouseMoveFunc = void(*)(INT, INT);
using NativeMouseScrollFunc = void(*)(double);
using NativeResizeFunc = void(*)(UINT, UINT);
using NativeBoolFunc = void(*)(BOOL);
using NativeWStringFunc = void(*)(LPCWSTR);
using NativeErrorFunc = void(*)(HRESULT, char const*);

enum class ConfigurationOptions : UINT
{
    ALLOW_TEARING = 1 << 0,
    SUPPORT_PIX   = 1 << 1,
    USE_GBV       = 1 << 2,
};

DEFINE_ENUM_FLAG_OPERATORS(ConfigurationOptions)

struct Configuration
{
    NativeStepFunc onRender;
    NativeStepFunc onUpdate;

    NativeCallbackFunc onInit;
    NativeCallbackFunc onDestroy;

    NativeCheckFunc canClose;

    NativeInputFunc       onKeyDown;
    NativeInputFunc       onKeyUp;
    NativeCharFunc        onChar;
    NativeMouseMoveFunc   onMouseMove;
    NativeMouseScrollFunc onMouseScroll;

    NativeResizeFunc onResize;
    NativeBoolFunc   onActiveStateChange;

    D3D12MessageFunc onDebug;

    UINT   width;
    UINT   height;
    LPWSTR title;
    HICON  icon;

    LPWSTR applicationName;
    LPWSTR applicationVersion;

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
