// <copyright file="VMath.cs" company="VoxelGame">
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
    public static Vector3d Clamp(Vector3d vector, double min, double max)
    {
        double length = vector.Length;

        if (length < min) return vector.Normalized() * min;
        if (length > max) return vector.Normalized() * max;

        return vector;
    }

    /// <summary>
    ///     Returns a copy of the vector where every component is positive.
    /// </summary>
    /// <param name="vector">The vector of which an absolute vector should be created.</param>
    /// <returns>The absolute vector.</returns>
    public static Vector3d Absolute(this Vector3d vector)
    {
        return new Vector3d(Math.Abs(vector.X), Math.Abs(vector.Y), Math.Abs(vector.Z));
    }

    /// <summary>
    ///     Convert a double Vector3 to a float Vector3.
    /// </summary>
    public static Vector3 ToVector3(this Vector3d vector)
    {
        return new Vector3((float) vector.X, (float) vector.Y, (float) vector.Z);
    }

    /// <summary>
    ///     Convert a double Vector4 to a float Vector4.
    /// </summary>
    public static Vector4 ToVector4(this Vector4d vector)
    {
        return new Vector4((float) vector.X, (float) vector.Y, (float) vector.Z, (float) vector.W);
    }

    /// <summary>
    ///     Convert a double Vector2 to a float Vector3.
    /// </summary>
    public static Vector2 ToVector2(this Vector2d vector)
    {
        return new Vector2((float) vector.X, (float) vector.Y);
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
    ///     Creates a scale matrix.
    /// </summary>
    public static Matrix4d CreateScaleMatrix(Vector3d scale)
    {
        Matrix4d result = Matrix4d.Identity;
        result.Row0.X = scale.X;
        result.Row1.Y = scale.Y;
        result.Row2.Z = scale.Z;

        return result;
    }

    /// <summary>
    ///     Rounds every component of a vector.
    /// </summary>
    /// <param name="vector">The vector to round.</param>
    /// <param name="digits">The number of fractional digits in the return value.</param>
    /// <param name="midpointRounding">The midpoint rounding behaviour.</param>
    /// <returns>The rounded vector.</returns>
    public static Vector3d Rounded(this Vector3d vector, int digits = 0,
        MidpointRounding midpointRounding = MidpointRounding.ToEven)
    {
        return new Vector3d(
            Math.Round(vector.X, digits, midpointRounding),
            Math.Round(vector.Y, digits, midpointRounding),
            Math.Round(vector.Z, digits, midpointRounding));
    }

    /// <summary>
    ///     Rounds every component to an integer.
    /// </summary>
    /// <param name="vector">The vector to round.</param>
    /// <param name="midpointRounding">The midpoint rounding behaviour.</param>
    /// <returns>The rounded vector.</returns>
    public static Vector3i RoundedToInt(this Vector3d vector, MidpointRounding midpointRounding = MidpointRounding.ToEven)
    {
        return new Vector3i(
            (int) Math.Round(vector.X, digits: 0, midpointRounding),
            (int) Math.Round(vector.Y, digits: 0, midpointRounding),
            (int) Math.Round(vector.Z, digits: 0, midpointRounding));
    }

    /// <summary>
    ///     Clamps every component of a vector.
    /// </summary>
    /// <param name="vector">The vector to clamp.</param>
    /// <param name="min">The minimum values for each component.</param>
    /// <param name="max">The maximum values for each component.</param>
    /// <returns>The vector with clamped components.</returns>
    public static Vector3d ClampComponents(Vector3d vector, Vector3d min, Vector3d max)
    {
        return new Vector3d(
            MathHelper.Clamp(vector.X, min.X, max.X),
            MathHelper.Clamp(vector.Y, min.Y, max.Y),
            MathHelper.Clamp(vector.Z, min.Z, max.Z));
    }

    /// <summary>
    ///     Clamps every component of a vector.
    /// </summary>
    /// <param name="vector">The vector to clamp.</param>
    /// <param name="min">The minimum values for each component.</param>
    /// <param name="max">The maximum values for each component.</param>
    /// <returns>The vector with clamped components.</returns>
    public static Vector3i ClampComponents(Vector3i vector, Vector3i min, Vector3i max)
    {
        return new Vector3i(
            MathHelper.Clamp(vector.X, min.X, max.X),
            MathHelper.Clamp(vector.Y, min.Y, max.Y),
            MathHelper.Clamp(vector.Z, min.Z, max.Z));
    }

    /// <summary>
    ///     Returns a vector where every component is the sign of the original component.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    /// <returns>The sign vector.</returns>
    public static Vector3i Sign(this Vector3d vector)
    {
        return new Vector3i(Math.Sign(vector.X), Math.Sign(vector.Y), Math.Sign(vector.Z));
    }

    /// <summary>
    ///     Returns a vector where every component is the sign of the original component.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    /// <returns>The sign vector.</returns>
    public static Vector2i Sign(this Vector2d vector)
    {
        return new Vector2i(Math.Sign(vector.X), Math.Sign(vector.Y));
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
    public static Vector3i Floor(this Vector3d vector)
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
    public static bool NearlyEqual(double a, double b, double epsilon = Epsilon)
    {
        return Math.Abs(a - b) < epsilon;
    }

    /// <summary>
    ///     Check near equality to zero.
    /// </summary>
    /// <param name="a">The value to check for near equality with zero.</param>
    /// <param name="epsilon">The epsilon distance.</param>
    /// <returns>True if the given value is nearly zero.</returns>
    public static bool NearlyZero(double a, double epsilon = Epsilon)
    {
        return NearlyEqual(a, b: 0, epsilon);
    }

    /// <summary>
    ///     Create a vector from a given angle.
    /// </summary>
    /// <param name="angle">The angle, in radians.</param>
    /// <returns>The vector.</returns>
    public static Vector2d CreateVectorFromAngle(double angle)
    {
        return new Vector2d(Math.Cos(angle), Math.Sin(angle));
    }

    /// <summary>
    ///     Create a box from a center point and the extents.
    /// </summary>
    /// <param name="center">The center point.</param>
    /// <param name="extents">The extents of the box, which are also half of the box size.</param>
    /// <returns>The created box.</returns>
    public static Box3d CreateBox3(Vector3d center, Vector3d extents)
    {
        return new Box3d(center - extents, center + extents);
    }

    /// <summary>
    ///     Given two points and a value, calculate the lerp factor to produce the value.
    /// </summary>
    public static double InverseLerp(double a, double b, double value)
    {
        return (value - a) / (b - a);
    }

    /// <summary>
    ///     Perform a bilinear interpolation between four values, using two factors.
    /// </summary>
    public static double Blerp(double f00, double f10, double f01, double f11, double tx, double ty)
    {
        return MathHelper.Lerp(MathHelper.Lerp(f00, f10, tx), MathHelper.Lerp(f01, f11, tx), ty);
    }

    /// <summary>
    ///     Calculates the angle between two vectors.
    /// </summary>
    public static double CalculateAngle(Vector2d a, Vector2d b)
    {
        return Math.Acos(Vector2d.Dot(a, b) / (a.Length * b.Length));
    }

    /// <summary>
    ///     Get a tuple with the minimum value first and the maximum value second.
    /// </summary>
    public static (T, T) MinMax<T>(T a, T b) where T : IComparable<T>
    {
        return a.CompareTo(b) < 0 ? (a, b) : (b, a);
    }
}
