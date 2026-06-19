// <copyright file="native.hpp" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

#define NATIVE extern "C" __declspec(dllexport)

/**
 * Modifier key state for different input events.
 * See also \c VoxelGame.GUI.Input.ModifierKeys .
 */
enum class ModifierKeys : BYTE
{
    NONE    = 0,
    CONTROL = 1 << 0,
    ALT     = 1 << 1,
    SHIFT   = 1 << 2,
};

DEFINE_ENUM_FLAG_OPERATORS(ModifierKeys)

using NativeCallbackFunction     = void(*)();
using NativeRenderUpdateFunction = void(*)(double, double);
using NativeLogicUpdateFunction  = void(*)(double, double);
using NativeCheckFunction        = BOOL(*)();
using NativeKeyFunction          = void(*)(UINT8 key, BOOL isDown, BOOL isRepeat, ModifierKeys modifiers);
using NativeCharFunction         = void(*)(UINT16 c);
using NativeMouseButtonFunction  = void(*)(UINT8 button, BOOL isDown, INT32 x, INT32 y, ModifierKeys modifiers);
using NativeMouseMoveFunction    = void(*)(INT32 x, INT32 y, INT32 dx, INT32 dy);
using NativeMouseScrollFunction  = void(*)(INT32 x, INT32 y, double sx, double sy);
using NativeResizeFunction       = void(*)(UINT, UINT);
using NativeBoolFunction         = void(*)(BOOL);
using NativeWStringFunction      = void(*)(LPCWSTR);
using NativeErrorFunction        = void(*)(HRESULT, char const*);

enum class ConfigurationOptions : UINT // NOLINT(performance-enum-size)
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

    NativeKeyFunction         onKey;
    NativeCharFunction        onChar;
    NativeMouseButtonFunction onMouseButton;
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
    LPWSTR applicationLocale;

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
