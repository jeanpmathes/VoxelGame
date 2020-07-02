// <copyright file="BoundingBox.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using System;
using System.Collections.Generic;
using VoxelGame.Logic;

namespace VoxelGame.Physics
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
        /// Gets a <see cref="BoundingBox"/> with the size of a block.
        /// </summary>
        public static BoundingBox Block => new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f));

        /// <summary>
        /// Returns a <see cref="BoundingBox"/> with the size of a block, translated to a specified position.
        /// </summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="z">The z position.</param>
        public static BoundingBox BlockAt(int x, int y, int z)
        {
            return new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f) + new Vector3(x, y, z), new Vector3(0.5f, 0.5f, 0.5f));
        }

        /// <summary>
        /// Returns a translated copy of this <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="z">The z position.</param>
        public BoundingBox Translated(int x, int y, int z)
        {
            return new BoundingBox(Center + new Vector3(x, y, z), Extents, children);
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

        /// <summary>
        /// Checks if this bounding box or one of its children intersects with the given <see cref="BoundingBox"/>.
        /// </summary>
        public bool Intersects(BoundingBox other)
        {
            bool containedInParent =
                this.Min.X <= other.Max.X && this.Max.X >= other.Min.X &&
                this.Min.Y <= other.Max.Y && this.Max.Y >= other.Min.Y &&
                this.Min.Z <= other.Max.Z && this.Max.Z >= other.Min.Z;

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
                    if (children[i].Intersects(other))
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Checks if a ray intersects this bounding box. The length of the ray is not used in the calculations.
        /// </summary>
        /// <param name="ray">The ray to check.</param>
        /// <returns>Returns true if the ray with an assumed infinite length intersect the bounding box.</returns>
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

        private void GetIntersectionPlane_NonRecursive(BoundingBox other, ref bool x, ref bool y, ref bool z)
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
        }

        public void GetIntersectionPlane(BoundingBox other, ref bool x, ref bool y, ref bool z)
        {
            GetIntersectionPlane_NonRecursive(other, ref x, ref y, ref z);

            for (int i = 0; i < ChildCount; i++)
            {
                children[i].GetIntersectionPlane(other, ref x, ref y, ref z);
            }
        }

        private bool IntersectsTerrain_NonRecursive(out bool xCollision, out bool yCollision, out bool zCollision, out List<(int x, int y, int z, Block block)> intersections)
        {
            bool intersects = false;

            xCollision = false;
            yCollision = false;
            zCollision = false;

            intersections = new List<(int, int, int, Block)>();

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
                        Block? current = Game.World.GetBlock(x + xPos, y + yPos, z + zPos, out _);

                        if (current != null)
                        {
                            BoundingBox currentBoundingBox = current.GetBoundingBox(x + xPos, y + yPos, z + zPos);

                            // Check for intersection
                            if ((current.IsSolid || current.IsTrigger) && Intersects(currentBoundingBox))
                            {
                                intersections.Add((x + xPos, y + yPos, z + zPos, current));

                                if (current.IsSolid)
                                {
                                    intersects = true;

                                    GetIntersectionPlane(currentBoundingBox, ref xCollision, ref yCollision, ref zCollision);
                                }
                            }
                        }
                    }
                }
            }

            return intersects;
        }

        public bool IntersectsTerrain(out bool xCollision, out bool yCollision, out bool zCollision, out List<(int x, int y, int z, Block block)> intersections)
        {
            bool isIntersecting = IntersectsTerrain_NonRecursive(out xCollision, out yCollision, out zCollision, out intersections);

            if (ChildCount == 0)
            {
                return isIntersecting;
            }
            else
            {
                for (int i = 0; i < ChildCount; i++)
                {
                    bool childIntersecting = children[i].IntersectsTerrain(out bool childX, out bool childY, out bool childZ, out List<(int x, int y, int z, Block block)> childIntersections);

                    isIntersecting = childIntersecting || isIntersecting;

                    xCollision = childX || xCollision;
                    yCollision = childY || yCollision;
                    zCollision = childZ || zCollision;

                    intersections.AddRange(childIntersections);
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

        public override int GetHashCode()
        {
            return HashCode.Combine(Center.GetHashCode(), Extents.GetHashCode());
        }

        public bool Equals(BoundingBox other)
        {
            return (this.Extents == other.Extents) && (this.Center == other.Center) && children.Equals(other.children);
        }
    }
}