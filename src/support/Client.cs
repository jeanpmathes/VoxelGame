//  <copyright file="Client.cs" company="VoxelGame">
//      MIT License
// 	 For full license see the repository.
//  </copyright>
//  <author>jeanpmathes</author>

using System.Drawing;
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

    private readonly ISet<VirtualKeys> mouseButtons = new HashSet<VirtualKeys>
    {
        VirtualKeys.LeftButton,
        VirtualKeys.RightButton,
        VirtualKeys.MiddleButton,
        VirtualKeys.ExtraButton1,
        VirtualKeys.ExtraButton2
    };

    private readonly List<NativeObject> objects = new();

    private Vector2i mousePosition;

    /// <summary>
    ///     Creates a new native client and initializes it.
    /// </summary>
    protected Client(WindowSettings windowSettings, bool enableSpace) // todo: remove the enable space arg asap
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
        configuration.onChar = OnChar;
        configuration.onMouseMove = OnMouseMove;
        configuration.onMouseWheel = OnMouseWheel;

        configuration.onResize = (width, height) =>
        {
            OnResize(new Vector2i((int) width, (int) height));
        };

        configuration.onDebug = D3D12Debug.Enable();

        configuration.allowTearing = false;
        configuration.enableSpace = enableSpace;

        // todo: add window settings values to configuration and use on native side

        Native = Support.Native.Initialize(configuration, OnError, OnErrorMessage);
        Space = new Space(this);
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

    /// <summary>
    ///     Register a new native object.
    /// </summary>
    internal void RegisterObject(NativeObject nativeObject)
    {
        objects.Add(nativeObject);
    }

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
    public event EventHandler<MouseButtonEventArgs> MouseButton = delegate {};

    /// <summary>
    ///     Called when the mouse moves.
    /// </summary>
    public event EventHandler<MouseMoveEventArgs> MouseMove = delegate {};

    /// <summary>
    ///     Called when the mouse wheel is scrolled.
    /// </summary>
    public event EventHandler<MouseWheelEventArgs> MouseWheel = delegate {};

    /// <summary>
    ///     Called when a keyboard key is pressed.
    /// </summary>
    public event EventHandler<KeyboardKeyEventArgs> KeyDown = delegate {};

    /// <summary>
    ///     Called when a keyboard key is released.
    /// </summary>
    public event EventHandler<KeyboardKeyEventArgs> KeyUp = delegate {};

    /// <summary>
    ///     Called when a text input is received.
    /// </summary>
    public event EventHandler<TextInputEventArgs> TextInput = delegate {};

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
    protected virtual void OnResize(Vector2i size) {}

    /// <summary>
    ///     Called when the client is destroyed.
    /// </summary>
    protected virtual void OnDestroy() {}

    private void OnKeyDown(byte key)
    {
        var virtualKey = (VirtualKeys) key;
        KeyState.SetKeyState(virtualKey, down: true);
        HandleKey(virtualKey, down: true);
    }

    private void OnKeyUp(byte key)
    {
        var virtualKey = (VirtualKeys) key;
        KeyState.SetKeyState(virtualKey, down: false);
        HandleKey(virtualKey, down: false);
    }

    private void OnChar(char character)
    {
        TextInput(this,
            new TextInputEventArgs
            {
                Character = character
            });
    }

    private void OnMouseMove(int x, int y)
    {
        mousePosition = new Vector2i(x, y);

        MouseMove(this,
            new MouseMoveEventArgs
            {
                Position = MousePosition
            });
    }

    private void OnMouseWheel(double delta)
    {
        MouseWheel(this,
            new MouseWheelEventArgs
            {
                Delta = delta
            });
    }

    private void HandleKey(VirtualKeys key, bool down)
    {
        if (mouseButtons.Contains(key))
        {
            MouseButton(this,
                new MouseButtonEventArgs
                {
                    Button = key,
                    IsPressed = down
                });
        }
        else
        {
            KeyboardKeyEventArgs args = new()
            {
                Key = key
            };

            if (down) KeyDown(this, args);
            else KeyUp(this, args);
        }
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
    ///     Create a raster pipeline.
    /// </summary>
    /// <param name="description">A description of the pipeline.</param>
    /// <param name="errorCallback">A callback for error messages.</param>
    /// <returns>The created pipeline.</returns>
    public RasterPipeline CreateRasterPipeline(PipelineDescription description, Action<string> errorCallback)
    {
        return Support.Native.CreateRasterPipeline(this, description, msg => errorCallback(msg));
    }

    /// <summary>
    ///     Create a raster pipeline with associated shader buffer.
    /// </summary>
    /// <param name="description">A description of the pipeline.</param>
    /// <param name="errorCallback">A callback for error messages.</param>
    /// <typeparam name="T">The type of the shader buffer data.</typeparam>
    /// <returns>The created pipeline and shader buffer.</returns>
    public (RasterPipeline, ShaderBuffer<T>) CreateRasterPipeline<T>(PipelineDescription description, Action<string> errorCallback) where T : unmanaged
    {
        return Support.Native.CreateRasterPipeline<T>(this, description, msg => errorCallback(msg));
    }

    /// <summary>
    ///     Set which pipeline is used for post processing.
    /// </summary>
    public void SetPostProcessingPipeline(RasterPipeline pipeline)
    {
        Support.Native.SetPostProcessingPipeline(this, pipeline);
    }

    /// <summary>
    ///     Add a pipeline to the draw2d rendering step.
    /// </summary>
    /// <param name="pipeline"></param>
    /// <param name="callback"></param>
    public void AddDraw2dPipeline(RasterPipeline pipeline, Action<Draw2D> callback)
    {
        Support.Native.AddDraw2DPipeline(this, pipeline, callback);
    }

    /// <summary>
    ///     Load a texture from a bitmap.
    /// </summary>
    /// <param name="bitmap">The bitmap to load from.</param>
    /// <returns>The loaded texture.</returns>
    public Texture LoadTexture(Bitmap bitmap)
    {
        return Support.Native.LoadTexture(this, bitmap);
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
