// <copyright file="Client.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.App;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Graphics;
using VoxelGame.Graphics.Objects;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Interop;
using VoxelGame.Toolkit.Utilities;
using Image = VoxelGame.Core.Visuals.Image;
using Timer = VoxelGame.Core.Profiling.Timer;

namespace VoxelGame.Graphics.Core;

/// <summary>
///     A proxy class for the native client.
/// </summary>
[NativeMarshalling(typeof(ClientMarshaller))]
public partial class Client : Application
{
#pragma warning disable S1450 // Keep the callback functions alive.
    private Config config;
#pragma warning restore S1450 // Keep the callback functions alive.

    private Cycle? cycle = new();

    /// <summary>
    ///     Creates a new native client and initializes it.
    /// </summary>
    protected unsafe Client(WindowSettings windowSettings, Version version) : base(version)
    {
        Debug.Assert(windowSettings.Size.X > 0);
        Debug.Assert(windowSettings.Size.Y > 0);

        Size = windowSettings.Size;

        Input = new Input.Input(this);

        Definition.Native.NativeConfiguration configuration = new()
        {
            onInitialization = () =>
            {
                using Timer? timer = logger.BeginTimedScoped("Client Initialization");

                ThrowIfNotOnMainThread(this);

                DoInitialization(timer);
            },
            onLogicUpdate = delta =>
            {
                using Timer? timer = logger.BeginTimedScoped("Client Logic Update");

                cycle = Cycle.Update;

                Time += delta;

                Input.PreLogicUpdate();

                DoLogicUpdate(delta, timer);

                Sync.LogicUpdate();

                Input.PostLogicUpdate();

                cycle = null;

            },
            onRenderUpdate = delta =>
            {
                using Timer? timer = logger.BeginTimedScoped("Client Render Update");

                cycle = Cycle.Render;

                DoRenderUpdate(delta, timer);

                cycle = null;

            },
            onDestroy = () =>
            {
                using Timer? timer = logger.BeginTimedScoped("Client Destroy");

                LogClosingWindow(logger);

                DoDestroy(timer);

            },
            canClose = CanClose,
            onKeyDown = Input.OnKeyDown,
            onKeyUp = Input.OnKeyUp,
            onChar = Input.OnChar,
            onMouseMove = Input.OnMouseMove,
            onMouseWheel = Input.OnMouseWheel,
            onResize = (width, height) =>
            {
                Vector2i oldSize = Size;
                Size = new Vector2i((Int32) width, (Int32) height);

                SizeChanged?.Invoke(this, new SizeChangeEventArgs(oldSize, Size));
            },
            onActiveStateChange = newState =>
            {
                Boolean oldState = IsFocused;
                IsFocused = newState;

                if (oldState != newState)
                    FocusChanged?.Invoke(this, new FocusChangeEventArgs(oldState, IsFocused));
            },
            onDebug = D3D12Debug.Enable(this),
            width = (UInt32) windowSettings.Size.X,
            height = (UInt32) windowSettings.Size.Y,
            title = windowSettings.Title,
            icon = Icon.ExtractAssociatedIcon(Process.GetCurrentProcess().MainModule?.FileName ?? String.Empty)?.Handle ?? IntPtr.Zero,
            applicationName = Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown Application",
            applicationVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown Version",
            renderScale = windowSettings.RenderScale,
            options = Definition.Native.BuildOptions(
                allowTearing: false,
                windowSettings.SupportPIX,
                windowSettings.UseGBV)
        };

        config = new Config(configuration, OnError);

        Native = NativeMethods.Configure(config.Configuration, config.ErrorFunc);
        Space = new Space(this);
    }

    /// <inheritdoc />
    protected override Client Self => this;

    /// <summary>
    ///     Get the input system of the client.
    /// </summary>
    public Input.Input Input { get; }

    /// <summary>
    ///     Whether the client is currently in the logic update cycle.
    /// </summary>
    internal Boolean IsInLogicUpdate => cycle == Cycle.Update && IsOnMainThread;

    /// <summary>
    ///     Whether the client is currently in the render update cycle.
    /// </summary>
    internal Boolean IsInRenderUpdate => cycle == Cycle.Render && IsOnMainThread;

    /// <summary>
    ///     Whether the client is currently outside any cycle but still on the main thread.
    /// </summary>
    internal Boolean IsOutOfCycle => cycle == null && IsOnMainThread;

    internal Synchronizer Sync { get; } = new();

    /// <summary>
    ///     Get the total elapsed time.
    /// </summary>
    private Double Time { get; set; }

    /// <summary>
    ///     Get the space rendered by the client.
    /// </summary>
    public Space Space { get; }

    /// <summary>
    ///     Get the native client pointer.
    /// </summary>
    public IntPtr Native { get; }

    /// <summary>
    ///     Get the current window size.
    /// </summary>
    public Vector2i Size { get; private set; }

    /// <summary>
    ///     Get the current aspect ratio <c>x/y</c>.
    /// </summary>
    public Double AspectRatio
    {
        get
        {
            Double ratio = Size.X / (Double) Size.Y;

            if (Double.IsNaN(ratio) || Double.IsInfinity(ratio))
                return 1.0;

            return ratio;
        }
    }

    /// <summary>
    ///     Get whether the window is focused.
    /// </summary>
    public Boolean IsFocused { get; private set; }

    /// <summary>
    ///     Called when the focus / active state of the window changes.
    /// </summary>
    public event EventHandler<FocusChangeEventArgs>? FocusChanged;

    /// <summary>
    ///     Called when the window is resized.
    /// </summary>
    public event EventHandler<SizeChangeEventArgs>? SizeChanged;

    /// <summary>
    ///     Initialize the raytracing pipeline. This is only necessary if the client is used for raytracing.
    /// </summary>
    internal ShaderBuffer<T>? InitializeRaytracing<T>(SpacePipelineDescription description) where T : unmanaged, IEquatable<T>
    {
        return VoxelGame.Graphics.Native.InitializeRaytracing<T>(this, description);
    }

    private static String FormatErrorMessage(Int32 hr, String message)
    {
        return $"{message} | {Marshal.GetExceptionForHR(hr)?.Message ?? "No Description"}";
    }

    private static unsafe void OnError(Int32 hr, Byte* messagePointer)
    {
        String message = Utf8StringMarshaller.ConvertToManaged(messagePointer) ?? "No message provided!";
        
        Debugger.Break();

        Exception exception = Marshal.GetExceptionForHR(hr) ?? new InvalidOperationException(message);
        Dialog.ShowError($"Fatal error ({hr:X}): {message}");

        var hex = hr.ToString("X", CultureInfo.InvariantCulture);
        LogFatalError(logger, exception, hex, message);

        throw exception;
    }

    internal String GetDRED()
    {
        return VoxelGame.Graphics.Native.GetDRED(this);
    }

    internal String GetAllocatorStatistics()
    {
        return VoxelGame.Graphics.Native.GetAllocatorStatistics(this);
    }

    /// <summary>
    ///     Close the window.
    /// </summary>
    public void Close()
    {
        NativeMethods.RequestClose(this);
    }

    /// <summary>
    ///     Decide whether the window can be closed right now.
    /// </summary>
    protected virtual Bool CanClose()
    {
        return true;
    }

    /// <summary>
    ///     Create a raster pipeline.
    /// </summary>
    /// <param name="description">A description of the pipeline.</param>
    /// <param name="errorCallback">A callback for error messages.</param>
    /// <returns>The created pipeline, or <c>null</c> if the pipeline could not be created.</returns>
    public RasterPipeline? CreateRasterPipeline(RasterPipelineDescription description, Action<String> errorCallback)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        return VoxelGame.Graphics.Native.CreateRasterPipeline(this, description, CreateErrorFunc(errorCallback));
    }

    /// <summary>
    ///     Create a raster pipeline with associated shader buffer.
    /// </summary>
    /// <param name="description">A description of the pipeline.</param>
    /// <param name="errorCallback">A callback for error messages.</param>
    /// <typeparam name="T">The type of the shader buffer data.</typeparam>
    /// <returns>The created pipeline and shader buffer, or <c>null</c> if the pipeline could not be created.</returns>
    public (RasterPipeline, ShaderBuffer<T>)? CreateRasterPipeline<T>(RasterPipelineDescription description, Action<String> errorCallback) where T : unmanaged, IEquatable<T>
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        return VoxelGame.Graphics.Native.CreateRasterPipeline<T>(this, description, CreateErrorFunc(errorCallback));
    }

    private static unsafe Definition.Native.NativeErrorFunc CreateErrorFunc(Action<String> errorCallback)
    {
        return (hr, messagePointer) =>
        {
            String message = Utf8StringMarshaller.ConvertToManaged(messagePointer) ?? "No message provided!";
            
            errorCallback(FormatErrorMessage(hr, message));
        };
    }

    /// <summary>
    ///     Set which pipeline is used for post-processing.
    /// </summary>
    public void SetPostProcessingPipeline(RasterPipeline pipeline)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        NativeMethods.DesignatePostProcessingPipeline(this, pipeline);
    }

    /// <summary>
    ///     Add a pipeline to the Draw2D rendering step.
    /// </summary>
    /// <param name="pipeline">The pipeline to add, must use the <see cref="ShaderPresets.ShaderPreset.Draw2D" /> preset.</param>
    /// <param name="priority">
    ///     The priority of the pipeline, higher priority pipelines are rendered later.
    ///     Use the constants <see cref="Draw2D.Foreground" /> and <see cref="Draw2D.Background" /> to add the current
    ///     front and back.
    /// </param>
    /// <param name="callback">A callback which will be called each frame and allows to submit draw calls.</param>
    /// <returns>A disposable object which can be used to remove the pipeline.</returns>
    public IDisposable AddDraw2dPipeline(RasterPipeline pipeline, Int32 priority, Action<Draw2D> callback)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        return VoxelGame.Graphics.Native.AddDraw2DPipeline(this, pipeline, priority, callback);
    }

    /// <summary>
    ///     Load a texture from an image.
    /// </summary>
    /// <param name="image">The image to load from.</param>
    /// <returns>The loaded texture.</returns>
    public Texture LoadTexture(Image image)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        return VoxelGame.Graphics.Native.LoadTexture(this, [image]);
    }

    /// <summary>
    ///     Load a texture from a span of images.
    /// </summary>
    /// <param name="images">The images to load from, each image represents a mip level.</param>
    /// <returns>The loaded texture.</returns>
    public Texture LoadTexture(Span<Image> images)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        return VoxelGame.Graphics.Native.LoadTexture(this, images);
    }

    /// <summary>
    ///     Toggle fullscreen mode.
    /// </summary>
    public void ToggleFullscreen()
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        NativeMethods.ToggleFullscreen(this);
    }

    /// <summary>
    ///     Take a screenshot of the next frame and save it to the given directory.
    /// </summary>
    /// <param name="directory">The directory to save the screenshot to.</param>
    public void TakeScreenshot(DirectoryInfo directory)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        VoxelGame.Graphics.Native.EnqueueScreenshot(this,
            (data, width, height) =>
            {
                var copy = new Int32[width * height];
                Marshal.Copy(data, copy, startIndex: 0, copy.Length);

                FileInfo path = directory.GetFile($"{DateTime.Now:yyyy-MM-dd__HH-mm-ss-fff}-screenshot.png");

                Operations.Launch(async token =>
                {
                    Image screenshot = new(copy, Image.Format.BGRA, (Int32) width, (Int32) height);
                    Result result = await screenshot.SaveAsync(path, token).InAnyContext();

                    result.Switch(
                        () => LogSavedScreenshot(logger, path.FullName),
                        exception => LogFailedToSaveScreenshot(logger, exception, path.FullName));
                });
            });
    }

    /// <summary>
    ///     Run the client. This method returns when the client is closed.
    /// </summary>
    /// <returns>The exit code of the client.</returns>
    public Int32 Run()
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        Int32 exit = NativeMethods.Run(this);

        LogClientStoppedRunning(logger, exit);

        return exit;
    }

    private record struct Config(
        Definition.Native.NativeConfiguration Configuration,
        Definition.Native.NativeErrorFunc ErrorFunc);

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Client>();

    [LoggerMessage(EventId = LogID.Client + 0, Level = LogLevel.Information, Message = "Closing window")]
    private static partial void LogClosingWindow(ILogger logger);

    [LoggerMessage(EventId = LogID.Client + 1, Level = LogLevel.Debug, Message = "Client stopped running with exit code: {ExitCode}")]
    private static partial void LogClientStoppedRunning(ILogger logger, Int32 exitCode);

    [LoggerMessage(EventId = LogID.Client + 2, Level = LogLevel.Debug, Message = "Disposing client")]
    private static partial void LogDisposingClient(ILogger logger);

    [LoggerMessage(EventId = LogID.Client + 3, Level = LogLevel.Information, Message = "Saved a screenshot to: {Path}")]
    private static partial void LogSavedScreenshot(ILogger logger, String path);

    [LoggerMessage(EventId = LogID.Client + 4, Level = LogLevel.Error, Message = "Failed to save a screenshot to: {Path}")]
    private static partial void LogFailedToSaveScreenshot(ILogger logger, Exception exception, String path);

    [LoggerMessage(EventId = LogID.Client + 5, Level = LogLevel.Critical, Message = "Fatal error ({HR}): {Message}")]
    private static partial void LogFatalError(ILogger logger, Exception exception, String hr, String message);

    #endregion LOGGING

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        if (disposed) return;

        if (disposing)
        {
            LogDisposingClient(logger);

            NativeMethods.Finalize(this);

            config = new Config();
        }
        else
        {
            ExceptionTools.ThrowForMissedDispose(nameof(Client));
        }

        disposed = true;
    }

    #endregion
}

[CustomMarshaller(typeof(Client), MarshalMode.ManagedToUnmanagedIn, typeof(ClientMarshaller))]
internal static class ClientMarshaller
{
    internal static IntPtr ConvertToUnmanaged(Client managed)
    {
        return managed.Native;
    }
}
