﻿// <copyright file="Image.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     A 2D image, stored in memory for fast modification.
///     Each image has a format, which defines how the data is stored.
/// </summary>
public class Image
{
    private const Int32 BitsPerByte = 8;
    private const Int32 ChannelMask = (1 << BitsPerByte) - 1;

    private const Int32 C0 = 0 * BitsPerByte;
    private const Int32 C1 = 1 * BitsPerByte;
    private const Int32 C2 = 2 * BitsPerByte;
    private const Int32 C3 = 3 * BitsPerByte;

    private readonly Int32[] data;

    private Image(Int32[] data, Format format, Int32 width, Int32 height)
    {
        Debug.Assert(data.Length == width * height);

        this.data = data;

        StorageFormat = format;

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
    public Image(Span<Int32> data, Format format, Int32 width, Int32 height) : this(data.ToArray(), format, width, height) {}

    /// <summary>
    ///     Creates an empty image.
    /// </summary>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <param name="format">The format of the image, or null to use the default format.</param>
    public Image(Int32 width, Int32 height, Format? format = null) : this(new Int32[width * height], format ?? BitmapImageFormat, width, height) {}

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

            Span<Int32> src = new((Int32*) content.Scan0, bitmap.Width * bitmap.Height);
            Span<Int32> dst = data;

            Reformat(dst, BitmapImageFormat, src, StorageFormat, data.Length);

            bitmap.UnlockBits(content);
        }
    }

    /// <summary>
    ///     The width of the image.
    /// </summary>
    public Int32 Width { get; }

    /// <summary>
    ///     The height of the image.
    /// </summary>
    public Int32 Height { get; }

    /// <summary>
    ///     Gets the size of the image.
    /// </summary>
    public Vector2i Size => new(Width, Height);

    private ref Int32 this[Int32 x, Int32 y] => ref data[y * Width + x];

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
    public Format StorageFormat { get; }

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

            Span<Int32> src = data;
            Span<Int32> dst = new((Int32*) content.Scan0, bitmap.Width * bitmap.Height);

            Reformat(dst, BitmapImageFormat, src, StorageFormat, data.Length);

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
        Format copyFormat = targetFormat ?? StorageFormat;

        Vector2i size = area == null ? Size : area.Value.Size.ToVector2i();

        Int32[] copy;

        if (area == null)
        {
            copy = new Int32[data.Length];
            Reformat(copy, copyFormat, data, StorageFormat, size.X * size.Y);
        }
        else
        {
            copy = new Int32[size.X * size.Y];

            for (var y = 0; y < size.Y; y++)
            {
                Span<Int32> src = data.AsSpan().Slice(area.Value.X + (area.Value.Y + y) * Width, size.X);
                Span<Int32> dst = copy.AsSpan().Slice(y * size.X);

                Reformat(dst, copyFormat, src, StorageFormat, size.X);
            }
        }

        return new Image(copy, copyFormat, size.X, size.Y);
    }

    /// <summary>
    ///     Saves the image to a file.
    /// </summary>
    /// <param name="file">The file to save to.</param>
    /// <returns>An exception if saving failed, otherwise null.</returns>
    #pragma warning disable S3242 // Type carries semantic information.
    public Exception? Save(FileInfo file)
    #pragma warning restore S3242
    {
        try
        {
            using Bitmap bitmap = CreateBitmap();

            bitmap.Save(file.FullName);

            return null;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or ExternalException)
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
    public static Image CreateFallback(Int32 size, Format? format = null)
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
    public void SetPixel(Int32 x, Int32 y, Color color)
    {
        this[x, y] = Reformat(color.ToArgb(), BitmapImageFormat, StorageFormat);
    }

    /// <summary>
    ///     Set the color (RGB components) of all transparent pixels to the average color of all non-transparent pixels.
    ///     The transparency of the pixels will be kept.
    /// </summary>
    public void RecolorTransparency()
    {
        Int64 r = 0;
        Int64 g = 0;
        Int64 b = 0;
        Int64 count = 0;

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
        {
            Color pixel = GetPixel(x, y);

            if (pixel.A == 0) continue;

            r += pixel.R * pixel.R;
            g += pixel.G * pixel.G;
            b += pixel.B * pixel.B;

            count++;
        }

        Int32 GetAverage(Int64 sum)
        {
            return (Int32) Math.Sqrt(sum / (Double) count);
        }

        Color average = Color.FromArgb(alpha: 0, GetAverage(r), GetAverage(g), GetAverage(b));

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
        {
            if (GetPixel(x, y).A != 0) continue;

            SetPixel(x, y, average);
        }
    }

    /// <summary>
    ///     Gets the pixel at given position.
    /// </summary>
    /// <param name="x">The x coordinate of the pixel.</param>
    /// <param name="y">The y coordinate of the pixel.</param>
    /// <returns>The color of the pixel.</returns>
    public Color GetPixel(Int32 x, Int32 y)
    {
        return Color.FromArgb(Reformat(this[x, y], StorageFormat, BitmapImageFormat));
    }

    /// <summary>
    ///     Get the data of the image in the given format.
    /// </summary>
    /// <param name="dataFormat">The format of the data.</param>
    /// <returns>The data, in the given format.</returns>
    public Int32[] GetData(Format dataFormat)
    {
        return StorageFormat == dataFormat ? data : CreateCopy(targetFormat: StorageFormat).data;
    }

    private static Rectangle GetFull(System.Drawing.Image image)
    {
        return new Rectangle(x: 0, y: 0, image.Width, image.Height);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Int32 Reformat(Int32 original, Format originalFormat, Format targetFormat)
    {
        Int32 result = default;

        result |= ((original >> originalFormat.R) & ChannelMask) << targetFormat.R;
        result |= ((original >> originalFormat.G) & ChannelMask) << targetFormat.G;
        result |= ((original >> originalFormat.B) & ChannelMask) << targetFormat.B;
        result |= ((original >> originalFormat.A) & ChannelMask) << targetFormat.A;

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Reformat(Span<Int32> dst, Format dstFormat, Span<Int32> src, Format srcFormat, Int32 length)
    {
        if (dstFormat != srcFormat)
            for (var i = 0; i < length; i++)
                dst[i] = Reformat(src[i], srcFormat, dstFormat);
        else if (dst != src)
            src.CopyTo(dst);
    }

    /// <summary>
    ///     Generate the mipmaps for this image.
    /// </summary>
    /// <param name="levels">The number of levels the texture uses. Includes the base level.</param>
    /// <param name="algorithm">The algorithm to use.</param>
    /// <returns>The mipmaps, without the base level.</returns>
    public IEnumerable<Image> GenerateMipmaps(Int32 levels, MipmapAlgorithm algorithm)
    {
        Image current = this;

        for (var level = 1; level < levels; level++)
        {
            current = algorithm.CreateNextLevel(current);

            yield return current;
        }
    }

    /// <summary>
    ///     Defines a color format based on the order of the channels.
    ///     These formats apply for colors using 32 bits per pixel, where each channel is 8 bits.
    /// </summary>
    public record struct Format(Int32 R, Int32 G, Int32 B, Int32 A)
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

    /// <summary>
    ///     Defines the algorithm used to generate mipmaps.
    /// </summary>
    public abstract class MipmapAlgorithm
    {
        /// <summary>
        ///     Averages all colors that are merged, except for transparent colors.
        /// </summary>
        public static MipmapAlgorithm AveragingWithoutTransparency { get; } = new AveragingAlgorithm(transparency: false);

        /// <summary>
        ///     Averages all colors that are merged, including transparent colors.
        /// </summary>
        public static MipmapAlgorithm AveragingWithTransparency { get; } = new AveragingAlgorithm(transparency: true);

        /// <summary>
        ///     Create the next level of the mipmap.
        /// </summary>
        /// <param name="previous">The previous image.</param>
        /// <returns>The next level. Will have half the size of the input.</returns>
        public Image CreateNextLevel(Image previous)
        {
            Image next = new(previous.Width / 2, previous.Height / 2);

            CreateNextLevel(previous, next);

            return next;
        }

        /// <summary>
        ///     Create the data for the next level of the mipmap.
        /// </summary>
        /// <param name="previous">The previous image.</param>
        /// <param name="next">The next level, has to be filled with the data.</param>
        protected abstract void CreateNextLevel(Image previous, Image next);

        private sealed class AveragingAlgorithm(Boolean transparency) : MipmapAlgorithm
        {
            private ((Int32, Int32, Int32, Int32) factors, Int32 alpha) DetermineFactorsAndAlpha(
                Color c1, Color c2, Color c3, Color c4)
            {
                (Int32, Int32, Int32, Int32) factors;
                Int32 alpha;

                if (transparency)
                {
                    factors = (1, 1, 1, 1);
                    alpha = CalculateAveragedColorChannel(c1.A, c2.A, c3.A, c4.A, factors);
                }
                else
                {
                    alpha = Math.Max(Math.Max(c1.A, c2.A), Math.Max(c3.A, c4.A));

                    if (alpha == 0)
                    {
                        factors = (1, 1, 1, 1);
                    }
                    else
                    {
                        Int32 f1 = c1.HasOpaqueness().ToInt();
                        Int32 f2 = c2.HasOpaqueness().ToInt();
                        Int32 f3 = c3.HasOpaqueness().ToInt();
                        Int32 f4 = c4.HasOpaqueness().ToInt();

                        factors = (f1, f2, f3, f4);
                    }
                }

                return (factors, alpha);
            }

            protected override void CreateNextLevel(Image previous, Image next)
            {
                for (var w = 0; w < next.Width; w++)
                for (var h = 0; h < next.Height; h++)
                {
                    Color c1 = previous.GetPixel(w * 2, h * 2);
                    Color c2 = previous.GetPixel(w * 2 + 1, h * 2);
                    Color c3 = previous.GetPixel(w * 2, h * 2 + 1);
                    Color c4 = previous.GetPixel(w * 2 + 1, h * 2 + 1);

                    ((Int32, Int32, Int32, Int32) factors, Int32 alpha) = DetermineFactorsAndAlpha(c1, c2, c3, c4);

                    Color average = Color.FromArgb(
                        alpha,
                        CalculateAveragedColorChannel(c1.R, c2.R, c3.R, c4.R, factors),
                        CalculateAveragedColorChannel(c1.G, c2.G, c3.G, c4.G, factors),
                        CalculateAveragedColorChannel(c1.B, c2.B, c3.B, c4.B, factors));

                    next.SetPixel(w, h, average);
                }
            }

            private static Int32 CalculateAveragedColorChannel(Int32 c1, Int32 c2, Int32 c3, Int32 c4, (Int32, Int32, Int32, Int32) factors)
            {
                (Int32 f1, Int32 f2, Int32 f3, Int32 f4) = factors;
                Double divisor = f1 + f2 + f3 + f4;

                Int32 s1 = c1 * c1 * f1;
                Int32 s2 = c2 * c2 * f2;
                Int32 s3 = c3 * c3 * f3;
                Int32 s4 = c4 * c4 * f4;

                return (Int32) Math.Sqrt((s1 + s2 + s3 + s4) / divisor);
            }
        }
    }
}
