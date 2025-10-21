// <copyright file="Viscosity.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Numerics;

namespace VoxelGame.Core.Utilities.Units;

/// <summary>
///     A measure for dynamic viscosity.
/// </summary>
public readonly struct Viscosity :
    IMeasure,
    IEquatable<Viscosity>,
    IComparable<Viscosity>,
    IComparisonOperators<Viscosity, Viscosity, Boolean>
{
    private const Double UpdateTicksPerMilliPascalSecond = 15.0;

    /// <summary>
    ///     Gets the viscosity expressed as update ticks.
    /// </summary>
    public Double UpdateTicks { get; init; }

    /// <summary>
    ///     Gets the viscosity in Pascal seconds.
    /// </summary>
    public Double PascalSeconds
    {
        get => UpdateTicks / (1000.0 * UpdateTicksPerMilliPascalSecond);
        init => UpdateTicks = value * 1000.0 * UpdateTicksPerMilliPascalSecond;
    }

    /// <summary>
    ///     Gets the viscosity in milli Pascal seconds.
    /// </summary>
    public Double MilliPascalSeconds
    {
        get => UpdateTicks / UpdateTicksPerMilliPascalSecond;
        init => UpdateTicks = value * UpdateTicksPerMilliPascalSecond;
    }

    /// <summary>
    ///     Converts this viscosity to the update delay used for scheduling.
    /// </summary>
    public UInt32 ToUpdateDelay()
    {
        return (UInt32) Math.Max(val1: 1, Math.Round(UpdateTicks));
    }

    /// <inheritdoc />
    public static Unit Unit => Unit.PascalSecond;

    /// <inheritdoc />
    public static Prefix.AllowedPrefixes Prefixes
        => Prefix.AllowedPrefixes.Milli | Prefix.AllowedPrefixes.Unprefixed;

    /// <inheritdoc />
    Double IMeasure.Value => PascalSeconds;

    /// <inheritdoc />
    public Int32 CompareTo(Viscosity other)
    {
        return UpdateTicks.CompareTo(other.UpdateTicks);
    }

    /// <inheritdoc />
    public Boolean Equals(Viscosity other)
    {
        return UpdateTicks.Equals(other.UpdateTicks);
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is Viscosity other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return UpdateTicks.GetHashCode();
    }

    /// <summary>
    ///     Equality operator.
    /// </summary>
    public static Boolean operator ==(Viscosity left, Viscosity right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Inequality operator.
    /// </summary>
    public static Boolean operator !=(Viscosity left, Viscosity right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    ///     Less-than operator.
    /// </summary>
    public static Boolean operator <(Viscosity left, Viscosity right)
    {
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    ///     Greater-than operator.
    /// </summary>
    public static Boolean operator >(Viscosity left, Viscosity right)
    {
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    ///     Less-than-or-equal operator.
    /// </summary>
    public static Boolean operator <=(Viscosity left, Viscosity right)
    {
        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    ///     Greater-than-or-equal operator.
    /// </summary>
    public static Boolean operator >=(Viscosity left, Viscosity right)
    {
        return left.CompareTo(right) >= 0;
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return IMeasure.ToString(this, format: null);
    }
}
