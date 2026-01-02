// <copyright file="Ray.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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
///     A ray trough 3D space.
/// </summary>
public readonly struct Ray : IEquatable<Ray>
{
    /// <summary>
    ///     The origin of the ray.
    /// </summary>
    public Vector3d Origin { get; }

    /// <summary>
    ///     The direction of the ray.
    /// </summary>
    public Vector3d Direction { get; }

    /// <summary>
    ///     The length of the ray.
    /// </summary>
    public Single Length { get; }

    /// <summary>
    ///     Create a new ray trough 3D space.
    /// </summary>
    public Ray(Vector3d origin, Vector3d direction, Single length)
    {
        Origin = origin;
        Direction = direction.Normalized();
        Length = length;
    }

    /// <summary>
    ///     Get a translated ray.
    /// </summary>
    public Ray Translated(Vector3d translation)
    {
        return new Ray(Origin + translation, Direction, Length);
    }

    /// <summary>
    ///     The end point of the ray.
    /// </summary>
    public Vector3d EndPoint => Origin + Direction * Length;

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return HashCode.Combine(Origin.GetHashCode(), Direction.GetHashCode());
    }

    /// <summary>
    ///     Compare two rays for equality.
    /// </summary>
    public static Boolean operator ==(Ray left, Ray right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Compare two rays for inequality.
    /// </summary>
    public static Boolean operator !=(Ray left, Ray right)
    {
        return !(left == right);
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        if (obj is Ray ray) return Equals(ray);

        return false;
    }

    /// <inheritdoc />
    public Boolean Equals(Ray other)
    {
        return Origin.Equals(other.Origin) && Direction.Equals(other.Direction);
    }
}
