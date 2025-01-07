// <copyright file="Conversion.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     A utility class for different conversion methods.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "This are public methods for general use.")]
public static class Conversion
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
    ///     Converts a <see cref="Color4" /> to a <see cref="Vector3" />.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The vector.</returns>
    public static Vector3 ToVector3(this Color4 color)
    {
        return new Vector3(color.R, color.G, color.B);
    }

    /// <summary>
    ///     Convert a double Vector3 to a float Vector3.
    /// </summary>
    public static Vector3 ToVector3(this Vector3d vector)
    {
        return new Vector3((Single) vector.X, (Single) vector.Y, (Single) vector.Z);
    }

    /// <summary>
    ///     Converts a <see cref="Vector4" /> to a <see cref="Color" />.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    /// <returns>The color.</returns>
    public static Color ToColor(this Vector4d vector)
    {
        return (Color) vector.ToColor4();
    }

    /// <summary>
    ///     Converts a <see cref="Vector4" /> to a <see cref="Color4" />.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    /// <returns>The color.</returns>
    private static Color4 ToColor4(this Vector4d vector)
    {
        return new Color4((Single) vector.X, (Single) vector.Y, (Single) vector.Z, (Single) vector.W);
    }

    /// <summary>
    ///     Converts a <see cref="Color" /> to a <see cref="Vector4" />.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>The vector.</returns>
    public static Vector4 ToVector4(this Color color)
    {
        return (Vector4) (Color4) color;
    }

    /// <summary>
    ///     Convert a double Vector4 to a float Vector4.
    /// </summary>
    public static Vector4 ToVector4(this Vector4d vector)
    {
        return new Vector4((Single) vector.X, (Single) vector.Y, (Single) vector.Z, (Single) vector.W);
    }

    /// <summary>
    ///     Convert a double Vector2 to a float Vector3.
    /// </summary>
    public static Vector2 ToVector2(this Vector2d vector)
    {
        return new Vector2((Single) vector.X, (Single) vector.Y);
    }

    /// <summary>
    ///     Convert a int Vector3 to a double Vector3.
    /// </summary>
    public static Vector3d ToVector3d(this Vector3i vector)
    {
        return new Vector3d(vector.X, vector.Y, vector.Z);
    }

    /// <summary>
    ///     Convert a double Matrix4 to a float Matrix4.
    /// </summary>
    public static Matrix4 ToMatrix4(this Matrix4d matrix)
    {
        return new Matrix4(matrix.Row0.ToVector4(), matrix.Row1.ToVector4(), matrix.Row2.ToVector4(), matrix.Row3.ToVector4());
    }

    /// <summary>
    ///     Get a vector as a tuple.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    /// <returns>The tuple.</returns>
    public static (Int32 x, Int32 y, Int32 z) ToTuple(this Vector3i vector)
    {
        return (vector.X, vector.Y, vector.Z);
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
    ///     Converts a <see cref="Vector2" /> to a <see cref="Vector2i" />.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    /// <returns>The vector.</returns>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static Vector2i ToVector2i(this Vector2 vector)
    {
        return new Vector2i((Int32) vector.X, (Int32) vector.Y);
    }
}
