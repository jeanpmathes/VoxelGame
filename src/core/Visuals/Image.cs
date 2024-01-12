// <copyright file="Image.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     A 2D image, stored in memory for fast modification.
///     Each image has a format, which defines how the data is stored.
/// </summary>
public class Image
{
    private const int BitsPerByte = 8;
    private const int ChannelMask = (1 << BitsPerByte) - 1;

    private const int C0 = 0 * BitsPerByte;
    private const int C1 = 1 * BitsPerByte;
    private const int C2 = 2 * BitsPerByte;
    private const int C3 = 3 * BitsPerByte;

    private readonly int[] data;
    private readonly Format format;

    private Image(int[] data, Format format, int width, int height)
    {
        Debug.Assert(data.Length == width * height);

        this.data = data;
        this.format = format;

        Width = width;
        Height = height;
    }

    /// <summary>
    ///     Creates an image from given data.
    /// </summary>
    /// <param name="data">The data, must have a length of <paramref name="width" /> * <paramref name="height" />.</param>
    /// <param name="format">The format of the data.</param>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    public Image(Span<int> data, Format format, int width, int height) : this(data.ToArray(), format, width, height) {}

    /// <summary>
    ///     Creates an empty image.
    /// </summary>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <param name="format">The format of the image, or null to use the default format.</param>
    public Image(int width, int height, Format? format = null) : this(new int[width * height], format ?? BitmapImageFormat, width, height) {}

    /// <summary>
    ///     Creates an image from a bitmap.
    /// </summary>
    /// <param name="bitmap">The bitmap, will be copied and not disposed.</param>
    /// <param name="format">The format of the image, or null to use the format of the bitmap.</param>
    public Image(Bitmap bitmap, Format? format = null) : this(bitmap.Width, bitmap.Height, format)
    {
        unsafe
        {
            BitmapData content = bitmap.LockBits(GetFull(bitmap), ImageLockMode.ReadOnly, BitmapPixelFormat);

            Span<int> src = new((int*) content.Scan0, bitmap.Width * bitmap.Height);
            Span<int> dst = data;

            Reformat(dst, BitmapImageFormat, src, this.format, data.Length);

            bitmap.UnlockBits(content);
        }
    }

    /// <summary>
    ///     The width of the image.
    /// </summary>
    public int Width { get; }

    /// <summary>
    ///     The height of the image.
    /// </summary>
    public int Height { get; }

    /// <summary>
    ///     Gets the size of the image.
    /// </summary>
    public Vector2i Size => new(Width, Height);

    private ref int this[int x, int y] => ref data[y * Width + x];

    /// <summary>
    ///     The ARGB format of the bitmap goes from most to least significant byte.
    ///     In contrast, the format of this class defines the first channel as the least significant byte.
    ///     Therefore, here the format of the bitmap is considered to be BGRA.
    /// </summary>
    private static Format BitmapImageFormat => Format.BGRA;

    private static PixelFormat BitmapPixelFormat => PixelFormat.Format32bppArgb;

    /// <summary>
    ///     Gets the format of the image.
    /// </summary>
    public Format GetFormat()
    {
        return format;
    }

    /// <summary>
    ///     Loads an image from a file. Can throw IO exceptions.
    /// </summary>
    /// <param name="file">The file to load from.</param>
    /// <param name="format">The format of the image, or null to use the bitmap format.</param>
    /// <returns>The loaded image.</returns>
    public static Image LoadFromFile(FileInfo file, Format? format = null)
    {
        using Stream stream = file.OpenRead();
        using Bitmap bitmap = new(stream);

        return new Image(bitmap, format);
    }

    /// <summary>
    ///     Creates a bitmap from this image. This will copy the data.
    /// </summary>
    /// <returns>The bitmap.</returns>
    private Bitmap CreateBitmap()
    {
        unsafe
        {
            Bitmap bitmap = new(Width, Height, BitmapPixelFormat);
            BitmapData content = bitmap.LockBits(GetFull(bitmap), ImageLockMode.WriteOnly, BitmapPixelFormat);

            Span<int> src = data;
            Span<int> dst = new((int*) content.Scan0, bitmap.Width * bitmap.Height);

            Reformat(dst, BitmapImageFormat, src, format, data.Length);

            bitmap.UnlockBits(content);

            return bitmap;
        }
    }

    /// <summary>
    ///     Creates a copy of this image.
    /// </summary>
    /// <param name="area">The area to copy, or null to copy the whole image.</param>
    /// <param name="targetFormat">The format of the copy, or null to use the format of this image.</param>
    /// <returns>The copy.</returns>
    public Image CreateCopy(Rectangle? area = null, Format? targetFormat = null)
    {
        Format copyFormat = targetFormat ?? format;

        Vector2i size = area == null ? Size : area.Value.Size.ToVector2i();

        int[] copy;

        if (area == null)
        {
            copy = new int[data.Length];
            Reformat(copy, copyFormat, data, format, size.X * size.Y);
        }
        else
        {
            copy = new int[size.X * size.Y];

            for (var y = 0; y < size.Y; y++)
            {
                Span<int> src = data.AsSpan().Slice(area.Value.X + (area.Value.Y + y) * Width, size.X);
                Span<int> dst = copy.AsSpan().Slice(y * size.X);

                Reformat(dst, copyFormat, src, format, size.X);
            }
        }

        return new Image(copy, copyFormat, size.X, size.Y);
    }

    /// <summary>
    ///     Saves the image to a file.
    /// </summary>
    /// <param name="file">The file to save to.</param>
    /// <returns>An exception if saving failed, otherwise null.</returns>
    public Exception? Save(FileInfo file)
    {
        try
        {
            using Bitmap bitmap = CreateBitmap();

            bitmap.Save(file.FullName);

            return null;
        }
        catch (Exception e)
        {
            return e;
        }
    }

    /// <summary>
    ///     Creates a fallback image.
    /// </summary>
    /// <param name="size">The size of the image to create, given in number of pixels per side.</param>
    /// <param name="format">The format of the image, or null to use the default format.</param>
    /// <returns>The created fallback image.</returns>
    public static Image CreateFallback(int size, Format? format = null)
    {
        Image fallback = new(size, size, format);

        Color magenta = Color.FromArgb(alpha: 64, red: 255, green: 0, blue: 255);
        Color black = Color.FromArgb(alpha: 64, red: 0, green: 0, blue: 0);

        for (var x = 0; x < fallback.Width; x++)
        for (var y = 0; y < fallback.Height; y++)
            fallback.SetPixel(x, y, (x % 2 == 0) ^ (y % 2 == 0) ? magenta : black);

        return fallback;
    }

    /// <summary>
    ///     Sets the pixel at given position.
    /// </summary>
    /// <param name="x">The x coordinate of the pixel.</param>
    /// <param name="y">The y coordinate of the pixel.</param>
    /// <param name="color">The color to set.</param>
    public void SetPixel(int x, int y, Color color)
    {
        this[x, y] = Reformat(color.ToArgb(), BitmapImageFormat, format);
    }

    /// <summary>
    ///     Gets the pixel at given position.
    /// </summary>
    /// <param name="x">The x coordinate of the pixel.</param>
    /// <param name="y">The y coordinate of the pixel.</param>
    /// <returns>The color of the pixel.</returns>
    public Color GetPixel(int x, int y)
    {
        return Color.FromArgb(Reformat(this[x, y], format, BitmapImageFormat));
    }

    /// <summary>
    ///     Get the data of the image in the given format.
    /// </summary>
    /// <param name="dataFormat">The format of the data.</param>
    /// <returns>The data, in the given format.</returns>
    public int[] GetData(Format dataFormat)
    {
        return format == dataFormat ? data : CreateCopy(targetFormat: format).data;
    }

    private static Rectangle GetFull(System.Drawing.Image image)
    {
        return new Rectangle(x: 0, y: 0, image.Width, image.Height);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Reformat(int original, Format originalFormat, Format targetFormat)
    {
        int result = default;

        result |= ((original >> originalFormat.R) & ChannelMask) << targetFormat.R;
        result |= ((original >> originalFormat.G) & ChannelMask) << targetFormat.G;
        result |= ((original >> originalFormat.B) & ChannelMask) << targetFormat.B;
        result |= ((original >> originalFormat.A) & ChannelMask) << targetFormat.A;

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Reformat(Span<int> dst, Format dstFormat, Span<int> src, Format srcFormat, int length)
    {
        if (dstFormat != srcFormat)
            for (var i = 0; i < length; i++)
                dst[i] = Reformat(src[i], srcFormat, dstFormat);
        else if (dst != src)
            src.CopyTo(dst);
    }

    /// <summary>
    ///     Defines a color format based on the order of the channels.
    ///     These formats apply for colors using 32 bits per pixel, where each channel is 8 bits.
    /// </summary>
    public record struct Format(int R, int G, int B, int A)
    {
        /// <summary>
        ///     The format where the channels are in the order R, G, B, A.
        /// </summary>
        public static readonly Format RGBA = new(C0, C1, C2, C3);

        /// <summary>
        ///     The format where the channels are in the order B, G, R, A.
        /// </summary>
        public static readonly Format BGRA = new(C2, C1, C0, C3);
    }
}
