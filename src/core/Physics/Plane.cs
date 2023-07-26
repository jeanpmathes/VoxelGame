// <copyright file="Plane.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Physics;

/// <summary>
///     A plane in 3D space.
/// </summary>
public readonly struct Plane : IEquatable<Plane>
{
    /// <summary>
    ///     The normal of the plane.
    /// </summary>
    public Vector3d Normal { get; }

    /// <summary>
    ///     A point that is in the plane.
    /// </summary>
    public Vector3d Point { get; }

    private readonly double d;

    /// <summary>
    ///     Creates a plane. The normal parameter has to be normalized.
    /// </summary>
    /// <param name="normal">The normalized normal vector.</param>
    /// <param name="point">A point in the plane.</param>
    public Plane(Vector3d normal, Vector3d point)
    {
        Normal = normal;
        Point = point;

        d = -Vector3d.Dot(normal, point);
    }

    /// <summary>
    ///     Get a translated plane.
    /// </summary>
    public Plane Translated(Vector3d offset)
    {
        return new Plane(Normal, Point + offset);
    }

    /// <summary>
    ///     Projects a point onto the plane.
    /// </summary>
    /// <param name="point">The point to project.</param>
    /// <returns>The projected point.</returns>
    public Vector3d Project(Vector3d point)
    {
        return point - Normal * GetDistanceTo(point);
    }

    /// <summary>
    ///     Projects a point onto the plane coordinate-system, loosing one dimension.
    /// </summary>
    /// <param name="point">The point to project.</param>
    /// <param name="axis">The vector to use as x-Axis. Must be orthogonal to the plane normal.</param>
    /// <returns>A 2D point on the plane.</returns>
    public Vector2d Project2D(Vector3d point, Vector3d axis)
    {
        Vector3d projected = Project(point);
        Vector3d offset = projected - Point;

        Vector3d xAxis = axis.Normalized();
        Vector3d yAxis = Vector3d.Cross(axis.Normalized(), Normal).Normalized();

        double projectedX = Vector3d.Dot(offset, xAxis);
        double projectedY = Vector3d.Dot(offset, yAxis);

        return new Vector2d(projectedX, projectedY);
    }

    /// <summary>
    ///     Calculate the intersection of two planes.
    /// </summary>
    /// <param name="other">The other plane.</param>
    /// <returns>The ray along the intersection, if there is any.</returns>
    public Line? Intersects(Plane other)
    {
        Vector3d n1 = Normal;
        Vector3d n2 = other.Normal;

        Vector3d p1 = Point;
        Vector3d p2 = other.Point;

        Vector3d normal = Vector3d.Cross(n1, n2);

        if (VMath.NearlyZero(normal.LengthSquared)) return null;

        Vector3d l = Vector3d.Cross(n2, normal);

        double n = Vector3d.Dot(n1, l);

        if (VMath.NearlyZero(n)) return null;

        Vector3d p = p1 - p2;
        double t = Vector3d.Dot(n1, p) / n;
        Vector3d point = p2 + t * l;

        return new Line(point, normal);
    }

    /// <summary>
    ///     Calculate the distance from a point to the plane. The sign of the distance indicates the side.
    /// </summary>
    /// <param name="point">The point to calculate the distance to.</param>
    /// <returns>The distance to the point.</returns>
    public double GetDistanceTo(Vector3d point)
    {
        return Vector3d.Dot(point, Normal) + d;
    }

    /// <inheritdoc />
    public bool Equals(Plane other)
    {
        return Normal.Equals(other.Normal) && Point.Equals(other.Point);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Plane other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Normal, Point);
    }

    /// <summary>
    ///     Checks if two planes are equal.
    /// </summary>
    public static bool operator ==(Plane left, Plane right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Checks if two planes are not equal.
    /// </summary>
    public static bool operator !=(Plane left, Plane right)
    {
        return !left.Equals(right);
    }
}
