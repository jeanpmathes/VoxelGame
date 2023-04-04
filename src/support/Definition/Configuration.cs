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
    ///     A simple callback function.
    /// </summary>
    public delegate void NativeCallbackFunc();

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
        ///     Called on a key down event.
        /// </summary>
        public NativeInputFunc onKeyDown;

        /// <summary>
        ///     Called on a key up event.
        /// </summary>
        public NativeInputFunc onKeyUp;

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
