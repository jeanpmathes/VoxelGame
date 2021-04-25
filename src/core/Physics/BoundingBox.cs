// <copyright file="BoundingBox.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using System;
using System.Collections.Generic;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Physics
{
    public struct BoundingBox : IEquatable<BoundingBox>
    {
        public Vector3 Center { get; set; }
        public Vector3 Extents { get; set; }

        public Vector3 Min => Center + -Extents;
        public Vector3 Max => Center + Extents;

        private readonly BoundingBox[] children;

        public BoundingBox this[int i]
        {
            get => children[i];
            private set => children[i] = value;
        }

        public int ChildCount
        {
            get => children.Length;
        }

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
        /// Gets a <see cref="BoundingBox"/> with the size of a <see cref="Logic.Block"/>.
        /// </summary>
        public static BoundingBox Block => new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f));

        /// <summary>
        /// Gets a <see cref="BoundingBox"/> with the size of a <see cref="Logic.Blocks.CrossBlock"/>.
        /// </summary>
        public static BoundingBox CrossBlock => new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.355f, 0.5f, 0.355f));

        /// <summary>
        /// Returns a <see cref="BoundingBox"/> with the size of a block, translated to a specified position.
        /// </summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="z">The z position.</param>
        public static BoundingBox BlockAt(int x, int y, int z)
        {
            return new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f) + (x, y, z), new Vector3(0.5f, 0.5f, 0.5f));
        }

        /// <summary>
        /// Get a <see cref="BoundingBox"/> with a set height at a given position.
        /// </summary>
        /// <param name="height">The height of the bounding box, should be a value between 0 and 15.</param>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="z">The z position.</param>
        /// <returns>The bounding box.</returns>
        public static BoundingBox BlockAt(int height, int x, int y, int z)
        {
            float halfHeight = (height + 1) * 0.03125f;
            return new BoundingBox(new Vector3(0.5f, halfHeight, 0.5f) + (x, y, z), new Vector3(0.5f, halfHeight, 0.5f));
        }

        /// <summary>
        /// Get a <see cref="BoundingBox"/> with a set height.
        /// </summary>
        /// <param name="height">The height of the bounding box, should be a value between 0 and 15.</param>
        /// <returns>The bounding box.</returns>
        public static BoundingBox BlockWithHeight(int height)
        {
            float halfHeight = (height + 1) * 0.03125f;
            return new BoundingBox(new Vector3(0.5f, halfHeight, 0.5f), new Vector3(0.5f, halfHeight, 0.5f));
        }

        /// <summary>
        /// Returns a translated copy of this <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="z">The z position.</param>
        public BoundingBox Translated(int x, int y, int z)
        {
            BoundingBox[] translatedChildren = new BoundingBox[ChildCount];

            for (int i = 0; i < ChildCount; i++)
            {
                translatedChildren[i] = children[i].Translated(x, y, z);
            }

            return new BoundingBox(Center + new Vector3(x, y, z), Extents, translatedChildren);
        }

        /// <summary>
        /// Checks if this bounding box or one of its children contain a point.
        /// </summary>
        public bool Contains(Vector3 point)
        {
            bool containedInParent =
                Min.X <= point.X && Max.X >= point.X &&
                Min.Y <= point.Y && Max.Y >= point.Y &&
                Min.Z <= point.Z && Max.Z >= point.Z;

            if (containedInParent)
                return true;

            if (ChildCount == 0)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < ChildCount; i++)
                {
                    if (children[i].Contains(point))
                        return true;
                }

                return false;
            }
        }

        private bool Intersects_NonRecursive(BoundingBox other, ref bool x, ref bool y, ref bool z)
        {
            if (this.Min.X <= other.Max.X && this.Max.X >= other.Min.X &&
                this.Min.Y <= other.Max.Y && this.Max.Y >= other.Min.Y &&
                this.Min.Z <= other.Max.Z && this.Max.Z >= other.Min.Z)
            {
                float inverseOverlap;

                // Check on which plane the collision happened
                float xOverlap = this.Max.X - other.Min.X;
                inverseOverlap = other.Max.X - this.Min.X;
                xOverlap = (xOverlap < inverseOverlap) ? xOverlap : inverseOverlap;

                float yOverlap = this.Max.Y - other.Min.Y;
                inverseOverlap = other.Max.Y - this.Min.Y;
                yOverlap = (yOverlap < inverseOverlap) ? yOverlap : inverseOverlap;

                float zOverlap = this.Max.Z - other.Min.Z;
                inverseOverlap = other.Max.Z - this.Min.Z;
                zOverlap = (zOverlap < inverseOverlap) ? zOverlap : inverseOverlap;

                if (xOverlap < yOverlap)
                {
                    if (xOverlap < zOverlap)
                    {
                        x = true;
                    }
                    else
                    {
                        z = true;
                    }
                }
                else
                {
                    if (yOverlap < zOverlap)
                    {
                        y = true;
                    }
                    else
                    {
                        z = true;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private bool Intersects_NonRecursive(Ray ray)
        {
            if (Contains(ray.Origin) || Contains(ray.EndPoint))
            {
                return true;
            }

            Vector3 dirfrac = new Vector3()
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

            if (tmax < 0f)
            {
                return false;
            }

            return tmin <= tmax;
        }

        private bool Intersects_OneWithAll(BoundingBox one, ref bool x, ref bool y, ref bool z)
        {
            bool intersects = false;

            if (Intersects_NonRecursive(one, ref x, ref y, ref z))
                intersects = true;

            for (int i = 0; i < ChildCount; i++)
            {
                if (children[i].Intersects_OneWithAll(one, ref x, ref y, ref z))
                    intersects = true;
            }

            return intersects;
        }

        private bool Intersects_OneWithAll(BoundingBox one)
        {
            if (this.Min.X <= one.Max.X && this.Max.X >= one.Min.X &&
                this.Min.Y <= one.Max.Y && this.Max.Y >= one.Min.Y &&
                this.Min.Z <= one.Max.Z && this.Max.Z >= one.Min.Z)
            {
                return true;
            }

            for (int i = 0; i < ChildCount; i++)
            {
                if (children[i].Intersects_OneWithAll(one))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if this <see cref="BoundingBox"/> intersects with the given <see cref="BoundingBox"/> and sets the collision planes.
        /// </summary>
        public bool Intersects(BoundingBox other, ref bool x, ref bool y, ref bool z)
        {
            bool intersects = false;

            if (other.Intersects_OneWithAll(this, ref x, ref y, ref z))
                intersects = true;

            for (int i = 0; i < ChildCount; i++)
            {
                if (children[i].Intersects(other))
                    intersects = true;
            }

            return intersects;
        }

        /// <summary>
        /// Checks if this <see cref="BoundingBox"/> or one of its children intersects with the given <see cref="BoundingBox"/> or its children.
        /// </summary>
        public bool Intersects(BoundingBox other)
        {
            if (other.Intersects_OneWithAll(this))
                return true;

            for (int i = 0; i < ChildCount; i++)
            {
                if (children[i].Intersects(other))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the given ray intersects this <see cref="BoundingBox"/> or any of its children.
        /// </summary>
        public bool Intersects(Ray ray)
        {
            bool intersectsParent = Intersects_NonRecursive(ray);

            if (intersectsParent)
                return true;

            if (ChildCount == 0)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < ChildCount; i++)
                {
                    if (children[i].Intersects(ray))
                        return true;
                }

                return false;
            }
        }

        private bool IntersectsTerrain_NonRecursive(out bool xCollision, out bool yCollision, out bool zCollision, ref HashSet<(int x, int y, int z, Block block)> blockIntersections, ref HashSet<(int x, int y, int z, Liquid liquid, LiquidLevel level)> liquidIntersections)
        {
            bool intersects = false;

            xCollision = false;
            yCollision = false;
            zCollision = false;

            // Calculate the range of blocks to check
            float highestExtent = (Extents.X > Extents.Y) ? Extents.X : Extents.Y;
            highestExtent = (highestExtent > Extents.Z) ? highestExtent : Extents.Z;

            int range = (int)Math.Round(highestExtent * 2, MidpointRounding.AwayFromZero) + 1;
            if (range % 2 == 0)
            {
                range++;
            }

            // Get the current position in world coordinates
            int xPos = (int)Math.Floor(Center.X);
            int yPos = (int)Math.Floor(Center.Y);
            int zPos = (int)Math.Floor(Center.Z);

            // Loop through the world and check for collisions
            for (int x = (range - 1) / -2; x <= (range - 1) / 2; x++)
            {
                for (int y = (range - 1) / -2; y <= (range - 1) / 2; y++)
                {
                    for (int z = (range - 1) / -2; z <= (range - 1) / 2; z++)
                    {
                        (Block? currentBlock, Liquid? currentLiquid) = Game.World.GetPosition(x + xPos, y + yPos, z + zPos, out _, out LiquidLevel level, out _);

                        if (currentBlock != null)
                        {
                            BoundingBox currentBoundingBox = currentBlock.GetBoundingBox(x + xPos, y + yPos, z + zPos);

                            bool newX = false, newY = false, newZ = false;

                            // Check for intersection
                            if ((currentBlock.IsSolid || currentBlock.IsTrigger) && Intersects(currentBoundingBox, ref newX, ref newY, ref newZ))
                            {
                                blockIntersections.Add((x + xPos, y + yPos, z + zPos, currentBlock));

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
                            BoundingBox currentBoundingBox = Liquid.GetBoundingBox(x + xPos, y + yPos, z + zPos, level);

                            if (Intersects(currentBoundingBox))
                            {
                                liquidIntersections.Add((x + xPos, y + yPos, z + zPos, currentLiquid, level));
                            }
                        }
                    }
                }
            }

            return intersects;
        }

        /// <summary>
        /// Calculate all intersections of a <see cref="BoundingBox"/> with the terrain.
        /// </summary>
        public bool IntersectsTerrain(out bool xCollision, out bool yCollision, out bool zCollision, ref HashSet<(int x, int y, int z, Block block)> blockIntersections, ref HashSet<(int x, int y, int z, Liquid liquid, LiquidLevel level)> liquidIntersections)
        {
            bool isIntersecting = IntersectsTerrain_NonRecursive(out xCollision, out yCollision, out zCollision, ref blockIntersections, ref liquidIntersections);

            if (ChildCount == 0)
            {
                return isIntersecting;
            }
            else
            {
                for (int i = 0; i < ChildCount; i++)
                {
                    bool childIntersecting = children[i].IntersectsTerrain(out bool childX, out bool childY, out bool childZ, ref blockIntersections, ref liquidIntersections);

                    isIntersecting = childIntersecting || isIntersecting;

                    xCollision = childX || xCollision;
                    yCollision = childY || yCollision;
                    zCollision = childZ || zCollision;
                }

                return isIntersecting;
            }
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
            return (left.Extents == right.Extents) && (left.Center == right.Center);
        }

        public override bool Equals(object? obj)
        {
            if (obj is BoundingBox other)
            {
                return (this.Extents == other.Extents) && (this.Center == other.Center) && children.Equals(other.children);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(BoundingBox other)
        {
            return (this.Extents == other.Extents) && (this.Center == other.Center) && children.Equals(other.children);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Center.GetHashCode(), Extents.GetHashCode());
        }
    }
}