// <copyright file="BoxCollider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Elements.Legacy;
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
    public Boolean Contains(Vector3d point)
    {
        return Volume.Contains(point - Position);
    }

    /// <summary>
    ///     Check if this collider is intersected by a ray.
    /// </summary>
    public Boolean Intersects(Ray ray)
    {
        return Volume.Intersects(ray.Translated(-Position));
    }

    /// <summary>
    ///     Check if this collider is intersected by a frustum.
    /// </summary>
    public Boolean Intersects(Frustum frustum)
    {
        return Volume.Intersects(frustum.Translated(-Position));
    }

    /// <summary>
    ///     Check if this collider intersects with another collider.
    /// </summary>
    /// <param name="other">The other collider.</param>
    /// <returns>True if they intersect.</returns>
    public Boolean Intersects(BoxCollider other)
    {
        BoxCollider self = this;
        Vector3d offset = other.Position - Position;

        Boolean Check(BoundingVolume volume)
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
    public Boolean Intersects(BoxCollider other, ref Boolean x, ref Boolean y, ref Boolean z)
    {
        BoxCollider self = this;
        Vector3d offset = other.Position - Position;

        Boolean Check(BoundingVolume volume, ref Boolean lx, ref Boolean ly, ref Boolean lz)
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

    private Boolean IntersectsTerrain_NonRecursive(World world, out Boolean xCollision, out Boolean yCollision,
        out Boolean zCollision, ISet<(Vector3i position, Block block)> blockIntersections,
        ISet<(Vector3i position, Fluid fluid, FluidLevel level)> fluidIntersections)
    {
        var intersects = false;

        xCollision = false;
        yCollision = false;
        zCollision = false;

        // Calculate the range of blocks to check.
        Double highestExtent = Volume.Extents.X > Volume.Extents.Y ? Volume.Extents.X : Volume.Extents.Y;
        highestExtent = highestExtent > Volume.Extents.Z ? highestExtent : Volume.Extents.Z;

        Int32 range = (Int32) Math.Round(highestExtent * 2, MidpointRounding.AwayFromZero) + 1;

        if (range % 2 == 0) range++;

        // Get the current position in world coordinates.
        Vector3i center = Center.Floor();

        // Loop through the world and check for collisions.
        for (Int32 x = (range - 1) / -2; x <= (range - 1) / 2; x++)
        for (Int32 y = (range - 1) / -2; y <= (range - 1) / 2; y++)
        for (Int32 z = (range - 1) / -2; z <= (range - 1) / 2; z++)
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
    public Boolean IntersectsTerrain(World world, out Boolean xCollision, out Boolean yCollision, out Boolean zCollision,
        HashSet<(Vector3i position, Block block)> blockIntersections,
        HashSet<(Vector3i position, Fluid fluid, FluidLevel level)> fluidIntersections)
    {
        Boolean isIntersecting = IntersectsTerrain_NonRecursive(
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

            Boolean childIntersecting = childCollider.IntersectsTerrain(
                world,
                out Boolean childX,
                out Boolean childY,
                out Boolean childZ,
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
    public Boolean Equals(BoxCollider other)
    {
        return ReferenceEquals(Volume, other.Volume) && Position.Equals(other.Position);
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is BoxCollider other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return HashCode.Combine(Volume, Position);
    }

    /// <summary>
    ///     Equality operator.
    /// </summary>
    public static Boolean operator ==(BoxCollider left, BoxCollider right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     Inequality operator.
    /// </summary>
    public static Boolean operator !=(BoxCollider left, BoxCollider right)
    {
        return !left.Equals(right);
    }
}
