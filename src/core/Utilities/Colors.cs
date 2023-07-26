// <copyright file="Colors.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Drawing;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Extension methods and other utilities for colors.
/// </summary>
public static class Colors
{
    /// <summary>
    ///     Check if a given color has any opaqueness, meaning the alpha channel is not 0.
    /// </summary>
    public static bool HasOpaqueness(this Color color)
    {
        return color.A != 0;
    }

    /// <summary>
    ///     Mix two colors.
    /// </summary>
    /// <param name="a">The first color.</param>
    /// <param name="b">The second color.</param>
    /// <param name="f">The mixing factor for linear interpolation.</param>
    public static Color Mix(Color a, Color b, double f = 0.5)
    {
        return Color.FromArgb(
            (int) MathHelper.Lerp(a.A, b.A, f),
            (int) MathHelper.Lerp(a.R, b.R, f),
            (int) MathHelper.Lerp(a.G, b.G, f),
            (int) MathHelper.Lerp(a.B, b.B, f));
    }

    /// <summary>
    ///     Create a color from floating-point RGB values. The values will be clamped to the range [0, 1].
    /// </summary>
    /// <param name="r">The red value.</param>
    /// <param name="g">The green value.</param>
    /// <param name="b">The blue value.</param>
    /// <returns>The color.</returns>
    public static Color FromRGB(float r, float g, float b)
    {
        return Color.FromArgb(
            alpha: 255,
            (int) (MathHelper.Clamp(r, min: 0, max: 1) * 255),
            (int) (MathHelper.Clamp(g, min: 0, max: 1) * 255),
            (int) (MathHelper.Clamp(b, min: 0, max: 1) * 255));
    }
}
