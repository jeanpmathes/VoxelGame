// <copyright file="Image.cs" company="VoxelGame">
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
using System.Threading;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     A 2D image, stored in memory for fast modification.
///     Each image has a format, which defines how the data is stored.
/// </summary>
public class Image
{
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

            Format.Reformat(dst, BitmapImageFormat, src, StorageFormat, data.Length);

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

            Format.Reformat(dst, BitmapImageFormat, src, StorageFormat, data.Length);

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
    public Image CreateCopy(Box2i? area = null, Format? targetFormat = null)
    {
        Format copyFormat = targetFormat ?? StorageFormat;

        Vector2i size = area?.Size + (1, 1) ?? Size;

        Int32[] copy;

        if (area == null)
        {
            copy = new Int32[data.Length];
            Format.Reformat(copy, copyFormat, data, StorageFormat, size.X * size.Y);
        }
        else
        {
            copy = new Int32[size.X * size.Y];

            for (var y = 0; y < size.Y; y++)
            {
                Span<Int32> src = data.AsSpan().Slice(area.Value.Min.X + (area.Value.Min.Y + y) * Width, size.X);
                Span<Int32> dst = copy.AsSpan().Slice(y * size.X);

                Format.Reformat(dst, copyFormat, src, StorageFormat, size.X);
            }
        }

        return new Image(copy, copyFormat, size.X, size.Y);
    }

    /// <summary>
    ///     Save the image to a file asynchronously.
    /// </summary>
    /// <param name="file">The file to save to.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public async Task<Result> SaveAsync(FileInfo file, CancellationToken token = default)
    {
        try
        {
            using Bitmap bitmap = CreateBitmap();
            using MemoryStream memoryStream = new();

            bitmap.Save(memoryStream, ImageFormat.Png);

            memoryStream.Seek(offset: 0, SeekOrigin.Begin);

            await using FileStream fileStream = file.Create();
            await memoryStream.CopyToAsync(fileStream, token).InAnyContext();

            return Result.Ok();
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or ExternalException)
        {
            return Result.Error(e);
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

        Color32 magenta = Color32.FromRGBA(alpha: 64, red: 255, green: 0, blue: 255);
        Color32 black = Color32.FromRGBA(alpha: 64, red: 0, green: 0, blue: 0);

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
    public void SetPixel(Int32 x, Int32 y, Color32 color)
    {
        this[x, y] = color.ToInt32(StorageFormat);
    }

    /// <summary>
    ///     Sets the pixel at given position.
    /// </summary>
    /// <param name="x">The x coordinate of the pixel.</param>
    /// <param name="y">The y coordinate of the pixel.</param>
    /// <param name="color">The color to set.</param>
    public void SetPixel(Int32 x, Int32 y, ColorS color)
    {
        SetPixel(x, y, color.ToColor32());
    }

    /// <summary>
    ///     Gets the pixel at given position.
    /// </summary>
    /// <param name="x">The x coordinate of the pixel.</param>
    /// <param name="y">The y coordinate of the pixel.</param>
    /// <returns>The color of the pixel.</returns>
    public Color32 GetPixel(Int32 x, Int32 y)
    {
        return Color32.FromInt32(this[x, y], StorageFormat);
    }

    /// <summary>
    ///     Get the average color of the image.
    ///     This ignores pixels that are completely transparent.
    ///     Only supported for images with a size less than or equal <c>32x32</c>.
    /// </summary>
    /// <returns>The average color, or transparent black if no non-transparent pixels are present.</returns>
    public Color32 CalculateAverage()
    {
        Debug.Assert(Width <= 32 && Height <= 32);

        Vector4i sum = Vector4i.Zero;
        var count = 0;

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
        {
            Color32 pixel = GetPixel(x, y);

            if (pixel.A == 0) continue;

            sum += pixel.ToVector4i();
            count += 1;
        }

        return count == 0
            ? Color32.FromRGBA(red: 0, green: 0, blue: 0, alpha: 0)
            : Color32.FromVector4i(sum / count);
    }

    /// <summary>
    ///     Set the color (RGB components) of all transparent pixels to the average color of all non-transparent pixels.
    ///     The transparency of the pixels will be kept.
    /// </summary>
    public void RecolorTransparency()
    {
        Color32 average = CalculateAverage() with {A = 0};

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
        {
            if (GetPixel(x, y).A != 0) continue;

            SetPixel(x, y, average);
        }
    }

    /// <summary>
    ///     Create a translated (moved) image, shifting by the given amount of pixels.
    ///     This wraps around the image, so pixels that are moved out of the image will appear on the other side.
    /// </summary>
    /// <param name="dx">The amount of pixels to move the image in the x direction.</param>
    /// <param name="dy">The amount of pixels to move the image in the y direction.</param>
    public Image Translated(Int32 dx, Int32 dy)
    {
        if (dx == 0 && dy == 0)
            return CreateCopy();

        dx = MathTools.Mod(dx, Width);
        dy = MathTools.Mod(dy, Height);

        Image translated = new(Width, Height, StorageFormat);

        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
        {
            Int32 tx = MathTools.Mod(x + dx, Width);
            Int32 ty = MathTools.Mod(y + dy, Height);

            translated.SetPixel(x, y, GetPixel(tx, ty));
        }

        return translated;
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
    ///     Check if an image is empty.
    ///     An image is considered empty if all pixels have the zero-value.
    /// </summary>
    /// <returns>The result of the check.</returns>
    [PerformanceSensitive]
    public Boolean IsEmpty()
    {
        foreach (Int32 value in data)
            if (value != 0)
                return false;

        return true;
    }

    /// <summary>
    ///     Defines a color format based on the order of the channels.
    ///     These formats apply for colors using 32 bits per pixel, where each channel is 8 bits.
    /// </summary>
    public record struct Format(Byte R, Byte G, Byte B, Byte A)
    {
        /// <summary>
        ///     The format where the channels are in the order R, G, B, A.
        /// </summary>
        public static readonly Format RGBA = new(Color32.C0, Color32.C1, Color32.C2, Color32.C3);

        /// <summary>
        ///     The format where the channels are in the order B, G, R, A.
        /// </summary>
        public static readonly Format BGRA = new(Color32.C2, Color32.C1, Color32.C0, Color32.C3);

        /// <summary>
        ///     Reformat a single value from one format to another.
        /// </summary>
        /// <param name="original">The original value.</param>
        /// <param name="originalFormat">The format of the original value.</param>
        /// <param name="targetFormat">The format of the target value.</param>
        /// <returns>The value in the target format.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Int32 Reformat(Int32 original, Format originalFormat, Format targetFormat)
        {
            var result = 0;

            result |= ((original >> originalFormat.R) & Color32.ChannelMask) << targetFormat.R;
            result |= ((original >> originalFormat.G) & Color32.ChannelMask) << targetFormat.G;
            result |= ((original >> originalFormat.B) & Color32.ChannelMask) << targetFormat.B;
            result |= ((original >> originalFormat.A) & Color32.ChannelMask) << targetFormat.A;

            return result;
        }

        /// <summary>
        ///     Reformat a span of values from one format to another.
        /// </summary>
        /// <param name="dst">The target span.</param>
        /// <param name="dstFormat">The format of the target values.</param>
        /// <param name="src">The source span.</param>
        /// <param name="srcFormat">The format of the source values.</param>
        /// <param name="length">The number of values to reformat.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Reformat(Span<Int32> dst, Format dstFormat, Span<Int32> src, Format srcFormat, Int32 length)
        {
            if (dstFormat != srcFormat)
                for (var i = 0; i < length; i++)
                    dst[i] = Reformat(src[i], srcFormat, dstFormat);
            else if (dst != src)
                src.CopyTo(dst);
        }
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

        /// <summary>
        ///     Averages colors, with an option to consider transparency.
        /// </summary>
        /// <param name="transparency">If <c>true</c>, transparent colors are included in the average; otherwise, they are ignored.</param>
        private sealed class AveragingAlgorithm(Boolean transparency) : MipmapAlgorithm
        {
            private Vector4i DetermineFactors(Color32 c1, Color32 c2, Color32 c3, Color32 c4)
            {
                if (transparency)
                    return (1, 1, 1, 1);

                Int32 f1 = (c1.A != 0).ToInt();
                Int32 f2 = (c2.A != 0).ToInt();
                Int32 f3 = (c3.A != 0).ToInt();
                Int32 f4 = (c4.A != 0).ToInt();

                return (f1, f2, f3, f4);
            }

            protected override void CreateNextLevel(Image previous, Image next)
            {
                for (var w = 0; w < next.Width; w++)
                for (var h = 0; h < next.Height; h++)
                {
                    Color32 c1 = previous.GetPixel(w * 2, h * 2);
                    Color32 c2 = previous.GetPixel(w * 2 + 1, h * 2);
                    Color32 c3 = previous.GetPixel(w * 2, h * 2 + 1);
                    Color32 c4 = previous.GetPixel(w * 2 + 1, h * 2 + 1);

                    Vector4i factors = DetermineFactors(c1, c2, c3, c4);
                    Color32 average = CalculateAverageColor(c1, c2, c3, c4, factors);

                    next.SetPixel(w, h, average);
                }
            }

            private static Color32 CalculateAverageColor(Color32 c1, Color32 c2, Color32 c3, Color32 c4, Vector4i factors)
            {
                Vector3i totalRGB = Vector3i.Zero;
                var totalAlpha = 0;

                Accumulate(c1, factors.X);
                Accumulate(c2, factors.Y);
                Accumulate(c3, factors.Z);
                Accumulate(c4, factors.W);

                Int32 totalFactors = factors.X + factors.Y + factors.Z + factors.W;

                if (totalFactors == 0)
                    return Color32.FromRGBA(red: 0, green: 0, blue: 0, alpha: 0);

                Vector3i averageRGB = new(totalRGB.X / totalFactors, totalRGB.Y / totalFactors, totalRGB.Z / totalFactors);
                Int32 averageAlpha = totalAlpha / totalFactors;

                if (averageAlpha == 0)
                    return Color32.FromRGBA(red: 0, green: 0, blue: 0, alpha: 0);

                // To get correct rounding instead of flooring, we add half of the divisor before dividing.
                Int32 roundingOffset = averageAlpha / 2;

                Int32 red = (averageRGB.X + roundingOffset) / averageAlpha;
                Int32 green = (averageRGB.Y + roundingOffset) / averageAlpha;
                Int32 blue = (averageRGB.Z + roundingOffset) / averageAlpha;

                return Color32.FromRGBA((Byte) red, (Byte) green, (Byte) blue, (Byte) averageAlpha);

                void Accumulate(Color32 color, Int32 factor)
                {
                    if (factor == 0) return;

                    Int32 alpha = color.A;

                    totalRGB += new Vector3i(color.R * alpha, color.G * alpha, color.B * alpha);
                    totalAlpha += alpha;
                }
            }
        }
    }
}
