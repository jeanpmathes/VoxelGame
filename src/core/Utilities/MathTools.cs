﻿// <copyright file="MathTools.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;
using VoxelGame.Toolkit.Utilities;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;
using Vector4 = OpenTK.Mathematics.Vector4;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     A class containing different mathematical methods and extensions.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "This are public methods for general use.")]
public static class MathTools
{
    private const Single Epsilon = 128 * Single.Epsilon;

    /// <summary>
    ///     Get a sequence of random integers.
    /// </summary>
    /// <param name="generator">The random number generator.</param>
    /// <param name="count">The number of integers to generate.</param>
    /// <param name="min">The inclusive lower bound of the integers.</param>
    /// <param name="max">The exclusive upper bound of the integers.</param>
    /// <returns>The sequence of random integers.</returns>
    public static IEnumerable<Int32> Random(Random generator, Int32 count, Int32 min = Int32.MinValue, Int32 max = Int32.MaxValue)
    {
        for (var i = 0; i < count; i++)
            yield return generator.Next(min, max);
    }

    /// <summary>
    ///     Get a sequence of random vectors.
    /// </summary>
    /// <param name="generator">The random number generator.</param>
    /// <param name="count">The number of vectors to generate.</param>
    /// <param name="min">The inclusive lower bound of the components.</param>
    /// <param name="max">The exclusive upper bound of the components.</param>
    /// <returns>The sequence of random vectors.</returns>
    public static IEnumerable<Vector2i> Random2(Random generator, Int32 count, Int32 min = Int32.MinValue, Int32 max = Int32.MaxValue)
    {
        for (var i = 0; i < count; i++)
            yield return new Vector2i(generator.Next(min, max), generator.Next(min, max));
    }

    /// <summary>
    ///     Get a sequence of random vectors.
    /// </summary>
    /// <param name="generator">The random number generator.</param>
    /// <param name="count">The number of vectors to generate.</param>
    /// <param name="min">The inclusive lower bound of the components.</param>
    /// <param name="max">The exclusive upper bound of the components.</param>
    /// <returns>The sequence of random vectors.</returns>
    public static IEnumerable<Vector3i> Random3(Random generator, Int32 count, Int32 min = Int32.MinValue, Int32 max = Int32.MaxValue)
    {
        for (var i = 0; i < count; i++)
            yield return new Vector3i(generator.Next(min, max), generator.Next(min, max), generator.Next(min, max));
    }

    /// <summary>
    ///     A simple one-dimensional range.
    /// </summary>
    /// <param name="x">The exclusive upper bound.</param>
    /// <returns>The range from 0 to x.</returns>
    public static IEnumerable<Int32> Range(Int32 x)
    {
        for (var i = 0; i < x; i++) yield return i;
    }

    /// <summary>
    ///     A simple two-dimensional range.
    /// </summary>
    /// <param name="x">The exclusive upper bound of the first dimension.</param>
    /// <param name="y">The exclusive upper bound of the second dimension.</param>
    /// <returns>The range from (0, 0) to (x, y).</returns>
    public static IEnumerable<(Int32, Int32)> Range2(Int32 x, Int32 y)
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
    public static IEnumerable<(Int32, Int32, Int32)> Range3(Int32 x, Int32 y, Int32 z)
    {
        for (var i = 0; i < x; i++)
        for (var j = 0; j < y; j++)
        for (var k = 0; k < z; k++)
            yield return (i, j, k);
    }

    /// <summary>
    ///     An advanced three-dimensional range, allowing to set the start and length of each dimension.
    /// </summary>
    /// <param name="start">The start (inclusive) values for each dimension.</param>
    /// <param name="length">The length of each dimension.</param>
    /// <returns>The coordinates in the range.</returns>
    public static IEnumerable<(Int32, Int32, Int32)> Range3(
        (Int32 x, Int32 y, Int32 z) start,
        (Int32 x, Int32 y, Int32 z) length)
    {
        (Int32 x, Int32 y, Int32 z) max = (start.x + length.x, start.y + length.y, start.z + length.z);

        for (Int32 i = start.x; i < max.x; i++)
        for (Int32 j = start.y; j < max.y; j++)
        for (Int32 k = start.z; k < max.z; k++)
            yield return (i, j, k);
    }

    /// <summary>
    ///     Clamps a vector between a minimum and maximum length.
    /// </summary>
    /// <param name="vector">The vector to clamp.</param>
    /// <param name="min">The minimum length.</param>
    /// <param name="max">The maximum length.</param>
    /// <returns>The clamped vector.</returns>
    public static Vector3d Clamp(Vector3d vector, Double min, Double max)
    {
        Double length = vector.Length;

        if (length < min) return vector.Normalized() * min;
        if (length > max) return vector.Normalized() * max;

        return vector;
    }

    /// <summary>
    ///     Clamp a value between zero and one.
    /// </summary>
    public static Double Clamp01(Double value)
    {
        return Math.Clamp(value, min: 0, max: 1);
    }

    /// <summary>
    ///     Clamp a value between zero and one.
    /// </summary>
    public static Single Clamp01(Single value)
    {
        return Math.Clamp(value, min: 0, max: 1);
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
    public static Vector3d Rounded(this Vector3d vector, Int32 digits = 0,
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
    public static Int32 RoundedToInt(Double value, MidpointRounding midpointRounding = MidpointRounding.ToEven)
    {
        return (Int32) Math.Round(value, digits: 0, midpointRounding);
    }

    /// <summary>
    ///     Rounds a double to an unsigned integer.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="midpointRounding">The midpoint rounding behaviour.</param>
    /// <returns>The rounded value.</returns>
    public static UInt32 RoundedToUInt(Double value, MidpointRounding midpointRounding = MidpointRounding.ToEven)
    {
        return (UInt32) Math.Round(value, digits: 0, midpointRounding);
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
    public static Vector2i Floor(this Vector2d vector)
    {
        return new Vector2i((Int32) Math.Floor(vector.X), (Int32) Math.Floor(vector.Y));
    }

    /// <summary>
    ///     Returns a floored vector of a given vector.
    /// </summary>
    /// <param name="vector">The vector to floor.</param>
    /// <returns>The component-wise floored vector.</returns>
    public static Vector3i Floor(this Vector3d vector)
    {
        return new Vector3i((Int32) Math.Floor(vector.X), (Int32) Math.Floor(vector.Y), (Int32) Math.Floor(vector.Z));
    }

    /// <summary>
    ///     Returns a ceil-ed vector of a given vector.
    /// </summary>
    /// <param name="vector">The vector to ceil.</param>
    /// <returns>The component-wise ceil-ed vector.</returns>
    public static Vector3i Ceiling(this Vector3d vector)
    {
        return new Vector3i((Int32) Math.Ceiling(vector.X), (Int32) Math.Ceiling(vector.Y), (Int32) Math.Ceiling(vector.Z));
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
    public static Vector3i Below(this Vector3i vector, Int32 offset)
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
    public static Int64 ClampRotating(Int64 value, Int64 min, Int64 max)
    {
        Debug.Assert(min < max);

        if (value >= max) return min;

        if (value < min) return max - 1;

        return value;
    }

    /// <summary>
    ///     Get the manhattan distance between two vectors.
    /// </summary>
    public static Int32 Manhattan(Vector3i a, Vector3i b)
    {
        return (a - b).ManhattanLength;
    }

    /// <summary>
    ///     Check if two floating-point values are nearly equal.
    /// </summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <param name="epsilon">The epsilon value, defining what difference is seen as equal.</param>
    /// <returns>True if the two values are nearly equal.</returns>
    public static Boolean NearlyEqual(Double a, Double b, Double epsilon = Epsilon)
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
    public static Boolean NearlyEqual(Vector3d a, Vector3d b, Double epsilon = Epsilon)
    {
        return NearlyEqual(a.X, b.X, epsilon) && NearlyEqual(a.Y, b.Y, epsilon) && NearlyEqual(a.Z, b.Z, epsilon);
    }

    /// <summary>
    ///     Check near equality to zero.
    /// </summary>
    /// <param name="a">The value to check for near equality with zero.</param>
    /// <param name="epsilon">The epsilon distance.</param>
    /// <returns>True if the given value is nearly zero.</returns>
    public static Boolean NearlyZero(Double a, Double epsilon = Epsilon)
    {
        return NearlyEqual(a, b: 0, epsilon);
    }

    /// <summary>
    ///     Create a vector from a given angle.
    /// </summary>
    /// <param name="angle">The angle, in radians.</param>
    /// <returns>The vector.</returns>
    public static Vector2d CreateVectorFromAngle(Double angle)
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
    ///     Perform a component-wise linear interpolation between two values.
    /// </summary>
    public static Vector2d Lerp(Vector2d a, Vector2d b, Vector2d t)
    {
        return new Vector2d(MathHelper.Lerp(a.X, b.X, t.X), MathHelper.Lerp(a.Y, b.Y, t.Y));
    }

    /// <summary>
    ///     Given two points and a value, calculate the lerp factor to produce the value.
    /// </summary>
    public static Double InverseLerp(Double a, Double b, Double value)
    {
        return (value - a) / (b - a);
    }

    /// <summary>
    ///     Perform a component-wise inverse lerp operation on two vectors.
    /// </summary>
    public static Vector2d InverseLerp(Vector2d a, Vector2d b, Vector2d value)
    {
        return new Vector2d(InverseLerp(a.X, b.X, value.X), InverseLerp(a.Y, b.Y, value.Y));
    }

    /// <summary>
    ///     Perform a bilinear interpolation between four values, using two factors. The factors must be in the range [0, 1].
    /// </summary>
    public static Double BiLerp(Double f00, Double f10, Double f01, Double f11, Double tx, Double ty)
    {
        return MathHelper.Lerp(MathHelper.Lerp(f00, f10, tx), MathHelper.Lerp(f01, f11, tx), ty);
    }

    /// <summary>
    ///     Perform a bilinear interpolation between four values, using two factors. The factors must be in the range [0, 1].
    /// </summary>
    public static Double BiLerp(Double f00, Double f10, Double f01, Double f11, Vector2d t)
    {
        return BiLerp(f00, f10, f01, f11, t.X, t.Y);
    }

    /// <summary>
    ///     Select from four values using two weights. If elements are equal, their weights are combined.
    /// </summary>
    public static ref readonly T SelectByWeight<T>(in T e00, in T e10, in T e01, in T e11, Vector2d weights)
    {
        Double w00 = (1 - weights.X) * (1 - weights.Y);
        Double w10 = weights.X * (1 - weights.Y);
        Double w01 = (1 - weights.X) * weights.Y;
        Double w11 = weights.X * weights.Y;

        Double GetWeight(in T e, in T e00, in T e10, in T e01, in T e11)
        {
            Double weight = 0;

            if (EqualityComparer<T>.Default.Equals(e, e00)) weight += w00;
            if (EqualityComparer<T>.Default.Equals(e, e10)) weight += w10;
            if (EqualityComparer<T>.Default.Equals(e, e01)) weight += w01;
            if (EqualityComparer<T>.Default.Equals(e, e11)) weight += w11;

            return weight;
        }

        Span<Double> totalWeights =
        [
            GetWeight(e00, e00, e10, e01, e11),
            GetWeight(e10, e00, e10, e01, e11),
            GetWeight(e01, e00, e10, e01, e11),
            GetWeight(e11, e00, e10, e01, e11)
        ];

        var indexOfMax = 0;

        for (var index = 0; index < totalWeights.Length; index++)
            if (totalWeights[index] > totalWeights[indexOfMax])
                indexOfMax = index;

        switch (indexOfMax)
        {
            // Note: SonarSource claims that only case 1 is reachable, but the test proves that all cases are reachable.

            case 0:
                return ref e00;
            case 1:
                return ref e10;
            case 2:
                return ref e01;
            case 3:
                return ref e11;

            default:
                throw Exceptions.UnsupportedValue(indexOfMax);
        }
    }

    /// <summary>
    ///     Get the square root of each vector component.
    /// </summary>
    public static Vector4 Sqrt(this Vector4 v)
    {
        return new Vector4(MathF.Sqrt(v.X), MathF.Sqrt(v.Y), MathF.Sqrt(v.Z), MathF.Sqrt(v.W));
    }

    /// <summary>
    ///     Calculates the angle between two vectors.
    /// </summary>
    public static Double CalculateAngle(Vector2d a, Vector2d b)
    {
        return Math.Acos(Vector2d.Dot(a, b) / (a.Length * b.Length));
    }

    /// <summary>
    ///     Perform a component-wise min-max operation on two vectors, returning all minimal components in the first vector and
    ///     all maximal components in the second vector.
    /// </summary>
    public static (Vector2i min, Vector2i max) MinMax(Vector2i a, Vector2i b)
    {
        return (new Vector2i(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y)), new Vector2i(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y)));
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
    public static Double Min(Single a, Single b, Single c, Single d)
    {
        return Math.Min(Math.Min(a, b), Math.Min(c, d));
    }

    /// <summary>
    ///     Get the maximum of four values.
    /// </summary>
    public static Double Max(Single a, Single b, Single c, Single d)
    {
        return Math.Max(Math.Max(a, b), Math.Max(c, d));
    }

    /// <summary>
    ///     Get the maximum component of a vector.
    /// </summary>
    public static Int32 MaxComponent(this Vector3i v)
    {
        return Math.Max(Math.Max(v.X, v.Y), v.Z);
    }

    /// <summary>
    ///     Get the maximum component of a vector.
    /// </summary>
    public static Int32 MaxComponent(this Vector2i v)
    {
        return Math.Max(v.X, v.Y);
    }

    /// <summary>
    ///     Get the maximum component of a vector.
    /// </summary>
    public static Single MaxComponent(this Vector3 v)
    {
        return Math.Max(Math.Max(v.X, v.Y), v.Z);
    }

    /// <summary>
    ///     Get the maximum component of a vector.
    /// </summary>
    public static Single MaxComponent(this Vector4 v)
    {
        return Math.Max(Math.Max(Math.Max(v.X, v.Y), v.Z), v.W);
    }

    /// <summary>
    ///     Get the minimum component of a vector.
    /// </summary>
    public static Single MinComponent(this Vector4 v)
    {
        return Math.Min(Math.Min(Math.Min(v.X, v.Y), v.Z), v.W);
    }

    /// <summary>
    ///     Get the minimum component of a vector.
    /// </summary>
    public static Single MinComponent(this Vector3 v)
    {
        return Math.Min(Math.Min(v.X, v.Y), v.Z);
    }

    /// <summary>
    ///     Get the minimum component of a vector.
    /// </summary>
    public static Single MinComponent(this Vector2 v)
    {
        return Math.Min(v.X, v.Y);
    }

    /// <summary>
    ///     Simply gets the square of a number.
    /// </summary>
    public static Int32 Square(Int32 x)
    {
        return x * x;
    }

    /// <summary>
    ///     Simply gets the cube of a number.
    /// </summary>
    public static Int32 Cube(Int32 x)
    {
        return x * x * x;
    }

    /// <summary>
    /// Simply gets the cube of a number.
    /// </summary>
    public static Single Cube(Single x)
    {
        return x * x * x;
    }

    /// <summary>
    ///     Get the modulo of a value. The result will always be positive.
    /// </summary>
    public static Int32 Mod(Int32 value, Int32 m)
    {
        return (value % m + m) % m;
    }

    /// <summary>
    ///     Get the fractional part of a double.
    /// </summary>
    /// <param name="value">The value to get the fractional part of.</param>
    /// <returns>The fractional part of the value.</returns>
    public static Double Fraction(Double value)
    {
        return value - Math.Floor(value);
    }
}
