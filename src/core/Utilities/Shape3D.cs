// <copyright file="Shape3D.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Defines different shapes in 3D space.
/// </summary>
public abstract class Shape3D
{
    /// <summary>
    ///     The position of the shape, in most cases the center.
    /// </summary>
    public Vector3d Position { get; init; }

    /// <summary>
    ///     Get a bounding box that completely contains the shape.
    /// </summary>
    public abstract Box3d BoundingBox { get; }

    /// <summary>
    ///     Get whether the shape contains the given point.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <param name="closeness">How close the point is to the shape.</param>
    /// <returns>Whether the point is contained in the shape.</returns>
    public abstract Boolean Contains(Vector3d point, out Double closeness);

    /// <summary>
    ///     Get whether the shape contains the given point.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <returns>Whether the point is contained in the shape.</returns>
    public Boolean Contains(Vector3d point)
    {
        return Contains(point, out _);
    }
}

/// <summary>
///     A sphere in 3D space.
/// </summary>
public sealed class Sphere : Shape3D
{
    /// <summary>
    ///     The radius of the sphere.
    /// </summary>
    public Double Radius { get; init; }

    private Double RadiusSquared => Radius * Radius;

    /// <inheritdoc />
    public override Box3d BoundingBox => new(Position - new Vector3d(Radius), Position + new Vector3d(Radius));

    /// <inheritdoc />
    public override Boolean Contains(Vector3d point, out Double closeness)
    {
        Double distanceSquared = (point - Position).LengthSquared;
        Double radiusSquared = RadiusSquared;

        closeness = 1 - distanceSquared / RadiusSquared;

        return distanceSquared <= radiusSquared;
    }
}

/// <summary>
///     A spheroid in 3D space.
/// </summary>
public sealed class Spheroid : Shape3D
{
    /// <summary>
    ///     The three radii of the spheroid.
    /// </summary>
    public Vector3d Radius { get; init; }

    private Vector3d RadiusSquared => Radius * Radius;

    /// <inheritdoc />
    public override Box3d BoundingBox => new(Position - Radius, Position + Radius);

    /// <inheritdoc />
    public override Boolean Contains(Vector3d point, out Double closeness)
    {
        point -= Position;

        Vector3d v = point * point / RadiusSquared;
        closeness = 1 - (v.X + v.Y + v.Z);

        return closeness >= 0;
    }
}

/// <summary>
///     A cone in 3D space.
/// </summary>
public sealed class Cone : Shape3D
{
    /// <summary>
    ///     The bottom radius of the cone.
    /// </summary>
    public Double BottomRadius { get; init; }

    /// <summary>
    ///     The top radius of the cone.
    /// </summary>
    public Double TopRadius { get; init; }

    /// <summary>
    ///     The height of the cone.
    /// </summary>
    public Double Height { get; init; }

    /// <inheritdoc />
    public override Box3d BoundingBox
    {
        get
        {
            Double radius = Math.Max(BottomRadius, TopRadius);

            return new Box3d(
                Position - new Vector3d(radius, y: 0, radius),
                Position + new Vector3d(radius, Height, radius));
        }
    }

    /// <inheritdoc />
    public override Boolean Contains(Vector3d point, out Double closeness)
    {
        point -= Position;

        Double height = point.Y / Height;
        Double radius = MathHelper.Lerp(BottomRadius, TopRadius, height);

        Double radiusSquared = radius * radius;
        Double distanceSquared = point.Xz.LengthSquared;

        closeness = 1 - distanceSquared / radiusSquared;

        return height is >= 0 and <= 1 && distanceSquared <= radiusSquared;
    }
}
