// <copyright file="VMath.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     A class containing different mathematical methods and extensions.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "This are public methods for general use.")]
public static class VMath
{
    private const float Epsilon = 128 * float.Epsilon;

    /// <summary>
    ///     A simple one-dimensional range.
    /// </summary>
    /// <param name="x">The exclusive upper bound.</param>
    /// <returns>The range from 0 to x.</returns>
    public static IEnumerable<int> Range(int x)
    {
        for (var i = 0; i < x; i++) yield return i;
    }

    /// <summary>
    ///     A simple two-dimensional range.
    /// </summary>
    /// <param name="x">The exclusive upper bound of the first dimension.</param>
    /// <param name="y">The exclusive upper bound of the second dimension.</param>
    /// <returns>The range from (0, 0) to (x, y).</returns>
    public static IEnumerable<(int, int)> Range2(int x, int y)
    {
        for (var i = 0; i < x; i++)
        for (var j = 0; j < y; j++)
            yield return (i, j);
    }

    /// <summary>
    ///     A simple three-dimensional range.
    /// </summary>
    /// <param name="x">The exclusive upper bound of the first dimension.</param>
    /// <param name="y">The exclusive upper bound of the second dimension.</param>
    /// <param name="z">The exclusive upper bound of the third dimension.</param>
    /// <returns>The range from (0, 0, 0) to (x, y, z).</returns>
    public static IEnumerable<(int, int, int)> Range3(int x, int y, int z)
    {
        for (var i = 0; i < x; i++)
        for (var j = 0; j < y; j++)
        for (var k = 0; k < z; k++)
            yield return (i, j, k);
    }

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
    ///     Returns a copy of the vector where every component is positive.
    /// </summary>
    /// <param name="vector">The vector of which an absolute vector should be created.</param>
    /// <returns>The absolute vector.</returns>
    public static Vector3i Absolute(this Vector3i vector)
    {
        return new Vector3i(Math.Abs(vector.X), Math.Abs(vector.Y), Math.Abs(vector.Z));
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
    ///     Get a vector as a tuple.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    /// <returns>The tuple.</returns>
    public static (int x, int y, int z) ToTuple(this Vector3i vector)
    {
        return (vector.X, vector.Y, vector.Z);
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
            RoundedToInt(vector.X, midpointRounding),
            RoundedToInt(vector.Y, midpointRounding),
            RoundedToInt(vector.Z, midpointRounding));
    }

    /// <summary>
    ///     Rounds a double to an integer.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="midpointRounding">The midpoint rounding behaviour.</param>
    /// <returns>The rounded value.</returns>
    public static int RoundedToInt(double value, MidpointRounding midpointRounding = MidpointRounding.ToEven)
    {
        return (int) Math.Round(value, digits: 0, midpointRounding);
    }

    /// <summary>
    ///     Rounds a double to an unsigned integer.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="midpointRounding">The midpoint rounding behaviour.</param>
    /// <returns>The rounded value.</returns>
    public static uint RoundedToUInt(double value, MidpointRounding midpointRounding = MidpointRounding.ToEven)
    {
        return (uint) Math.Round(value, digits: 0, midpointRounding);
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
    ///     Check if two floating-point vector values are nearly equal.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <param name="epsilon">The epsilon value, defining what difference is seen as equal.</param>
    /// <returns>True if the two vectors are nearly equal.</returns>
    public static bool NearlyEqual(Vector3d a, Vector3d b, double epsilon = Epsilon)
    {
        return NearlyEqual(a.X, b.X, epsilon) && NearlyEqual(a.Y, b.Y, epsilon) && NearlyEqual(a.Z, b.Z, epsilon);
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
    ///     Perform a bilinear interpolation between four values, using two factors. The factors must be in the range [0, 1].
    /// </summary>
    public static double BiLerp(double f00, double f10, double f01, double f11, double tx, double ty)
    {
        return MathHelper.Lerp(MathHelper.Lerp(f00, f10, tx), MathHelper.Lerp(f01, f11, tx), ty);
    }

    /// <summary>
    ///     Get the gradient of the bilinear interpolation function. The factors must be in the range [0, 1].
    /// </summary>
    public static Vector2d GradBiLerp(double f00, double f10, double f01, double f11, double tx, double ty)
    {
        // bilerp: f(tx, ty) = (1 - tx) * (1 - ty) * f00 + tx * (1 - ty) * f10 + (1 - tx) * ty * f01 + tx * ty * f11

        double fx = (1 - ty) * (f10 - f00) + ty * (f11 - f01);
        double fy = (1 - tx) * (f01 - f00) + tx * (f11 - f10);

        return new Vector2d(fx, fy);
    }

    /// <summary>
    ///     Perform a bilinear interpolation between four values and then lerp between the result and a fifth value.
    /// </summary>
    public static double MixingBilinearInterpolation(double f00, double f10, double f01, double f11, double fZ, Vector3d t)
    {
        return MathHelper.Lerp(BiLerp(f00, f10, f01, f11, t.X, t.Y), fZ, t.Z);
    }

    /// <summary>
    ///     Select from four values using two weights. If elements are equal, their weights are combined.
    /// </summary>
    public static ref readonly T SelectByWeight<T>(in T e00, in T e10, in T e01, in T e11, Vector2d weights)
    {
        double w00 = (1 - weights.X) * (1 - weights.Y);
        double w10 = weights.X * (1 - weights.Y);
        double w01 = (1 - weights.X) * weights.Y;
        double w11 = weights.X * weights.Y;

        double GetWeight(in T e, in T e00, in T e10, in T e01, in T e11)
        {
            double weight = 0;

            if (EqualityComparer<T>.Default.Equals(e, e00)) weight += w00;
            if (EqualityComparer<T>.Default.Equals(e, e10)) weight += w10;
            if (EqualityComparer<T>.Default.Equals(e, e01)) weight += w01;
            if (EqualityComparer<T>.Default.Equals(e, e11)) weight += w11;

            return weight;
        }

        Span<double> totalWeights = stackalloc double[]
        {
            GetWeight(e00, e00, e10, e01, e11),
            GetWeight(e10, e00, e10, e01, e11),
            GetWeight(e01, e00, e10, e01, e11),
            GetWeight(e11, e00, e10, e01, e11)
        };

        var indexOfMax = 1;

        for (var index = 0; index < totalWeights.Length; index++)
            if (totalWeights[index] > totalWeights[indexOfMax])
                indexOfMax = index;

        switch (indexOfMax)
        {
            case 0:
                return ref e00;
            case 1:
                return ref e10;
            case 2:
                return ref e01;
            case 3:
                return ref e11;

            default:
                throw new NotSupportedException();
        }
    }

    /// <summary>
    ///     Select from two values using one weight.
    /// </summary>
    public static ref readonly T SelectByWeight<T>(in T e0, in T e1, double w)
    {
        if (w < 0.5) return ref e0;

        return ref e1;
    }


    /// <summary>
    ///     Select from five values using three weights.
    /// </summary>
    public static ref readonly T SelectByWeight<T>(in T e00, in T e10, in T e01, in T e11, in T eZ, Vector3d weights)
    {
        return ref SelectByWeight(SelectByWeight(e00, e10, e01, e11, weights.Xy), eZ, weights.Z);
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
    public static (T min, T max) MinMax<T>(T a, T b) where T : IComparable<T>
    {
        return a.CompareTo(b) < 0 ? (a, b) : (b, a);
    }

    /// <summary>
    ///     Get a tuple with the minimum argument first and the maximum argument second.
    /// </summary>
    /// <param name="first">The first value-argument pair.</param>
    /// <param name="second">The second value-argument pair.</param>
    /// <typeparam name="TV">The type of the value. It will be used to compare the values.</typeparam>
    /// <typeparam name="TA">The type of the argument.</typeparam>
    /// <returns>A tuple with the minimum argument first and the maximum argument second.</returns>
    public static (TA min, TA max) ArgMinMax<TV, TA>((TV value, TA argument) first, (TV value, TA argument) second)
        where TV : IComparable<TV>
    {
        return first.value.CompareTo(second.value) < 0 ? (first.argument, second.argument) : (second.argument, first.argument);
    }

    /// <summary>
    ///     Get the minimum of four values.
    /// </summary>
    public static double Min(float a, float b, float c, float d)
    {
        return Math.Min(Math.Min(a, b), Math.Min(c, d));
    }

    /// <summary>
    ///     Get the maximum of four values.
    /// </summary>
    public static double Max(float a, float b, float c, float d)
    {
        return Math.Max(Math.Max(a, b), Math.Max(c, d));
    }

    /// <summary>
    ///     Get the maximum component of a vector.
    /// </summary>
    public static int MaxComponent(this Vector3i v)
    {
        return Math.Max(Math.Max(v.X, v.Y), v.Z);
    }

    /// <summary>
    ///     Get the maximum component of a vector.
    /// </summary>
    public static int MaxComponent(this Vector2i v)
    {
        return Math.Max(v.X, v.Y);
    }

    /// <summary>
    ///     Get the maximum component of a vector.
    /// </summary>
    public static float MaxComponent(this Vector3 v)
    {
        return Math.Max(Math.Max(v.X, v.Y), v.Z);
    }

    /// <summary>
    ///     Get the maximum component of a vector.
    /// </summary>
    public static float MaxComponent(this Vector4 v)
    {
        return Math.Max(Math.Max(Math.Max(v.X, v.Y), v.Z), v.W);
    }

    /// <summary>
    ///     Get the minimum component of a vector.
    /// </summary>
    public static float MinComponent(this Vector4 v)
    {
        return Math.Min(Math.Min(Math.Min(v.X, v.Y), v.Z), v.W);
    }

    /// <summary>
    ///     Get the corner of a box by its index.
    /// </summary>
    /// <param name="box">The box.</param>
    /// <param name="index">The index of the corner, in the range [0, 7].</param>
    /// <returns>The corner.</returns>
    public static Vector3d GetCorner(this Box3d box, int index)
    {
        Debug.Assert(index is >= 0 and < 8);

        return new Vector3d(
            index % 2 == 0 ? box.Min.X : box.Max.X,
            index / 2 % 2 == 0 ? box.Min.Y : box.Max.Y,
            index / 4 % 2 == 0 ? box.Min.Z : box.Max.Z
        );
    }

    /// <summary>
    ///     Simply gets the square of a number.
    /// </summary>
    public static int Square(int x)
    {
        return x * x;
    }

    /// <summary>
    ///     Simply gets the cube of a number.
    /// </summary>
    public static int Cube(int x)
    {
        return x * x * x;
    }
}
