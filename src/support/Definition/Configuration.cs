//  <copyright file="NativeConfiguration.cs" company="VoxelGame">
//      MIT License
// 	 For full license see the repository.
//  </copyright>
//  <author>jeanpmathes</author>

using System.Runtime.InteropServices;

namespace VoxelGame.Support.Definition;

/// <summary>
///     Contains static methods that map to the respective functions on the native side.
/// </summary>
internal static partial class Native
{
    /// <summary>
    ///     Builds the options from the given parameters.
    ///     See <see cref="ConfigurationOptions" /> for more information.
    /// </summary>
    internal static ConfigurationOptions BuildOptions(bool allowTearing, bool supportPIX, bool useGBV)
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
    internal delegate void NativeBoolFunc([MarshalAs(UnmanagedType.Bool)] bool arg);

    /// <summary>
    ///     A simple callback function.
    /// </summary>
    internal delegate void NativeCallbackFunc();

    /// <summary>
    ///     A callback that receives a char value describing an input event.
    /// </summary>
    internal delegate void NativeCharFunc([MarshalAs(UnmanagedType.U2)] char arg);

    /// <summary>
    ///     Checks if a condition is true.
    /// </summary>
    [return: MarshalAs(UnmanagedType.Bool)]
    internal delegate bool NativeCheckFunc();

    /// <summary>
    ///     A callback that receives an HRESULT and an error message, indicating a fatal error.
    /// </summary>
    internal delegate void NativeErrorFunc(int hresult, [MarshalAs(UnmanagedType.LPStr)] string message);

    /// <summary>
    ///     A callback that receives a byte value describing an input event.
    /// </summary>
    internal delegate void NativeInputFunc([MarshalAs(UnmanagedType.U1)] byte arg);

    /// <summary>
    ///     A callback that receives the new mouse position on a mouse move event.
    /// </summary>
    internal delegate void NativeMouseMoveFunc([MarshalAs(UnmanagedType.I4)] int x, [MarshalAs(UnmanagedType.I4)] int y);

    /// <summary>
    ///     A callback that receives the mouse wheel delta on a mouse wheel event.
    /// </summary>
    internal delegate void NativeMouseWheelFunc(double delta);

    /// <summary>
    ///     A callback that receives the new window size on a resize event.
    /// </summary>
    internal delegate void NativeResizeFunc([MarshalAs(UnmanagedType.U4)] uint width, [MarshalAs(UnmanagedType.U4)] uint height);

    /// <summary>
    ///     A callback that receives a double delta time value.
    /// </summary>
    internal delegate void NativeStepFunc([MarshalAs(UnmanagedType.R8)] double arg);

    /// <summary>
    ///     A callback that receives a wide string value.
    /// </summary>
    internal delegate void NativeWStringFunc([MarshalAs(UnmanagedType.LPWStr)] string arg);

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
        ///     Whether to disable some features and change allocation behaviour to improve PIX support.
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
    [StructLayout(LayoutKind.Sequential)]
    #pragma warning disable S3898 // No equality comparison used.
    internal struct NativeConfiguration
    #pragma warning restore S3898 // No equality comparison used.
    {
        /// <summary>
        ///     Called for each rendering step.
        /// </summary>
        internal NativeStepFunc onRender;

        /// <summary>
        ///     Called for each update event.
        /// </summary>
        internal NativeStepFunc onUpdate;

        /// <summary>
        ///     Called on initialization of the native client.
        /// </summary>
        internal NativeCallbackFunc onInit;

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
        internal uint width;

        /// <summary>
        ///     The initial window height.
        /// </summary>
        internal uint height;

        /// <summary>
        ///     The initial window title.
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)] internal string title;

        /// <summary>
        ///     A handle to the icon to use for the window.
        /// </summary>
        internal IntPtr icon;

        /// <summary>
        ///     The scale at which the world is rendered, as a percentage of the window size.
        /// </summary>
        internal float renderScale;

        /// <summary>
        ///     Additional options for the native side.
        /// </summary>
        internal ConfigurationOptions options;
    }
}
