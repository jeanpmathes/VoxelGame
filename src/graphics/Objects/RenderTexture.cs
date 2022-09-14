// <copyright file="Texture.cs" company="VoxelGame">
//     Code from https://github.com/opentk/LearnOpenTK
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using VoxelGame.Logging;

namespace VoxelGame.Graphics.Objects;

/// <summary>
///     A texture that is used to hold rendered data. It is combined with an framebuffer object.
/// </summary>
public sealed class RenderTexture : IDisposable
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<RenderTexture>();

    private int TextureHandle { get; init; }

    private int FBOHandle { get; init; }

    private TextureUnit Unit { get; init; }

    private PixelFormat PixelFormat { get; init; }

    private PixelInternalFormat PixelInternalFormat { get; init; }

    private Vector2i Size { get; set; }

    /// <summary>
    ///     Create a new render texture.
    /// </summary>
    public static RenderTexture Create(Vector2i size, TextureUnit unit, PixelFormat pixelFormat, PixelInternalFormat pixelInternalFormat, FramebufferAttachment attachment)
    {
        int tex = GL.GenTexture();
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2D, tex);

        GL.TexImage2D(
            TextureTarget.Texture2D,
            level: 0,
            pixelInternalFormat,
            size.X,
            size.Y,
            border: 0,
            pixelFormat,
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

        GL.CreateFramebuffers(n: 1, out int fbo);
        GL.NamedFramebufferTexture(fbo, attachment, tex, level: 0);

        FramebufferStatus fboStatus = GL.CheckNamedFramebufferStatus(fbo, FramebufferTarget.Framebuffer);

        while (fboStatus != FramebufferStatus.FramebufferComplete)
        {
            logger.LogWarning(Events.VisualsSetup, "FBO not complete [{Status}], waiting...", fboStatus);
            Thread.Sleep(millisecondsTimeout: 100);

            fboStatus = GL.CheckNamedFramebufferStatus(fbo, FramebufferTarget.Framebuffer);
        }

        GL.ActiveTexture(TextureUnit.Texture0);

        return new RenderTexture
        {
            TextureHandle = tex,
            FBOHandle = fbo,
            Unit = unit,
            PixelFormat = pixelFormat,
            PixelInternalFormat = pixelInternalFormat,
            Size = size
        };
    }

    /// <summary>
    ///     Resize the render texture.
    /// </summary>
    /// <param name="size">The new size of the render texture.</param>
    public void Resize(Vector2i size)
    {
        GL.ActiveTexture(Unit);
        GL.BindTexture(TextureTarget.Texture2D, TextureHandle);

        GL.TexImage2D(
            TextureTarget.Texture2D,
            level: 0,
            PixelInternalFormat,
            size.X,
            size.Y,
            border: 0,
            PixelFormat,
            PixelType.Float,
            IntPtr.Zero);

        Size = size;

        GL.ActiveTexture(TextureUnit.Texture0);
    }

    /// <summary>
    ///     Fill the texture with data from a framebuffer. Before filling, the texture is cleared.
    /// </summary>
    /// <param name="source">The source framebuffer.</param>
    /// <param name="buffer">The buffer to clear.</param>
    /// <param name="clear">The clear value.</param>
    /// <param name="mask">The blit mask.</param>
    public void Fill(int source, ClearBuffer buffer, float[] clear, ClearBufferMask mask)
    {
        GL.ActiveTexture(Unit);
        GL.BindTexture(TextureTarget.Texture2D, TextureHandle);

        GL.ClearNamedFramebuffer(FBOHandle, buffer, drawbuffer: 0, clear);

        GL.BlitNamedFramebuffer(
            source,
            FBOHandle,
            srcX0: 0,
            srcY0: 0,
            Size.X,
            Size.Y,
            dstX0: 0,
            dstY0: 0,
            Size.X,
            Size.Y,
            mask,
            BlitFramebufferFilter.Nearest);

        GL.ActiveTexture(TextureUnit.Texture0);
    }

    #region IDisposable Support

    private bool disposed;

    private void Dispose(bool disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            GL.DeleteTexture(TextureHandle);
            GL.DeleteFramebuffer(FBOHandle);
        }
        else
        {
            logger.LogWarning(
                Events.UndeletedTexture,
                "RenderTexture disposed by GC without freeing storage");
        }

        disposed = true;
    }

    /// <summary>
    ///     RenderTexture finalizer.
    /// </summary>
    ~RenderTexture()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    ///     Dispose of the texture.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable Support
}
