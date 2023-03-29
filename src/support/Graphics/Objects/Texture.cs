// <copyright file="Texture.cs" company="VoxelGame">
//     Code from https://github.com/opentk/LearnOpenTK
// </copyright>
// <author>jeanpmathes</author>

using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Support.Graphics.Objects;

/// <summary>
///     A texture.
/// </summary>
public sealed class Texture : IDisposable
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Texture>();

    /// <summary>
    ///     Creates a new texture from an image.
    ///     If the image cannot be loaded, a fallback texture is used.
    /// </summary>
    /// <param name="loadingContext">The loading context.</param>
    /// <param name="path">The path to an image.</param>
    /// <param name="fallbackResolution">The resolution to use for the fallback texture.</param>
    public Texture(LoadingContext loadingContext, FileInfo path, int fallbackResolution = 16)
    {
        // todo: port to DirectX

        /*TextureUnit = unit;

        GL.CreateTextures(TextureTarget.Texture2D, n: 1, out int handle);
        Handle = handle;

        Use(TextureUnit);

        try
        {
            using var bitmap = new Bitmap(path.Open(FileMode.Open));
            SetupTexture(bitmap);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            using (Bitmap bitmap = CreateFallback(fallbackResolution))
            {
                SetupTexture(bitmap);
            }

            loadingContext.ReportWarning(Events.MissingResource, nameof(Texture), path, exception);
        }

        GL.TextureParameter(Handle, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
        GL.TextureParameter(Handle, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);

        GL.TextureParameter(Handle, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
        GL.TextureParameter(Handle, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);

        GL.GenerateTextureMipmap(Handle);*/

        loadingContext.ReportSuccess(Events.ResourceLoad, nameof(Texture), path);
    }

    private int Handle { get; }

    // /// <summary>
    // ///     Get the texture unit this texture is bound to.
    // /// </summary>
    // public TextureUnit TextureUnit { get; private set; }

    private void SetupTexture(Bitmap bitmap)
    {
        bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);

        BitmapData data = bitmap.LockBits(
            new Rectangle(x: 0, y: 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);

        // todo: maybe upload to C++ here

        /*GL.TextureStorage2D(Handle, levels: 1, SizedInternalFormat.Rgba8, bitmap.Width, bitmap.Height);

        GL.TextureSubImage2D(
            Handle,
            level: 0,
            xoffset: 0,
            yoffset: 0,
            bitmap.Width,
            bitmap.Height,
            OpenTK.Graphics.OpenGL4.PixelFormat.Bgra,
            PixelType.UnsignedByte,
            data.Scan0);*/

        bitmap.UnlockBits(data);
    }

    /// <summary>
    ///     Creates a fallback image.
    /// </summary>
    /// <param name="resolution">The resolution of the image to create.</param>
    /// <returns>The created fallback image.</returns>
    public static Bitmap CreateFallback(int resolution)
    {
        var fallback = new Bitmap(resolution, resolution, PixelFormat.Format32bppArgb);

        Color magenta = Color.FromArgb(alpha: 64, red: 255, green: 0, blue: 255);
        Color black = Color.FromArgb(alpha: 64, red: 0, green: 0, blue: 0);

        for (var x = 0; x < fallback.Width; x++)
        for (var y = 0; y < fallback.Height; y++)
            fallback.SetPixel(x, y, (x % 2 == 0) ^ (y % 2 == 0) ? magenta : black);

        return fallback;
    }

    #region IDisposable Support

    private bool disposed;

    private void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing) ; //GL.DeleteTexture(Handle); todo: freeing still important
            else
                logger.LogWarning(
                    Events.UndeletedTexture,
                    "Texture disposed by GC without freeing storage");

            disposed = true;
        }
    }

    /// <summary>
    ///     Texture finalizer.
    /// </summary>
    ~Texture()
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

