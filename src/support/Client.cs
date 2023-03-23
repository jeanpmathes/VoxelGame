//  <copyright file="Client.cs" company="VoxelGame">
//      MIT License
// 	 For full license see the repository.
//  </copyright>
//  <author>jeanpmathes</author>

using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using VoxelGame.Logging;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Input;
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

    private (int x, int y) mousePosition;

    /// <summary>
    ///     Creates a new native client and initializes it.
    /// </summary>
    protected Client()
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
        configuration.onDebug = OnDebug;

        configuration.allowTearing = false;

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
    protected (int x, int y) MousePosition
    {
        get => mousePosition;
        set
        {
            mousePosition = value;
            Support.Native.SetMousePosition(Native, mousePosition.x, mousePosition.y);
        }
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
    protected void ToggleFullscreen()
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

    /// <summary>
    ///     Called when D3D12 debug messages are received.
    /// </summary>
    protected virtual void OnDebug(
        Definition.Native.D3D12_MESSAGE_CATEGORY category,
        Definition.Native.D3D12_MESSAGE_SEVERITY severity,
        Definition.Native.D3D12_MESSAGE_ID id,
        string? message, IntPtr context)
    {
        // todo: connect to debug system
        logger.LogWarning("D3D12 debug message: {Message}", message);
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


