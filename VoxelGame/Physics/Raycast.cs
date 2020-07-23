// <copyright file="Raycast.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using System;
using VoxelGame.Logic;

namespace VoxelGame.Physics
{
    public static class Raycast
    {
        /// <summary>
        /// Checks if a ray intersects with a block that is not air.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <param name="hitX">The x position where the intersection happens.</param>
        /// <param name="hitY">The y position where the intersection happens.</param>
        /// <param name="hitZ">The z position where the intersection happens.</param>
        /// <param name="side">The side of the voxel which is hit first.</param>
        /// <returns>True if an intersection happens.</returns>
        public static bool CastWorld(Ray ray, out int hitX, out int hitY, out int hitZ, out BlockSide side)
        {
            /*
             * Voxel Traversal Algorithm
             * Adapted from code by francisengelmann (https://github.com/francisengelmann/fast_voxel_traversal)
             * See: J. Amanatides and A. Woo, A Fast Voxel Traversal Algorithm for Ray Tracing, Eurographics, 1987.
             */

            // Calculate the direction of the ray with length
            Vector3 direction = ray.Direction;

            // Get the origin position in world coordinates.
            int x = (int)Math.Floor(ray.Origin.X);
            int y = (int)Math.Floor(ray.Origin.Y);
            int z = (int)Math.Floor(ray.Origin.Z);

            // Get the end position in world coordinates.
            int endX = (int)Math.Floor(ray.EndPoint.X);
            int endY = (int)Math.Floor(ray.EndPoint.Y);
            int endZ = (int)Math.Floor(ray.EndPoint.Z);

            // Get the direction in which the components are incremented.
            int stepX = Math.Sign(direction.X);
            int stepY = Math.Sign(direction.Y);
            int stepZ = Math.Sign(direction.Z);

            // Calculate the distance to the next voxel border from the current position.
            double nextVoxelBoundaryX = (stepX > 0) ? x + stepX : x;
            double nextVoxelBoundaryY = (stepY > 0) ? y + stepY : y;
            double nextVoxelBoundaryZ = (stepZ > 0) ? z + stepZ : z;

            // Calculate the distance to the next voxel border.
            double tMaxX = (direction.X != 0) ? (nextVoxelBoundaryX - ray.Origin.X) / direction.X : double.MaxValue;
            double tMaxY = (direction.Y != 0) ? (nextVoxelBoundaryY - ray.Origin.Y) / direction.Y : double.MaxValue;
            double tMaxZ = (direction.Z != 0) ? (nextVoxelBoundaryZ - ray.Origin.Z) / direction.Z : double.MaxValue;

            // Calculate distance so component equals voxel border.
            double tDeltaX = (direction.X != 0) ? stepX / direction.X : double.MaxValue;
            double tDeltaY = (direction.Y != 0) ? stepY / direction.Y : double.MaxValue;
            double tDeltaZ = (direction.Z != 0) ? stepZ / direction.Z : double.MaxValue;

            // Check the current block.
            Block? currentBlock = Game.World.GetBlock(x, y, z, out _);

            // Check if the ray intersects the bounding box of the block.
            if (currentBlock != null && currentBlock != Block.AIR && currentBlock.GetBoundingBox(x, y, z).Intersects(ray))
            {
                hitX = x;
                hitY = y;
                hitZ = z;

                // As the ray starts in this voxel, no side is selected.
                side = BlockSide.All;

                return true;
            }

            while (!(x == endX && y == endY && z == endZ))
            {
                if (tMaxX < tMaxY)
                {
                    if (tMaxX < tMaxZ)
                    {
                        x += stepX;
                        tMaxX += tDeltaX;

                        side = (stepX > 0) ? BlockSide.Left : BlockSide.Right;
                    }
                    else
                    {
                        z += stepZ;
                        tMaxZ += tDeltaZ;

                        side = (stepZ > 0) ? BlockSide.Back : BlockSide.Front;
                    }
                }
                else
                {
                    if (tMaxY < tMaxZ)
                    {
                        y += stepY;
                        tMaxY += tDeltaY;

                        side = (stepY > 0) ? BlockSide.Bottom : BlockSide.Top;
                    }
                    else
                    {
                        z += stepZ;
                        tMaxZ += tDeltaZ;

                        side = (stepZ > 0) ? BlockSide.Back : BlockSide.Front;
                    }
                }

                //Check the current block
                currentBlock = Game.World.GetBlock(x, y, z, out _);

                // Check if the ray intersects the bounding box of the block
                if (currentBlock != null && currentBlock != Block.AIR && currentBlock.GetBoundingBox(x, y, z).Intersects(ray))
                {
                    hitX = x;
                    hitY = y;
                    hitZ = z;

                    return true;
                }
            }

            hitX = hitY = hitZ = -1;
            side = BlockSide.All;
            return false;
        }
    }
}