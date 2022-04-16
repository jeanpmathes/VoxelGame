// <copyright file="Plane.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

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
    public Vector3 Normal { get; }

    /// <summary>
    ///     A point that is in the plane.
    /// </summary>
    public Vector3 Point { get; }

    private readonly float d;

    /// <summary>
    ///     Creates a plane. The normal parameter has to be normalized.
    /// </summary>
    /// <param name="normal">The normalized normal vector.</param>
    /// <param name="point">A point in the plane.</param>
    public Plane(Vector3 normal, Vector3 point)
    {
        Normal = normal;
        Point = point;

        d = -Vector3.Dot(normal, point);
    }

    /// <summary>
    ///     Projects a point onto the plane.
    /// </summary>
    /// <param name="point">The point to project.</param>
    /// <returns>The projected point.</returns>
    public Vector3 Project(Vector3 point)
    {
        return point - Normal * Distance(point);
    }

    /// <summary>
    ///     Projects a point onto the plane coordinate-system, loosing one dimension.
    /// </summary>
    /// <param name="point">The point to project.</param>
    /// <param name="axis">The vector to use as x-Axis. Must be orthogonal to the plane normal.</param>
    /// <returns>A 2D point on the plane.</returns>
    public Vector2 Project2D(Vector3 point, Vector3 axis)
    {
        Vector3 projected = Project(point);
        Vector3 offset = projected - Point;

        Vector3 xAxis = axis.Normalized();
        Vector3 yAxis = Vector3.Cross(xAxis, Normal).Normalized();

        float projectedX = Vector3.Dot(offset, xAxis);
        float projectedY = Vector3.Dot(offset, yAxis);

        return new Vector2(projectedX, projectedY);
    }

    /// <summary>
    ///     Calculate the intersection of two planes.
    /// </summary>
    /// <param name="other">The other plane.</param>
    /// <returns>The ray along the intersection, if there is any.</returns>
    public Line? Intersects(Plane other)
    {
        Vector3 n1 = Normal;
        Vector3 n2 = other.Normal;

        Vector3 p1 = Point;
        Vector3 p2 = other.Point;

        Vector3 normal = Vector3.Cross(n1, n2);

        if (VMath.NearlyZero(normal.LengthSquared)) return null;

        Vector3 l = Vector3.Cross(n2, normal);

        float n = Vector3.Dot(n1, l);

        if (VMath.NearlyZero(n)) return null;

        Vector3 p = p1 - p2;
        float t = Vector3.Dot(n1, p) / n;
        Vector3 point = p2 + t * l;

        return new Line(point, normal);
    }

    /// <summary>
    ///     Calculate the distance from a point to the plane.
    /// </summary>
    /// <param name="point">The point to calculate the distance to.</param>
    /// <returns>The distance to the point.</returns>
    public float Distance(Vector3 point)
    {
        return Vector3.Dot(point, Normal) + d;
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
