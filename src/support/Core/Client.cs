//  <copyright file="Client.cs" company="VoxelGame">
//      MIT License
// 	 For full license see the repository.
//  </copyright>
//  <author>jeanpmathes</author>

using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Graphics;
using VoxelGame.Support.Input;
using VoxelGame.Support.Input.Devices;
using VoxelGame.Support.Input.Events;
using VoxelGame.Support.Objects;
using Image = VoxelGame.Core.Visuals.Image;

namespace VoxelGame.Support.Core;

/// <summary>
///     A proxy class for the native client.
/// </summary>
public class Client : IDisposable // todo: get type usage count down, e.g. by pulling out events
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Client>();

    private readonly ISet<VirtualKeys> mouseButtons = new HashSet<VirtualKeys>
    {
        VirtualKeys.LeftButton,
        VirtualKeys.RightButton,
        VirtualKeys.MiddleButton,
        VirtualKeys.ExtraButton1,
        VirtualKeys.ExtraButton2
    };

#pragma warning disable S1450 // Keep the callback functions alive.
    private Config config;
#pragma warning restore S1450 // Keep the callback functions alive.

    private Thread mainThread = null!;

    private Cycle? cycle = new();

    /// <summary>
    ///     Creates a new native client and initializes it.
    /// </summary>
    protected Client(WindowSettings windowSettings)
    {
        Debug.Assert(windowSettings.Size.X > 0);
        Debug.Assert(windowSettings.Size.Y > 0);

        Size = windowSettings.Size;

        Mouse = new Mouse(this);

        Definition.Native.NativeConfiguration configuration = new()
        {
            onInit = () =>
            {
                mainThread = Thread.CurrentThread;

                OnInit();
            },
            onUpdate = delta =>
            {
                cycle = Cycle.Update;

                Time += delta;

                Mouse.Update();

                OnUpdate(delta);

                Sync.Update();
                KeyState.Update();

                cycle = null;
            },
            onRender = delta =>
            {
                cycle = Cycle.Render;

                OnRender(delta);

                cycle = null;
            },
            onDestroy = OnDestroy,
            canClose = CanClose,
            onKeyDown = OnKeyDown,
            onKeyUp = OnKeyUp,
            onChar = OnChar,
            onMouseMove = OnMouseMove,
            onMouseWheel = OnMouseWheel,
            onResize = (width, height) =>
            {
                Size = new Vector2i((int) width, (int) height);
                OnResize(Size);
            },
            onActiveStateChange = active =>
            {
                if (IsFocused && !active) KeyState.Wipe();

                IsFocused = active;
            },
            onDebug = D3D12Debug.Enable(this),
            width = (uint) windowSettings.Size.X,
            height = (uint) windowSettings.Size.Y,
            title = windowSettings.Title,
            icon = Icon.ExtractAssociatedIcon(Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty)?.Handle ?? IntPtr.Zero,
            renderScale = windowSettings.RenderScale,
            allowTearing = false,
            supportPIX = windowSettings.SupportPIX
        };

        config = new Config(configuration, OnError);

        Native = Support.Native.Initialize(config.Configuration, config.ErrorFunc);
        Space = new Space(this);
    }

    /// <summary>
    ///     Get the mouse device.
    /// </summary>
    public Mouse Mouse { get; }

    /// <summary>
    ///     Whether the client is currently in the update cycle.
    /// </summary>
    internal bool IsInUpdate => cycle == Cycle.Update && Thread.CurrentThread == mainThread;

    /// <summary>
    ///     Whether the client is currently in the render cycle.
    /// </summary>
    internal bool IsInRender => cycle == Cycle.Render && Thread.CurrentThread == mainThread;

    /// <summary>
    ///     Whether the client is currently outside of any cycle but still on the main thread.
    /// </summary>
    internal bool IsOutOfCycle => cycle == null && Thread.CurrentThread == mainThread;

    internal Synchronizer Sync { get; } = new();

    /// <summary>
    ///     Get the total elapsed time.
    /// </summary>
    private double Time { get; set; }

    /// <summary>
    ///     Get the space rendered by the client.
    /// </summary>
    public Space Space { get; } // todo: check all static usages of this and go trough resources instead where possible

    /// <summary>
    ///     Get the native client pointer.
    /// </summary>
    public IntPtr Native { get; }

    /// <summary>
    ///     Get the current key state.
    /// </summary>
    protected KeyState KeyState { get; } = new();

    /// <summary>
    ///     Get the current window size.
    /// </summary>
    public Vector2i Size { get; private set; }

    /// <summary>
    ///     Get the current aspect ratio <c>x/y</c>.
    /// </summary>
    public double AspectRatio => Size.X / (double) Size.Y;

    /// <summary>
    ///     Get whether the window is focused.
    /// </summary>
    public bool IsFocused { get; private set; }

    /// <summary>
    ///     Initialize the raytracing pipeline. This is only necessary if the client is used for raytracing.
    /// </summary>
    internal ShaderBuffer<T>? InitializeRaytracing<T>(SpacePipeline pipeline) where T : unmanaged, IEquatable<T>
    {
        return Support.Native.InitializeRaytracing<T>(this, pipeline);
    }

    private static string FormatErrorMessage(int hr, string message)
    {
        return $"{message} | {Marshal.GetExceptionForHR(hr)?.Message ?? "No Description"}";
    }

    private static void OnError(int hr, string message)
    {
        Debugger.Break();

        Exception exception = Marshal.GetExceptionForHR(hr) ?? new InvalidOperationException(message);
        Support.Native.ShowErrorBox($"Fatal error ({hr:X}): {message}");

        var hex = hr.ToString("X", CultureInfo.InvariantCulture);
        logger.LogCritical(exception, "Fatal error ({HR}): {Message}", hex, message);

        throw exception;
    }

    internal string GetDRED()
    {
        return Support.Native.GetDRED(this);
    }

    internal string GetAllocatorStatistics()
    {
        return Support.Native.GetAllocatorStatistics(this);
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
        Support.Native.RequestClose(this);
    }

    /// <summary>
    ///     Decide whether the window can be closed right now.
    /// </summary>
    protected virtual bool CanClose()
    {
        return true;
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
        Mouse.OnMouseMove((x, y));

        MouseMove(this,
            new MouseMoveEventArgs
            {
                Position = Mouse.Position
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
    ///     Create a raster pipeline.
    /// </summary>
    /// <param name="description">A description of the pipeline.</param>
    /// <param name="errorCallback">A callback for error messages.</param>
    /// <returns>The created pipeline, or <c>null</c> if the pipeline could not be created.</returns>
    public RasterPipeline? CreateRasterPipeline(RasterPipelineDescription description, Action<string> errorCallback)
    {
        return Support.Native.CreateRasterPipeline(this, description, CreateErrorFunc(errorCallback));
    }

    /// <summary>
    ///     Create a raster pipeline with associated shader buffer.
    /// </summary>
    /// <param name="description">A description of the pipeline.</param>
    /// <param name="errorCallback">A callback for error messages.</param>
    /// <typeparam name="T">The type of the shader buffer data.</typeparam>
    /// <returns>The created pipeline and shader buffer, or <c>null</c> if the pipeline could not be created.</returns>
    public (RasterPipeline, ShaderBuffer<T>)? CreateRasterPipeline<T>(RasterPipelineDescription description, Action<string> errorCallback) where T : unmanaged, IEquatable<T>
    {
        return Support.Native.CreateRasterPipeline<T>(this, description, CreateErrorFunc(errorCallback));
    }

    private static Definition.Native.NativeErrorFunc CreateErrorFunc(Action<string> errorCallback)
    {
        return (hr, message) => errorCallback(FormatErrorMessage(hr, message));
    }

    /// <summary>
    ///     Set which pipeline is used for post processing.
    /// </summary>
    public void SetPostProcessingPipeline(RasterPipeline pipeline)
    {
        Support.Native.SetPostProcessingPipeline(this, pipeline);
    }

    /// <summary>
    ///     Add a pipeline to the Draw2D rendering step.
    /// </summary>
    /// <param name="pipeline">The pipeline to add, must use the <see cref="ShaderPresets.ShaderPreset.Draw2D"/> preset.</param>
    /// <param name="priority">
    ///     The priority of the pipeline, higher priority pipelines are rendered later.
    ///     Use the constants <see cref="Draw2D.Foreground"/> and <see cref="Draw2D.Background"/> to add the the current front and back.
    /// </param>
    /// <param name="callback">A callback which will be called each frame and allows to submit draw calls.</param>
    public void AddDraw2dPipeline(RasterPipeline pipeline, int priority, Action<Draw2D> callback)
    {
        Support.Native.AddDraw2DPipeline(this, pipeline, priority, callback);
    }

    /// <summary>
    ///     Load a texture from an image.
    /// </summary>
    /// <param name="image">The image to load from.</param>
    /// <returns>The loaded texture.</returns>
    public Texture LoadTexture(Image image)
    {
        return Support.Native.LoadTexture(this, new[] {image});
    }

    /// <summary>
    ///     Load a texture from a span of images.
    /// </summary>
    /// <param name="images">The images to load from, each image represents a mip level.</param>
    /// <returns>The loaded texture.</returns>
    public Texture LoadTexture(Span<Image> images)
    {
        return Support.Native.LoadTexture(this, images);
    }

    /// <summary>
    ///     Toggle fullscreen mode.
    /// </summary>
    public void ToggleFullscreen()
    {
        Support.Native.ToggleFullscreen(this);
    }

    /// <summary>
    ///     Take a screenshot of the next frame and save it to the given directory.
    /// </summary>
    /// <param name="directory">The directory to save the screenshot to.</param>
    public void TakeScreenshot(DirectoryInfo directory)
    {
        Support.Native.TakeScreenshot(this,
            (data, width, height) =>
            {
                var copy = new int[width * height];
                Marshal.Copy(data, copy, startIndex: 0, copy.Length);

                FileInfo path = directory.GetFile($"{DateTime.Now:yyyy-MM-dd__HH-mm-ss-fff}-screenshot.png");

                Task.Run(() =>
                {
                    Image screenshot = new(copy, Image.Format.BGRA, (int) width, (int) height);
                    Exception? exception = screenshot.Save(path);

                    if (exception == null) logger.LogInformation(Events.Screenshot, "Saved a screenshot to: {Path}", path);
                    else logger.LogError(Events.Screenshot, exception, "Failed to save a screenshot to: {Path}", path);
                });
            });
    }

    /// <summary>
    ///     Run the client. This methods returns when the client is closed.
    /// </summary>
    /// <returns>The exit code of the client.</returns>
    public int Run()
    {
        int exit = Support.Native.Run(this);

        logger.LogDebug(Events.ApplicationState, "Client stopped running with exit code: {ExitCode}", exit);

        return exit;
    }

    private record struct Config(
        Definition.Native.NativeConfiguration Configuration,
        Definition.Native.NativeErrorFunc ErrorFunc);

    #region IDisposable Support

    private void ReleaseUnmanagedResources()
    {
        Support.Native.Finalize(this);
    }

    /// <summary>
    ///     Dispose the client.
    /// </summary>
    /// <param name="disposing">Whether the method was called by the user.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing) logger.LogDebug(Events.ApplicationState, "Disposing client");

        ReleaseUnmanagedResources();

        if (!disposing) return;

        config = new Config();
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
