﻿// <copyright file="Raycast.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Physics;

/// <summary>
///     Utility class for raycasts.
/// </summary>
public static class Raycast
{
    /// <summary>
    ///     Checks if a ray intersects with a block that is not <see cref="Block.Air" />.
    /// </summary>
    /// <param name="world">The world in which to cast the ray.</param>
    /// <param name="ray">The ray.</param>
    /// <returns>Intersection information, if a hit occurred.</returns>
    public static (Vector3i hit, BlockSide side)? CastBlock(World world, Ray ray)
    {
        return CastVoxel(ray, (r, pos) => BlockIntersectionCheck(world, r, pos));
    }

    /// <summary>
    ///     Checks if a ray intersects with a fluid that is not <see cref="Fluid.None" />
    /// </summary>
    /// <param name="world">The world in which to cast the ray.</param>
    /// <param name="ray">The ray.</param>
    /// <returns>Intersection information, if a hit occurred.</returns>
    public static (Vector3i hit, BlockSide side)? CastFluid(World world, Ray ray)
    {
        return CastVoxel(ray, (r, pos) => FluidIntersectionCheck(world, r, pos));
    }

    private static (Vector3i hit, BlockSide side)? CastVoxel(Ray ray, Func<Ray, Vector3i, bool> rayIntersectionCheck)
    {
        Vector3i hit;
        BlockSide side;

        /*
         * Voxel Traversal Algorithm
         * Adapted from code by francisengelmann (https://github.com/francisengelmann/fast_voxel_traversal)
         * See: J. Amanatides and A. Woo, A Fast Voxel Traversal Algorithm for Ray Tracing, Eurographics, 1987.
         */

        // Calculate the direction of the ray with length
        Vector3 direction = ray.Direction;

        // Get the origin position in world coordinates.
        Vector3i current = ray.Origin.Floor();

        // Get the end position in world coordinates.
        Vector3i end = ray.EndPoint.Floor();

        // Get the direction in which the components are incremented.
        int stepX = Math.Sign(direction.X);
        int stepY = Math.Sign(direction.Y);
        int stepZ = Math.Sign(direction.Z);

        // Calculate the distance to the next voxel border from the current position.
        double nextVoxelBoundaryX = stepX > 0 ? current.X + stepX : current.X;
        double nextVoxelBoundaryY = stepY > 0 ? current.Y + stepY : current.Y;
        double nextVoxelBoundaryZ = stepZ > 0 ? current.Z + stepZ : current.Z;

        /*
         * Important: The floating-point equality comparison with zero must be exact, do not use the nearly-methods.
         * Using them can lead to unexpected results, like endless loops.
         */

        // Calculate the distance to the next voxel border.
        double tMaxX = direction.X != 0 ? (nextVoxelBoundaryX - ray.Origin.X) / direction.X : double.MaxValue;
        double tMaxY = direction.Y != 0 ? (nextVoxelBoundaryY - ray.Origin.Y) / direction.Y : double.MaxValue;
        double tMaxZ = direction.Z != 0 ? (nextVoxelBoundaryZ - ray.Origin.Z) / direction.Z : double.MaxValue;

        // Calculate distance so component equals voxel border.
        double tDeltaX = direction.X != 0 ? stepX / direction.X : double.MaxValue;
        double tDeltaY = direction.Y != 0 ? stepY / direction.Y : double.MaxValue;
        double tDeltaZ = direction.Z != 0 ? stepZ / direction.Z : double.MaxValue;

        // Check if the ray intersects the bounding box of the voxel.
        if (rayIntersectionCheck(ray, current))
        {
            hit = current;

            // As the ray starts in this voxel, no side is selected.
            side = BlockSide.All;

            return (hit, side);
        }

        while (current != end)
        {
            if (tMaxX < tMaxY && tMaxX < tMaxZ)
            {
                current.X += stepX;
                tMaxX += tDeltaX;

                side = stepX > 0 ? BlockSide.Left : BlockSide.Right;
            }
            else if (tMaxY < tMaxZ)
            {
                current.Y += stepY;
                tMaxY += tDeltaY;

                side = stepY > 0 ? BlockSide.Bottom : BlockSide.Top;
            }
            else
            {
                current.Z += stepZ;
                tMaxZ += tDeltaZ;

                side = stepZ > 0 ? BlockSide.Back : BlockSide.Front;
            }

            // Check if the ray intersects the bounding box of the block
            if (rayIntersectionCheck(ray, current))
            {
                hit = current;

                return (hit, side);
            }
        }

        return null;
    }

    private static bool BlockIntersectionCheck(World world, Ray ray, Vector3i position)
    {
        BlockInstance? potentialBlock = world.GetBlock(position);

        if (potentialBlock is not {} block) return false;

        // Check if the ray intersects the bounding box of the block.
        return block.Block != Block.Air && block.Block.GetCollider(world, position).Intersects(ray);
    }

    private static bool FluidIntersectionCheck(World world, Ray ray, Vector3i position)
    {
        FluidInstance? potentialFluid = world.GetFluid(position);

        if (potentialFluid is not {} fluid) return false;

        // Check if the ray intersects the bounding box of the fluid.
        return fluid.Fluid != Fluid.None &&
               Fluid.GetCollider(position, fluid.Level).Intersects(ray);
    }
}
