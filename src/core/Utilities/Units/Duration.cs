// <copyright file="Duration.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Utilities.Units;

/// <summary>
///     A real-time duration. Do not use this for time spans or time intervals.
/// </summary>
public readonly struct Duration : IMeasure, IEquatable<Duration>
{
    /// <summary>
    ///     Get the duration, in seconds.
    /// </summary>
    public Double Seconds { get; init; }

    /// <summary>
    ///     Get the duration, in milliseconds.
    /// </summary>
    public Double Milliseconds
    {
        get => Seconds * 1000;
        init => Seconds = value / 1000;
    }

    /// <inheritdoc />
    public static Unit Unit => Unit.Second;

    Double IMeasure.Value => Seconds;

    /// <inheritdoc />
    public Boolean Equals(Duration other)
    {
        return Seconds.Equals(other.Seconds);
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is Duration other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return Seconds.GetHashCode();
    }

    /// <summary>
    ///     Equality operator.
    /// </summary>
    public static Boolean operator ==(Duration left, Duration right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Inequality operator.
    /// </summary>
    public static Boolean operator !=(Duration left, Duration right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return IMeasure.ToString(this);
    }
}
