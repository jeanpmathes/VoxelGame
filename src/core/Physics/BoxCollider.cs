// <copyright file="BoxCollider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Physics;

/// <summary>
///     A box collider is a specific instance of an bounding volume, at a certain position in the world.
/// </summary>
public struct BoxCollider : IEquatable<BoxCollider>
{
    /// <summary>
    ///     Get or set the box that this collider uses.
    /// </summary>
    public BoundingVolume Volume { get; set; }

    /// <summary>
    ///     Get or set the position of this collider.
    /// </summary>
    public Vector3d Position { get; set; }

    /// <summary>
    ///     Get the center of this collider.
    /// </summary>
    public Vector3d Center => Position + Volume.Center;

    /// <summary>
    ///     Creates a new box collider.
    /// </summary>
    /// <param name="volume">The box to use.</param>
    /// <param name="position">The position of the collider.</param>
    public BoxCollider(BoundingVolume volume, Vector3d position)
    {
        Volume = volume;
        Position = position;
    }

    /// <summary>
    ///     Check if this collider contains a point.
    /// </summary>
    public bool Contains(Vector3d point)
    {
        return Volume.Contains(point - Position);
    }

    /// <summary>
    ///     Check if this collider is intersected by a ray.
    /// </summary>
    public bool Intersects(Ray ray)
    {
        return Volume.Intersects(ray.Translated(-Position));
    }

    /// <summary>
    ///     Check if this collider intersects with another collider.
    /// </summary>
    /// <param name="other">The other collider.</param>
    /// <returns>True if they intersect.</returns>
    public bool Intersects(BoxCollider other)
    {
        BoxCollider self = this;
        Vector3d offset = other.Position - Position;

        bool Check(BoundingVolume volume)
        {
            Box3d box = new(volume.Min + offset, volume.Max + offset);

            if (self.Volume.Intersects(box)) return true;

            if (volume.ChildCount == 0) return false;

            for (var i = 0; i < volume.ChildCount; i++)
                if (Check(volume[i]))
                    return true;

            return false;
        }

        return Check(other.Volume);
    }

    /// <summary>
    ///     Check if this collider intersects with another collider, and also set the collision planes.
    /// </summary>
    public bool Intersects(BoxCollider other, ref bool x, ref bool y, ref bool z)
    {
        BoxCollider self = this;
        Vector3d offset = other.Position - Position;

        bool Check(BoundingVolume volume, ref bool lx, ref bool ly, ref bool lz)
        {
            Box3d box = new(volume.Min + offset, volume.Max + offset);

            if (self.Volume.Intersects(box, ref lx, ref ly, ref lz)) return true;

            if (volume.ChildCount == 0) return false;

            for (var i = 0; i < volume.ChildCount; i++)
                if (Check(volume[i], ref lx, ref ly, ref lz))
                    return true;

            return false;
        }

        return Check(other.Volume, ref x, ref y, ref z);
    }

    private bool IntersectsTerrain_NonRecursive(World world, out bool xCollision, out bool yCollision,
        out bool zCollision, ISet<(Vector3i position, Block block)> blockIntersections,
        ISet<(Vector3i position, Fluid fluid, FluidLevel level)> fluidIntersections)
    {
        var intersects = false;

        xCollision = false;
        yCollision = false;
        zCollision = false;

        // Calculate the range of blocks to check.
        double highestExtent = Volume.Extents.X > Volume.Extents.Y ? Volume.Extents.X : Volume.Extents.Y;
        highestExtent = highestExtent > Volume.Extents.Z ? highestExtent : Volume.Extents.Z;

        int range = (int) Math.Round(highestExtent * 2, MidpointRounding.AwayFromZero) + 1;

        if (range % 2 == 0) range++;

        // Get the current position in world coordinates.
        Vector3i center = Center.Floor();

        // Loop through the world and check for collisions.
        for (int x = (range - 1) / -2; x <= (range - 1) / 2; x++)
        for (int y = (range - 1) / -2; y <= (range - 1) / 2; y++)
        for (int z = (range - 1) / -2; z <= (range - 1) / 2; z++)
        {
            Vector3i position = center + new Vector3i(x, y, z);

            Content? content = world.GetContent(position);

            if (content == null) continue;

            (BlockInstance currentBlock, FluidInstance currentFluid) = content.Value;

            BoxCollider blockCollider = currentBlock.Block.GetCollider(
                world,
                position);

            var newX = false;
            var newY = false;
            var newZ = false;

            if ((currentBlock.Block.IsSolid || currentBlock.Block.IsTrigger) && Intersects(
                    blockCollider,
                    ref newX,
                    ref newY,
                    ref newZ))
            {
                blockIntersections.Add((position, currentBlock.Block));

                if (currentBlock.Block.IsSolid)
                {
                    intersects = true;

                    xCollision |= newX;
                    yCollision |= newY;
                    zCollision |= newZ;
                }
            }

            if (currentFluid.Fluid.CheckContact)
            {
                BoxCollider fluidCollider = Fluid.GetCollider(position, currentFluid.Level);

                if (Intersects(fluidCollider))
                    fluidIntersections.Add((position, currentFluid.Fluid, currentFluid.Level));
            }
        }

        return intersects;
    }

    /// <summary>
    ///     Calculate all intersections of this <see cref="BoxCollider" /> with the terrain.
    /// </summary>
    public bool IntersectsTerrain(World world, out bool xCollision, out bool yCollision, out bool zCollision,
        HashSet<(Vector3i position, Block block)> blockIntersections,
        HashSet<(Vector3i position, Fluid fluid, FluidLevel level)> fluidIntersections)
    {
        bool isIntersecting = IntersectsTerrain_NonRecursive(
            world,
            out xCollision,
            out yCollision,
            out zCollision,
            blockIntersections,
            fluidIntersections);

        if (Volume.ChildCount == 0) return isIntersecting;

        for (var i = 0; i < Volume.ChildCount; i++)
        {
            BoxCollider childCollider = Volume[i].GetColliderAt(Position);

            bool childIntersecting = childCollider.IntersectsTerrain(
                world,
                out bool childX,
                out bool childY,
                out bool childZ,
                blockIntersections,
                fluidIntersections);

            isIntersecting = childIntersecting || isIntersecting;

            xCollision = childX || xCollision;
            yCollision = childY || yCollision;
            zCollision = childZ || zCollision;
        }

        return isIntersecting;
    }

    /// <inheritdoc />
    public bool Equals(BoxCollider other)
    {
        return Volume.Equals(other.Volume) && Position.Equals(other.Position);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is BoxCollider other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Volume, Position);
    }

    /// <summary>
    ///     Equality operator.
    /// </summary>
    public static bool operator ==(BoxCollider left, BoxCollider right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Inequality operator.
    /// </summary>
    public static bool operator !=(BoxCollider left, BoxCollider right)
    {
        return !left.Equals(right);
    }
}
