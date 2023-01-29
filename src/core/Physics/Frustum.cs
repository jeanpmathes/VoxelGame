// <copyright file="Frustum.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Physics;

/// <summary>
///     A camera view frustum.
/// </summary>
public readonly struct Frustum : IEquatable<Frustum>
{
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
        Debug.Assert(clip.near < clip.far);
        Debug.Assert(clip.near >= 0.0);

        direction.Normalize();
        up.Normalize();
        right.Normalize();

        (double wFar, double hFar) = GetDimensionsAt(clip.far, fovY, ratio);

        Vector3d nc = position + direction * clip.near;
        Vector3d fc = position + direction * clip.far;

        Vector3d nl = Vector3d.Cross((fc - right * wFar / 2.0 - position).Normalized(), up);
        Vector3d nr = Vector3d.Cross(up, (fc + right * wFar / 2.0 - position).Normalized());

        Vector3d nb = Vector3d.Cross(right, (fc - up * hFar / 2.0 - position).Normalized());
        Vector3d nt = Vector3d.Cross((fc + up * hFar / 2.0 - position).Normalized(), right);

        Near = new Plane(direction, nc);
        Far = new Plane(-direction, fc);
        Left = new Plane(nl, position);
        Right = new Plane(nr, position);
        Bottom = new Plane(nb, position);
        Top = new Plane(nt, position);
    }

    private Frustum(Frustum original, Vector3d offset)
    {
        Near = original.Near.Translated(offset);
        Far = original.Far.Translated(offset);
        Left = original.Left.Translated(offset);
        Right = original.Right.Translated(offset);
        Bottom = original.Bottom.Translated(offset);
        Top = original.Top.Translated(offset);
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
        double height = 2.0 * Math.Tan(fovY * 0.5) * distance;
        double width = height * ratio;

        return (width, height);
    }

    /// <summary>
    ///     Get the near plane.
    /// </summary>
    public Plane Near { get; }

    /// <summary>
    ///     Get the far plane.
    /// </summary>
    public Plane Far { get; }

    /// <summary>
    ///     Get the left plane.
    /// </summary>
    public Plane Left { get; }

    /// <summary>
    ///     Get the right plane.
    /// </summary>
    public Plane Right { get; }

    /// <summary>
    ///     Get the bottom plane.
    /// </summary>
    public Plane Bottom { get; }

    /// <summary>
    ///     Get the top plane.
    /// </summary>
    public Plane Top { get; }

    private Plane GetPlane(int index)
    {
        return index switch
        {
            0 => Near,
            1 => Far,
            2 => Left,
            3 => Right,
            4 => Bottom,
            5 => Top,
            _ => throw new ArgumentOutOfRangeException(nameof(index), index, message: null)
        };
    }

    /// <summary>
    ///     Check whether a <see cref="Box3" /> is inside this <see cref="Frustum" />.
    /// </summary>
    /// <returns>true if the <see cref="Box3" /> is inside; false if not.</returns>
    public bool IsBoxInFrustum(Box3d volume)
    {
        for (var i = 0; i < 6; i++)
        {
            Plane plane = GetPlane(i);

            double px = plane.Normal.X < 0 ? volume.Min.X : volume.Max.X;
            double py = plane.Normal.Y < 0 ? volume.Min.Y : volume.Max.Y;
            double pz = plane.Normal.Z < 0 ? volume.Min.Z : volume.Max.Z;

            if (plane.Distance(new Vector3d(px, py, pz)) < 0) return false;
        }

        return true;
    }

    /// <summary>
    ///     Get a translated frustum.
    /// </summary>
    public Frustum Translated(Vector3d offset)
    {
        return new Frustum(this, offset);
    }

    /// <inheritdoc />
    public bool Equals(Frustum other)
    {
        for (var i = 0; i < 6; i++)
            if (!GetPlane(i).Equals(other.GetPlane(i)))
                return false;

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Frustum other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Near, Far, Left, Right, Bottom, Top);
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
