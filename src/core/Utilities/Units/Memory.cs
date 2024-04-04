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
public readonly struct Memory : IMeasure, IEquatable<Memory>
{
    /// <summary>
    ///     Get the memory, in bytes.
    /// </summary>
    public Double Bytes { get; init; }

    /// <inheritdoc />
    public static Unit Unit => Unit.Byte;

    /// <inheritdoc />
    Double IMeasure.Value => Bytes;

    /// <inheritdoc />
    public Boolean Equals(Memory other)
    {
        return Bytes.Equals(other.Bytes);
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is Memory other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return Bytes.GetHashCode();
    }

    /// <summary>
    ///     Equality operator.
    /// </summary>
    public static Boolean operator ==(Memory left, Memory right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Inequality operator.
    /// </summary>
    public static Boolean operator !=(Memory left, Memory right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return IMeasure.ToString(this);
    }
}
