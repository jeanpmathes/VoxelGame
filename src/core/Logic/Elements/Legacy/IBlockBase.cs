// <copyright file="IBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Definitions;

namespace VoxelGame.Core.Logic.Elements.Legacy;

/// <summary>
///     Defines the basic <see cref="Block" /> methods required for a lot of block functionality.
/// </summary>
public interface IBlockBase : IContent
{
    /// <summary>
    ///     Gets the block id which can be any value from 0 to 4095.
    /// </summary>
    public UInt32 ID { get; }

    /// <summary>
    ///     Gets the localized name of the block.
    /// </summary>
    public String Name { get; }

    /// <summary>
    ///     This property is only relevant for non-opaque full blocks. It decides if their faces should be rendered next to
    ///     another non-opaque block.
    /// </summary>
    public Boolean RenderFaceAtNonOpaques { get; }

    /// <summary>
    ///     Gets whether the collision method should be called in case of a collision with an actor.
    /// </summary>
    public Boolean ReceiveCollisions { get; }

    /// <summary>
    ///     Gets whether this block should be checked in collision calculations even if it is not solid.
    /// </summary>
    public Boolean IsTrigger { get; }

    /// <summary>
    ///     Gets whether this block can be replaced when placing a block.
    /// </summary>
    public Boolean IsReplaceable { get; }

    /// <summary>
    ///     Gets whether this block responds to interactions.
    /// </summary>
    public Boolean IsInteractable { get; }

    /// <summary>
    ///     Gets whether this block is unshaded.
    /// </summary>
    public Boolean IsUnshaded { get; }

    /// <summary>
    ///     Gets whether this block always completely fills a 1x1x1 volume or not. Prefer the <see cref="IsSideFull" /> method
    ///     as it handles blocks that are sometimes full.
    /// </summary>
    public Boolean IsFull { get; }

    /// <summary>
    ///     Gets whether it is possible to see through this block.
    ///     Note that this only indicates whether the actual filled portion of the block is opaque.
    ///     If the block is not full, it is possible to see around the block.
    /// </summary>
    public Boolean IsOpaque { get; }

    /// <summary>
    ///     Gets whether this block hinders movement.
    /// </summary>
    public Boolean IsSolid { get; }

    /// <summary>
    ///     Tries to place a block in the world.
    /// </summary>
    /// <param name="world"></param>
    /// <param name="position"></param>
    /// <param name="actor"></param>
    /// <returns>Returns true if placing the block was successful.</returns>
    public Boolean Place(World world, Vector3i position, PhysicsActor? actor = null);

    /// <summary>
    ///     Destroys a block in the world if it is the same type as this block.
    /// </summary>
    /// <param name="world"></param>
    /// <param name="position"></param>
    /// <param name="actor">The actor which caused the destruction, or null if no actor caused it.</param>
    /// <returns>Returns true if the block has been destroyed.</returns>
    public Boolean Destroy(World world, Vector3i position, PhysicsActor? actor = null);

    /// <summary>
    ///     Get whether a side of the block is completely full, which means it covers the entire side of the unit block.
    /// </summary>
    /// <param name="side">The side to check. This can also be <see cref="Side.All" /> to check for the entire block.</param>
    /// <param name="data">The block data.</param>
    /// <returns>True if the side is completely full.</returns>
    public Boolean IsSideFull(Side side, UInt32 data)
    {
        return IsFull;
    }

    /// <summary>
    ///     Check whether this block is always solid and full.
    /// </summary>
    Boolean IsSolidAndFull()
    {
        return IsSolid && IsFull;
    }

    /// <summary>
    ///     Check whether this block is solid and full with the given data.
    /// </summary>
    public Boolean IsSolidAndFull(UInt32 data)
    {
        return IsSolid && IsSideFull(Side.All, data);
    }

    /// <summary>
    ///     Check whether this block is always opaque and full.
    /// </summary>
    Boolean IsOpaqueAndFull()
    {
        return IsOpaque && IsFull;
    }

    /// <summary>
    ///     Check whether this block is opaque and full with the given data.
    /// </summary>
    public Boolean IsOpaqueAndFull(UInt32 data)
    {
        return IsOpaque && IsSideFull(Side.All, data);
    }
}
