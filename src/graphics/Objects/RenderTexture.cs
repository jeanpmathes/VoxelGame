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
///     A texture that can be attached to a framebuffer.
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
    ///     Create a new render texture. It will be bound and attached to the framebuffer.
    /// </summary>
    public static RenderTexture Create(int fbo, Vector2i size, TextureUnit unit,
        (PixelFormat pixelFormat, PixelInternalFormat pixelInternalFormat, PixelType pixelType) format,
        FramebufferAttachment attachment)
    {
        int texture = GL.GenTexture();
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2D, texture);

        GL.TexImage2D(
            TextureTarget.Texture2D,
            level: 0,
            format.pixelInternalFormat,
            size.X,
            size.Y,
            border: 0,
            format.pixelFormat,
            format.pixelType,
            IntPtr.Zero);

        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int) TextureMinFilter.Linear);

        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int) TextureMagFilter.Linear);

        GL.NamedFramebufferTexture(fbo, attachment, texture, level: 0);

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
            TextureHandle = texture,
            FBOHandle = fbo,
            Unit = unit,
            PixelFormat = format.pixelFormat,
            PixelInternalFormat = format.pixelInternalFormat,
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
    /// Clear the render texture.
    /// </summary>
    public void Clear(ClearBuffer buffer, float[] value)
    {
        GL.ClearNamedFramebuffer(FBOHandle, buffer, drawbuffer: 0, value);
    }

    /// <summary>
    ///     Bind the render texture.
    /// </summary>
    public void Unbind()
    {
        GL.ActiveTexture(Unit);
        GL.BindTexture(TextureTarget.Texture2D, texture: 0);

        GL.ActiveTexture(TextureUnit.Texture0);
    }

    /// <summary>
    /// Unbind the render texture.
    /// </summary>
    public void Bind()
    {
        GL.ActiveTexture(Unit);
        GL.BindTexture(TextureTarget.Texture2D, TextureHandle);

        GL.ActiveTexture(TextureUnit.Texture0);
    }

    /// <summary>
    ///     Fill the texture with data from a framebuffer.
    /// </summary>
    public void Fill(int source, ClearBufferMask mask)
    {
        GL.ActiveTexture(Unit);
        GL.BindTexture(TextureTarget.Texture2D, TextureHandle);

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
