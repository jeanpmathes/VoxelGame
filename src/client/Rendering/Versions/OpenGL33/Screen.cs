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

namespace VoxelGame.Client.Rendering.Versions.OpenGL33
{
    /// <summary>
    /// Common functionality associated with the screen.
    /// </summary>
    public class Screen : Rendering.Screen
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<Screen>();

        private protected override Client Client { get; set; }

        private readonly int samples;

        private readonly int msTex;
        private readonly int msFBO;
        private readonly int msRBO;

        private readonly int screenshotFBO;
        private readonly int screenshotRBO;

        internal Screen(Client client)
        {
            Instance = this;

            Client = client;

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

            msFBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, msFBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2DMultisample, msTex, 0);

            FramebufferErrorCode multisampledFboStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

            while (multisampledFboStatus != FramebufferErrorCode.FramebufferComplete)
            {
                logger.LogWarning("Multi-sampled FBO not complete [{status}], waiting...", multisampledFboStatus);
                Thread.Sleep(100);

                multisampledFboStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            }

            msRBO = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, msRBO);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, samples, RenderbufferStorage.Depth24Stencil8, Size.X, Size.Y);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, msRBO);

            #endregion MULTISAMPLED FBO

            #region SCREENSHOT FBO

            screenshotFBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, screenshotFBO);

            screenshotRBO = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, screenshotRBO);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, Size.X, Size.Y);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, screenshotRBO);

            FramebufferErrorCode screenshotFboStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

            while (screenshotFboStatus != FramebufferErrorCode.FramebufferComplete)
            {
                logger.LogWarning("Screenshot FBO not complete [{status}], waiting...", screenshotFboStatus);
                Thread.Sleep(100);

                screenshotFboStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            }

            #endregion SCREENSHOT FBO
        }

        public override void Clear()
        {
            GL.ClearColor(0.5f, 0.8f, 0.9f, 1.0f);

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, msFBO);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public override void Draw()
        {
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, msFBO);
            GL.DrawBuffer(DrawBufferMode.Back);
            GL.BlitFramebuffer(0, 0, Size.X, Size.Y, 0, 0, Size.X, Size.Y, ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
        }

        private void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, Size.X, Size.Y);

            #region MULTISAMPLED FBO

            GL.BindTexture(TextureTarget.Texture2DMultisample, msTex);
            GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, samples, PixelInternalFormat.Rgba8, Size.X, Size.Y, true);
            GL.BindTexture(TextureTarget.Texture2DMultisample, 0);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, msRBO);
            GL.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, samples, RenderbufferStorage.Depth24Stencil8, Size.X, Size.Y);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            #endregion MULTISAMPLED FBO

            #region SCREENSHOT FBO

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, screenshotRBO);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba8, Size.X, Size.Y);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

            #endregion SCREENSHOT FBO

            Client.Scene.OnResize(Size);

            Client.ScreenElementShader.SetMatrix4("projection", Matrix4.CreateOrthographic(Size.X, Size.Y, 0f, 1f));

            logger.LogDebug("Window has been resized to: {size}", e.Size);
        }

        #region PUBLIC STATIC METHODS

        private protected override void SetCursor_Implementation(bool visible, bool tracked, bool grabbed)
        {
            Client.CursorVisible = visible;
            Client.DoMouseTracking = tracked;
            Client.CursorGrabbed = grabbed;
        }

        private Vector2i previousScreenSize;
        private Vector2i previousScreenLocation;

        private protected override void SetFullscreen_Implementation(bool fullscreen)
        {
            if (fullscreen == Client.IsFullscreen) return;

            if (fullscreen)
            {
                previousScreenSize = Client.Size;
                previousScreenLocation = Client.Location;

                Client.WindowState = WindowState.Fullscreen;
                Client.IsFullscreen = true;
                logger.LogDebug("Fullscreen: Switched to fullscreen mode.");
            }
            else
            {
                unsafe { GLFW.SetWindowMonitor(Client.WindowPointer, null, previousScreenLocation.X, previousScreenLocation.Y, previousScreenSize.X, previousScreenSize.Y, (int)Client.RenderFrequency); }
                Client.IsFullscreen = false;

                logger.LogDebug("Fullscreen: Switched to normal mode.");
            }
        }

        private protected override void TakeScreenshot_Implementation(string directory)
        {
            IntPtr data = Marshal.AllocHGlobal(Size.X * Size.Y * 4);

            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, msFBO);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, screenshotFBO);
            GL.BlitFramebuffer(0, 0, Size.X, Size.Y, 0, 0, Size.X, Size.Y, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);

            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, screenshotFBO);
            GL.ReadPixels(0, 0, Size.X, Size.Y, PixelFormat.Bgra, PixelType.UnsignedByte, data);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, msFBO);

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

        protected override void Dispose(bool disposing)
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

        #endregion IDisposable Support
    }
}