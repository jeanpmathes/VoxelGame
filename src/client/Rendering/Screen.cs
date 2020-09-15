// <copyright file="Screen.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Windowing.GraphicsLibraryFramework;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using VoxelGame.Core;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Rendering
{
    /// <summary>
    /// Common functionality associated with the screen.
    /// </summary>
    public class Screen : IDisposable
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<Screen>();

        #region PUBLIC STATIC PROPERTIES

        /// <summary>
        /// Gets the window size. The value is equal to the value retrieved from <see cref="Client.Instance"/>.
        /// </summary>
        public static Vector2i Size { get => Instance.client.Size; set { Instance.client.Size = value; } }

        /// <summary>
        /// Gets the aspect ratio <c>x/y</c>.
        /// </summary>
        public static float AspectRatio { get => Size.X / (float)Size.Y; }

        /// <summary>
        /// Gets whether the screen is in fullscreen.
        /// </summary>
        public static bool IsFullscreen { get => Instance.client.IsFullscreen; }

        /// <summary>
        /// Gets whether the screen is focused.
        /// </summary>
        public static bool IsFocused { get => Instance.client.IsFocused; }

        #endregion PUBLIC STATIC PROPERTIES

        private static Screen Instance { get; set; } = null!;

        private readonly Client client;

        private readonly int samples;

        private readonly int msTex;
        private readonly int msFBO;
        private readonly int msRBO;

        private readonly int screenshotFBO;
        private readonly int screenshotRBO;

        internal Screen(Client client)
        {
            Instance = this;

            this.client = client;

            client.Resize += OnResize;

            #region MULTISAMPLED FBO

            int maxSamples = GL.GetInteger(GetPName.MaxSamples);
            samples = Properties.client.Default.SampleCount;
            logger.LogDebug("Set sample count to {samples}, of maximum {max} possible samples.", samples, maxSamples);

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
                logger.LogWarning("Multi-sampled FBO not complete [{status}], waiting...", multisampledFboStatus);
                Thread.Sleep(100);

                multisampledFboStatus = GL.CheckNamedFramebufferStatus(msFBO, FramebufferTarget.Framebuffer);
            }

            GL.CreateRenderbuffers(1, out msRBO);
            GL.NamedRenderbufferStorageMultisample(msRBO, samples, RenderbufferStorage.Depth24Stencil8, Size.X, Size.Y);
            GL.NamedFramebufferRenderbuffer(msFBO, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, msRBO);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, msFBO);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            #endregion MULTISAMPLED FBO

            #region SCREENSHOT FBO

            GL.CreateFramebuffers(1, out screenshotFBO);

            GL.CreateRenderbuffers(1, out screenshotRBO);
            GL.NamedRenderbufferStorage(screenshotRBO, RenderbufferStorage.Rgba8, Size.X, Size.Y);
            GL.NamedFramebufferRenderbuffer(screenshotFBO, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, screenshotRBO);

            FramebufferStatus screenshotFboStatus = GL.CheckNamedFramebufferStatus(screenshotFBO, FramebufferTarget.Framebuffer);

            while (screenshotFboStatus != FramebufferStatus.FramebufferComplete)
            {
                logger.LogWarning("Screenshot FBO not complete [{status}], waiting...", screenshotFboStatus);
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
            GL.BlitNamedFramebuffer(msFBO, 0, 0, 0, Size.X, Size.Y, 0, 0, Size.X, Size.Y, ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
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

            #region SCREENSHOT FBO

            GL.NamedRenderbufferStorage(screenshotRBO, RenderbufferStorage.Rgba8, Size.X, Size.Y);

            #endregion SCREENSHOT FBO

            client.Scene.OnResize(Size);

            Client.ScreenElementShader.SetMatrix4("projection", Matrix4.CreateOrthographic(Size.X, Size.Y, 0f, 1f));

            logger.LogDebug("Window has been resized to: {size}", e.Size);
        }

        #region PUBLIC STATIC METHODS

        public static void SetCursor(bool visible, bool tracked = false, bool grabbed = false)
        {
            Instance.client.CursorVisible = visible;
            Instance.client.DoMouseTracking = tracked;
            Instance.client.CursorGrabbed = grabbed;
        }

        private static Vector2i previousScreenSize;
        private static Vector2i previousScreenLocation;

        /// <summary>
        /// Set if the screen should be in fullscreen.
        /// </summary>
        /// <param name="fullscreen">If fullscreen should be active.</param>
        public static void SetFullscreen(bool fullscreen)
        {
            if (fullscreen == Instance.client.IsFullscreen) return;

            if (fullscreen)
            {
                previousScreenSize = Instance.client.Size;
                previousScreenLocation = Instance.client.Location;

                Instance.client.WindowState = WindowState.Fullscreen;
                Instance.client.IsFullscreen = true;
                logger.LogDebug("Fullscreen: Switched to fullscreen mode.");
            }
            else
            {
                unsafe { GLFW.SetWindowMonitor(Instance.client.WindowPointer, null, previousScreenLocation.X, previousScreenLocation.Y, previousScreenSize.X, previousScreenSize.Y, (int)Instance.client.RenderFrequency); }
                Instance.client.IsFullscreen = false;

                logger.LogDebug("Fullscreen: Switched to normal mode.");
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
            logger.LogInformation("Saved a screenshot to: {path}", path);

            Marshal.FreeHGlobal(data);
        }

        #endregion PUBLIC STATIC METHODS

        #region IDisposable Support

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    GL.DeleteTexture(msTex);
                    GL.DeleteFramebuffer(msFBO);
                    GL.DeleteRenderbuffer(msRBO);

                    GL.DeleteFramebuffer(screenshotFBO);
                    GL.DeleteRenderbuffer(screenshotRBO);
                }

                logger.LogWarning(LoggingEvents.UndeletedGlObjects, "A screen object has been destroyed without disposing it.");

                disposed = true;
            }
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