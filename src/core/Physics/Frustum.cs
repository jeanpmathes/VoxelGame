// <copyright file="Frustum.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Linq;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Physics;

/// <summary>
///     A camera view frustum.
/// </summary>
public readonly struct Frustum : IEquatable<Frustum>
{
    private readonly Plane[] planes;

    private const int PlaneNear = 0;
    private const int PlaneFar = 1;
    private const int PlaneLeft = 2;
    private const int PlaneRight = 3;
    private const int PlaneBottom = 4;
    private const int PlaneTop = 5;

    /// <summary>
    ///     Create a new frustum.
    /// </summary>
    /// <param name="fovY">The field-of-view value, on the y axis.</param>
    /// <param name="ratio">The screen ratio.</param>
    /// <param name="clip">The distances to the near and far clipping planes.</param>
    /// <param name="position">The position of the camera.</param>
    /// <param name="direction">The view direction.</param>
    /// <param name="up">The up direction.</param>
    /// <param name="right">The right direction.</param>
    public Frustum(double fovY, double ratio, (double near, double far) clip,
        Vector3d position, Vector3d direction, Vector3d up, Vector3d right)
    {
        direction.Normalize();
        up.Normalize();
        right.Normalize();

        (double wNear, double hNear) = GetDimensionsAt(clip.near, fovY, ratio);

        Vector3d nc = position + direction * clip.near;
        Vector3d fc = position + direction * clip.far;

        Vector3d nl = Vector3d.Cross((nc - right * wNear / 2f - position).Normalized(), up);
        Vector3d nr = Vector3d.Cross(up, (nc + right * wNear / 2f - position).Normalized());

        Vector3d nb = Vector3d.Cross(right, (nc - up * hNear / 2f - position).Normalized());
        Vector3d nt = Vector3d.Cross((nc + up * hNear / 2f - position).Normalized(), right);

        planes = new[]
        {
            new Plane(direction, nc), // Near.
            new Plane(-direction, fc), // Far.
            new Plane(nl, position), // Left.
            new Plane(nr, position), // Right.
            new Plane(nb, position), // Bottom.
            new Plane(nt, position) // Top.
        };
    }

    /// <summary>
    ///     Get the dimensions of a frustum at a given distance.
    /// </summary>
    /// <param name="distance">The distance from the frustum origin to get the dimensions for.</param>
    /// <param name="fovY">The vertical fov.</param>
    /// <param name="ratio">The screen ratio.</param>
    /// <returns>The calculated dimensions.</returns>
    public static (double width, double height) GetDimensionsAt(double distance, double fovY, double ratio)
    {
        double height = 2f * Math.Tan(fovY * 0.5f) * distance;
        double width = height * ratio;

        return (width, height);
    }

    /// <summary>
    ///     Get the near plane.
    /// </summary>
    public Plane Near => planes[PlaneNear];

    /// <summary>
    ///     Get the far plane.
    /// </summary>
    public Plane Far => planes[PlaneFar];

    /// <summary>
    ///     Get the left plane.
    /// </summary>
    public Plane Left => planes[PlaneLeft];

    /// <summary>
    ///     Get the right plane.
    /// </summary>
    public Plane Right => planes[PlaneRight];

    /// <summary>
    ///     Get the bottom plane.
    /// </summary>
    public Plane Bottom => planes[PlaneBottom];

    /// <summary>
    ///     Get the top plane.
    /// </summary>
    public Plane Top => planes[PlaneTop];

    /// <summary>
    ///     Check whether a <see cref="Box3" /> is inside this <see cref="Frustum" />.
    /// </summary>
    /// <returns>true if the <see cref="Box3" /> is inside; false if not.</returns>
    public bool IsBoxInFrustum(Box3d volume)
    {
        for (var i = 0; i < 6; i++)
        {
            double px = planes[i].Normal.X < 0 ? volume.Min.X : volume.Max.X;
            double py = planes[i].Normal.Y < 0 ? volume.Min.Y : volume.Max.Y;
            double pz = planes[i].Normal.Z < 0 ? volume.Min.Z : volume.Max.Z;

            if (planes[i].Distance(new Vector3d(px, py, pz)) < 0) return false;
        }

        return true;
    }

    /// <inheritdoc />
    public bool Equals(Frustum other)
    {
        return planes.SequenceEqual(other.planes);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Frustum other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return planes.GetHashCode();
    }

    /// <summary>
    ///     Checks whether two <see cref="Frustum" /> are equal.
    /// </summary>
    public static bool operator ==(Frustum left, Frustum right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Checks whether two <see cref="Frustum" /> are not equal.
    /// </summary>
    public static bool operator !=(Frustum left, Frustum right)
    {
        return !left.Equals(right);
    }
}