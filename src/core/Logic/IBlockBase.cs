// <copyright file="IBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Entities;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Defines the basic <see cref="Block" /> methods required for a lot of block functionality.
/// </summary>
public interface IBlockBase
{
    /// <summary>
    ///     Gets the block id which can be any value from 0 to 4095.
    /// </summary>
    public uint Id { get; }

    /// <summary>
    ///     Gets the localized name of the block.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     An unlocalized string that identifies this block.
    /// </summary>
    public string NamedId { get; }

    /// <summary>
    ///     This property is only relevant for non-opaque full blocks. It decides if their faces should be rendered next to
    ///     another non-opaque block.
    /// </summary>
    public bool RenderFaceAtNonOpaques { get; }

    /// <summary>
    ///     Gets whether the collision method should be called in case of a collision with an entity.
    /// </summary>
    public bool ReceiveCollisions { get; }

    /// <summary>
    ///     Gets whether this block should be checked in collision calculations even if it is not solid.
    /// </summary>
    public bool IsTrigger { get; }

    /// <summary>
    ///     Gets whether this block can be replaced when placing a block.
    /// </summary>
    public bool IsReplaceable { get; }

    /// <summary>
    ///     Gets whether this block responds to interactions.
    /// </summary>
    public bool IsInteractable { get; }

    /// <summary>
    ///     Gets whether this block always completely fills a 1x1x1 volume or not. Prefer the <see cref="IsSideFull"/> method as it handles blocks that are sometimes full.
    /// </summary>
    public bool IsFull { get; }

    /// <summary>
    ///     Gets whether it is possible to see through this block. If an opaque block is not full, it is still possible to see trough the position of the block.
    /// </summary>
    public bool IsOpaque { get; }

    /// <summary>
    ///     Gets whether this block hinders movement.
    /// </summary>
    public bool IsSolid { get; }

    /// <summary>
    ///     Tries to place a block in the world.
    /// </summary>
    /// <param name="world"></param>
    /// <param name="position"></param>
    /// <param name="entity">The entity that tries to place the block. May be null.</param>
    /// <returns>Returns true if placing the block was successful.</returns>
    public bool Place(World world, Vector3i position, PhysicsEntity? entity = null);

    /// <summary>
    ///     Destroys a block in the world if it is the same type as this block.
    /// </summary>
    /// <param name="world"></param>
    /// <param name="position"></param>
    /// <param name="entity">The entity which caused the destruction, or null if no entity caused it.</param>
    /// <returns>Returns true if the block has been destroyed.</returns>
    public bool Destroy(World world, Vector3i position, PhysicsEntity? entity = null);

    /// <summary>
    ///     Get whether a side of the block is completely full, which means it covers the entire side of the unit block.
    /// </summary>
    /// <param name="side">The side to check. This can also be <see cref="BlockSide.All"/> to check for the entire block.</param>
    /// <param name="data">The block data.</param>
    /// <returns>True if the side is completely full.</returns>
    public bool IsSideFull(BlockSide side, uint data)
    {
        return IsFull;
    }

    /// <summary>
    ///     Check whether this block is always solid and full.
    /// </summary>
    bool IsSolidAndFull()
    {
        return IsSolid && IsFull;
    }

    /// <summary>
    ///     Check whether this block is solid and full with the given data.
    /// </summary>
    public bool IsSolidAndFull(uint data)
    {
        return IsSolid && IsSideFull(BlockSide.All, data);
    }

    /// <summary>
    ///     Check whether this block is always opaque and full.
    /// </summary>
    bool IsOpaqueAndFull()
    {
        return IsOpaque && IsFull;
    }

    /// <summary>
    ///     Check whether this block is opaque and full with the given data.
    /// </summary>
    public bool IsOpaqueAndFull(uint data)
    {
        return IsOpaque && IsSideFull(BlockSide.All, data);
    }
}
