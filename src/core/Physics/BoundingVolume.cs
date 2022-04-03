// <copyright file="BoundingVolume.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Physics
{
    /// <summary>
    ///     A bounding volume made up out of many boxes. This class is immutable and position-independent.
    /// </summary>
    public class BoundingVolume : IEquatable<BoundingVolume>
    {
        private readonly BoundingVolume[] children;
        private Box3 box;

        /// <summary>
        ///     Create a bounding box.
        /// </summary>
        public BoundingVolume(Vector3 extents)
        {
            box = new Box3(-extents, extents);
            children = Array.Empty<BoundingVolume>();
        }

        /// <summary>
        ///     Create a bounding box with a given offset.
        /// </summary>
        public BoundingVolume(Vector3 offset, Vector3 extents)
        {
            box = new Box3(offset - extents, offset + extents);
            children = Array.Empty<BoundingVolume>();
        }

        /// <summary>
        ///     Create a bounding box with children.
        /// </summary>
        public BoundingVolume(Vector3 extents, params BoundingVolume[] boundingBoxes)
        {
            box = new Box3(-extents, extents);
            children = boundingBoxes;
        }

        /// <summary>
        ///     Create a bounding box with children, and a given offset.
        /// </summary>
        public BoundingVolume(Vector3 offset, Vector3 extents, params BoundingVolume[] boundingBoxes)
        {
            box = new Box3(offset - extents, offset + extents);
            children = boundingBoxes;
        }

        /// <summary>
        ///     Get the center of the bounding box. This is used as offset for child bounding boxes.
        /// </summary>
        public Vector3 Center => box.Center;

        /// <summary>
        ///     Get the extents of the bounding box.
        /// </summary>
        public Vector3 Extents => box.HalfSize;

        /// <summary>
        ///     The minimum point of the box collider.
        /// </summary>
        public Vector3 Min => box.Min;

        /// <summary>
        ///     The maximum point of the box collider.
        /// </summary>
        public Vector3 Max => box.Max;

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
        /// Gets a <see cref="BoundingVolume" /> that has zero size.
        /// </summary>
        public static BoundingVolume Empty => new(Vector3.Zero, Vector3.Zero);

        /// <inheritdoc />
        public bool Equals(BoundingVolume? other)
        {
            return box.Equals(other?.box);
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
        /// Get a collider using this bounding box.
        /// </summary>
        /// <param name="position">The position to place the collider at.</param>
        /// <returns>The created collider.</returns>
        public BoxCollider GetColliderAt(Vector3 position)
        {
            return new(this, position);
        }

        /// <summary>
        ///     Checks if this bounding box or one of its children contain a point.
        /// </summary>
        public bool Contains(Vector3 point)
        {
            if (box.Contains(point))
                return true;

            if (ChildCount == 0) return false;

            for (var i = 0; i < ChildCount; i++)
                if (children[i].Contains(point))
                    return true;

            return false;
        }

        /// <summary>
        /// Check if this <see cref="BoundingVolume" /> intersects with a given box.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Intersects(Box3 other)
        {
            if (Collision.IsIntersecting(box, other))
                return true;

            if (ChildCount == 0) return false;

            for (var i = 0; i < ChildCount; i++)
                if (children[i].Intersects(other))
                    return true;

            return false;
        }

        /// <summary>
        /// Check if this <see cref="BoundingVolume" /> intersects with the given <see cref="Box3" />.
        /// This will also set the collision planes.
        /// </summary>
        public bool Intersects(Box3 other, ref bool x, ref bool y, ref bool z)
        {
            if (Collision.IsIntersecting(box, other, ref x, ref y, ref z))
                return true;

            if (ChildCount == 0) return false;

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
            if (Collision.IsIntersecting(box, ray))
                return true;

            if (ChildCount == 0) return false;

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
}
