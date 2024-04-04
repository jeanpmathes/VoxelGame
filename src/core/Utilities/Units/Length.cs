// <copyright file="Length.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Utilities.Units;

/// <summary>
///     A measure for length.
/// </summary>
public readonly struct Length : IMeasure, IEquatable<Length>
{
    /// <summary>
    ///     Get the length, in meters.
    /// </summary>
    public Double Meters { get; init; }

    /// <inheritdoc />
    public static Unit Unit => Unit.Meter;

    /// <inheritdoc />
    Double IMeasure.Value => Meters;

    /// <inheritdoc />
    public Boolean Equals(Length other)
    {
        return Meters.Equals(other.Meters);
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is Length other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return Meters.GetHashCode();
    }

    /// <summary>
    ///     Equality operator.
    /// </summary>
    public static Boolean operator ==(Length left, Length right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Inequality operator.
    /// </summary>
    public static Boolean operator !=(Length left, Length right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return IMeasure.ToString(this);
    }
}
