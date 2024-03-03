// <copyright file="Length.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Utilities.Units;

/// <summary>
///     A measure for length
/// </summary>
public struct Length : IMeasure, IEquatable<Length>
{
    /// <summary>
    ///     Get the length, in meters.
    /// </summary>
    public double Meters { get; set; }

    /// <inheritdoc />
    public Unit Unit => Unit.Meter;

    /// <inheritdoc />
    double IMeasure.Value => Meters;

    /// <inheritdoc />
    public bool Equals(Length other)
    {
        return Meters.Equals(other.Meters);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Length other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Meters.GetHashCode();
    }

    /// <summary>
    ///     Equality operator.
    /// </summary>
    public static bool operator ==(Length left, Length right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Inequality operator.
    /// </summary>
    public static bool operator !=(Length left, Length right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return IMeasure.ToString(this);
    }
}
