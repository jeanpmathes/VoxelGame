// <copyright file="Screen.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Logging;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     Common functionality associated with the screen.
/// </summary>
public sealed class Screen : IDisposable // todo: first, move all functionality from here to instance methods of client, then pull out functionality from client to new Screen class that only has instance methods, try to match it on C++ side
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Screen>();

    private bool fullscreen;

    internal Screen(Application.Client client)
    {
        Instance = this;

        Client = client;
    }

    private static Screen Instance { get; set; } = null!;

    private Application.Client Client { get; }

    #region PUBLIC STATIC PROPERTIES // todo: try to remove these, move them to client and use instance methods / properties

    /// <summary>
    ///     Gets the window size. The value is equal to the value retrieved from the client.
    /// </summary>
    public static Vector2i Size => Instance.Client.Size;

    /// <summary>
    ///     Gets whether the screen is in fullscreen.
    /// </summary>
    public static bool IsFullscreen => Instance.fullscreen;

    /// <summary>
    ///     Gets whether the screen is focused.
    /// </summary>
    public static bool IsFocused => Instance.Client.IsFocused;

    /// <summary>
    ///     Gets whether an overlay is open and therefore input should be ignored.
    /// </summary>
    public static bool IsOverlayLockActive { get; private set; }

    #endregion PUBLIC STATIC PROPERTIES

    #region PUBLIC STATIC METHODS

    /// <summary>
    ///     Set the cursor state. Locking the cursor will store the position, unlocking restores it.
    /// </summary>
    /// <param name="locked">Whether the cursor should be locked.</param>
    public static void SetCursor(bool locked)
    {
        if (locked) Instance.Client.Mouse.StorePosition();

        bool visible = !locked;
        bool grabbed = locked;

        // todo: implement cursor locking (but not separate grabbing and visibility, instead just one property)

        // Instance.Client.CursorVisible = visible;
        // Instance.Client.CursorGrabbed = grabbed;

        if (!locked) Instance.Client.Mouse.RestorePosition();
    }

    /// <summary>
    ///     Set if the screen should be in fullscreen.
    /// </summary>
    /// <param name="fullscreen">If fullscreen should be active.</param>
    public static void SetFullscreen(bool fullscreen)
    {
        if (fullscreen == IsFullscreen) return;

        Instance.fullscreen = fullscreen;
        Instance.Client.ToggleFullscreen();
    }

    /// <summary>
    ///     Set the overlay lock, to indicate that an ui overlay is currently active.
    /// </summary>
    public static void SetOverlayLock()
    {
        IsOverlayLockActive = true;
    }

    /// <summary>
    ///     Clear the overlay lock, to indicate that no ui overlay is currently active.
    /// </summary>
    public static void ClearOverlayLock()
    {
        IsOverlayLockActive = false;
    }

    #endregion PUBLIC STATIC METHODS

    #region IDisposable Support

    private bool disposed;

    private void Dispose(bool disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            /*GL.DeleteTexture(msTex);
            GL.DeleteFramebuffer(msFBO);
            GL.DeleteRenderbuffer(depthRBO);

            depthTexture.Dispose();
            colorTexture.Dispose();
            transparencyAccumulationTexture.Dispose();
            transparencyRevealageTexture.Dispose();

            GL.DeleteFramebuffer(shaderFBO);
            GL.DeleteFramebuffer(transparencyFBO);

            GL.DeleteFramebuffer(screenshotFBO);
            GL.DeleteRenderbuffer(screenshotRBO);

            GL.DeleteVertexArray(emptyVAO);*/
        }

        // todo: cleanup, adapt logging here

        logger.LogWarning(Events.UndeletedGlObjects, "Screen object disposed by GC without freeing storage");

        disposed = true;
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~Screen()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    ///     Dispose of the screen object.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable Support
}
