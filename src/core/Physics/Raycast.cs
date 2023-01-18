// <copyright file="Raycast.cs" company="VoxelGame">
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
    ///     Checks if a ray intersects with a fluid that is not <see cref="Fluids.None" />
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
         * Adapted from code by a5kin and ProjectPhysX: https://stackoverflow.com/a/38552664
         * See: J. Amanatides and A. Woo, A Fast Voxel Traversal Algorithm for Ray Tracing, Eurographics, 1987.
         */

        double Frac0(double value)
        {
            return value - Math.Floor(value);
        }

        double Frac1(double value)
        {
            return 1 - value + Math.Floor(value);
        }

        // Calculate the direction of the ray with length
        Vector3d direction = ray.Direction;
        Vector3d length = direction * ray.Length;

        // Get the origin position in world coordinates.
        Vector3i current = ray.Origin.Floor();

        // Get the direction in which the components are incremented.
        Vector3i step = direction.Sign();

        // Calculate distance so component equals voxel border.
        double tDeltaX = step.X != 0 ? step.X / length.X : double.MaxValue;
        double tDeltaY = step.Y != 0 ? step.Y / length.Y : double.MaxValue;
        double tDeltaZ = step.Z != 0 ? step.Z / length.Z : double.MaxValue;

        // Calculate the distance to the next voxel border.
        double tMaxX = step.X > 0 ? tDeltaX * Frac1(ray.Origin.X) : tDeltaX * Frac0(ray.Origin.X);
        double tMaxY = step.Y > 0 ? tDeltaY * Frac1(ray.Origin.Y) : tDeltaY * Frac0(ray.Origin.Y);
        double tMaxZ = step.Z > 0 ? tDeltaZ * Frac1(ray.Origin.Z) : tDeltaZ * Frac0(ray.Origin.Z);

        // Check if the ray intersects the bounding box of the voxel.
        if (rayIntersectionCheck(ray, current))
        {
            hit = current;

            // As the ray starts in this voxel, no side is selected.
            side = BlockSide.All;

            return (hit, side);
        }

        while (true)
        {
            if (tMaxX < tMaxY)
            {
                if (tMaxX < tMaxZ)
                {
                    current.X += step.X;
                    tMaxX += tDeltaX;

                    side = step.X > 0 ? BlockSide.Left : BlockSide.Right;
                }
                else
                {
                    current.Z += step.Z;
                    tMaxZ += tDeltaZ;

                    side = step.Z > 0 ? BlockSide.Back : BlockSide.Front;
                }
            }
            else
            {
                if (tMaxY < tMaxZ)
                {
                    current.Y += step.Y;
                    tMaxY += tDeltaY;

                    side = step.Y > 0 ? BlockSide.Bottom : BlockSide.Top;
                }
                else
                {
                    current.Z += step.Z;
                    tMaxZ += tDeltaZ;

                    side = step.Z > 0 ? BlockSide.Back : BlockSide.Front;
                }
            }

            if (tMaxX > 1 && tMaxY > 1 && tMaxZ > 1) break;

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
        return fluid.Fluid != Fluids.Instance.None &&
               Fluid.GetCollider(position, fluid.Level).Intersects(ray);
    }
}


