// <copyright file="Raycast.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using System;

using VoxelGame.Logic;

namespace VoxelGame.Physics
{
    public static class Raycast
    {
        static int looool = 0;

        public static bool Cast(Ray ray, out int hitX, out int hitY, out int hitZ)
        {
            looool++;

            //if (looool > 60)
            //{
            //    System.Diagnostics.Debugger.Break();
            //}

            // Get the origin position in world coordinates
            int x = (int)Math.Floor(ray.Origin.X);
            int y = (int)Math.Floor(ray.Origin.Y);
            int z = (int)Math.Floor(ray.Origin.Z);

            // Get the end position in world coordinates
            int endX = (int)Math.Floor(ray.EndPoint.X);
            int endY = (int)Math.Floor(ray.EndPoint.Y);
            int endZ = (int)Math.Floor(ray.EndPoint.Z);

            // Calculate the deltas for all axes
            int deltaX = Math.Abs(endX - x);
            int deltaY = Math.Abs(endY - y);
            int deltaZ = Math.Abs(endZ - z);

            // Calculate the sign for all axes
            int signX = (x < endX) ? 1 : -1;
            int signY = (y < endY) ? 1 : -1;
            int signZ = (z < endZ) ? 1 : -1;

            // Check which axis is the main axis
            if (deltaX >= deltaY && deltaX >= deltaZ) // X axis
            {
                // Calculate slope error
                int slopeErrorY = 2 * deltaY - deltaX;
                int slopeErrorZ = 2 * deltaZ - deltaX;

                // Loop along the axis
                while (x != endX)
                {
                    x += signX;

                    bool atBlock = false;

                    if (slopeErrorY >= 0)
                    {
                        y += signY;
                        slopeErrorY -= 2 * deltaX;

                        atBlock = true;
                    }

                    if (slopeErrorZ >= 0)
                    {
                        z += signZ;
                        slopeErrorZ -= 2 * deltaX;

                        atBlock = true;
                    }

                    if (atBlock)
                    {
                        Block block = Game.World.GetBlock(x, y, z);

                        if (block != null && block != Block.AIR)
                        {
                            // Check if the ray intersects the bounding box of the block
                            if (block.GetBoundingBox(x, y, z).Intersects(ray))
                            {
                                hitX = x;
                                hitY = y;
                                hitZ = z;

                                return true;
                            }
                        }
                    }

                    slopeErrorY += 2 * deltaY;
                    slopeErrorZ += 2 * deltaZ;
                }
            }
            else if (deltaY >= deltaX && deltaY >= deltaZ) // Y axis
            {
                // Calculate slope error
                int slopeErrorX = 2 * deltaX - deltaY;
                int slopeErrorZ = 2 * deltaZ - deltaY;

                // Loop along the axis
                while (y != endY)
                {
                    y += signY;

                    bool atBlock = false;

                    if (slopeErrorX >= 0)
                    {
                        x += signX;
                        slopeErrorX -= 2 * deltaY;

                        atBlock = true;
                    }

                    if (slopeErrorZ >= 0)
                    {
                        z += signZ;
                        slopeErrorZ -= 2 * deltaY;

                        atBlock = true;
                    }

                    if (atBlock)
                    {
                        Block block = Game.World.GetBlock(x, y, z);

                        if (block != null && block != Block.AIR)
                        {
                            // Check if the ray intersects the bounding box of the block
                            if (block.GetBoundingBox(x, y, z).Intersects(ray))
                            {
                                hitX = x;
                                hitY = y;
                                hitZ = z;

                                return true;
                            }
                        }
                    }

                    slopeErrorX += 2 * deltaX;
                    slopeErrorZ += 2 * deltaZ;
                }
            }
            else // Z axis
            {
                // Calculate slope error
                int slopeErrorX = 2 * deltaX - deltaZ;
                int slopeErrorY = 2 * deltaY - deltaZ;

                // Loop along the axis
                while (z != endZ)
                {
                    z += signZ;

                    bool atBlock = false;

                    if (slopeErrorX >= 0)
                    {
                        x += signX;
                        slopeErrorX -= 2 * deltaZ;

                        atBlock = true;
                    }

                    if (slopeErrorY >= 0)
                    {
                        y += signY;
                        slopeErrorY -= 2 * deltaZ;

                        atBlock = true;
                    }

                    if (atBlock)
                    {
                        Block block = Game.World.GetBlock(x, y, z);

                        if (block != null && block != Block.AIR)
                        {
                            // Check if the ray intersects the bounding box of the block
                            if (block.GetBoundingBox(x, y, z).Intersects(ray))
                            {
                                hitX = x;
                                hitY = y;
                                hitZ = z;

                                return true;
                            }
                        }
                    }

                    slopeErrorX += 2 * deltaX;
                    slopeErrorY += 2 * deltaY;
                }
            }

            hitX = 0;
            hitY = 0;
            hitZ = 0;

            return false;
        }
    }
}
