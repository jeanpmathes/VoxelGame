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
public static partial class Native
{
    /// <summary>
    ///     A callback that receives a bool value.
    /// </summary>
    public delegate void NativeBoolFunc([MarshalAs(UnmanagedType.Bool)] bool arg);

    /// <summary>
    ///     A simple callback function.
    /// </summary>
    public delegate void NativeCallbackFunc();

    /// <summary>
    ///     A callback that receives a char value describing an input event.
    /// </summary>
    public delegate void NativeCharFunc([MarshalAs(UnmanagedType.U2)] char arg);

    /// <summary>
    ///     Checks if a condition is true.
    /// </summary>
    [return: MarshalAs(UnmanagedType.Bool)]
    public delegate bool NativeCheckFunc();

    /// <summary>
    ///     A callback that receives an HRESULT and an error message, indicating a fatal error.
    /// </summary>
    public delegate void NativeErrorFunc(int hresult, [MarshalAs(UnmanagedType.LPStr)] string message);

    /// <summary>
    ///     A callback that receives an error message indicating a fatal error.
    /// </summary>
    public delegate void NativeErrorMessageFunc([MarshalAs(UnmanagedType.LPStr)] string message);

    /// <summary>
    ///     A callback that receives a byte value describing an input event.
    /// </summary>
    public delegate void NativeInputFunc([MarshalAs(UnmanagedType.U1)] byte arg);

    /// <summary>
    ///     A callback that receives the new mouse position on a mouse move event.
    /// </summary>
    public delegate void NativeMouseMoveFunc([MarshalAs(UnmanagedType.I4)] int x, [MarshalAs(UnmanagedType.I4)] int y);

    /// <summary>
    ///     A callback that receives the mouse wheel delta on a mouse wheel event.
    /// </summary>
    public delegate void NativeMouseWheelFunc(double delta);

    /// <summary>
    ///     A callback that receives the new window size on a resize event.
    /// </summary>
    public delegate void NativeResizeFunc([MarshalAs(UnmanagedType.U4)] uint width, [MarshalAs(UnmanagedType.U4)] uint height);

    /// <summary>
    ///     A callback that receives a double delta time value.
    /// </summary>
    public delegate void NativeStepFunc([MarshalAs(UnmanagedType.R8)] double arg);

    /// <summary>
    ///     Contains the configuration of the native side.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    #pragma warning disable S3898 // No equality comparison used.
    public struct NativeConfiguration
    #pragma warning restore S3898 // No equality comparison used.
    {
        /// <summary>
        ///     Called for each rendering step.
        /// </summary>
        public NativeStepFunc onRender;

        /// <summary>
        ///     Called for each update event.
        /// </summary>
        public NativeStepFunc onUpdate;

        /// <summary>
        ///     Called on initialization of the native client.
        /// </summary>
        public NativeCallbackFunc onInit;

        /// <summary>
        ///     Called on shutdown of the native client.
        /// </summary>
        public NativeCallbackFunc onDestroy;

        /// <summary>
        ///     Decides whether the window can be closed right now.
        /// </summary>
        public NativeCheckFunc canClose;

        /// <summary>
        ///     Called on a key down event.
        /// </summary>
        public NativeInputFunc onKeyDown;

        /// <summary>
        ///     Called on a key up event.
        /// </summary>
        public NativeInputFunc onKeyUp;

        /// <summary>
        ///     Called on a char event.
        /// </summary>
        public NativeCharFunc onChar;

        /// <summary>
        ///     Called on a mouse move event.
        /// </summary>
        public NativeMouseMoveFunc onMouseMove;

        /// <summary>
        ///     Called on a mouse wheel event.
        /// </summary>
        public NativeMouseWheelFunc onMouseWheel;

        /// <summary>
        ///     Called on a size change event.
        /// </summary>
        public NativeResizeFunc onResize;

        /// <summary>
        ///     Called when the window active state changes.
        /// </summary>
        public NativeBoolFunc onActiveStateChange;

        /// <summary>
        ///     Called when debug messages of D3D12 are received.
        /// </summary>
        public D3D12MessageFunc onDebug;

        /// <summary>
        ///     Whether to allow tearing.
        /// </summary>
        [MarshalAs(UnmanagedType.Bool)] public bool allowTearing;

        /// <summary>
        ///     Whether to render the 3D scene.
        /// </summary>
        [MarshalAs(UnmanagedType.Bool)] public bool enableSpace; // todo: remove this as soon as 3D DXR pipeline can be configured, then not setting up the pipeline is implicitly the same as disabling it
    }
}
