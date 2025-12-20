// <copyright file="Line.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

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
    public Vector3d Direction { get; }

    /// <summary>
    ///     Get any point on the line.
    /// </summary>
    public Vector3d Any { get; }

    /// <summary>
    ///     Create a new line.
    /// </summary>
    /// <param name="point">Any point on the line.</param>
    /// <param name="direction">The direction of the line.</param>
    public Line(Vector3d point, Vector3d direction)
    {
        Any = point;
        Direction = direction.Normalized();
    }

    /// <inheritdoc />
    public Boolean Equals(Line other)
    {
        return Direction.Equals(other.Direction) && Any.Equals(other.Any);
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is Line other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return HashCode.Combine(Direction, Any);
    }

    /// <summary>
    ///     Equality operator.
    /// </summary>
    public static Boolean operator ==(Line left, Line right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Inequality operator.
    /// </summary>
    public static Boolean operator !=(Line left, Line right)
    {
        return !left.Equals(right);
    }
}
