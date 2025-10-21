// <copyright file="Density.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Numerics;

namespace VoxelGame.Core.Utilities.Units;

/// <summary>
///     A measure for density.
/// </summary>
public readonly struct Density :
    IMeasure,
    IEquatable<Density>,
    IComparable<Density>,
    IComparisonOperators<Density, Density, Boolean>
{
    /// <summary>
    ///     Gets the density in kilograms per cubic meter.
    /// </summary>
    public Double KilogramsPerCubicMeter { get; init; }

    /// <inheritdoc />
    public static Unit Unit => Unit.KilogramPerCubicMeter;

    /// <inheritdoc />
    public static Prefix.AllowedPrefixes Prefixes
        => Prefix.AllowedPrefixes.Kilo | Prefix.AllowedPrefixes.Unprefixed;

    /// <inheritdoc />
    Double IMeasure.Value => KilogramsPerCubicMeter;

    /// <inheritdoc />
    public Int32 CompareTo(Density other)
    {
        return KilogramsPerCubicMeter.CompareTo(other.KilogramsPerCubicMeter);
    }

    /// <inheritdoc />
    public Boolean Equals(Density other)
    {
        return KilogramsPerCubicMeter.Equals(other.KilogramsPerCubicMeter);
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is Density other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return KilogramsPerCubicMeter.GetHashCode();
    }

    /// <summary>
    ///     Equality operator.
    /// </summary>
    public static Boolean operator ==(Density left, Density right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Inequality operator.
    /// </summary>
    public static Boolean operator !=(Density left, Density right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    ///     Less-than operator.
    /// </summary>
    public static Boolean operator <(Density left, Density right)
    {
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    ///     Greater-than operator.
    /// </summary>
    public static Boolean operator >(Density left, Density right)
    {
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    ///     Less-than-or-equal operator.
    /// </summary>
    public static Boolean operator <=(Density left, Density right)
    {
        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    ///     Greater-than-or-equal operator.
    /// </summary>
    public static Boolean operator >=(Density left, Density right)
    {
        return left.CompareTo(right) >= 0;
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return IMeasure.ToString(this, format: null);
    }
}
