// <copyright file="Raycast.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Physics;

/// <summary>
///     Utility class for ray casts and similar operations.
/// </summary>
public static class Raycast
{
    /// <summary>
    ///     Checks if a ray intersects with a block that is not <see cref="Logic.Elements.Core.Air" />.
    /// </summary>
    /// <param name="world">The world in which to cast the ray.</param>
    /// <param name="ray">The ray.</param>
    /// <returns>Intersection information, if a hit occurred.</returns>
    public static (Vector3i hit, Side side)? CastBlockRay(World world, Ray ray)
    {
        return CastVoxelRay(ray, (r, pos) => BlockIntersectionCheck(world, r, pos)); 
    }

    /// <summary>
    ///     Checks if a ray intersects with a fluid that is not <see cref="Fluids.None" />
    /// </summary>
    /// <param name="world">The world in which to cast the ray.</param>
    /// <param name="ray">The ray.</param>
    /// <returns>Intersection information, if a hit occurred.</returns>
    public static (Vector3i hit, Side side)? CastFluidRay(World world, Ray ray)
    {
        return CastVoxelRay(ray, (r, pos) => FluidIntersectionCheck(world, r, pos));
    }

    private static (Vector3i hit, Side side)? CastVoxelRay(Ray ray, Func<Ray, Vector3i, Boolean> rayIntersectionCheck)
    {
        Vector3i hit;
        Side side;

        /*
         * Voxel Traversal Algorithm
         * Adapted from code by a5kin and ProjectPhysX: https://stackoverflow.com/a/38552664
         * See: J. Amanatides and A. Woo, A Fast Voxel Traversal Algorithm for Ray Tracing, Eurographics, 1987.
         */

        Double Frac0(Double value)
        {
            return value - Math.Floor(value);
        }

        Double Frac1(Double value)
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
        Double tDeltaX = step.X != 0 ? step.X / length.X : Double.MaxValue;
        Double tDeltaY = step.Y != 0 ? step.Y / length.Y : Double.MaxValue;
        Double tDeltaZ = step.Z != 0 ? step.Z / length.Z : Double.MaxValue;

        // Calculate the distance to the next voxel border.
        Double tMaxX = step.X > 0 ? tDeltaX * Frac1(ray.Origin.X) : tDeltaX * Frac0(ray.Origin.X);
        Double tMaxY = step.Y > 0 ? tDeltaY * Frac1(ray.Origin.Y) : tDeltaY * Frac0(ray.Origin.Y);
        Double tMaxZ = step.Z > 0 ? tDeltaZ * Frac1(ray.Origin.Z) : tDeltaZ * Frac0(ray.Origin.Z);

        // Check if the ray intersects the bounding box of the voxel.
        if (rayIntersectionCheck(ray, current))
        {
            hit = current;

            // As the ray starts in this voxel, no side is selected.
            side = Side.All;

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

                    side = step.X > 0 ? Side.Left : Side.Right;
                }
                else
                {
                    current.Z += step.Z;
                    tMaxZ += tDeltaZ;

                    side = step.Z > 0 ? Side.Back : Side.Front;
                }
            }
            else
            {
                if (tMaxY < tMaxZ)
                {
                    current.Y += step.Y;
                    tMaxY += tDeltaY;

                    side = step.Y > 0 ? Side.Bottom : Side.Top;
                }
                else
                {
                    current.Z += step.Z;
                    tMaxZ += tDeltaZ;

                    side = step.Z > 0 ? Side.Back : Side.Front;
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

    private static Boolean BlockIntersectionCheck(World world, Ray ray, Vector3i position)
    {
        BlockInstance? potentialBlock = world.GetBlock(position);

        if (potentialBlock is not {} block) return false;

        // Check if the ray intersects the bounding box of the block.
        return block.Block != Blocks.Instance.Core.Air && block.Block.GetCollider(world, position).Intersects(ray);
    }

    private static Boolean FluidIntersectionCheck(World world, Ray ray, Vector3i position)
    {
        FluidInstance? potentialFluid = world.GetFluid(position);

        if (potentialFluid is not {} fluid) return false;

        // Check if the ray intersects the bounding box of the fluid.
        return !fluid.IsEmpty &&
               Fluid.GetCollider(position, fluid.Level).Intersects(ray);
    }

    /// <summary>
    ///     Get all positions that intersect with the frustum.
    /// </summary>
    /// <param name="world">The world to check.</param>
    /// <param name="center">The center of the area to check.</param>
    /// <param name="range">The range of the area to check in each direction.</param>
    /// <param name="frustum">The frustum to check against.</param>
    /// <returns>A list of positions that intersect with the frustum.</returns>
    public static IEnumerable<(Content content, Vector3i position)> CastFrustum(World world, Vector3i center, Int32 range, Frustum frustum)
    {
        Int32 extents = range * 2 + 1;
        Vector3i min = center - new Vector3i(range);

        List<(Content content, Vector3i position)> positions = new(extents * extents * extents);

        foreach ((Int32 x, Int32 y, Int32 z) offset in MathTools.Range3(extents, extents, extents))
        {
            Vector3i position = min + offset;
            Content? content = world.GetContent(position);

            if (content is not var (block, fluid)) continue;

            if (block.Block != Blocks.Instance.Core.Air && block.Block.GetCollider(world, position).Intersects(frustum)) positions.Add((content.Value, position));
            else if (fluid.Fluid != Fluids.Instance.None && Fluid.GetCollider(position, fluid.Level).Intersects(frustum)) positions.Add((content.Value, position));
        }

        return positions;
    }
}
