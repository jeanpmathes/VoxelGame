﻿// <copyright file="BoundingBox.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using System;
using System.Collections.Generic;

using VoxelGame.Logic;

namespace VoxelGame.Physics
{
    public struct BoundingBox
    {
        public Vector3 Center { get; set; }
        public Vector3 Extents { get; set; }

        public Vector3 Min => Center + -Extents;
        public Vector3 Max => Center + Extents;

        public BoundingBox(Vector3 center, Vector3 extents)
        {
            Center = center;
            Extents = extents;
        }

        public static BoundingBox Block => new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f));

        public bool Contains(Vector3 point)
        {
            return (Min.X <= point.X && Max.X >= point.X) &&
                (Min.Y <= point.Y && Max.Y >= point.Y) &&
                (Min.Z <= point.Z && Max.Z >= point.Z);
        }

        public bool Intersects(BoundingBox other)
        {
            return (this.Min.X <= other.Max.X && this.Max.X >= other.Min.X) &&
                (this.Min.Y <= other.Max.Y && this.Max.Y >= other.Min.Y) &&
                (this.Min.Z <= other.Max.Z && this.Max.Z >= other.Min.Z);
        }

        /// <summary>
        /// Checks if a ray intersects this bounding box. The length of the ray is not used in the calculations.
        /// </summary>
        /// <param name="ray">The ray to check.</param>
        /// <returns>Returns true if the ray with an assumed infinite length intersect the bounding box.</returns>
        public bool Intersects(Ray ray)
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

            if (tmin > tmax)
            {
                return false;
            }

            return true;
        }

        public bool IntersectsTerrain(out bool xCollision, out bool yCollision, out bool zCollision)
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
                        Block current = Game.World.GetBlock(x + xPos, y + yPos, z + zPos);

                        if (current != null)
                        {
                            BoundingBox currentBoundingBox = current.GetBoundingBox(x + xPos, y + yPos, z + zPos);

                            // Check for intersection
                            if (current.IsSolid && Intersects(currentBoundingBox))
                            {
                                intersects = true;

                                float inverseOverlap;

                                // Check on which plane the collision happened
                                float xOverlap = this.Max.X - currentBoundingBox.Min.X;
                                inverseOverlap = currentBoundingBox.Max.X - this.Min.X;
                                xOverlap = (xOverlap < inverseOverlap) ? xOverlap : inverseOverlap;

                                float yOverlap = this.Max.Y - currentBoundingBox.Min.Y;
                                inverseOverlap = currentBoundingBox.Max.Y - this.Min.Y;
                                yOverlap = (yOverlap < inverseOverlap) ? yOverlap : inverseOverlap;

                                float zOverlap = this.Max.Z - currentBoundingBox.Min.Z;
                                inverseOverlap = currentBoundingBox.Max.Z - this.Min.Z;
                                zOverlap = (zOverlap < inverseOverlap) ? zOverlap : inverseOverlap;
                                
                                if (xOverlap < yOverlap)
                                {
                                    if (xOverlap < zOverlap)
                                    {
                                        xCollision = true;
                                    }
                                    else
                                    {
                                        zCollision = true;
                                    }
                                }
                                else
                                {
                                    if (yOverlap < zOverlap)
                                    {
                                        yCollision = true;
                                    }
                                    else
                                    {
                                        zCollision = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return intersects;
        }

        public bool IntersectsTerrain(out bool xCollision, out bool yCollision, out bool zCollision, out List<(int, int, int, Block)> intersections)
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
                        Block current = Game.World.GetBlock(x + xPos, y + yPos, z + zPos);

                        if (current != null)
                        {
                            BoundingBox currentBoundingBox = current.GetBoundingBox(x + xPos, y + yPos, z + zPos);

                            // Check for intersection
                            if ((current.IsSolid || current.IsTrigger) && Intersects(currentBoundingBox))
                            {
                                intersections.Add((x, y, z, current));

                                if (current.IsSolid)
                                {
                                    intersects = true;

                                    float inverseOverlap;

                                    // Check on which plane the collision happened
                                    float xOverlap = this.Max.X - currentBoundingBox.Min.X;
                                    inverseOverlap = currentBoundingBox.Max.X - this.Min.X;
                                    xOverlap = (xOverlap < inverseOverlap) ? xOverlap : inverseOverlap;

                                    float yOverlap = this.Max.Y - currentBoundingBox.Min.Y;
                                    inverseOverlap = currentBoundingBox.Max.Y - this.Min.Y;
                                    yOverlap = (yOverlap < inverseOverlap) ? yOverlap : inverseOverlap;

                                    float zOverlap = this.Max.Z - currentBoundingBox.Min.Z;
                                    inverseOverlap = currentBoundingBox.Max.Z - this.Min.Z;
                                    zOverlap = (zOverlap < inverseOverlap) ? zOverlap : inverseOverlap;

                                    if (xOverlap < yOverlap)
                                    {
                                        if (xOverlap < zOverlap)
                                        {
                                            xCollision = true;
                                        }
                                        else
                                        {
                                            zCollision = true;
                                        }
                                    }
                                    else
                                    {
                                        if (yOverlap < zOverlap)
                                        {
                                            yCollision = true;
                                        }
                                        else
                                        {
                                            zCollision = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return intersects;
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
    }
}
