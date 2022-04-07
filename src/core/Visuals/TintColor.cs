// <copyright file="TintColor.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     A tint that can be applied to blocks.
/// </summary>
public readonly struct TintColor : IEquatable<TintColor>
{
    private readonly float r;
    private readonly float g;
    private readonly float b;

    /// <summary>
    ///     Whether the tint is neutral. It will then be replaced with a tint defined externally.
    /// </summary>
    public bool IsNeutral { get; }

    /// <summary>
    ///     Create a new tint color.
    /// </summary>
    /// <param name="r">The red value, in the range [0, 1].</param>
    /// <param name="g">The green value, in the range [0, 1].</param>
    /// <param name="b">The blue value, in the range [0, 1].</param>
    public TintColor(float r, float g, float b)
    {
        this.r = r;
        this.g = g;
        this.b = b;

        IsNeutral = false;
    }

    private TintColor(float r, float g, float b, bool isNeutral)
    {
        this.r = r;
        this.g = g;
        this.b = b;

        IsNeutral = isNeutral;
    }

    /// <summary>
    ///     Create a tint color that indicates no tint.
    /// </summary>
    public static TintColor None => new(r: 1f, g: 1f, b: 1f);

    /// <summary>
    ///     Create a tint color that indicates neutral tint.
    /// </summary>
    public static TintColor Neutral => new(r: 0f, g: 0f, b: 0f, isNeutral: true);

    #region PREDEFINED COLORS

    /// <summary>
    ///     Gets a white color: <c>(1|1|1)</c>
    /// </summary>
    public static TintColor White => new(r: 1f, g: 1f, b: 1f);

    /// <summary>
    ///     Gets a red color: <c>(1|0|0)</c>
    /// </summary>
    public static TintColor Red => new(r: 1f, g: 0f, b: 0f);

    /// <summary>
    ///     Gets a green color: <c>(0|1|0)</c>
    /// </summary>
    public static TintColor Green => new(r: 0f, g: 1f, b: 0f);

    /// <summary>
    ///     Gets a blue color: <c>(0|0|1)</c>
    /// </summary>
    public static TintColor Blue => new(r: 0f, g: 0f, b: 1f);

    /// <summary>
    ///     Gets a yellow color: <c>(1|1|0)</c>
    /// </summary>
    public static TintColor Yellow => new(r: 1f, g: 1f, b: 0f);

    /// <summary>
    ///     Gets a cyan color: <c>(0|1|1)</c>
    /// </summary>
    public static TintColor Cyan => new(r: 0f, g: 1f, b: 1f);

    /// <summary>
    ///     Gets a magenta color: <c>(1|0|1)</c>
    /// </summary>
    public static TintColor Magenta => new(r: 1f, g: 0f, b: 1f);

    /// <summary>
    ///     Gets an orange color: <c>(1|0.5|0)</c>
    /// </summary>
    public static TintColor Orange => new(r: 1f, g: 0.5f, b: 0f);

    /// <summary>
    ///     Gets a dark green color: <c>(0|0.5|0)</c>
    /// </summary>
    public static TintColor DarkGreen => new(r: 0f, g: 0.5f, b: 0f);

    /// <summary>
    ///     Gets a lime color: <c>(0.75|1|0)</c>
    /// </summary>
    public static TintColor Lime => new(r: 0.75f, g: 1f, b: 0f);

    /// <summary>
    ///     Gets a gray color: <c>(0.15|0.15|0.15)</c>
    /// </summary>
    public static TintColor Gray => new(r: 0.15f, g: 0.15f, b: 0.15f);

    /// <summary>
    ///     Gets a light color: <c>(0.8|0.8|0.8)</c>
    /// </summary>
    public static TintColor LightGray => new(r: 0.8f, g: 0.8f, b: 0.8f);

    /// <summary>
    ///     Gets an indigo color: <c>(0.5|1|0)</c>
    /// </summary>
    public static TintColor Indigo => new(r: 0.5f, g: 1f, b: 0f);

    /// <summary>
    ///     Gets a maroon color: <c>(0.5|0|0)</c>
    /// </summary>
    public static TintColor Maroon => new(r: 0.5f, g: 0f, b: 0f);

    /// <summary>
    ///     Gets an olive color: <c>(0.5|0.5|0)</c>
    /// </summary>
    public static TintColor Olive => new(r: 0.5f, g: 0.5f, b: 0f);

    /// <summary>
    ///     Gets a brown color: <c>(0.5|0.25|0)</c>
    /// </summary>
    public static TintColor Brown => new(r: 0.5f, g: 0.25f, b: 0f);

    /// <summary>
    ///     Gets a navy color: <c>(0|0|0.5)</c>
    /// </summary>
    public static TintColor Navy => new(r: 0f, g: 0f, b: 0.5f);

    /// <summary>
    ///     Gets an amaranth color: <c>(0.9|0.2|0.3)</c>
    /// </summary>
    public static TintColor Amaranth => new(r: 0.9f, g: 0.2f, b: 0.3f);

    /// <summary>
    ///     Gets an amber color: <c>(1|0.75|0)</c>
    /// </summary>
    public static TintColor Amber => new(r: 1f, g: 0.75f, b: 0f);

    /// <summary>
    ///     Gets an apricot color: <c>(1|0.8|0.65)</c>
    /// </summary>
    public static TintColor Apricot => new(r: 1f, g: 0.8f, b: 0.65f);

    /// <summary>
    ///     Gets an aquamarine color: <c>(0.5|1|0.85)</c>
    /// </summary>
    public static TintColor Aquamarine => new(r: 0.5f, g: 1f, b: 0.85f);

    /// <summary>
    ///     Gets a beige color: <c>(0.9|0.9|0.8)</c>
    /// </summary>
    public static TintColor Beige => new(r: 0.9f, g: 0.9f, b: 0.8f);

    /// <summary>
    ///     Gets a coffee color: <c>(0.45|0.3|0.2)</c>
    /// </summary>
    public static TintColor Coffee => new(r: 0.45f, g: 0.3f, b: 0.2f);

    /// <summary>
    ///     Gets a coral color: <c>(1|0.5|0.3)</c>
    /// </summary>
    public static TintColor Coral => new(r: 1f, g: 0.5f, b: 0.3f);

    /// <summary>
    ///     Gets a crimson color: <c>(0.9|0.15|0.3)</c>
    /// </summary>
    public static TintColor Crimson => new(r: 0.9f, g: 0.15f, b: 0.3f);

    /// <summary>
    ///     Gets an emerald color: <c>(0.3|0.8|0.5)</c>
    /// </summary>
    public static TintColor Emerald => new(r: 0.3f, g: 0.8f, b: 0.5f);

    /// <summary>
    ///     Gets a lilac color: <c>(0.8|0.6|0.8)</c>
    /// </summary>
    public static TintColor Lilac => new(r: 0.8f, g: 0.6f, b: 0.8f);

    /// <summary>
    ///     Gets a mauve color: <c>(0.9|0.7|1)</c>
    /// </summary>
    public static TintColor Mauve => new(r: 0.9f, g: 0.7f, b: 1f);

    /// <summary>
    ///     Gets a periwinkle color: <c>(0.8|0.8|1)</c>
    /// </summary>
    public static TintColor Periwinkle => new(r: 0.8f, g: 0.8f, b: 1f);

    /// <summary>
    ///     Gets a Prussian blue color: <c>(0|0.2|0.32)</c>
    /// </summary>
    public static TintColor PrussianBlue => new(r: 0f, g: 0.2f, b: 0.32f);

    /// <summary>
    ///     Gets a slate gray color: <c>(0.5|0.5|0.6)</c>
    /// </summary>
    public static TintColor SlateGray => new(r: 0.5f, g: 0.5f, b: 0.6f);

    /// <summary>
    ///     Gets a taupe color: <c>(0.3|0.2|0.2)</c>
    /// </summary>
    public static TintColor Taupe => new(r: 0.3f, g: 0.2f, b: 0.2f);

    /// <summary>
    ///     Gets a viridian color: <c>(0.3|0.5|0.45)</c>
    /// </summary>
    public static TintColor Viridian => new(r: 0.3f, g: 0.5f, b: 0.45f);

    #endregion PREDEFINED COLORS

    private int ToBits => ((int) (r * 7f) << 6) | ((int) (g * 7f) << 3) | (int) (b * 7f);

    /// <summary>
    ///     Get the tint encoded into bits.
    /// </summary>
    /// <param name="neutral">The tint color to use as for neutral tint.</param>
    /// <returns>The tint bits.</returns>
    public int GetBits(TintColor neutral)
    {
        return IsNeutral ? neutral.ToBits : ToBits;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is TintColor other) return Equals(other);

        return false;
    }

    /// <inheritdoc />
    public bool Equals(TintColor other)
    {
        return ToBits == other.ToBits && other.IsNeutral == IsNeutral;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return (IsNeutral ? 1 : 0 << 9) | ((int) (r * 7f) << 6) | ((int) (g * 7f) << 3) | (int) (b * 7f);
    }

    /// <summary>
    ///     Compare two tints for equality.
    /// </summary>
    public static bool operator ==(TintColor left, TintColor right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Compare two tints for inequality.
    /// </summary>
    public static bool operator !=(TintColor left, TintColor right)
    {
        return !(left == right);
    }
}
