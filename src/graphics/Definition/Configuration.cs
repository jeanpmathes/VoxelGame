// <copyright file="NativeConfiguration.cs" company="VoxelGame">
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

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using JetBrains.Annotations;
using VoxelGame.Graphics.Interop;
using VoxelGame.Toolkit.Interop;

namespace VoxelGame.Graphics.Definition;

#pragma warning disable S3898 // No equality comparison used.

/// <summary>
///     Contains static methods that map to the respective functions on the native side.
/// </summary>
internal static partial class Native
{
    /// <summary>
    ///     Builds the options from the given parameters.
    ///     See <see cref="ConfigurationOptions" /> for more information.
    /// </summary>
    internal static ConfigurationOptions BuildOptions(Boolean allowTearing, Boolean supportPIX, Boolean useGBV)
    {
        var options = ConfigurationOptions.None;

        if (allowTearing) options |= ConfigurationOptions.AllowTearing;

        if (supportPIX) options |= ConfigurationOptions.SupportPIX;

        if (useGBV) options |= ConfigurationOptions.UseGBV;

        return options;
    }

    /// <summary>
    ///     A callback that receives a bool value.
    /// </summary>
    internal delegate void NativeBoolFunc(Bool arg);

    /// <summary>
    ///     A simple callback function.
    /// </summary>
    internal delegate void NativeCallbackFunc();

    /// <summary>
    ///     A callback that receives a char value describing an input event.
    /// </summary>
    internal delegate void NativeCharFunc(Char arg);

    /// <summary>
    ///     Checks if a condition is true.
    /// </summary>
    internal delegate Bool NativeCheckFunc();

    /// <summary>
    ///     A callback that receives an HRESULT and an error message, indicating a fatal error.
    /// </summary>
    internal unsafe delegate void NativeErrorFunc(Int32 hresult, Byte* message);

    /// <summary>
    ///     A callback that receives a byte value describing an input event.
    /// </summary>
    internal delegate void NativeInputFunc(Byte arg);

    /// <summary>
    ///     A callback that receives the new mouse position on a mouse move event.
    /// </summary>
    internal delegate void NativeMouseMoveFunc(Int32 x, Int32 y);

    /// <summary>
    ///     A callback that receives the mouse wheel delta on a mouse wheel event.
    /// </summary>
    internal delegate void NativeMouseWheelFunc(Double delta);

    /// <summary>
    ///     A callback that receives the new window size on a resize event.
    /// </summary>
    internal delegate void NativeResizeFunc(UInt32 width, UInt32 height);

    /// <summary>
    ///     A callback that receives a double delta time value.
    /// </summary>
    internal delegate void NativeStepFunc(Double arg);

    /// <summary>
    ///     A callback that receives a wide string value.
    /// </summary>
    internal unsafe delegate void NativeWStringFunc(UInt16* arg);

    /// <summary>
    ///     Flags that can be used to configure the native side.
    /// </summary>
    [Flags]
    internal enum ConfigurationOptions : uint
    {
        /// <summary>
        ///     No options.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Whether to allow tearing.
        /// </summary>
        AllowTearing = 1 << 0,

        /// <summary>
        ///     Whether to disable some features and change allocation behaviour to improve PIX Graphics.
        ///     In release builds, this has no effect.
        /// </summary>
        SupportPIX = 1 << 1,

        /// <summary>
        ///     Whether to use GPU-based validation. Has no effect if <see cref="SupportPIX" /> is set.
        /// </summary>
        UseGBV = 1 << 2
    }

    /// <summary>
    ///     Contains the configuration of the native side.
    /// </summary>
    [NativeMarshalling(typeof(NativeConfigurationMarshaller))]
    internal struct NativeConfiguration
    {
        /// <summary>
        ///     Called for each rendering step.
        /// </summary>
        internal NativeStepFunc onRenderUpdate;

        /// <summary>
        ///     Called for each update event.
        /// </summary>
        internal NativeStepFunc onLogicUpdate;

        /// <summary>
        ///     Called on initialization of the native client.
        /// </summary>
        internal NativeCallbackFunc onInitialization;

        /// <summary>
        ///     Called on shutdown of the native client.
        /// </summary>
        internal NativeCallbackFunc onDestroy;

        /// <summary>
        ///     Decides whether the window can be closed right now.
        /// </summary>
        internal NativeCheckFunc canClose;

        /// <summary>
        ///     Called on a key down event.
        /// </summary>
        internal NativeInputFunc onKeyDown;

        /// <summary>
        ///     Called on a key up event.
        /// </summary>
        internal NativeInputFunc onKeyUp;

        /// <summary>
        ///     Called on a char event.
        /// </summary>
        internal NativeCharFunc onChar;

        /// <summary>
        ///     Called on a mouse move event.
        /// </summary>
        internal NativeMouseMoveFunc onMouseMove;

        /// <summary>
        ///     Called on a mouse wheel event.
        /// </summary>
        internal NativeMouseWheelFunc onMouseWheel;

        /// <summary>
        ///     Called on a size change event.
        /// </summary>
        internal NativeResizeFunc onResize;

        /// <summary>
        ///     Called when the window active state changes.
        /// </summary>
        internal NativeBoolFunc onActiveStateChange;

        /// <summary>
        ///     Called when debug messages of D3D12 are received.
        /// </summary>
        internal D3D12MessageFunc onDebug;

        /// <summary>
        ///     The initial window width.
        /// </summary>
        internal UInt32 width;

        /// <summary>
        ///     The initial window height.
        /// </summary>
        internal UInt32 height;

        /// <summary>
        ///     The initial window title.
        /// </summary>
        internal String title;

        /// <summary>
        ///     The name of the application.
        /// </summary>
        internal String applicationName;

        /// <summary>
        ///     The version of the application.
        /// </summary>
        internal String applicationVersion;

        /// <summary>
        ///     A handle to the icon to use for the window.
        /// </summary>
        internal IntPtr icon;

        /// <summary>
        ///     The scale at which the world is rendered, as a percentage of the window size.
        /// </summary>
        internal Single renderScale;

        /// <summary>
        ///     Additional options for the native side.
        /// </summary>
        internal ConfigurationOptions options;
    }

    [CustomMarshaller(typeof(NativeConfiguration), MarshalMode.ManagedToUnmanagedIn, typeof(NativeConfigurationMarshaller))]
    internal static class NativeConfigurationMarshaller
    {
        internal static Unmanaged ConvertToUnmanaged(NativeConfiguration managed)
        {
            return new Unmanaged
            {
                onRender = Marshal.GetFunctionPointerForDelegate(managed.onRenderUpdate),
                onUpdate = Marshal.GetFunctionPointerForDelegate(managed.onLogicUpdate),
                onInit = Marshal.GetFunctionPointerForDelegate(managed.onInitialization),
                onDestroy = Marshal.GetFunctionPointerForDelegate(managed.onDestroy),
                canClose = Marshal.GetFunctionPointerForDelegate(managed.canClose),
                onKeyDown = Marshal.GetFunctionPointerForDelegate(managed.onKeyDown),
                onKeyUp = Marshal.GetFunctionPointerForDelegate(managed.onKeyUp),
                onChar = Marshal.GetFunctionPointerForDelegate(managed.onChar),
                onMouseMove = Marshal.GetFunctionPointerForDelegate(managed.onMouseMove),
                onMouseWheel = Marshal.GetFunctionPointerForDelegate(managed.onMouseWheel),
                onResize = Marshal.GetFunctionPointerForDelegate(managed.onResize),
                onActiveStateChange = Marshal.GetFunctionPointerForDelegate(managed.onActiveStateChange),
                onDebug = Marshal.GetFunctionPointerForDelegate(managed.onDebug),
                width = managed.width,
                height = managed.height,
                title = UnicodeStringMarshaller.ConvertToUnmanaged(managed.title),
                icon = managed.icon,
                applicationName = UnicodeStringMarshaller.ConvertToUnmanaged(managed.applicationName),
                applicationVersion = UnicodeStringMarshaller.ConvertToUnmanaged(managed.applicationVersion),
                renderScale = managed.renderScale,
                options = managed.options
            };
        }

        internal static void Free(Unmanaged unmanaged)
        {
            UnicodeStringMarshaller.Free(unmanaged.title);
        }

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        internal struct Unmanaged
        {
            internal IntPtr onRender;
            internal IntPtr onUpdate;
            internal IntPtr onInit;
            internal IntPtr onDestroy;
            internal IntPtr canClose;
            internal IntPtr onKeyDown;
            internal IntPtr onKeyUp;
            internal IntPtr onChar;
            internal IntPtr onMouseMove;
            internal IntPtr onMouseWheel;
            internal IntPtr onResize;
            internal IntPtr onActiveStateChange;
            internal IntPtr onDebug;
            internal UInt32 width;
            internal UInt32 height;
            internal IntPtr title;
            internal IntPtr icon;
            internal IntPtr applicationName;
            internal IntPtr applicationVersion;
            internal Single renderScale;
            internal ConfigurationOptions options;
        }
    }
}
