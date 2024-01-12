// <copyright file="Images.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Utility to work with images.
/// </summary>
public static class Images
{
    // todo: and it should be put into a separate class, there all constants can become members
    // todo: class could also do other bitmap operations - search usages of Bitmap in the codebase

    private const int BitsPerByte = 8;
    private const int ChannelMask = (1 << BitsPerByte) - 1;

    private const int C0 = 0 * BitsPerByte;
    private const int C1 = 1 * BitsPerByte;
    private const int C2 = 2 * BitsPerByte;
    private const int C3 = 3 * BitsPerByte;

    /// <summary>
    ///     Creates an image from given data.
    /// </summary>
    /// <param name="data">The data, must have a length of <paramref name="width" /> * <paramref name="height" />.</param>
    /// <param name="format">The format of the data.</param>
    /// <param name="width">The width of the image.</param>
    /// <param name="height">The height of the image.</param>
    /// <returns>The image.</returns>
    public static Bitmap CreateFromData(int[] data, Format format, int width, int height)
    {
        Debug.Assert(data.Length == width * height);

        Bitmap bitmap = new(width, height, PixelFormat.Format32bppArgb);

        unsafe
        {
            Rectangle rectangle = new(x: 0, y: 0, bitmap.Width, bitmap.Height);
            BitmapData content = bitmap.LockBits(rectangle, ImageLockMode.WriteOnly, bitmap.PixelFormat);

            fixed (int* src = data)
            {
                var dst = (int*) content.Scan0;

                // The ARGB format of the bitmap goes from most to least significant byte.
                // In contrast, the format of this class defines the first channel as the least significant byte.
                // Therefore, here the format of the bitmap is considered to be BGRA.

                Reformat(dst, Format.BGRA, src, format, data.Length);
            }

            bitmap.UnlockBits(content);
        }

        return bitmap;
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
    private static unsafe void Reformat(int* dest, Format destFormat, int* src, Format srcFormat, int length)
    {
        for (var i = 0; i < length; i++) dest[i] = Reformat(src[i], srcFormat, destFormat);
    }

    /// <summary>
    ///     Save an image to a file and return an exception if it fails.
    /// </summary>
    /// <param name="bitmap">The image to save.</param>
    /// <param name="file">The file to save to.</param>
    /// <returns>An exception if saving failed, otherwise null.</returns>
    public static Exception? Save(Bitmap bitmap, FileInfo file)
    {
        try
        {
            bitmap.Save(file.FullName);

            return null;
        }
        catch (Exception e)
        {
            return e;
        }
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
