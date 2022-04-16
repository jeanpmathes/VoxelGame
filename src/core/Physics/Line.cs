// <copyright file="Line.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Physics;

/// <summary>
///     Represents a line in 3D space.
/// </summary>
public readonly struct Line : IEquatable<Line>
{
    /// <summary>
    ///     Get the direction of the line.
    /// </summary>
    public Vector3 Direction { get; }

    /// <summary>
    ///     Get any point on the line.
    /// </summary>
    public Vector3 Any { get; }

    /// <summary>
    ///     Create a new line.
    /// </summary>
    /// <param name="point">Any point on the line.</param>
    /// <param name="direction">The direction of the line.</param>
    public Line(Vector3 point, Vector3 direction)
    {
        this.Any = point;
        Direction = direction.Normalized();
    }

    /// <inheritdoc />
    public bool Equals(Line other)
    {
        return Direction.Equals(other.Direction) && Any.Equals(other.Any);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Line other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Direction, Any);
    }

    /// <summary>
    ///     Equality operator.
    /// </summary>
    public static bool operator ==(Line left, Line right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Inequality operator.
    /// </summary>
    public static bool operator !=(Line left, Line right)
    {
        return !left.Equals(right);
    }
}
