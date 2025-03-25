// <copyright file="ColorS.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Drawing;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Stores a color using four single-precision floating-point numbers.
///     A color can be marked neutral using a <see cref="float.NaN" /> in the alpha channel.
///     Neutral colors will be provided by the environment at appropriate points.
///     This is used by the tint system, which uses multiplication to apply tints.
/// </summary>
public struct ColorS(Single red, Single green, Single blue, Single alpha = 1.0f) : IEquatable<ColorS>
{
    /// <summary>
    ///     The red color channel.
    /// </summary>
    public Single R { get; set; } = red;

    /// <summary>
    ///     The green color channel.
    /// </summary>
    public Single G { get; set; } = green;

    /// <summary>
    ///     The blue color channel.
    /// </summary>
    public Single B { get; set; } = blue;

    /// <summary>
    ///     The alpha color channel.
    /// </summary>
    public Single A { get; set; } = alpha;

    /// <summary>
    ///     Whether this color is neutral.
    /// </summary>
    public Boolean IsNeutral => Single.IsNaN(A);

    /// <summary>
    ///     Create a default color with all channels set to 0.
    /// </summary>
    public ColorS() : this(red: 0, green: 0, blue: 0, alpha: 0) {}

    /// <summary>
    ///     Convert this color to a <see cref="Color32" />.
    /// </summary>
    /// <returns>The <see cref="Color32" /> representation of this color.</returns>
    public Color32 ToColor32()
    {
        Debug.Assert(!IsNeutral);

        return Color32.FromColorS(this);
    }

    /// <summary>
    ///     Create a new color from a <see cref="Color32" />.
    /// </summary>
    /// <param name="color">The color to use.</param>
    /// <returns>The new color.</returns>
    public static ColorS FromColor32(Color32 color)
    {
        return color.ToColorS();
    }

    /// <summary>
    ///     Create a new color from the given channels.
    /// </summary>
    public static ColorS FromRGB(Single red, Single green, Single blue)
    {
        return new ColorS(red, green, blue);
    }

    /// <summary>
    ///     Create a new color from the given HSV channels.
    /// </summary>
    public static ColorS FromHSV(Single hue, Single saturation, Single value)
    {
        Color4 color = Color4.FromHsv((hue, saturation, value, 1.0f));

        return new ColorS(color.R, color.G, color.B, color.A);
    }

    /// <summary>
    ///     Create a new color from the given channels.
    /// </summary>
    public static ColorS FromRGBA(Single red, Single green, Single blue, Single alpha)
    {
        Debug.Assert(!Single.IsNaN(alpha));

        return new ColorS(red, green, blue, alpha);
    }

    /// <summary>
    ///     Create a new color from a <see cref="Color" />.
    /// </summary>
    /// <param name="color">The color to use.</param>
    /// <returns>The new color.</returns>
    public static ColorS FromColor(Color color)
    {
        return Color32.FromColor(color).ToColorS();
    }

    /// <summary>
    ///     Convert this color to a <see cref="Color" />.
    /// </summary>
    /// <returns>The new color.</returns>
    public Color ToColor()
    {
        Color32 color = Color32.FromColorS(this);

        return Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    /// <summary>
    ///     Get a color from a string, either as a color name or as a hex code.
    ///     Uses the names defined by <see cref="Color" />, not the named colors of this class.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <returns>The color, or <c>null</c> if the parsing failed.</returns>
    public static ColorS? FromString(String text)
    {
        try
        {
            Color color = text.StartsWith(value: '#')
                ? ColorTranslator.FromHtml(text)
                : Color.FromName(text);

            return FromColor(color);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    ///     Create a vector containing the color channels in RGBA order.
    /// </summary>
    /// <returns>The vector.</returns>
    public Vector4 ToVector4()
    {
        Debug.Assert(!IsNeutral);

        return new Vector4(R, G, B, A);
    }

    /// <summary>
    ///     Create a new color from a <see cref="Vector4" />.
    ///     Use this to send the color to the native side.
    /// </summary>
    /// <returns>The color.</returns>
    public Color4 ToColor4()
    {
        return (Color4) ToVector4();
    }

    /// <summary>
    ///     Create a new color from a <see cref="Vector4" />.
    /// </summary>
    /// <param name="vector">The vector to use.</param>
    /// <returns>The new color.</returns>
    public static ColorS FromVector4(Vector4 vector)
    {
        Debug.Assert(!Single.IsNaN(vector.W));

        return new ColorS(vector.X, vector.Y, vector.Z, vector.W);
    }

    #region OPERATIONS

    /// <summary>
    ///     The precision of the tint color passed to the shaders.
    /// </summary>
    public const Int32 TintPrecision = 4;

    /// <summary>
    ///     Gets the color as a limited bit representation used for the tint system in shaders.
    /// </summary>
    public UInt32 ToBits()
    {
        const Int32 shift = 8 - TintPrecision;

        Color32 rounded = ToColor32().ReduceBits(TintPrecision);

        UInt32 bits = 0;

        bits |= (UInt32) (rounded.R >> shift) << (TintPrecision * 2);
        bits |= (UInt32) (rounded.G >> shift) << (TintPrecision * 1);
        bits |= (UInt32) (rounded.B >> shift) << (TintPrecision * 0);

        return bits;
    }

    /// <summary>
    ///     Select this color or a given one if this color is neutral.
    /// </summary>
    /// <param name="neutral">The color to use instead if this color is neutral.</param>
    /// <returns>The selected color.</returns>
    public ColorS Select(ColorS neutral)
    {
        return IsNeutral ? neutral : this;
    }

    /// <summary>
    ///     Multiply two colors.
    /// </summary>
    private static ColorS Multiply(ColorS a, ColorS b)
    {
        Debug.Assert(!a.IsNeutral && !b.IsNeutral);

        return new ColorS(a.R * b.R, a.G * b.G, a.B * b.B, a.A);
    }

    /// <summary>
    ///     Multiply a color by a factor.
    /// </summary>
    private static ColorS Multiply(ColorS color, Single factor)
    {
        Debug.Assert(!color.IsNeutral);

        return new ColorS(color.R * factor, color.G * factor, color.B * factor, color.A);
    }

    /// <summary>
    ///     Add two colors.
    /// </summary>
    private static ColorS Add(ColorS a, ColorS b)
    {
        Debug.Assert(!a.IsNeutral && !b.IsNeutral);

        return new ColorS(a.R + b.R, a.G + b.G, a.B + b.B, a.A);
    }

    /// <summary>
    ///     Multiply two colors.
    /// </summary>
    public static ColorS operator *(ColorS a, ColorS b)
    {
        return Multiply(a, b);
    }

    /// <summary>
    ///     Multiply a color by a factor.
    /// </summary>
    public static ColorS operator *(ColorS color, Single factor)
    {
        return Multiply(color, factor);
    }

    /// <summary>
    ///     Multiply a color by a factor.
    /// </summary>
    public static ColorS operator *(Single factor, ColorS color)
    {
        return Multiply(color, factor);
    }

    /// <summary>
    ///     Add two colors.
    /// </summary>
    public static ColorS operator +(ColorS a, ColorS b)
    {
        return Add(a, b);
    }

    /// <summary>
    ///     Mix two colors.
    /// </summary>
    /// <param name="a">The first color.</param>
    /// <param name="b">The second color.</param>
    /// <param name="f">The mixing factor for linear interpolation.</param>
    public static ColorS Mix(ColorS a, ColorS b, Double f = 0.5)
    {
        var factor = (Single) f;

        return FromVector4(a.ToVector4() * (1 - factor) + b.ToVector4() * factor);
    }

    /// <summary>
    ///     Blend two colors, using alpha blending.
    /// </summary>
    /// <param name="back">The background color.</param>
    /// <param name="front">The foreground color.</param>
    /// <returns>The blended color.</returns>
    public static ColorS Blend(ColorS back, ColorS front)
    {
        return back * (1 - front.A) + front * front.A;
    }

    #endregion OPERATIONS

    #region SPECIAL

    /// <summary>
    ///     Create a neutral color.
    /// </summary>
    public static ColorS Neutral => new(Single.NaN, Single.NaN, Single.NaN, Single.NaN);

    /// <summary>
    ///     Create a color that will have no effect as a tint.
    /// </summary>
    public static ColorS None => new(red: 1, green: 1, blue: 1, alpha: 1);

    #endregion SPECIAL

    #region PREDEFINED COLORS

    /// <summary>
    ///     Gets a white color: <c>(1|1|1)</c>
    /// </summary>
    public static ColorS White => new(red: 1f, green: 1f, blue: 1f);

    /// <summary>
    ///     Gets a black color: <c>(0|0|0)</c>
    /// </summary>
    public static ColorS Black => new(red: 0f, green: 0f, blue: 0f);

    /// <summary>
    ///     Gets a red color: <c>(1|0|0)</c>
    /// </summary>
    public static ColorS Red => new(red: 1f, green: 0f, blue: 0f);

    /// <summary>
    ///     Gets a green color: <c>(0|1|0)</c>
    /// </summary>
    public static ColorS Green => new(red: 0f, green: 1f, blue: 0f);

    /// <summary>
    ///     Gets a blue color: <c>(0|0|1)</c>
    /// </summary>
    public static ColorS Blue => new(red: 0f, green: 0f, blue: 1f);

    /// <summary>
    ///     Gets a yellow color: <c>(1|1|0)</c>
    /// </summary>
    public static ColorS Yellow => new(red: 1f, green: 1f, blue: 0f);

    /// <summary>
    ///     Gets a cyan color: <c>(0|1|1)</c>
    /// </summary>
    public static ColorS Cyan => new(red: 0f, green: 1f, blue: 1f);

    /// <summary>
    ///     Gets a magenta color: <c>(1|0|1)</c>
    /// </summary>
    public static ColorS Magenta => new(red: 1f, green: 0f, blue: 1f);

    /// <summary>
    ///     Gets an orange color: <c>(1|0.5|0)</c>
    /// </summary>
    public static ColorS Orange => new(red: 1f, green: 0.5f, blue: 0f);

    /// <summary>
    ///     Gets a dark green color: <c>(0|0.5|0)</c>
    /// </summary>
    public static ColorS DarkGreen => new(red: 0f, green: 0.5f, blue: 0f);

    /// <summary>
    ///     Gets a light green color: <c>(0.55|0.90|0.55)</c>
    /// </summary>
    public static ColorS LightGreen => new(red: 0.55f, green: 0.90f, blue: 0.55f);

    /// <summary>
    ///     Gets a sea green color: <c>(0.20|0.55|0.35)</c>
    /// </summary>
    public static ColorS SeaGreen => new(red: 0.20f, green: 0.55f, blue: 0.35f);

    /// <summary>
    ///     Gets a light sea green color: <c>(0.10|0.70|0.65)</c>
    /// </summary>
    public static ColorS LightSeaGreen => new(red: 0.10f, green: 0.70f, blue: 0.65f);

    /// <summary>
    ///     Gets a lawn green color: <c>(0.5|1|0)</c>
    /// </summary>
    public static ColorS LawnGreen => new(red: 0.5f, green: 1f, blue: 0.0f);

    /// <summary>
    ///     Gets a lime color: <c>(0.75|1|0)</c>
    /// </summary>
    public static ColorS Lime => new(red: 0.75f, green: 1f, blue: 0f);

    /// <summary>
    ///     Gets a dark green color: <c>(0.05|0.4|0.05)</c>
    /// </summary>
    public static ColorS ForrestGreen => new(red: 0.05f, green: 0.4f, blue: 0.05f);

    /// <summary>
    ///     Gets a gray color: <c>(0.15|0.15|0.15)</c>
    /// </summary>
    public static ColorS Gray => new(red: 0.15f, green: 0.15f, blue: 0.15f);

    /// <summary>
    ///     Gets a light color: <c>(0.8|0.8|0.8)</c>
    /// </summary>
    public static ColorS LightGray => new(red: 0.8f, green: 0.8f, blue: 0.8f);

    /// <summary>
    ///     Gets an indigo color: <c>(0.5|1|0)</c>
    /// </summary>
    public static ColorS Indigo => new(red: 0.3f, green: 0.0f, blue: 0.5f);

    /// <summary>
    ///     Gets a maroon color: <c>(0.5|0|0)</c>
    /// </summary>
    public static ColorS Maroon => new(red: 0.5f, green: 0f, blue: 0f);

    /// <summary>
    ///     Gets an olive color: <c>(0.5|0.5|0)</c>
    /// </summary>
    public static ColorS Olive => new(red: 0.5f, green: 0.5f, blue: 0f);

    /// <summary>
    ///     Gets a brown color: <c>(0.5|0.25|0)</c>
    /// </summary>
    public static ColorS Brown => new(red: 0.5f, green: 0.25f, blue: 0f);

    /// <summary>
    ///     Gets a navy color: <c>(0|0|0.5)</c>
    /// </summary>
    public static ColorS Navy => new(red: 0f, green: 0f, blue: 0.5f);

    /// <summary>
    ///     Gets an amaranth color: <c>(0.9|0.2|0.3)</c>
    /// </summary>
    public static ColorS Amaranth => new(red: 0.9f, green: 0.2f, blue: 0.3f);

    /// <summary>
    ///     Gets an amber color: <c>(1|0.75|0)</c>
    /// </summary>
    public static ColorS Amber => new(red: 1f, green: 0.75f, blue: 0f);

    /// <summary>
    ///     Gets an apricot color: <c>(1|0.8|0.65)</c>
    /// </summary>
    public static ColorS Apricot => new(red: 1f, green: 0.8f, blue: 0.65f);

    /// <summary>
    ///     Gets an aquamarine color: <c>(0.5|1|0.85)</c>
    /// </summary>
    public static ColorS Aquamarine => new(red: 0.5f, green: 1f, blue: 0.85f);

    /// <summary>
    ///     Gets a beige color: <c>(0.9|0.9|0.8)</c>
    /// </summary>
    public static ColorS Beige => new(red: 0.9f, green: 0.9f, blue: 0.8f);

    /// <summary>
    ///     Gets a coffee color: <c>(0.45|0.3|0.2)</c>
    /// </summary>
    public static ColorS Coffee => new(red: 0.45f, green: 0.3f, blue: 0.2f);

    /// <summary>
    ///     Gets a coral color: <c>(1|0.5|0.3)</c>
    /// </summary>
    public static ColorS Coral => new(red: 1f, green: 0.5f, blue: 0.3f);

    /// <summary>
    ///     Gets a crimson color: <c>(0.9|0.15|0.3)</c>
    /// </summary>
    public static ColorS Crimson => new(red: 0.9f, green: 0.15f, blue: 0.3f);

    /// <summary>
    ///     Gets an emerald color: <c>(0.3|0.8|0.5)</c>
    /// </summary>
    public static ColorS Emerald => new(red: 0.3f, green: 0.8f, blue: 0.5f);

    /// <summary>
    ///     Gets a lilac color: <c>(0.8|0.6|0.8)</c>
    /// </summary>
    public static ColorS Lilac => new(red: 0.8f, green: 0.6f, blue: 0.8f);

    /// <summary>
    ///     Gets a mauve color: <c>(0.9|0.7|1)</c>
    /// </summary>
    public static ColorS Mauve => new(red: 0.9f, green: 0.7f, blue: 1f);

    /// <summary>
    ///     Gets a periwinkle color: <c>(0.8|0.8|1)</c>
    /// </summary>
    public static ColorS Periwinkle => new(red: 0.8f, green: 0.8f, blue: 1f);

    /// <summary>
    ///     Gets a Prussian blue color: <c>(0|0.2|0.32)</c>
    /// </summary>
    public static ColorS PrussianBlue => new(red: 0f, green: 0.2f, blue: 0.32f);

    /// <summary>
    ///     Gets a medium blue color: <c>(0|0|0.8)</c>
    /// </summary>
    public static ColorS MediumBlue => new(red: 0.0f, green: 0.0f, blue: 0.8f);

    /// <summary>
    ///     Gets a cadet blue color: <c>(0.40|0.60|0.60)</c>
    /// </summary>
    public static ColorS CadetBlue => new(red: 0.40f, green: 0.60f, blue: 0.60f);

    /// <summary>
    ///     Gets an aqua color: <c>(0.35|0.60|0.65)</c>
    /// </summary>
    public static ColorS Aqua => new(red: 0.35f, green: 0.60f, blue: 0.65f);

    /// <summary>
    ///     Gets a slate gray color: <c>(0.5|0.5|0.6)</c>
    /// </summary>
    public static ColorS SlateGray => new(red: 0.5f, green: 0.5f, blue: 0.6f);

    /// <summary>
    ///     Gets a taupe color: <c>(0.3|0.2|0.2)</c>
    /// </summary>
    public static ColorS Taupe => new(red: 0.3f, green: 0.2f, blue: 0.2f);

    /// <summary>
    ///     Gets a viridian color: <c>(0.3|0.5|0.45)</c>
    /// </summary>
    public static ColorS Viridian => new(red: 0.3f, green: 0.5f, blue: 0.45f);

    /// <summary>
    ///     Gets a salmon color: <c>(0.90|0.60|0.55)</c>
    /// </summary>
    public static ColorS Salmon => new(red: 0.90f, green: 0.60f, blue: 0.55f);

    /// <summary>
    ///     Gets a saddle brown color: <c>(0.55|0.25|0.05)</c>
    /// </summary>
    public static ColorS SaddleBrown => new(red: 0.55f, green: 0.25f, blue: 0.05f);

    #endregion PREDEFINED COLORS

    #region EQUALITY

    /// <inheritdoc />
    public Boolean Equals(ColorS other)
    {
        if (IsNeutral && other.IsNeutral)
            return true;

        return R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B) && A.Equals(other.A);
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is ColorS other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return IsNeutral ? 1 : HashCode.Combine(R, G, B, A);
    }

    /// <summary>
    ///     Equality operator.
    /// </summary>
    public static Boolean operator ==(ColorS left, ColorS right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Inequality operator.
    /// </summary>
    public static Boolean operator !=(ColorS left, ColorS right)
    {
        return !left.Equals(right);
    }

    #endregion EQUALITY
}
