// <copyright file="BoundingVolume.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Physics;

/// <summary>
///     A bounding volume made up out of many boxes. This class is immutable and position-independent.
/// </summary>
public sealed class BoundingVolume : IEquatable<BoundingVolume>
{
    private readonly Box3 childBounds;
    private readonly BoundingVolume[] children;

    private BoundingVolume(Box3 box, BoundingVolume[] children)
    {
        this.Box = box;
        this.children = children;

        if (children.Length == 0)
        {
            childBounds = new Box3(Vector3.Zero, Vector3.Zero);
        }
        else
        {
            childBounds = children[0].childBounds;

            for (var i = 1; i < children.Length; i++)
            {
                Box3 currentChild = children[i].childBounds;
                childBounds = childBounds.Inflated(currentChild.Min).Inflated(currentChild.Max);
            }
        }
    }

    /// <summary>
    ///     Create a bounding box.
    /// </summary>
    public BoundingVolume(Vector3 extents) : this(new Box3(-extents, extents), Array.Empty<BoundingVolume>()) {}

    /// <summary>
    ///     Create a bounding box with a given offset.
    /// </summary>
    public BoundingVolume(Vector3 offset, Vector3 extents) : this(
        VMath.CreateBox3(offset, extents),
        Array.Empty<BoundingVolume>()) {}

    /// <summary>
    ///     Create a bounding box with children.
    /// </summary>
    public BoundingVolume(Vector3 extents, params BoundingVolume[] boundingBoxes) : this(
        new Box3(-extents, extents),
        boundingBoxes) {}

    /// <summary>
    ///     Create a bounding box with children, and a given offset.
    /// </summary>
    public BoundingVolume(Vector3 offset, Vector3 extents, params BoundingVolume[] boundingBoxes) : this(
        VMath.CreateBox3(offset, extents),
        boundingBoxes) {}

    /// <summary>
    ///     Get the center of the bounding box. This is used as offset for child bounding boxes.
    /// </summary>
    public Vector3 Center => Box.Center;

    /// <summary>
    ///     Get the extents of the bounding box.
    /// </summary>
    public Vector3 Extents => Box.HalfSize;

    /// <summary>
    ///     The minimum point of the box collider.
    /// </summary>
    public Vector3 Min => Box.Min;

    /// <summary>
    ///     The maximum point of the box collider.
    /// </summary>
    public Vector3 Max => Box.Max;

    /// <summary>
    ///     Get the box that contains all child boxes.
    /// </summary>
    private Box3 ChildBounds => childBounds;

    private Box3 Box { get; }

    /// <summary>
    ///     Get a child bounding box.
    /// </summary>
    /// <param name="i">The index of the child.</param>
    public BoundingVolume this[int i] => children[i];

    /// <summary>
    ///     Get the number of children.
    /// </summary>
    public int ChildCount => children.Length;

    /// <summary>
    ///     Gets a <see cref="BoundingVolume" /> with the size of a <see cref="Logic.Block" />.
    /// </summary>
    public static BoundingVolume Block =>
        new(new Vector3(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3(x: 0.5f, y: 0.5f, z: 0.5f));

    /// <summary>
    ///     Gets a <see cref="BoundingVolume" /> with the size of a <see cref="Logic.Blocks.CrossBlock" />.
    /// </summary>
    public static BoundingVolume CrossBlock => new(
        new Vector3(x: 0.5f, y: 0.5f, z: 0.5f),
        new Vector3(x: 0.355f, y: 0.5f, z: 0.355f));

    /// <summary>
    ///     Gets a <see cref="BoundingVolume" /> that has zero size.
    /// </summary>
    public static BoundingVolume Empty => new(Vector3.Zero, Vector3.Zero);

    /// <inheritdoc />
    public bool Equals(BoundingVolume? other)
    {
        return Box.Equals(other?.Box);
    }

    /// <summary>
    ///     Get a <see cref="BoundingVolume" /> with a set height.
    /// </summary>
    /// <param name="height">The height of the bounding box, should be a value between 0 and 15.</param>
    /// <returns>The bounding box.</returns>
    public static BoundingVolume BlockWithHeight(int height)
    {
        float halfHeight = (height + 1) * 0.03125f;

        return new BoundingVolume(
            new Vector3(x: 0.5f, halfHeight, z: 0.5f),
            new Vector3(x: 0.5f, halfHeight, z: 0.5f));
    }

    /// <summary>
    ///     Get a collider using this bounding box.
    /// </summary>
    /// <param name="position">The position to place the collider at.</param>
    /// <returns>The created collider.</returns>
    public BoxCollider GetColliderAt(Vector3 position)
    {
        return new BoxCollider(this, position);
    }

    /// <summary>
    ///     Checks if this bounding box or one of its children contain a point.
    /// </summary>
    public bool Contains(Vector3 point)
    {
        if (Box.Contains(point))
            return true;

        if (ChildCount == 0 || !ChildBounds.Contains(point)) return false;

        for (var i = 0; i < ChildCount; i++)
            if (children[i].Contains(point))
                return true;

        return false;
    }

    /// <summary>
    ///     Check if this <see cref="BoundingVolume" /> intersects with a given box.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Intersects(Box3 other)
    {
        if (Collision.IsIntersecting(Box, other))
            return true;

        if (ChildCount == 0 || !Collision.IsIntersecting(childBounds, other)) return false;

        for (var i = 0; i < ChildCount; i++)
            if (children[i].Intersects(other))
                return true;

        return false;
    }

    /// <summary>
    ///     Check if this <see cref="BoundingVolume" /> intersects with the given <see cref="Box3" />.
    ///     This will also set the collision planes.
    /// </summary>
    public bool Intersects(Box3 other, ref bool x, ref bool y, ref bool z)
    {
        if (Collision.IsIntersecting(Box, other, ref x, ref y, ref z))
            return true;

        bool dx = x;
        bool dy = y;
        bool dz = z;

        if (ChildCount == 0 || !Collision.IsIntersecting(Box, other, ref dx, ref dy, ref dz)) return false;

        for (var i = 0; i < ChildCount; i++)
            if (children[i].Intersects(other, ref x, ref y, ref z))
                return true;

        return false;
    }

    /// <summary>
    ///     Returns true if the given ray intersects this <see cref="BoundingVolume" /> or any of its children.
    /// </summary>
    public bool Intersects(Ray ray)
    {
        if (Collision.IsIntersecting(Box, ray))
            return true;

        if (ChildCount == 0 || !Collision.IsIntersecting(childBounds, ray)) return false;

        for (var i = 0; i < ChildCount; i++)
            if (children[i].Intersects(ray))
                return true;

        return false;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is BoundingVolume other) return Equals(other);

        return false;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Center.GetHashCode(), Extents.GetHashCode(), children);
    }
}
