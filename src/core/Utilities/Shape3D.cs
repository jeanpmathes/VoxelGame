// <copyright file="Shape3D.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

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
    public Vector3 Position { get; init; }

    /// <summary>
    ///     Get the size, which is the distance between the furthest points.
    /// </summary>
    public abstract float Size { get; }

    /// <summary>
    ///     Get whether the shape contains the given point.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <param name="closeness">How close the point is to the shape.</param>
    /// <returns>Whether the point is contained in the shape.</returns>
    public abstract bool Contains(Vector3 point, out float closeness);

    /// <summary>
    ///     Get whether the shape contains the given point.
    /// </summary>
    /// <param name="point">The point to check.</param>
    /// <returns>Whether the point is contained in the shape.</returns>
    public bool Contains(Vector3 point)
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
    public float Radius { get; init; }

    private float RadiusSquared => Radius * Radius;

    /// <inheritdoc />
    public override float Size => Radius * 2;

    /// <inheritdoc />
    public override bool Contains(Vector3 point, out float closeness)
    {
        float distanceSquared = (point - Position).LengthSquared;
        float radiusSquared = RadiusSquared;

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
    public Vector3 Radius { get; init; }

    private Vector3 RadiusSquared => Radius * Radius;

    /// <inheritdoc />
    public override float Size => Radius.Length * 2;

    /// <inheritdoc />
    public override bool Contains(Vector3 point, out float closeness)
    {
        point -= Position;

        Vector3 v = point * point / RadiusSquared;
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
    public float BottomRadius { get; init; }

    /// <summary>
    ///     The top radius of the cone.
    /// </summary>
    public float TopRadius { get; init; }

    /// <summary>
    ///     The height of the cone.
    /// </summary>
    public float Height { get; init; }

    /// <inheritdoc />
    public override float Size => Math.Max(Math.Max(BottomRadius, TopRadius) * 2, Height);

    /// <inheritdoc />
    public override bool Contains(Vector3 point, out float closeness)
    {
        point -= Position;

        float height = point.Y / Height;
        float radius = MathHelper.Lerp(BottomRadius, TopRadius, height);

        float radiusSquared = radius * radius;
        float distanceSquared = point.Xz.LengthSquared;

        closeness = 1 - distanceSquared / radiusSquared;

        return height is >= 0 and <= 1 && distanceSquared <= radiusSquared;
    }
}
