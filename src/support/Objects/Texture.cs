// <copyright file="Texture.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Support.Core;

namespace VoxelGame.Support.Objects;

/// <summary>
///     A texture.
/// </summary>
public class Texture : NativeObject
{
    private readonly Vector2i size;

    /// <summary>
    ///     Create a new texture from a native pointer.
    /// </summary>
    internal Texture(IntPtr nativePointer, Client client, Vector2i size) : base(nativePointer, client)
    {
        this.size = size;
    }

    /// <summary>
    ///     Gets the width of the texture.
    /// </summary>
    public int Width => size.X;

    /// <summary>
    ///     Gets the height of the texture.
    /// </summary>
    public int Height => size.Y;

    /// <summary>
    ///     Load a texture from a file. This is only allowed during the loading phase.
    /// </summary>
    /// <param name="client">The client instance, used to determine texture lifetime and to access the graphics API.</param>
    /// <param name="path">The path to the texture file.</param>
    /// <param name="loadingContext">The loading context.</param>
    /// <param name="fallbackResolution">The resolution to use for the fallback texture.</param>
    /// <returns></returns>
    public static Texture Load(Client client, FileInfo path, LoadingContext? loadingContext, int fallbackResolution = 16)
    {
        Bitmap bitmap;

        try
        {
            bitmap = new Bitmap(path.Open(FileMode.Open));
            loadingContext?.ReportSuccess(Events.ResourceLoad, nameof(Texture), path);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            bitmap = CreateFallback(fallbackResolution);
            loadingContext?.ReportWarning(Events.MissingResource, nameof(Texture), path, exception);
        }

        bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
        Texture texture = client.LoadTexture(bitmap);
        bitmap.Dispose();

        return texture;
    }

    /// <summary>
    ///     Creates a fallback image.
    /// </summary>
    /// <param name="resolution">The size of the image to create.</param>
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

    /// <summary>
    ///     Frees the texture. Not allowed in same frame as creation.
    /// </summary>
    public void Free()
    {
        Deregister();
        Native.FreeTexture(this);
    }
}
