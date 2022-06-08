﻿// <copyright file="VMath.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     A class containing different mathematical methods and extensions.
/// </summary>
public static class VMath
{
    private const float Epsilon = 128 * float.Epsilon;

    /// <summary>
    ///     Clamps a vector between a minimum and maximum length.
    /// </summary>
    /// <param name="vector">The vector to clamp.</param>
    /// <param name="min">The minimum length.</param>
    /// <param name="max">The maximum length.</param>
    /// <returns>The clamped vector.</returns>
    public static Vector3 Clamp(Vector3 vector, float min, float max)
    {
        float length = vector.Length;

        if (length < min) return vector.Normalized() * min;
        if (length > max) return vector.Normalized() * max;

        return vector;
    }

    /// <summary>
    ///     Returns a copy of the vector where every component is positive.
    /// </summary>
    /// <param name="vector">The vector of which an absolute vector should be created.</param>
    /// <returns>The absolute vector.</returns>
    public static Vector3 Absolute(this Vector3 vector)
    {
        return new Vector3(Math.Abs(vector.X), Math.Abs(vector.Y), Math.Abs(vector.Z));
    }

    /// <summary>
    ///     Rounds every component of a vector.
    /// </summary>
    /// <param name="vector">The vector to round.</param>
    /// <param name="digits">The number of fractional digits in the return value.</param>
    /// <param name="midpointRounding">The midpoint rounding behaviour.</param>
    /// <returns>The rounded vector.</returns>
    public static Vector3 Rounded(this Vector3 vector, int digits = 0,
        MidpointRounding midpointRounding = MidpointRounding.ToEven)
    {
        return new Vector3(
            (float) Math.Round(vector.X, digits, midpointRounding),
            (float) Math.Round(vector.Y, digits, midpointRounding),
            (float) Math.Round(vector.Z, digits, midpointRounding));
    }

    /// <summary>
    ///     Clamps every component of a vector.
    /// </summary>
    /// <param name="vector">The vector to clamp.</param>
    /// <param name="min">The minimum values for each component.</param>
    /// <param name="max">The maximum values for each component.</param>
    /// <returns>The vector with clamped components</returns>
    public static Vector3 ClampComponents(Vector3 vector, Vector3 min, Vector3 max)
    {
        return new Vector3(
            MathHelper.Clamp(vector.X, min.X, max.X),
            MathHelper.Clamp(vector.Y, min.Y, max.Y),
            MathHelper.Clamp(vector.Z, min.Z, max.Z));
    }

    /// <summary>
    ///     Returns a vector where every component is the sign of the original component.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    /// <returns>The sign vector</returns>
    public static Vector3i Sign(this Vector3 vector)
    {
        return new Vector3i(Math.Sign(vector.X), Math.Sign(vector.Y), Math.Sign(vector.Z));
    }

    /// <summary>
    ///     Returns a vector where every component is the modulo of m.
    /// </summary>
    /// <param name="vector">The vector to use.</param>
    /// <param name="m">The number m.</param>
    /// <returns>The modulo vector.</returns>
    public static Vector3i Mod(this Vector3i vector, int m)
    {
        return new Vector3i(
            (vector.X % m + m) % m,
            (vector.Y % m + m) % m,
            (vector.Z % m + m) % m);
    }

    /// <summary>
    ///     Returns a floored vector of a given vector.
    /// </summary>
    /// <param name="vector">The vector to floor.</param>
    /// <returns>The component-wise floored vector.</returns>
    public static Vector3i Floor(this Vector3 vector)
    {
        return new Vector3i((int) Math.Floor(vector.X), (int) Math.Floor(vector.Y), (int) Math.Floor(vector.Z));
    }

    /// <summary>
    ///     Get the position below a given position.
    /// </summary>
    public static Vector3i Below(this Vector3i vector)
    {
        return vector - Vector3i.UnitY;
    }

    /// <summary>
    ///     Get the position below a given position, with a given offset.
    /// </summary>
    public static Vector3i Below(this Vector3i vector, int offset)
    {
        return vector - Vector3i.UnitY * offset;
    }

    /// <summary>
    ///     Get the position above a given position.
    /// </summary>
    public static Vector3i Above(this Vector3i vector)
    {
        return vector + Vector3i.UnitY;
    }

    /// <summary>
    ///     Clamp a value between two values. If the given value is outside of the range, it will be clamped to the limit on
    ///     the other side.
    /// </summary>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The lower end of the range. (inclusive)</param>
    /// <param name="max">The upper end of the range. (exclusive)</param>
    /// <returns>A value in the given range.</returns>
    public static long ClampRotating(long value, long min, long max)
    {
        Debug.Assert(min < max);

        if (value >= max) return min;

        if (value < min) return max - 1;

        return value;
    }

    /// <summary>
    ///     Check if two floating-point values are nearly equal.
    /// </summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <param name="epsilon">The epsilon value, defining what difference is seen as equal.</param>
    /// <returns>True if the two values are nearly equal.</returns>
    public static bool NearlyEqual(float a, float b, float epsilon = Epsilon)
    {
        return Math.Abs(a - b) < epsilon;
    }

    /// <summary>
    ///     Check near equality to zero.
    /// </summary>
    /// <param name="a">The value to check for near equality with zero.</param>
    /// <param name="epsilon">The epsilon distance.</param>
    /// <returns>True if the given value is nearly zero.</returns>
    public static bool NearlyZero(float a, float epsilon = Epsilon)
    {
        return NearlyEqual(a, b: 0, epsilon);
    }

    /// <summary>
    ///     Create a box from a center point and the extents.
    /// </summary>
    /// <param name="center">The center point.</param>
    /// <param name="extents">The extents of the box, which are also half of the box size.</param>
    /// <returns>The created box.</returns>
    public static Box3 CreateBox3(Vector3 center, Vector3 extents)
    {
        return new Box3(center - extents, center + extents);
    }

    /// <summary>
    ///     Given two points and a value, calculate the lerp factor to produce the value.
    /// </summary>
    public static float InverseLerp(float a, float b, float value)
    {
        return (value - a) / (b - a);
    }
}