// <copyright file="IBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Visuals;

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
    ///     Gets the section buffer this blocks mesh data should be stored in.
    /// </summary>
    public TargetBuffer TargetBuffer { get; }

    /// <summary>
    ///     Gets whether this block completely fills a 1x1x1 volume or not. If a block is not full, it cannot be opaque.
    /// </summary>
    public bool IsFull { get; }

    /// <summary>
    ///     Gets whether it is possible to see through this block. This will affect the rendering of this block and the blocks
    ///     around it.
    /// </summary>
    public bool IsOpaque { get; }

    /// <summary>
    ///     Gets whether this block hinders movement.
    /// </summary>
    public bool IsSolid { get; }

    /// <summary>
    ///     Gets whether this block is solid and full.
    /// </summary>
    public bool IsSolidAndFull { get; }

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
}
