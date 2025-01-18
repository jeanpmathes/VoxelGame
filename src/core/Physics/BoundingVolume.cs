// <copyright file="BoundingVolume.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Physics;

/// <summary>
///     A bounding volume made up out of many boxes. This class is immutable and position-independent.
/// </summary>
public sealed class BoundingVolume : IEquatable<BoundingVolume>
{
    private readonly Box3d childBounds;
    private readonly BoundingVolume[] children;

    private BoundingVolume(Box3d box, BoundingVolume[] children)
    {
        Box = box;
        this.children = children;

        if (children.Length == 0)
        {
            childBounds = new Box3d(Vector3d.PositiveInfinity, Vector3d.NegativeInfinity);
        }
        else
        {
            childBounds = children[0].GetChildBoundsOrBounds();

            for (var i = 1; i < children.Length; i++)
            {
                Box3d currentChild = children[i].GetChildBoundsOrBounds();
                childBounds = childBounds.Inflated(currentChild.Min).Inflated(currentChild.Max);
            }
        }
    }

    /// <summary>
    ///     Create a bounding box.
    /// </summary>
    public BoundingVolume(Vector3d extents) : this(new Box3d(-extents, extents), Array.Empty<BoundingVolume>()) {}

    /// <summary>
    ///     Create a bounding box with a given offset.
    /// </summary>
    public BoundingVolume(Vector3d offset, Vector3d extents) : this(
        MathTools.CreateBox3(offset, extents),
        Array.Empty<BoundingVolume>()) {}

    /// <summary>
    ///     Create a bounding box with children.
    /// </summary>
    public BoundingVolume(Vector3d extents, params BoundingVolume[] boundingBoxes) : this(
        new Box3d(-extents, extents),
        boundingBoxes) {}

    /// <summary>
    ///     Create a bounding box with children, and a given offset.
    /// </summary>
    public BoundingVolume(Vector3d offset, Vector3d extents, params BoundingVolume[] boundingBoxes) : this(
        MathTools.CreateBox3(offset, extents),
        boundingBoxes) {}

    /// <summary>
    ///     Get the center of the bounding box. This is used as offset for child bounding boxes.
    /// </summary>
    public Vector3d Center => Box.Center;

    /// <summary>
    ///     Get the extents of the bounding box.
    /// </summary>
    public Vector3d Extents => Box.HalfSize;

    /// <summary>
    ///     The minimum point of the box collider.
    /// </summary>
    public Vector3d Min => Box.Min;

    /// <summary>
    ///     The maximum point of the box collider.
    /// </summary>
    public Vector3d Max => Box.Max;

    /// <summary>
    ///     Get the box that contains all child boxes.
    ///     Should not be used if there are no children.
    /// </summary>
    private Box3d ChildBounds => childBounds;

    private Box3d Box { get; }

    /// <summary>
    ///     Get a child bounding box.
    /// </summary>
    /// <param name="i">The index of the child.</param>
    public BoundingVolume this[Int32 i] => children[i];

    /// <summary>
    ///     Get the number of children.
    /// </summary>
    public Int32 ChildCount => children.Length;

    /// <summary>
    ///     Gets a <see cref="BoundingVolume" /> with the size of a <see cref="Logic.Elements.Block" />.
    /// </summary>
    public static BoundingVolume Block =>
        new(new Vector3d(x: 0.5, y: 0.5, z: 0.5), new Vector3d(x: 0.5, y: 0.5, z: 0.5));

    /// <summary>
    ///     Gets a <see cref="BoundingVolume" /> with the given height.
    /// </summary>
    public static BoundingVolume CrossBlock(Double height = 1.0)
    {
        return new BoundingVolume(
            new Vector3d(x: 0.5, height / 2.0, z: 0.5),
            new Vector3d(x: 0.355, height / 2.0, z: 0.355));
    }

    /// <summary>
    ///     Gets the child bounds of this bounding volume, or the bounds of this bounding volume if it has no children.
    /// </summary>
    private Box3d GetChildBoundsOrBounds()
    {
        return ChildCount == 0 ? Box : ChildBounds;
    }

    /// <summary>
    ///     Get a <see cref="BoundingVolume" /> with a set height.
    /// </summary>
    /// <param name="height">The height of the bounding box, should be a value between 0 and 15.</param>
    /// <returns>The bounding box.</returns>
    public static BoundingVolume BlockWithHeight(Int32 height)
    {
        Single halfHeight = (height + 1) * 0.03125f;

        return new BoundingVolume(
            new Vector3d(x: 0.5f, halfHeight, z: 0.5f),
            new Vector3d(x: 0.5f, halfHeight, z: 0.5f));
    }

    /// <summary>
    ///     Get a collider using this bounding box.
    /// </summary>
    /// <param name="position">The position to place the collider at.</param>
    /// <returns>The created collider.</returns>
    public BoxCollider GetColliderAt(Vector3d position)
    {
        return new BoxCollider(this, position);
    }

    /// <summary>
    ///     Checks if this bounding box or one of its children contain a point.
    /// </summary>
    public Boolean Contains(Vector3d point)
    {
        if (Box.Contains(point, boundaryInclusive: true))
            return true;

        if (ChildCount == 0) return false;
        if (!ChildBounds.Contains(point, boundaryInclusive: true)) return false;

        for (var i = 0; i < ChildCount; i++)
            if (children[i].Contains(point))
                return true;

        return false;
    }

    /// <summary>
    ///     Check if this box intersects a frustum.
    /// </summary>
    public Boolean Intersects(Frustum frustum)
    {
        if (frustum.IsBoxInFrustum(Box)) return true;

        if (ChildCount == 0) return false;
        if (!frustum.IsBoxInFrustum(ChildBounds)) return false;

        for (var i = 0; i < ChildCount; i++)
            if (children[i].Intersects(frustum))
                return true;

        return false;
    }

    /// <summary>
    ///     Check if this <see cref="BoundingVolume" /> intersects with a given box.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public Boolean Intersects(Box3d other)
    {
        if (Collision.IsIntersecting(Box, other))
            return true;

        if (ChildCount == 0) return false;
        if (!Collision.IsIntersecting(ChildBounds, other)) return false;

        for (var i = 0; i < ChildCount; i++)
            if (children[i].Intersects(other))
                return true;

        return false;
    }

    /// <summary>
    ///     Check if this <see cref="BoundingVolume" /> intersects with the given <see cref="Box3" />.
    ///     This will also set the collision planes.
    /// </summary>
    public Boolean Intersects(Box3d other, ref Boolean x, ref Boolean y, ref Boolean z)
    {
        if (Collision.IsIntersecting(Box, other, ref x, ref y, ref z))
            return true;

        Boolean dx = x;
        Boolean dy = y;
        Boolean dz = z;

        if (ChildCount == 0) return false;
        if (!Collision.IsIntersecting(ChildBounds, other, ref dx, ref dy, ref dz)) return false;

        for (var i = 0; i < ChildCount; i++)
            if (children[i].Intersects(other, ref x, ref y, ref z))
                return true;

        return false;
    }

    /// <summary>
    ///     Returns true if the given ray intersects this <see cref="BoundingVolume" /> or any of its children.
    /// </summary>
    public Boolean Intersects(Ray ray)
    {
        if (Collision.IsIntersecting(Box, ray))
            return true;

        if (ChildCount == 0) return false;
        if (!Collision.IsIntersecting(ChildBounds, ray)) return false;

        for (var i = 0; i < ChildCount; i++)
            if (children[i].Intersects(ray))
                return true;

        return false;
    }

    #region EQUALITY

    /// <inheritdoc />
    public Boolean Equals(BoundingVolume? other)
    {
        if (!Box.Equals(other?.Box)) return false;
        if (ChildCount != other.ChildCount) return false;

        for (var i = 0; i < ChildCount; i++)
            if (!children[i].Equals(other.children[i]))
                return false;

        return true;
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        if (obj is BoundingVolume other) return Equals(other);

        return false;
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return HashCode.Combine(Center.GetHashCode(), Extents.GetHashCode(), children);
    }

    #endregion EQUALITY
}
