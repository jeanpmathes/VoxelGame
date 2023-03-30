//  <copyright file="Client.cs" company="VoxelGame">
//      MIT License
// 	 For full license see the repository.
//  </copyright>
//  <author>jeanpmathes</author>

using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Logging;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Graphics;
using VoxelGame.Support.Input;
using VoxelGame.Support.Input.Events;
using VoxelGame.Support.Objects;

namespace VoxelGame.Support;

/// <summary>
///     A proxy class for the native client.
/// </summary>
public class Client : IDisposable
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Client>();

    private readonly Definition.Native.NativeConfiguration configuration;

    private readonly List<NativeObject> objects = new();

    private Vector2i mousePosition;

    /// <summary>
    ///     Creates a new native client and initializes it.
    /// </summary>
    protected Client(WindowSettings windowSettings)
    {
        configuration.onInit = OnInit;

        configuration.onUpdate = delta =>
        {
            Time += delta;

            mousePosition = Support.Native.GetMousePosition(Native);

            OnUpdate(delta);

            foreach (NativeObject nativeObject in objects) nativeObject.PrepareSynchronization();

            foreach (NativeObject nativeObject in objects) nativeObject.Synchronize();

            KeyState.Update();
        };

        configuration.onRender = OnRender;
        configuration.onDestroy = OnDestroy;
        configuration.onKeyDown = OnKeyDown;
        configuration.onKeyUp = OnKeyUp;
        configuration.onDebug = D3D12Debug.Enable();

        configuration.allowTearing = false;

        // todo: add window settings values to configuration and use on native side

        Native = Support.Native.Initialize(configuration, OnError, OnErrorMessage);
        Space = new Space(this, o => objects.Add(o));
    }

    /// <summary>
    ///     Get the total elapsed time.
    /// </summary>
    protected double Time { get; private set; }

    /// <summary>
    ///     Get the space rendered by the client.
    /// </summary>
    public Space Space { get; }

    /// <summary>
    ///     Get the native client pointer.
    /// </summary>
    public IntPtr Native { get; }

    /// <summary>
    ///     Get the current key state.
    /// </summary>
    protected KeyState KeyState { get; } = new();

    /// <summary>
    ///     Get or set the mouse position.
    /// </summary>
    public Vector2i MousePosition
    {
        get => mousePosition;
        set
        {
            mousePosition = value;
            Support.Native.SetMousePosition(Native, mousePosition.X, mousePosition.Y);
        }
    }

    /// <summary>
    ///     Get the current window size.
    /// </summary>
    public Vector2i Size => Vector2i.Zero; // todo: implement

    /// <summary>
    ///     Get whether the window is focused.
    /// </summary>
    public bool IsFocused => true; // todo: implement

    /// <summary>
    ///     Get or set the current mouse cursor.
    /// </summary>
    public MouseCursor Cursor
    {
        get;
        set;
        // todo: pass to C++, use SetCursor and LoadCursorA (https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-loadcursora)
    } = MouseCursor.Arrow;

    private static void OnError(int hr, string message)
    {
        Exception? exception = Marshal.GetExceptionForHR(hr);

        if (exception == null) return;

        Support.Native.ShowErrorBox($"Fatal error ({hr:X}): {message}");

        var hex = hr.ToString("X", CultureInfo.InvariantCulture);
        logger.LogCritical(exception, "Fatal error ({HR}): {Message}", hex, message);

        throw exception;
    }

    private static void OnErrorMessage(string message)
    {
        Support.Native.ShowErrorBox($"Fatal error: {message}");
        logger.LogCritical("Fatal error: {Message}", message);
    }

    /// <summary>
    ///     Called when a mouse button is pressed or released.
    /// </summary>
    public event EventHandler<MouseButtonEventArgs> MouseButton = delegate {}; // todo: in key up/down, filter for mouse events

    /// <summary>
    ///     Called when the mouse moves.
    /// </summary>
    public event EventHandler<MouseMoveEventArgs> MouseMove = delegate {}; // todo: pass mouse move from C++ to C#

    /// <summary>
    ///     Called when the mouse wheel is scrolled.
    /// </summary>
    public event EventHandler<MouseWheelEventArgs> MouseWheel = delegate {}; // todo: pass mouse wheel (WM_MOUSEWHEEL) from C++ to C# 

    /// <summary>
    ///     Called when a keyboard key is pressed.
    /// </summary>
    public event EventHandler<KeyboardKeyEventArgs> KeyDown = delegate {}; // todo: call this in OnKeyDown (if not calling the mouse events)

    /// <summary>
    ///     Called when a keyboard key is released.
    /// </summary>
    public event EventHandler<KeyboardKeyEventArgs> KeyUp = delegate {}; // todo: call this in OnKeyUp (if not calling the mouse events)

    /// <summary>
    ///     Called when a text input is received.
    /// </summary>
    public event EventHandler<TextInputEventArgs> TextInput = delegate {}; // todo: pass text input from C++ to C#, use WM_CHAR 

    /// <summary>
    ///     Close the window.
    /// </summary>
    public void Close()
    {
        // todo: implement, ensure that OnDestroy is called
    }

    /// <summary>
    ///     Called on initialization of the client.
    /// </summary>
    protected virtual void OnInit() {}

    /// <summary>
    ///     Called for each update step.
    /// </summary>
    /// <param name="delta">The time since the last update in seconds.</param>
    protected virtual void OnUpdate(double delta) {}

    /// <summary>
    ///     Called for each render step.
    /// </summary>
    /// <param name="delta">The time since the last render in seconds.</param>
    protected virtual void OnRender(double delta) {}

    /// <summary>
    ///     Called when the window is resized.
    /// </summary>
    /// <param name="size">The new size.</param>
    protected virtual void OnResize(Vector2i size) {} // todo: call it (new callback must be added to C++)

    /// <summary>
    ///     Called when the client is destroyed.
    /// </summary>
    protected virtual void OnDestroy() {}

    private void OnKeyDown(byte key)
    {
        KeyState.SetKeyState((VirtualKeys) key, down: true);
    }

    private void OnKeyUp(byte key)
    {
        KeyState.SetKeyState((VirtualKeys) key, down: false);
    }

    /// <summary>
    ///     Set the resolution of the client.
    /// </summary>
    /// <param name="width">The new width.</param>
    /// <param name="height">The new height.</param>
    protected void SetResolution(uint width, uint height)
    {
        Support.Native.SetResolution(Native, width, height);
    }

    /// <summary>
    ///     Toggle fullscreen mode.
    /// </summary>
    public void ToggleFullscreen()
    {
        Support.Native.ToggleFullscreen(Native);
    }

    /// <summary>
    ///     Run the client. This methods returns when the client is closed.
    /// </summary>
    /// <returns>The exit code of the client.</returns>
    public int Run()
    {
        return Support.Native.Run(Native);
    }

    #region IDisposable Support

    private void ReleaseUnmanagedResources()
    {
        Support.Native.Finalize(Native);
    }

    /// <summary>
    ///     Dispose the client.
    /// </summary>
    /// <param name="disposing">Whether the method was called by the user.</param>
    protected virtual void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();

        if (disposing)
        {
            // release managed resources here
        }
    }

    /// <summary>
    ///     Dispose the client.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~Client()
    {
        Dispose(disposing: false);
    }

    #endregion
}

