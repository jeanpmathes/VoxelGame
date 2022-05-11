// <copyright file="SectionPosition.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;

namespace VoxelGame.Core.Logic;

/// <summary>
///     The position of a section in the world.
/// </summary>
public readonly struct SectionPosition : IEquatable<SectionPosition>
{
    /// <summary>
    ///     Create a section position with the given coordinates.
    /// </summary>
    public SectionPosition(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    ///     The x coordinate.
    /// </summary>
    public int X { get; init; }

    /// <summary>
    ///     The y coordinate.
    /// </summary>
    public int Y { get; init; }

    /// <summary>
    ///     The z coordinate.
    /// </summary>
    public int Z { get; init; }

    /// <summary>
    ///     The equality comparison.
    /// </summary>
    public bool Equals(SectionPosition other)
    {
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is SectionPosition other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    /// <summary>
    ///     The equality operator.
    /// </summary>
    public static bool operator ==(SectionPosition left, SectionPosition right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     The inequality operator.
    /// </summary>
    public static bool operator !=(SectionPosition left, SectionPosition right)
    {
        return !left.Equals(right);
    }
}
