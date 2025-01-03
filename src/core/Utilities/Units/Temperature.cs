// <copyright file="Temperature.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Utilities.Units;

/// <summary>
///     A measure for temperature.
/// </summary>
public readonly struct Temperature : IMeasure, IEquatable<Temperature>
{
    /// <summary>
    ///     Get the temperature in degrees Celsius.
    /// </summary>
    public Double DegreesCelsius { get; init; }

    /// <summary>
    ///     Whether the temperature is below the freezing point of water.
    /// </summary>
    public Boolean IsFreezing => DegreesCelsius <= 0;

    /// <inheritdoc />
    public static Unit Unit => Unit.Celsius;

    /// <inheritdoc />
    public static Prefix.AllowedPrefixes Prefixes
        => Prefix.AllowedPrefixes.None;

    /// <inheritdoc />
    Double IMeasure.Value => DegreesCelsius;

    /// <inheritdoc />
    public Boolean Equals(Temperature other)
    {
        return DegreesCelsius.Equals(other.DegreesCelsius);
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is Temperature other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return DegreesCelsius.GetHashCode();
    }

    /// <summary>
    ///     Equality operator.
    /// </summary>
    public static Boolean operator ==(Temperature left, Temperature right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Inequality operator.
    /// </summary>
    public static Boolean operator !=(Temperature left, Temperature right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return IMeasure.ToString(this);
    }
}
