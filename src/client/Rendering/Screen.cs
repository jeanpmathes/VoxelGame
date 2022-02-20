// <copyright file="Screen.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.GraphicsLibraryFramework;
using VoxelGame.Logging;
using Monitor = OpenToolkit.Windowing.GraphicsLibraryFramework.Monitor;

namespace VoxelGame.Client.Rendering
{
    /// <summary>
    ///     Common functionality associated with the screen.
    /// </summary>
    public sealed class Screen : IDisposable
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<Screen>();
        private readonly int depthFBO;

        private readonly int depthTex;
        private readonly int msFBO;
        private readonly int msRBO;

        private readonly int msTex;

        private readonly int samples;

        private readonly int screenshotFBO;
        private readonly int screenshotRBO;
        private bool isWireframeActive;
        private Vector2i previousScreenLocation;

        private Vector2i previousScreenSize;

        private bool useWireframe;

        internal Screen(Application.Client client)
        {
            Instance = this;

            Client = client;

            client.Resize += OnResize;

            #region MULTISAMPLED FBO

            int maxSamples = GL.GetInteger(GetPName.MaxSamples);
            samples = Math.Clamp(Client.Graphics.SampleCount, min: 1, maxSamples);

            logger.LogDebug(
                Events.VisualQuality,
                "Set sample count to {Samples}, of maximum {Max} possible samples",
                samples,
                maxSamples);

            GL.Enable(EnableCap.DepthTest);
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

            GL.CreateRenderbuffers(n: 1, out msRBO);
            GL.NamedRenderbufferStorageMultisample(msRBO, samples, RenderbufferStorage.Depth24Stencil8, Size.X, Size.Y);

            GL.NamedFramebufferRenderbuffer(
                msFBO,
                FramebufferAttachment.DepthStencilAttachment,
                RenderbufferTarget.Renderbuffer,
                msRBO);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, msFBO);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            #endregion MULTISAMPLED FBO

            #region DEPTH TEXTURE

            depthTex = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture20);
            GL.BindTexture(TextureTarget.Texture2D, depthTex);

            GL.TexImage2D(
                TextureTarget.Texture2D,
                level: 0,
                PixelInternalFormat.DepthComponent,
                Size.X,
                Size.Y,
                border: 0,
                PixelFormat.DepthComponent,
                PixelType.Float,
                IntPtr.Zero);

            GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter,
                (int) TextureMinFilter.Nearest);

            GL.TexParameter(
                TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter,
                (int) TextureMagFilter.Nearest);

            GL.CreateFramebuffers(n: 1, out depthFBO);
            GL.NamedFramebufferTexture(depthFBO, FramebufferAttachment.DepthAttachment, depthTex, level: 0);

            FramebufferStatus depthFboStatus = GL.CheckNamedFramebufferStatus(depthFBO, FramebufferTarget.Framebuffer);

            while (depthFboStatus != FramebufferStatus.FramebufferComplete)
            {
                logger.LogWarning(Events.VisualsSetup, "Depth FBO not complete [{Status}], waiting...", depthFboStatus);
                Thread.Sleep(millisecondsTimeout: 100);

                depthFboStatus = GL.CheckNamedFramebufferStatus(depthFBO, FramebufferTarget.Framebuffer);
            }

            GL.ActiveTexture(TextureUnit.Texture0);

            #endregion DEPTH TEXTURE

            #region SCREENSHOT FBO

            GL.CreateFramebuffers(n: 1, out screenshotFBO);

            GL.CreateRenderbuffers(n: 1, out screenshotRBO);
            GL.NamedRenderbufferStorage(screenshotRBO, RenderbufferStorage.Rgba8, Size.X, Size.Y);

            GL.NamedFramebufferRenderbuffer(
                screenshotFBO,
                FramebufferAttachment.ColorAttachment0,
                RenderbufferTarget.Renderbuffer,
                screenshotRBO);

            FramebufferStatus screenshotFboStatus =
                GL.CheckNamedFramebufferStatus(screenshotFBO, FramebufferTarget.Framebuffer);

            while (screenshotFboStatus != FramebufferStatus.FramebufferComplete)
            {
                logger.LogWarning(
                    Events.VisualsSetup,
                    "Screenshot FBO not complete [{Status}], waiting...",
                    screenshotFboStatus);

                Thread.Sleep(millisecondsTimeout: 100);

                screenshotFboStatus = GL.CheckNamedFramebufferStatus(screenshotFBO, FramebufferTarget.Framebuffer);
            }

            #endregion SCREENSHOT FBO

        }

        private static Screen Instance { get; set; } = null!;

        private Application.Client Client { get; }

        /// <summary>
        ///     Clear the screen buffer content.
        /// </summary>
        public void Clear()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, msFBO);

            GL.ClearNamedFramebuffer(msFBO, ClearBuffer.Color, drawbuffer: 0, new[] { 0.5f, 0.8f, 0.9f, 1.0f });
            GL.ClearNamedFramebuffer(msFBO, ClearBuffer.Depth, drawbuffer: 0, new[] { 1f });
        }

        /// <summary>
        ///     Draw all rendered content to the screen buffer.
        /// </summary>
        public void Draw()
        {
            GL.BlitNamedFramebuffer(
                msFBO,
                drawFramebuffer: 0,
                srcX0: 0,
                srcY0: 0,
                Size.X,
                Size.Y,
                dstX0: 0,
                dstY0: 0,
                Size.X,
                Size.Y,
                ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit,
                BlitFramebufferFilter.Nearest);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer: 0);
        }

        private void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(x: 0, y: 0, Size.X, Size.Y);

            #region MULTISAMPLED FBO

            GL.BindTexture(TextureTarget.Texture2DMultisample, msTex);

            GL.TexImage2DMultisample(
                TextureTargetMultisample.Texture2DMultisample,
                samples,
                PixelInternalFormat.Rgba8,
                Size.X,
                Size.Y,
                fixedsamplelocations: true);

            GL.BindTexture(TextureTarget.Texture2DMultisample, texture: 0);

            GL.NamedRenderbufferStorageMultisample(msRBO, samples, RenderbufferStorage.Depth24Stencil8, Size.X, Size.Y);

            #endregion MULTISAMPLED FBO

            #region DEPTH TEXTURE

            GL.ActiveTexture(TextureUnit.Texture20);
            GL.BindTexture(TextureTarget.Texture2D, depthTex);

            GL.TexImage2D(
                TextureTarget.Texture2D,
                level: 0,
                PixelInternalFormat.DepthComponent,
                Size.X,
                Size.Y,
                border: 0,
                PixelFormat.DepthComponent,
                PixelType.Float,
                IntPtr.Zero);

            GL.ActiveTexture(TextureUnit.Texture0);

            #endregion DEPTH TEXTURE

            #region SCREENSHOT FBO

            GL.NamedRenderbufferStorage(screenshotRBO, RenderbufferStorage.Rgba8, Size.X, Size.Y);

            #endregion SCREENSHOT FBO

            Client.OnResize(Size);

            Application.Client.Instance.Resources.Shaders.UpdateOrthographicProjection();

            logger.LogDebug(Events.WindowState, "Window has been resized to: {Size}", e.Size);
        }

        private void EnableWireframe()
        {
            GL.LineWidth(width: 5f);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            isWireframeActive = true;
        }

        private void DisableWireframe()
        {
            GL.LineWidth(width: 1f);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            isWireframeActive = false;
        }

        #region PUBLIC STATIC PROPERTIES

        /// <summary>
        ///     Gets the window size. The value is equal to the value retrieved from the client.
        /// </summary>
        public static Vector2i Size => Instance.Client.Size;

        /// <summary>
        ///     Get the center of the screen.
        /// </summary>
        public static Vector2i Center => new(Size.X / 2, Size.Y / 2);

        /// <summary>
        ///     Gets the aspect ratio <c>x/y</c>.
        /// </summary>
        public static float AspectRatio => Size.X / (float) Size.Y;

        /// <summary>
        ///     Gets whether the screen is in fullscreen.
        /// </summary>
        public static bool IsFullscreen => Instance.Client.IsFullscreen;

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
        ///     Set the cursor options.
        /// </summary>
        /// <param name="visible">Whether the cursor should be visible.</param>
        /// <param name="locked">Whether the cursor should be locked to the center of the screen.</param>
        /// <param name="grabbed">Whether the cursor should be restricted to the game window.</param>
        public static void SetCursor(bool visible, bool locked = false, bool grabbed = false)
        {
            Instance.Client.CursorVisible = visible;
            Instance.Client.Mouse.Locked = locked;
            Instance.Client.CursorGrabbed = grabbed;
        }

        /// <summary>
        ///     Set if the screen should be in fullscreen.
        /// </summary>
        /// <param name="fullscreen">If fullscreen should be active.</param>
        public static void SetFullscreen(bool fullscreen)
        {
            if (fullscreen == Instance.Client.IsFullscreen) return;

            if (fullscreen)
            {
                Instance.previousScreenSize = Instance.Client.Size;
                Instance.previousScreenLocation = Instance.Client.Location;

                Vector2i monitorSize;

                unsafe
                {
                    Monitor* monitor = GLFW.GetPrimaryMonitor();
                    VideoMode* mode = GLFW.GetVideoMode(monitor);

                    monitorSize = new Vector2i(mode->Width, mode->Height);
                }

                Instance.Client.Size = monitorSize;

                Instance.Client.IsFullscreen = true;

                logger.LogDebug(Events.WindowState, "Fullscreen: Switched to fullscreen mode");

            }
            else
            {
                Instance.Client.IsFullscreen = false;

                Instance.Client.Size = Instance.previousScreenSize;
                Instance.Client.Location = Instance.previousScreenLocation;

                logger.LogDebug(Events.WindowState, "Fullscreen: Switched to normal mode");
            }
        }

        /// <summary>
        ///     Set the wire-frame mode. Wireframe is only active when in game draw mode.
        /// </summary>
        /// <param name="wireframe">True to activate wireframe, false to deactivate it.</param>
        public static void SetWireframe(bool wireframe)
        {
            if (Instance.isWireframeActive && !wireframe)
            {
                Instance.DisableWireframe();
            }

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
        public static void TakeScreenshot(string directory)
        {
            IntPtr data = Marshal.AllocHGlobal(Size.X * Size.Y * 4);

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

            string path = Path.Combine(directory, $"{DateTime.Now:yyyy-MM-dd__HH-mm-ss-fff}-screenshot.png");

            screenshot.Save(path);
            logger.LogInformation(Events.Screenshot, "Saved a screenshot to: {Path}", path);

            Marshal.FreeHGlobal(data);
        }

        /// <summary>
        ///     Fill the depth texture with the current depth data..
        /// </summary>
        public static void FillDepthTexture()
        {
            GL.ActiveTexture(TextureUnit.Texture20);
            GL.BindTexture(TextureTarget.Texture2D, Instance.depthTex);

            GL.ClearNamedFramebuffer(Instance.depthFBO, ClearBuffer.Depth, drawbuffer: 0, new[] { 1f });

            GL.BlitNamedFramebuffer(
                Instance.msFBO,
                Instance.depthFBO,
                srcX0: 0,
                srcY0: 0,
                Size.X,
                Size.Y,
                dstX0: 0,
                dstY0: 0,
                Size.X,
                Size.Y,
                ClearBufferMask.DepthBufferBit,
                BlitFramebufferFilter.Nearest);

            GL.ActiveTexture(TextureUnit.Texture0);
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
                GL.DeleteTexture(msTex);
                GL.DeleteFramebuffer(msFBO);
                GL.DeleteRenderbuffer(msRBO);

                GL.DeleteTexture(depthTex);
                GL.DeleteFramebuffer(depthFBO);

                GL.DeleteFramebuffer(screenshotFBO);
                GL.DeleteRenderbuffer(screenshotRBO);
            }

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
}