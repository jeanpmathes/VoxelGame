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
using VoxelGame.Utilities;

namespace VoxelGame.Rendering
{
    /// <summary>
    /// Common functionality associated with the screen.
    /// </summary>
    public class Screen
    {
        private static readonly ILogger logger = Program.CreateLogger<Screen>();

        #region PUBLIC STATIC PROPERTIES

        /// <summary>
        /// Gets the window size. The value is equal to the value retrieved from <see cref="Game.Instance"/>.
        /// </summary>
        public static Vector2i Size { get => Game.Instance.Size; set { Game.Instance.Size = value; } }

        /// <summary>
        /// Gets the aspect ratio <c>x/y</c>.
        /// </summary>
        public static float AspectRatio { get => Size.X / (float)Size.Y; }

        #endregion PUBLIC STATIC PROPERTIES

        private static Screen Instance { get; set; } = null!;

        private readonly int samples;

        private readonly int multisampledTexture;
        private readonly int multisampledFrameBufferObject;
        private readonly int multisampledRenderBufferObject;

        private readonly int screenshotFrameBufferObject;
        private readonly int screenshotRenderBufferObject;

        public Screen()
        {
            Instance = this;

            Game.Instance.Resize += OnResize;

            #region MULTISAMPLED FBO

            int maxSamples = GL.GetInteger(GetPName.MaxSamples);
            samples = Config.GetInt("sampleCount", min: 1, max: maxSamples);
            logger.LogDebug("Set sample count to {samples}, of maximum {max} possible samples.", samples, maxSamples);

            GL.ClearColor(0.5f, 0.8f, 0.9f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Multisample);

            multisampledTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DMultisample, multisampledTexture);
            GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, samples, PixelInternalFormat.Rgba8, Game.Instance.Size.X, Size.Y, true);
            GL.BindTexture(TextureTarget.Texture2DMultisample, 0);

            multisampledFrameBufferObject = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, multisampledFrameBufferObject);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2DMultisample, multisampledTexture, 0);

            FramebufferErrorCode multisampledFboStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

            while (multisampledFboStatus != FramebufferErrorCode.FramebufferComplete)
            {
                logger.LogWarning("Multi-sampled FBO not complete [{status}], waiting...", multisampledFboStatus);
                Thread.Sleep(100);

                multisampledFboStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            }

            multisampledRenderBufferObject = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, multisampledRenderBufferObject);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, samples, RenderbufferStorage.Depth24Stencil8, Size.X, Size.Y);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, multisampledRenderBufferObject);

            #endregion MULTISAMPLED FBO

            #region SCREENSHOT FBO

            screenshotFrameBufferObject = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, screenshotFrameBufferObject);

            screenshotRenderBufferObject = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, screenshotRenderBufferObject);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, Size.X, Size.Y);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, screenshotRenderBufferObject);

            FramebufferErrorCode screenshotFboStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

            while (screenshotFboStatus != FramebufferErrorCode.FramebufferComplete)
            {
                logger.LogWarning("Screenshot FBO not complete [{status}], waiting...", screenshotFboStatus);
                Thread.Sleep(100);

                screenshotFboStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            }

            #endregion SCREENSHOT FBO
        }

        public void Clear()
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, multisampledFrameBufferObject);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void Draw()
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, multisampledFrameBufferObject);
            GL.DrawBuffer(DrawBufferMode.Back);
            GL.BlitFramebuffer(0, 0, Size.X, Size.Y, 0, 0, Size.X, Size.Y, ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
        }

        private void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);

            #region MULTISAMPLED FBO

            GL.BindTexture(TextureTarget.Texture2DMultisample, multisampledTexture);
            GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, samples, PixelInternalFormat.Rgba8, Size.X, Size.Y, true);
            GL.BindTexture(TextureTarget.Texture2DMultisample, 0);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, multisampledRenderBufferObject);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, samples, RenderbufferStorage.Depth24Stencil8, Size.X, Size.Y);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            #endregion MULTISAMPLED FBO

            #region SCREENSHOT FBO

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, screenshotRenderBufferObject);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, Size.X, Size.Y);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            #endregion SCREENSHOT FBO

            Game.ScreenElementShader.SetMatrix4("projection", Matrix4.CreateOrthographic(Size.X, Size.Y, 0f, 1f));

            logger.LogDebug("Window has been resized to: {size}", e.Size);
        }

        #region PUBLIC STATIC METHODS

        private static Vector2i previousScreenSize;
        private static Vector2i previousScreenLocation;

        /// <summary>
        /// Set if the screen should be in fullscreen.
        /// </summary>
        /// <param name="fullscreen">If fullscreen should be active.</param>
        public static void SetFullscreen(bool fullscreen)
        {
            if (fullscreen == Game.Instance.IsFullscreen) return;

            if (fullscreen)
            {
                previousScreenSize = Game.Instance.Size;
                previousScreenLocation = Game.Instance.Location;

                Game.Instance.WindowState = WindowState.Fullscreen;
                Game.Instance.IsFullscreen = true;
                logger.LogDebug("Fullscreen: Switched to fullscreen mode.");
            }
            else
            {
                unsafe { GLFW.SetWindowMonitor(Game.Instance.WindowPointer, null, previousScreenLocation.X, previousScreenLocation.Y, previousScreenSize.X, previousScreenSize.Y, (int)Game.Instance.RenderFrequency); }
                Game.Instance.IsFullscreen = false;

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

            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, Instance.multisampledFrameBufferObject);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, Instance.screenshotFrameBufferObject);
            GL.BlitFramebuffer(0, 0, Size.X, Size.Y, 0, 0, Size.X, Size.Y, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Instance.screenshotFrameBufferObject);
            GL.ReadPixels(0, 0, Size.X, Size.Y, PixelFormat.Bgra, PixelType.UnsignedByte, data);

            using Bitmap screenshot = new Bitmap(Size.X, Size.Y, 4 * Size.X, System.Drawing.Imaging.PixelFormat.Format32bppArgb, data);
            screenshot.RotateFlip(RotateFlipType.RotateNoneFlipY);

            string path = Path.Combine(directory, $"{DateTime.Now:yyyy-MM-dd__HH-mm-ss-fff}-screenshot.png");

            screenshot.Save(path);
            logger.LogInformation("Saved a screenshot to: {path}", path);

            Marshal.FreeHGlobal(data);
        }

        #endregion PUBLIC STATIC METHODS
    }
}