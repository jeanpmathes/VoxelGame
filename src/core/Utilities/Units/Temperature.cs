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
    public double DegreesCelsius { get; init; }

    /// <summary>
    ///     Whether the temperature is below the freezing point of water.
    /// </summary>
    public bool IsFreezing => DegreesCelsius <= 0;

    /// <inheritdoc />
    public static Unit Unit => Unit.Celsius;

    /// <inheritdoc />
    public Prefix Prefix => Prefix.None;

    /// <inheritdoc />
    double IMeasure.Value => DegreesCelsius;

    /// <inheritdoc />
    public bool Equals(Temperature other)
    {
        return DegreesCelsius.Equals(other.DegreesCelsius);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Temperature other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return DegreesCelsius.GetHashCode();
    }

    /// <summary>
    ///     Equality operator.
    /// </summary>
    public static bool operator ==(Temperature left, Temperature right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Inequality operator.
    /// </summary>
    public static bool operator !=(Temperature left, Temperature right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return IMeasure.ToString(this);
    }
}
