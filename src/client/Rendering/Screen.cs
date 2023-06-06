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
using VoxelGame.Support.Graphics;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     Common functionality associated with the screen.
/// </summary>
public sealed class Screen : IDisposable
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Screen>();

    private readonly int depthRBO;

    private readonly int emptyVAO;

    private readonly int msFBO;

    private readonly int msTex;

    private readonly int samples;

    private readonly int screenshotFBO;
    private readonly int screenshotRBO;

    private readonly int shaderFBO;

    private readonly int transparencyFBO;
    private bool fullscreen;
    private bool isWireframeActive;

    private Vector2i previousScreenLocation;
    private Vector2i previousScreenSize;

    private bool useWireframe;

    internal Screen(Application.Client client)
    {
        Instance = this;

        Client = client;

        #region MULTISAMPLED FBO

        // todo: port multisampling or maybe just remove it for now (covered by resolution scaling)
        // todo: add future note on using a smart sampling pattern which would make a sampling setting interesting again
        int maxSamples = Context.MaxTextureSamples;
        samples = Math.Clamp(Client.Graphics.SampleCount, min: 1, maxSamples);

        logger.LogDebug(
            Events.VisualQuality,
            "Set sample count to {Samples}, of maximum {Max} possible samples",
            samples,
            maxSamples);

        /*GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.Multisample);

        msTex = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2DMultisample, msTex);

        GL.TexImage2DMultisample(
            TextureTargetMultisample.Texture2DMultisample,
            samples,
            PixelInternalFormat.Rgba8,
            Size.X,
            Size.Y,
            fixedsamplelocations: true);

        GL.BindTexture(TextureTarget.Texture2DMultisample, texture: 0);

        GL.CreateFramebuffers(n: 1, out msFBO);
        GL.NamedFramebufferTexture(msFBO, FramebufferAttachment.ColorAttachment0, msTex, level: 0);

        FramebufferStatus multisampledFboStatus =
            GL.CheckNamedFramebufferStatus(msFBO, FramebufferTarget.Framebuffer);

        while (multisampledFboStatus != FramebufferStatus.FramebufferComplete)
        {
            logger.LogWarning(
                Events.VisualsSetup,
                "Multi-sampled FBO not complete [{Status}], waiting...",
                multisampledFboStatus);

            Thread.Sleep(millisecondsTimeout: 100);

            multisampledFboStatus = GL.CheckNamedFramebufferStatus(msFBO, FramebufferTarget.Framebuffer);
        }

        GL.CreateRenderbuffers(n: 1, out depthRBO);
        GL.NamedRenderbufferStorageMultisample(depthRBO, samples, RenderbufferStorage.Depth24Stencil8, Size.X, Size.Y);

        GL.NamedFramebufferRenderbuffer(
            msFBO,
            FramebufferAttachment.DepthStencilAttachment,
            RenderbufferTarget.Renderbuffer,
            depthRBO);

        GL.NamedFramebufferDrawBuffer(msFBO, DrawBufferMode.ColorAttachment0);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, msFBO);*/

        #endregion MULTISAMPLED FBO

        /*GL.CreateFramebuffers(n: 1, out shaderFBO);

        depthTexture = RenderTexture.Create(shaderFBO,
            Size,
            TextureUnit.Texture20,
            (PixelFormat.DepthComponent, PixelInternalFormat.DepthComponent, PixelType.Float),
            FramebufferAttachment.DepthAttachment);

        colorTexture = RenderTexture.Create(shaderFBO,
            Size,
            TextureUnit.Texture21,
            (PixelFormat.Rgba, PixelInternalFormat.Rgba, PixelType.Float),
            FramebufferAttachment.ColorAttachment0);

        GL.CreateFramebuffers(n: 1, out transparencyFBO);

        transparencyAccumulationTexture = RenderTexture.Create(transparencyFBO,
            Size,
            TextureUnit.Texture22,
            (PixelFormat.Rgba, PixelInternalFormat.Rgba16f, PixelType.HalfFloat),
            FramebufferAttachment.ColorAttachment0);

        transparencyRevealageTexture = RenderTexture.Create(transparencyFBO,
            Size,
            TextureUnit.Texture23,
            (PixelFormat.Red, PixelInternalFormat.R8, PixelType.Float),
            FramebufferAttachment.ColorAttachment1);

        GL.NamedFramebufferDrawBuffers(transparencyFBO, n: 2, new[] {DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1});

        GL.NamedFramebufferRenderbuffer(
            transparencyFBO,
            FramebufferAttachment.DepthStencilAttachment,
            RenderbufferTarget.Renderbuffer,
            depthRBO);*/

        // GL.CreateVertexArrays(n: 1, out emptyVAO);
    }

    private static Screen Instance { get; set; } = null!;

    private Application.Client Client { get; }

    private void EnableWireframe()
    {
        // todo: add wireframe shading

        // GL.LineWidth(width: 5f);
        // GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        //
        // isWireframeActive = true;
    }

    private void DisableWireframe()
    {
        // GL.LineWidth(width: 1f);
        // GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        //
        // isWireframeActive = false;
    }

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
    ///     Set the wire-frame mode. Wireframe is only active when in game draw mode.
    /// </summary>
    /// <param name="wireframe">True to activate wireframe, false to deactivate it.</param>
    public static void SetWireframe(bool wireframe)
    {
        if (Instance.isWireframeActive && !wireframe) Instance.DisableWireframe();

        Instance.useWireframe = wireframe;
    }

    /// <summary>
    ///     Enter the game drawing mode, to use when drawing world content.
    /// </summary>
    public static void EnterGameDrawMode()
    {
        if (Instance.useWireframe) Instance.EnableWireframe();
    }

    /// <summary>
    ///     Enter the ui drawing mode, to use when drawing ui or overlays.
    /// </summary>
    public static void EnterUIDrawMode()
    {
        Instance.DisableWireframe();
    }

    /// <summary>
    ///     Takes a screenshot and saves it to the specified directory.
    /// </summary>
    /// <param name="directory">The directory in which the screenshot should be saved.</param>
    public static void TakeScreenshot(DirectoryInfo directory)
    {
        // todo: implement screenshot taking
        /*IntPtr data = Marshal.AllocHGlobal(Size.X * Size.Y * 4);

        GL.BlitNamedFramebuffer(
            Instance.msFBO,
            Instance.screenshotFBO,
            srcX0: 0,
            srcY0: 0,
            Size.X,
            Size.Y,
            dstX0: 0,
            dstY0: 0,
            Size.X,
            Size.Y,
            ClearBufferMask.ColorBufferBit,
            BlitFramebufferFilter.Nearest);

        GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, Instance.screenshotFBO);
        GL.ReadPixels(x: 0, y: 0, Size.X, Size.Y, PixelFormat.Bgra, PixelType.UnsignedByte, data);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, Instance.msFBO);

        using Bitmap screenshot = new(
            Size.X,
            Size.Y,
            4 * Size.X,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb,
            data);

        screenshot.RotateFlip(RotateFlipType.RotateNoneFlipY);

        FileInfo path = directory.GetFile($"{DateTime.Now:yyyy-MM-dd__HH-mm-ss-fff}-screenshot.png");

        screenshot.Save(path.FullName);
        logger.LogInformation(Events.Screenshot, "Saved a screenshot to: {Path}", path);

        Marshal.FreeHGlobal(data);*/
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
