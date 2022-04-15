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
    public Frustum(float fovY, float ratio, (float near, float far) clip,
        Vector3 position, Vector3 direction, Vector3 up, Vector3 right)
    {
        direction = direction.Normalized();
        up = up.Normalized();
        right = right.Normalized();

        (float hNear, float wNear) = GetDimensionsAt(clip.near, fovY, ratio);

        Vector3 nc = position + direction * clip.near;
        Vector3 fc = position + direction * clip.far;

        Vector3 nl = Vector3.Cross((nc - right * wNear / 2f - position).Normalized(), up);
        Vector3 nr = Vector3.Cross(up, (nc + right * wNear / 2f - position).Normalized());

        Vector3 nb = Vector3.Cross(right, (nc - up * hNear / 2f - position).Normalized());
        Vector3 nt = Vector3.Cross((nc + up * hNear / 2f - position).Normalized(), right);

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
    public static (float width, float height) GetDimensionsAt(float distance, float fovY, float ratio)
    {
        var height = (float) (2f * Math.Tan(fovY / 2f) * distance);
        float width = height * ratio;

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
    public bool IsBoxInFrustum(Box3 volume)
    {
        for (var i = 0; i < 6; i++)
        {
            float px = planes[i].Normal.X < 0 ? volume.Min.X : volume.Max.X;
            float py = planes[i].Normal.Y < 0 ? volume.Min.Y : volume.Max.Y;
            float pz = planes[i].Normal.Z < 0 ? volume.Min.Z : volume.Max.Z;

            if (planes[i].Distance(new Vector3(px, py, pz)) < 0) return false;
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
