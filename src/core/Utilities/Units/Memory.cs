// <copyright file="Memory.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Utilities.Units;

/// <summary>
///     A measure for computer memory.
/// </summary>
public struct Memory : IMeasure, IEquatable<Memory>
{
    /// <summary>
    ///     Get the memory, in bytes.
    /// </summary>
    public double Bytes { get; set; }

    /// <inheritdoc />
    public Unit Unit => Unit.Byte;

    /// <inheritdoc />
    double IMeasure.Value => Bytes;

    /// <inheritdoc />
    public bool Equals(Memory other)
    {
        return Bytes.Equals(other.Bytes);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Memory other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Bytes.GetHashCode();
    }

    /// <summary>
    ///     Equality operator.
    /// </summary>
    public static bool operator ==(Memory left, Memory right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Inequality operator.
    /// </summary>
    public static bool operator !=(Memory left, Memory right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return IMeasure.ToString(this);
    }
}
