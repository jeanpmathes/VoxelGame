// <copyright file="BoundingBox.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Linq;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Physics
{
    public struct BoundingBox : IEquatable<BoundingBox>
    {
        public Vector3 Center { get; set; }
        public Vector3 Extents { get; }

        public Vector3 Min => Center + -Extents;
        public Vector3 Max => Center + Extents;

        private readonly BoundingBox[] children;

        public BoundingBox this[int i] => children[i];

        public int ChildCount => children.Length;

        public BoundingBox(Vector3 center, Vector3 extents)
        {
            Center = center;
            Extents = extents;

            children = Array.Empty<BoundingBox>();
        }

        public BoundingBox(Vector3 center, Vector3 extents, params BoundingBox[] boundingBoxes)
        {
            Center = center;
            Extents = extents;

            children = boundingBoxes;
        }

        /// <summary>
        ///     Gets a <see cref="BoundingBox" /> with the size of a <see cref="Logic.Block" />.
        /// </summary>
        public static BoundingBox Block =>
            new(new Vector3(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3(x: 0.5f, y: 0.5f, z: 0.5f));

        /// <summary>
        ///     Gets a <see cref="BoundingBox" /> with the size of a <see cref="Logic.Blocks.CrossBlock" />.
        /// </summary>
        public static BoundingBox CrossBlock => new(
            new Vector3(x: 0.5f, y: 0.5f, z: 0.5f),
            new Vector3(x: 0.355f, y: 0.5f, z: 0.355f));

        /// <summary>
        ///     Get a <see cref="BoundingBox" /> with a set height.
        /// </summary>
        /// <param name="height">The height of the bounding box, should be a value between 0 and 15.</param>
        /// <returns>The bounding box.</returns>
        public static BoundingBox BlockWithHeight(int height)
        {
            float halfHeight = (height + 1) * 0.03125f;

            return new BoundingBox(
                new Vector3(x: 0.5f, halfHeight, z: 0.5f),
                new Vector3(x: 0.5f, halfHeight, z: 0.5f));
        }

        /// <summary>
        ///     Returns a translated copy of this <see cref="BoundingBox" />.
        /// </summary>
        /// <param name="translation">The translation vector.</param>
        /// <returns>The translated bounding box.</returns>
        public BoundingBox Translated(Vector3i translation)
        {
            BoundingBox[] translatedChildren = new BoundingBox[ChildCount];

            for (var i = 0; i < ChildCount; i++) translatedChildren[i] = children[i].Translated(translation);

            return new BoundingBox(Center + translation.ToVector3(), Extents, translatedChildren);
        }

        /// <summary>
        ///     Checks if this bounding box or one of its children contain a point.
        /// </summary>
        public bool Contains(Vector3 point)
        {
            bool containedInParent =
                Min.X <= point.X && Max.X >= point.X &&
                Min.Y <= point.Y && Max.Y >= point.Y &&
                Min.Z <= point.Z && Max.Z >= point.Z;

            if (containedInParent)
                return true;

            if (ChildCount == 0) return false;

            for (var i = 0; i < ChildCount; i++)
                if (children[i].Contains(point))
                    return true;

            return false;
        }

        private bool Intersects_NonRecursive(BoundingBox other, ref bool x, ref bool y, ref bool z)
        {
            if (Min.X <= other.Max.X && Max.X >= other.Min.X &&
                Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
                Min.Z <= other.Max.Z && Max.Z >= other.Min.Z)
            {
                float inverseOverlap;

                // Check on which plane the collision happened
                float xOverlap = Max.X - other.Min.X;
                inverseOverlap = other.Max.X - Min.X;
                xOverlap = xOverlap < inverseOverlap ? xOverlap : inverseOverlap;

                float yOverlap = Max.Y - other.Min.Y;
                inverseOverlap = other.Max.Y - Min.Y;
                yOverlap = yOverlap < inverseOverlap ? yOverlap : inverseOverlap;

                float zOverlap = Max.Z - other.Min.Z;
                inverseOverlap = other.Max.Z - Min.Z;
                zOverlap = zOverlap < inverseOverlap ? zOverlap : inverseOverlap;

                if (xOverlap < yOverlap)
                {
                    if (xOverlap < zOverlap) x = true;
                    else z = true;
                }
                else
                {
                    if (yOverlap < zOverlap) y = true;
                    else z = true;
                }

                return true;
            }

            return false;
        }

        private bool Intersects_NonRecursive(Ray ray)
        {
            if (Contains(ray.Origin) || Contains(ray.EndPoint)) return true;

            var dirfrac = new Vector3
            {
                X = 1.0f / ray.Direction.X,
                Y = 1.0f / ray.Direction.Y,
                Z = 1.0f / ray.Direction.Z
            };

            float t1 = (Min.X - ray.Origin.X) * dirfrac.X;
            float t2 = (Max.X - ray.Origin.X) * dirfrac.X;

            float t3 = (Min.Y - ray.Origin.Y) * dirfrac.Y;
            float t4 = (Max.Y - ray.Origin.Y) * dirfrac.Y;

            float t5 = (Min.Z - ray.Origin.Z) * dirfrac.Z;
            float t6 = (Max.Z - ray.Origin.Z) * dirfrac.Z;

            float tmin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
            float tmax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

            if (tmax < 0f) return false;

            return tmin <= tmax;
        }

        private bool Intersects_OneWithAll(BoundingBox one, ref bool x, ref bool y, ref bool z)
        {
            bool intersects = Intersects_NonRecursive(one, ref x, ref y, ref z);

            for (var i = 0; i < ChildCount; i++)
                if (children[i].Intersects_OneWithAll(one, ref x, ref y, ref z))
                    intersects = true;

            return intersects;
        }

        private bool Intersects_OneWithAll(BoundingBox one)
        {
            if (Min.X <= one.Max.X && Max.X >= one.Min.X &&
                Min.Y <= one.Max.Y && Max.Y >= one.Min.Y &&
                Min.Z <= one.Max.Z && Max.Z >= one.Min.Z) return true;

            for (var i = 0; i < ChildCount; i++)
                if (children[i].Intersects_OneWithAll(one))
                    return true;

            return false;
        }

        /// <summary>
        ///     Checks if this <see cref="BoundingBox" /> intersects with the given <see cref="BoundingBox" /> and sets the
        ///     collision planes.
        /// </summary>
        public bool Intersects(BoundingBox other, ref bool x, ref bool y, ref bool z)
        {
            bool intersects = other.Intersects_OneWithAll(this, ref x, ref y, ref z);

            for (var i = 0; i < ChildCount; i++)
                if (children[i].Intersects(other))
                    intersects = true;

            return intersects;
        }

        /// <summary>
        ///     Checks if this <see cref="BoundingBox" /> or one of its children intersects with the given
        ///     <see cref="BoundingBox" /> or its children.
        /// </summary>
        public bool Intersects(BoundingBox other)
        {
            if (other.Intersects_OneWithAll(this))
                return true;

            for (var i = 0; i < ChildCount; i++)
                if (children[i].Intersects(other))
                    return true;

            return false;
        }

        /// <summary>
        ///     Returns true if the given ray intersects this <see cref="BoundingBox" /> or any of its children.
        /// </summary>
        public bool Intersects(Ray ray)
        {
            bool intersectsParent = Intersects_NonRecursive(ray);

            if (intersectsParent)
                return true;

            if (ChildCount == 0) return false;

            for (var i = 0; i < ChildCount; i++)
                if (children[i].Intersects(ray))
                    return true;

            return false;
        }

        private bool IntersectsTerrain_NonRecursive(World world, out bool xCollision, out bool yCollision,
            out bool zCollision, ISet<(Vector3i position, Block block)> blockIntersections,
            ISet<(Vector3i position, Liquid liquid, LiquidLevel level)> liquidIntersections)
        {
            var intersects = false;

            xCollision = false;
            yCollision = false;
            zCollision = false;

            // Calculate the range of blocks to check
            float highestExtent = Extents.X > Extents.Y ? Extents.X : Extents.Y;
            highestExtent = highestExtent > Extents.Z ? highestExtent : Extents.Z;

            int range = (int) Math.Round(highestExtent * 2, MidpointRounding.AwayFromZero) + 1;

            if (range % 2 == 0) range++;

            // Get the current position in world coordinates
            Vector3i center = Center.Floor();

            // Loop through the world and check for collisions
            for (int x = (range - 1) / -2; x <= (range - 1) / 2; x++)
            for (int y = (range - 1) / -2; y <= (range - 1) / 2; y++)
            for (int z = (range - 1) / -2; z <= (range - 1) / 2; z++)
            {
                Vector3i position = center + new Vector3i(x, y, z);

                (Block? currentBlock, Liquid? currentLiquid) = world.GetPosition(
                    position,
                    out _,
                    out LiquidLevel level,
                    out _);

                if (currentBlock != null)
                {
                    BoundingBox currentBoundingBox = currentBlock.GetBoundingBox(
                        world,
                        position);

                    bool newX = false, newY = false, newZ = false;

                    // Check for intersection
                    if ((currentBlock.IsSolid || currentBlock.IsTrigger) && Intersects(
                        currentBoundingBox,
                        ref newX,
                        ref newY,
                        ref newZ))
                    {
                        blockIntersections.Add((position, currentBlock));

                        if (currentBlock.IsSolid)
                        {
                            intersects = true;

                            xCollision |= newX;
                            yCollision |= newY;
                            zCollision |= newZ;
                        }
                    }
                }

                if (currentLiquid?.CheckContact == true)
                {
                    BoundingBox currentBoundingBox = Liquid.GetBoundingBox(position, level);

                    if (Intersects(currentBoundingBox))
                        liquidIntersections.Add((position, currentLiquid, level));
                }
            }

            return intersects;
        }

        /// <summary>
        ///     Calculate all intersections of a <see cref="BoundingBox" /> with the terrain.
        /// </summary>
        public bool IntersectsTerrain(World world, out bool xCollision, out bool yCollision, out bool zCollision,
            HashSet<(Vector3i position, Block block)> blockIntersections,
            HashSet<(Vector3i position, Liquid liquid, LiquidLevel level)> liquidIntersections)
        {
            bool isIntersecting = IntersectsTerrain_NonRecursive(
                world,
                out xCollision,
                out yCollision,
                out zCollision,
                blockIntersections,
                liquidIntersections);

            if (ChildCount == 0) return isIntersecting;

            for (var i = 0; i < ChildCount; i++)
            {
                bool childIntersecting = children[i].IntersectsTerrain(
                    world,
                    out bool childX,
                    out bool childY,
                    out bool childZ,
                    blockIntersections,
                    liquidIntersections);

                isIntersecting = childIntersecting || isIntersecting;

                xCollision = childX || xCollision;
                yCollision = childY || yCollision;
                zCollision = childZ || zCollision;
            }

            return isIntersecting;
        }

        public static bool operator ==(BoundingBox left, BoundingBox right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BoundingBox left, BoundingBox right)
        {
            return !(left == right);
        }

        public static bool Equals(BoundingBox left, BoundingBox right)
        {
            return left.Extents == right.Extents && left.Center == right.Center;
        }

        public override bool Equals(object? obj)
        {
            if (obj is BoundingBox other) return Equals(other);

            return false;
        }

        public bool Equals(BoundingBox other)
        {
            return Extents == other.Extents && Center == other.Center && children.SequenceEqual(other.children);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Center.GetHashCode(), Extents.GetHashCode(), children);
        }
    }
}
