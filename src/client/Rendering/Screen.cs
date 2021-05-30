// <copyright file="Screen.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using Microsoft.Extensions.Logging;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.GraphicsLibraryFramework;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using OpenToolkit.Graphics.OpenGL4;
using VoxelGame.Core;
using VoxelGame.Core.Utilities;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace VoxelGame.Client.Rendering
{
    /// <summary>
    /// Common functionality associated with the screen.
    /// </summary>
    public class Screen : IDisposable
    {
        private static readonly ILogger Logger = LoggingHelper.CreateLogger<Screen>();

        private readonly int samples;

        private readonly int msTex;
        private readonly int msFBO;
        private readonly int msRBO;

        private readonly int depthTex;
        private readonly int depthFBO;

        private readonly int screenshotFBO;
        private readonly int screenshotRBO;

        #region PUBLIC STATIC PROPERTIES

        /// <summary>
        /// Gets the window size. The value is equal to the value retrieved from <see cref="Client.Instance"/>.
        /// </summary>
        public static Vector2i Size
        {
            get => Instance.Client.Size; set => Instance.Client.Size = value;
        }

        /// <summary>
        /// Gets the aspect ratio <c>x/y</c>.
        /// </summary>
        public static float AspectRatio => Size.X / (float)Size.Y;

        /// <summary>
        /// Gets whether the screen is in fullscreen.
        /// </summary>
        public static bool IsFullscreen => Instance.Client.IsFullscreen;

        /// <summary>
        /// Gets whether the screen is focused.
        /// </summary>
        public static bool IsFocused => Instance.Client.IsFocused;

        #endregion PUBLIC STATIC PROPERTIES

        private protected static Screen Instance { get; set; } = null!;

        private Client Client { get; set; }

        internal Screen(Client client)
        {
            Instance = this;

            Client = client;

            client.Resize += OnResize;

            #region MULTISAMPLED FBO

            int maxSamples = GL.GetInteger(GetPName.MaxSamples);
            samples = Properties.client.Default.SampleCount;
            Logger.LogDebug("Set sample count to {samples}, of maximum {max} possible samples.", samples, maxSamples);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Multisample);

            msTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DMultisample, msTex);
            GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, samples, PixelInternalFormat.Rgba8, Size.X, Size.Y, true);
            GL.BindTexture(TextureTarget.Texture2DMultisample, 0);

            GL.CreateFramebuffers(1, out msFBO);
            GL.NamedFramebufferTexture(msFBO, FramebufferAttachment.ColorAttachment0, msTex, 0);

            FramebufferStatus multisampledFboStatus = GL.CheckNamedFramebufferStatus(msFBO, FramebufferTarget.Framebuffer);

            while (multisampledFboStatus != FramebufferStatus.FramebufferComplete)
            {
                Logger.LogWarning("Multi-sampled FBO not complete [{status}], waiting...", multisampledFboStatus);
                Thread.Sleep(100);

                multisampledFboStatus = GL.CheckNamedFramebufferStatus(msFBO, FramebufferTarget.Framebuffer);
            }

            GL.CreateRenderbuffers(1, out msRBO);
            GL.NamedRenderbufferStorageMultisample(msRBO, samples, RenderbufferStorage.Depth24Stencil8, Size.X, Size.Y);
            GL.NamedFramebufferRenderbuffer(msFBO, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, msRBO);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, msFBO);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            #endregion MULTISAMPLED FBO

            #region DEPTH TEXTURE

            depthTex = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture20);
            GL.BindTexture(TextureTarget.Texture2D, depthTex);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, Size.X, Size.Y, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.CreateFramebuffers(1, out depthFBO);
            GL.NamedFramebufferTexture(depthFBO, FramebufferAttachment.DepthAttachment, depthTex, 0);

            FramebufferStatus depthFboStatus = GL.CheckNamedFramebufferStatus(depthFBO, FramebufferTarget.Framebuffer);

            while (depthFboStatus != FramebufferStatus.FramebufferComplete)
            {
                Logger.LogWarning("Depth FBO not complete [{status}], waiting...", depthFboStatus);
                Thread.Sleep(100);

                depthFboStatus = GL.CheckNamedFramebufferStatus(depthFBO, FramebufferTarget.Framebuffer);
            }

            GL.ActiveTexture(TextureUnit.Texture0);

            #endregion DEPTH TEXTURE

            #region SCREENSHOT FBO

            GL.CreateFramebuffers(1, out screenshotFBO);

            GL.CreateRenderbuffers(1, out screenshotRBO);
            GL.NamedRenderbufferStorage(screenshotRBO, RenderbufferStorage.Rgba8, Size.X, Size.Y);
            GL.NamedFramebufferRenderbuffer(screenshotFBO, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, screenshotRBO);

            FramebufferStatus screenshotFboStatus = GL.CheckNamedFramebufferStatus(screenshotFBO, FramebufferTarget.Framebuffer);

            while (screenshotFboStatus != FramebufferStatus.FramebufferComplete)
            {
                Logger.LogWarning("Screenshot FBO not complete [{status}], waiting...", screenshotFboStatus);
                Thread.Sleep(100);

                screenshotFboStatus = GL.CheckNamedFramebufferStatus(screenshotFBO, FramebufferTarget.Framebuffer);
            }

            #endregion SCREENSHOT FBO
        }

        public void Clear()
        {
            GL.ClearNamedFramebuffer(msFBO, ClearBuffer.Color, 0, new float[] { 0.5f, 0.8f, 0.9f, 1.0f });
            GL.ClearNamedFramebuffer(msFBO, ClearBuffer.Depth, 0, new float[] { 1f });
        }

        public void Draw()
        {
            GL.BlitNamedFramebuffer(msFBO, 0, 0, 0, Size.X, Size.Y, 0, 0, Size.X, Size.Y,
                ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
        }

        private void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);

            #region MULTISAMPLED FBO

            GL.BindTexture(TextureTarget.Texture2DMultisample, msTex);
            GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, samples, PixelInternalFormat.Rgba8, Size.X, Size.Y, true);
            GL.BindTexture(TextureTarget.Texture2DMultisample, 0);

            GL.NamedRenderbufferStorageMultisample(msRBO, samples, RenderbufferStorage.Depth24Stencil8, Size.X, Size.Y);

            #endregion MULTISAMPLED FBO

            #region DEPTH TEXTURE

            GL.ActiveTexture(TextureUnit.Texture20);
            GL.BindTexture(TextureTarget.Texture2D, depthTex);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, Size.X, Size.Y, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

            GL.ActiveTexture(TextureUnit.Texture0);

            #endregion DEPTH TEXTURE

            #region SCREENSHOT FBO

            GL.NamedRenderbufferStorage(screenshotRBO, RenderbufferStorage.Rgba8, Size.X, Size.Y);

            #endregion SCREENSHOT FBO

            Client.OnResize(Size);

            Client.OverlayShader.SetMatrix4("projection", Matrix4.CreateOrthographic(1f, 1f / Screen.AspectRatio, 0f, 1f));
            Client.ScreenElementShader.SetMatrix4("projection", Matrix4.CreateOrthographic(Size.X, Size.Y, 0f, 1f));

            Logger.LogDebug("Window has been resized to: {size}", e.Size);
        }

        #region PUBLIC STATIC METHODS

        public static void SetCursor(bool visible, bool tracked = false, bool grabbed = false)
        {
            Instance.Client.CursorVisible = visible;
            Instance.Client.DoMouseTracking = tracked;
            Instance.Client.CursorGrabbed = grabbed;
        }

        private static Vector2i previousScreenSize;
        private static Vector2i previousScreenLocation;

        /// <summary>
        /// Set if the screen should be in fullscreen.
        /// </summary>
        /// <param name="fullscreen">If fullscreen should be active.</param>
        public static void SetFullscreen(bool fullscreen)
        {
            if (fullscreen == Instance.Client.IsFullscreen) return;

            if (fullscreen)
            {
                previousScreenSize = Instance.Client.Size;
                previousScreenLocation = Instance.Client.Location;

                Instance.Client.WindowState = WindowState.Fullscreen;
                Instance.Client.IsFullscreen = true;
                Logger.LogDebug("Fullscreen: Switched to fullscreen mode.");
            }
            else
            {
                unsafe { GLFW.SetWindowMonitor(Instance.Client.WindowPointer, null, previousScreenLocation.X, previousScreenLocation.Y, previousScreenSize.X, previousScreenSize.Y, (int)Instance.Client.RenderFrequency); }
                Instance.Client.IsFullscreen = false;

                Logger.LogDebug("Fullscreen: Switched to normal mode.");
            }
        }

        /// <summary>
        /// Set the wire-frame mode.
        /// </summary>
        /// <param name="wireframe">True to activate wireframe, false to deactivate it.</param>
        public static void SetWireFrame(bool wireframe)
        {
            if (wireframe)
            {
                GL.LineWidth(5f);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            }
            else
            {
                GL.LineWidth(1f);
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }
        }

        /// <summary>
        /// Takes a screenshot and saves it to the specified directory.
        /// </summary>
        /// <param name="directory">The directory in which the screenshot should be saved.</param>
        public static void TakeScreenshot(string directory)
        {
            IntPtr data = Marshal.AllocHGlobal(Size.X * Size.Y * 4);

            GL.BlitNamedFramebuffer(Instance.msFBO, Instance.screenshotFBO, 0, 0, Size.X, Size.Y, 0, 0, Size.X, Size.Y, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);

            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, Instance.screenshotFBO);
            GL.ReadPixels(0, 0, Size.X, Size.Y, PixelFormat.Bgra, PixelType.UnsignedByte, data);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Instance.msFBO);

            using Bitmap screenshot = new Bitmap(Size.X, Size.Y, 4 * Size.X, System.Drawing.Imaging.PixelFormat.Format32bppArgb, data);
            screenshot.RotateFlip(RotateFlipType.RotateNoneFlipY);

            string path = Path.Combine(directory, $"{DateTime.Now:yyyy-MM-dd__HH-mm-ss-fff}-screenshot.png");

            screenshot.Save(path);
            Logger.LogInformation("Saved a screenshot to: {path}", path);

            Marshal.FreeHGlobal(data);
        }

        public static void FillDepthTexture()
        {
            GL.ActiveTexture(TextureUnit.Texture20);
            GL.BindTexture(TextureTarget.Texture2D, Instance.depthTex);

            GL.ClearNamedFramebuffer(Instance.depthFBO, ClearBuffer.Depth, 0, new float[] { 1f });
            GL.BlitNamedFramebuffer(Instance.msFBO, Instance.depthFBO, 0, 0, Size.X, Size.Y, 0, 0, Size.X, Size.Y, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);

            GL.ActiveTexture(TextureUnit.Texture0);
        }

        #endregion PUBLIC STATIC METHODS

        #region IDisposable Support

        private bool disposed;

        protected virtual void Dispose(bool disposing)
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

            Logger.LogWarning(LoggingEvents.UndeletedGlObjects, "A screen object has been destroyed without disposing it.");

            disposed = true;
        }

        ~Screen()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}