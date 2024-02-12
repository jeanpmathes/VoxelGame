// <copyright file="Frustum.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Physics;

/// <summary>
///     A camera view frustum.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
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
        Debug.Assert(fovY is > 0.0 and < Math.PI);

        Debug.Assert(clip.near < clip.far);
        Debug.Assert(clip.near >= 0.0);

        direction.Normalize();
        up.Normalize();
        right.Normalize();

        this.right = right;
        this.up = up;

        (wNear, hNear) = GetDimensionsAt(clip.near, fovY, ratio);
        (wFar, hFar) = GetDimensionsAt(clip.far, fovY, ratio);

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

    private Vector3d GetPositionOnNearPlane(double x, double y)
    {
        return Near.Point + x * right * wNear / 2.0 + y * up * hNear / 2.0;
    }

    private Vector3d GetPositionOnFarPlane(double x, double y)
    {
        return Far.Point + x * right * wFar / 2.0 + y * up * hFar / 2.0;
    }

    private readonly Vector3d right;
    private readonly Vector3d up;
    private readonly double wNear;
    private readonly double hNear;
    private readonly double wFar;
    private readonly double hFar;

    /// <summary>
    ///     Get the position of the frustum origin.
    /// </summary>
    public Vector3d Position => Top.Point;

    /// <summary>
    ///     Get the front direction of the frustum.
    /// </summary>
    public Vector3d FrontDirection => Top.Normal;

    /// <summary>
    ///     Get the right direction of the frustum.
    /// </summary>
    public Vector3d RightDirection => right;

    /// <summary>
    ///     Get the up direction of the frustum.
    /// </summary>
    public Vector3d UpDirection => up;

    private Frustum(Frustum original, Vector3d offset)
    {
        Near = original.Near.Translated(offset);
        Far = original.Far.Translated(offset);
        Left = original.Left.Translated(offset);
        Right = original.Right.Translated(offset);
        Bottom = original.Bottom.Translated(offset);
        Top = original.Top.Translated(offset);

        right = original.right;
        up = original.up;
        wNear = original.wNear;
        hNear = original.hNear;
        wFar = original.wFar;
        hFar = original.hFar;
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
    ///     Get the dimensions of the near view plane.
    /// </summary>
    public (Vector3d a, Vector3d b) NearDimensions
    {
        get
        {
            Vector3d scaledUp = UpDirection * hNear * 0.5f;
            Vector3d scaledRight = RightDirection * wNear * 0.5f;

            return (Near.Point - scaledUp - scaledRight, Near.Point + scaledUp + scaledRight);
        }
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

    private static void SetBoxNormals(Span<Vector3d> normals)
    {
        normals[index: 0] = Vector3d.UnitX;
        normals[index: 1] = Vector3d.UnitY;
        normals[index: 2] = Vector3d.UnitZ;
    }

    private void SetFrustumNormals(Span<Vector3d> normals)
    {
        normals[index: 0] = Near.Normal;
        normals[index: 1] = Left.Normal;
        normals[index: 2] = Right.Normal;
        normals[index: 3] = Bottom.Normal;
        normals[index: 4] = Top.Normal;
    }

    private void SetCrossEdges(Span<Vector3d> normals)
    {
        void AddEdge(Span<Vector3d> destination, Vector3d edge)
        {
            destination[index: 0] = Vector3d.Cross(edge, Vector3d.UnitX);
            destination[index: 1] = Vector3d.Cross(edge, Vector3d.UnitY);
            destination[index: 2] = Vector3d.Cross(edge, Vector3d.UnitZ);
        }

        Vector3d edge0 = GetPositionOnFarPlane(x: -1.0, y: -1.0) - GetPositionOnFarPlane(x: 1.0, y: -1.0);
        AddEdge(normals[..], edge0);

        Vector3d edge1 = GetPositionOnFarPlane(x: -1.0, y: -1.0) - GetPositionOnFarPlane(x: -1.0, y: 1.0);
        AddEdge(normals[3..], edge1);

        Vector3d edge2 = GetPositionOnFarPlane(x: -1.0, y: -1.0) - GetPositionOnNearPlane(x: -1.0, y: -1.0);
        AddEdge(normals[6..], edge2);

        Vector3d edge3 = GetPositionOnFarPlane(x: -1.0, y: 1.0) - GetPositionOnNearPlane(x: -1.0, y: 1.0);
        AddEdge(normals[9..], edge3);

        Vector3d edge4 = GetPositionOnFarPlane(x: 1.0, y: -1.0) - GetPositionOnNearPlane(x: 1.0, y: -1.0);
        AddEdge(normals[12..], edge4);

        Vector3d edge5 = GetPositionOnFarPlane(x: 1.0, y: 1.0) - GetPositionOnNearPlane(x: 1.0, y: 1.0);
        AddEdge(normals[15..], edge5);
    }

    private static (double min, double max) ProjectBox(Box3d box, Vector3d axis)
    {
        double radius = Math.Abs(Vector3d.Dot(box.HalfSize, axis.Absolute()));
        double distance = Vector3d.Dot(box.Center, axis);

        return (distance - radius, distance + radius);
    }

    private (double min, double max) ProjectFrustum(Vector3d axis)
    {
        Span<Vector3d> corners = stackalloc Vector3d[8];

        corners[index: 0] = GetPositionOnNearPlane(x: -1.0, y: -1.0);
        corners[index: 1] = GetPositionOnNearPlane(x: -1.0, y: 1.0);
        corners[index: 2] = GetPositionOnNearPlane(x: 1.0, y: -1.0);
        corners[index: 3] = GetPositionOnNearPlane(x: 1.0, y: 1.0);

        corners[index: 4] = GetPositionOnFarPlane(x: -1.0, y: -1.0);
        corners[index: 5] = GetPositionOnFarPlane(x: -1.0, y: 1.0);
        corners[index: 6] = GetPositionOnFarPlane(x: 1.0, y: -1.0);
        corners[index: 7] = GetPositionOnFarPlane(x: 1.0, y: 1.0);

        var min = double.MaxValue;
        var max = double.MinValue;

        foreach (Vector3d corner in corners)
        {
            double dot = Vector3d.Dot(axis, corner);
            min = Math.Min(min, dot);
            max = Math.Max(max, dot);
        }

        return (min, max);
    }

    /// <summary>
    ///     Check whether a <see cref="Box3" /> is inside this <see cref="Frustum" />.
    ///     This is an exact test which is more expensive than <see cref="IsBoxVisible" />.
    ///     No false-positive results are allowed.
    /// </summary>
    /// <returns>true if the <see cref="Box3" /> is inside; false if not.</returns>
    public bool IsBoxInFrustum(Box3d volume)
    {
        const int boxNormalCount = 3;
        const int frustumNormalCount = 5;
        const int crossEdgeCount = 3 * 6;

        Span<Vector3d> normals = stackalloc Vector3d[boxNormalCount + frustumNormalCount + crossEdgeCount];

        SetBoxNormals(normals);
        SetFrustumNormals(normals[boxNormalCount..]);
        SetCrossEdges(normals[(boxNormalCount + frustumNormalCount)..]);

        foreach (Vector3d normal in normals)
        {
            (double min, double max) boxProjection = ProjectBox(volume, normal);
            (double min, double max) frustumProjection = ProjectFrustum(normal);

            if (boxProjection.max < frustumProjection.min || boxProjection.min > frustumProjection.max) return false;
        }

        return true;
    }

    /// <summary>
    ///     Check whether a <see cref="Box3" /> is visible in this <see cref="Frustum" />.
    ///     This differs from <see cref="IsBoxInFrustum" /> in that false-positives are possible,
    ///     e.g. a box is not culled despite being outside the frustum.
    /// </summary>
    /// <param name="box">The box to check.</param>
    /// <param name="tolerance">The tolerance (in world units) to use, which is an extension around the frustum.</param>
    /// <returns><c>true</c> if the <see cref="Box3" /> is visible; <c>false</c> if not.</returns>
    public bool IsBoxVisible(Box3d box, double tolerance = 0.0)
    {
        for (var i = 0; i < 6; i++)
        {
            Plane plane = GetPlane(i);

            double px = plane.Normal.X < 0 ? box.Min.X : box.Max.X;
            double py = plane.Normal.Y < 0 ? box.Min.Y : box.Max.Y;
            double pz = plane.Normal.Z < 0 ? box.Min.Z : box.Max.Z;

            if (plane.GetDistanceTo(new Vector3d(px, py, pz)) < -tolerance)
                return false;
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
