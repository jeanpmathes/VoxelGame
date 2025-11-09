// <copyright file="Color32.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Stores a color in a single 32-bit integer.
/// </summary>
public struct Color32 : IEquatable<Color32>
{
    /// <summary>
    ///     The number of bits in a byte.
    /// </summary>
    public const Byte BitsPerByte = sizeof(Byte) * 8;

    /// <summary>
    ///     A mask to select a single color channel from a 32-bit color.
    /// </summary>
    public const Int32 ChannelMask = (1 << BitsPerByte) - 1;

    /// <summary>
    ///     Shift to access the first color channel.
    /// </summary>
    public const Byte C0 = 0 * BitsPerByte;

    /// <summary>
    ///     Shift to access the second color channel.
    /// </summary>
    public const Byte C1 = 1 * BitsPerByte;

    /// <summary>
    ///     Shift to access the third color channel.
    /// </summary>
    public const Byte C2 = 2 * BitsPerByte;

    /// <summary>
    ///     Shift to access the fourth color channel.
    /// </summary>
    public const Byte C3 = 3 * BitsPerByte;

    /// <summary>
    ///     The format used to store the color.
    /// </summary>
    public static readonly Image.Format Format = Image.Format.BGRA;

    private Int32 bgra;

    /// <summary>
    ///     Creates a new <see cref="Color32" /> from a 32-bit integer.
    ///     Uses the <see cref="Image.Format.BGRA" /> format.
    /// </summary>
    /// <param name="bgra">The 32-bit integer to create the color from.</param>
    private Color32(Int32 bgra)
    {
        this.bgra = bgra;
    }

    /// <summary>
    ///     Creates a new <see cref="Color32" /> from a 32-bit integer.
    /// </summary>
    /// <param name="value">The value of the 32-bit integer.</param>
    /// <param name="format">The format of the 32-bit integer.</param>
    /// <returns>The created <see cref="Color32" />.</returns>
    public static Color32 FromInt32(Int32 value, Image.Format format)
    {
        return new Color32(Image.Format.Reformat(value, format, Format));
    }

    /// <summary>
    ///     Creates a new <see cref="Color32" /> from the specified color channels.
    /// </summary>
    public static Color32 FromRGBA(Byte red, Byte green, Byte blue, Byte alpha)
    {
        return new Color32(red << Format.R | green << Format.G | blue << Format.B | alpha << Format.A);
    }

    /// <summary>
    ///     Creates a new <see cref="Color32" /> from the specified color channels, given as a vector in RGBA order.
    /// </summary>
    public static Color32 FromRGBA(Vector4i rgba)
    {
        Int32 red = Math.Clamp(rgba.X, Byte.MinValue, Byte.MaxValue);
        Int32 green = Math.Clamp(rgba.Y, Byte.MinValue, Byte.MaxValue);
        Int32 blue = Math.Clamp(rgba.Z, Byte.MinValue, Byte.MaxValue);
        Int32 alpha = Math.Clamp(rgba.W, Byte.MinValue, Byte.MaxValue);

        return FromRGBA((Byte) red, (Byte) green, (Byte) blue, (Byte) alpha);
    }

    /// <summary>
    ///     Creates a new <see cref="Color32" /> from a <see cref="ColorS" />.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <returns>The created <see cref="Color32" />.</returns>
    public static Color32 FromColorS(ColorS color)
    {
        var r = (Byte) (MathTools.Clamp01(color.R) * Byte.MaxValue);
        var g = (Byte) (MathTools.Clamp01(color.G) * Byte.MaxValue);
        var b = (Byte) (MathTools.Clamp01(color.B) * Byte.MaxValue);
        var a = (Byte) (MathTools.Clamp01(color.A) * Byte.MaxValue);

        return FromRGBA(r, g, b, a);
    }

    /// <summary>
    ///     Creates a new <see cref="Color32" /> from a <see cref="Color" />.
    /// </summary>
    /// <param name="color">The original color.</param>
    /// <returns>The created <see cref="Color32" />.</returns>
    public static Color32 FromColor(Color color)
    {
        return FromRGBA(color.R, color.G, color.B, color.A);
    }

    /// <summary>
    ///     Converts this color to a <see cref="ColorS" />.
    /// </summary>
    /// <returns>The <see cref="ColorS" /> representation of this color.</returns>
    public ColorS ToColorS()
    {
        Single r = R / (Single) Byte.MaxValue;
        Single g = G / (Single) Byte.MaxValue;
        Single b = B / (Single) Byte.MaxValue;
        Single a = A / (Single) Byte.MaxValue;

        return new ColorS(r, g, b, a);
    }

    /// <summary>
    ///     Gets this color as a 32-bit integer.
    /// </summary>
    /// <param name="format">The format of the 32-bit integer.</param>
    /// <returns>The 32-bit integer.</returns>
    public Int32 ToInt32(Image.Format format)
    {
        return Image.Format.Reformat(bgra, Format, format);
    }

    /// <summary>
    ///     Create an integer vector containing the color channels in RGBA order.
    /// </summary>
    /// <returns>The integer vector.</returns>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "OpenTK naming conventions.")]
    public Vector4i ToVector4i()
    {
        return new Vector4i(R, G, B, A);
    }

    /// <summary>
    ///     Create a color from a vector containing the color channels in RGBA order.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "OpenTK naming conventions.")]
    public static Color32 FromVector4i(Vector4i rgba)
    {
        var r = (Byte) Math.Clamp(rgba.X, Byte.MinValue, Byte.MaxValue);
        var g = (Byte) Math.Clamp(rgba.Y, Byte.MinValue, Byte.MaxValue);
        var b = (Byte) Math.Clamp(rgba.Z, Byte.MinValue, Byte.MaxValue);
        var a = (Byte) Math.Clamp(rgba.W, Byte.MinValue, Byte.MaxValue);

        return FromRGBA(r, g, b, a);
    }

    /// <summary>
    ///     The red channel of the color.
    /// </summary>
    public Byte R
    {
        get => (Byte) (bgra >> Format.R & ChannelMask);
        set => bgra = bgra & ~(ChannelMask << Format.R) | value << Format.R;
    }

    /// <summary>
    ///     The green channel of the color.
    /// </summary>
    public Byte G
    {
        get => (Byte) (bgra >> Format.G & ChannelMask);
        set => bgra = bgra & ~(ChannelMask << Format.G) | value << Format.G;
    }

    /// <summary>
    ///     The blue channel of the color.
    /// </summary>
    public Byte B
    {
        get => (Byte) (bgra >> Format.B & ChannelMask);
        set => bgra = bgra & ~(ChannelMask << Format.B) | value << Format.B;
    }

    /// <summary>
    ///     The alpha channel of the color.
    /// </summary>
    public Byte A
    {
        get => (Byte) (bgra >> Format.A & ChannelMask);
        set => bgra = bgra & ~(ChannelMask << Format.A) | value << Format.A;
    }

    #region OPERATIONS

    /// <summary>
    ///     Round the color values to match the given bit depth.
    ///     This will not change the alpha channel.
    /// </summary>
    /// <param name="bits">The number of bits to round to, inclusive between 0 and 8.</param>
    /// <returns>The rounded color.</returns>
    public Color32 ReduceBits(Byte bits)
    {
        Debug.Assert(bits <= 8);

        if (bits == 0)
            return new Color32(bgra: 0);

        var divisor = (Byte) (1 << 8 - bits);

        return FromRGBA(
            (Byte) (R / divisor * divisor),
            (Byte) (G / divisor * divisor),
            (Byte) (B / divisor * divisor),
            A);
    }

    #endregion OPERATIONS

    #region EQUALITY

    /// <inheritdoc />
    public Boolean Equals(Color32 other)
    {
        return bgra == other.bgra;
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is Color32 other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return bgra;
    }

    /// <summary>
    ///     Equality operator.
    /// </summary>
    public static Boolean operator ==(Color32 left, Color32 right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Inequality operator.
    /// </summary>
    public static Boolean operator !=(Color32 left, Color32 right)
    {
        return !left.Equals(right);
    }

    #endregion EQUALITY
}
