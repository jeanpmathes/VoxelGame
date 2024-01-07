// <copyright file="Conversion.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     A utility class for different conversion methods.
/// </summary>
public static class Conversion // todo: check if there are conversions in VMath and move them here
{
    /// <summary>
    ///     Converts a <see cref="Color" /> to a <see cref="Vector3" />.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The vector.</returns>
    public static Vector3 ToVector3(this Color color)
    {
        return color.ToVector4().Xyz;
    }

    /// <summary>
    ///     Converts a <see cref="Color" /> to a <see cref="Vector4" />.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The vector.</returns>
    private static Vector4 ToVector4(this Color color)
    {
        return (Vector4) (Color4) color;
    }

    /// <summary>
    ///     Converts a <see cref="Color4" /> to a <see cref="Vector3" />.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The vector.</returns>
    public static Vector3 ToVector3(this Color4 color)
    {
        return new Vector3(color.R, color.G, color.B);
    }

    /// <summary>
    ///     Converts a <see cref="Size" /> to a <see cref="Vector2i" />.
    /// </summary>
    /// <param name="size">The size to convert.</param>
    /// <returns>The vector.</returns>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static Vector2i ToVector2i(this Size size)
    {
        return new Vector2i(size.Width, size.Height);
    }

    /// <summary>
    ///     Convert a bool to an int.
    /// </summary>
    public static int ToInt(this bool b)
    {
        return b ? 1 : 0;
    }

    /// <summary>
    ///     Convert a bool to an uint.
    /// </summary>
    public static uint ToUInt(this bool b)
    {
        return b ? 1u : 0u;
    }
}
