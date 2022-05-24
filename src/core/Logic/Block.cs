// <copyright file="Block.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic;

/// <summary>
///     The basic block class. Blocks are used to construct the world.
/// </summary>
public abstract partial class Block : IBlockBase, IBlockMeshable, IIdentifiable<uint>, IIdentifiable<string>
{
    private readonly BoundingVolume boundingVolume;

    /// <summary>
    ///     Create a new block.
    /// </summary>
    /// <param name="name">The name of the block. Can be localized.</param>
    /// <param name="namedId">The named ID of the block. A unique and unlocalized identifier.</param>
    /// <param name="flags">The block flags setting specific options.</param>
    /// <param name="boundingVolume">The base bounding volume for this block. Is used for placement checks.</param>
    /// <param name="targetBuffer">The target rendering buffer.</param>
    protected Block(string name, string namedId, BlockFlags flags, BoundingVolume boundingVolume,
        TargetBuffer targetBuffer)
    {
        Name = name;
        NamedId = namedId;

        IsFull = flags.IsFull;
        IsOpaque = flags.IsOpaque;
        RenderFaceAtNonOpaques = flags.RenderFaceAtNonOpaques;
        IsSolid = flags.IsSolid;
        ReceiveCollisions = flags.ReceiveCollisions;
        IsTrigger = flags.IsTrigger;
        IsReplaceable = flags.IsReplaceable;
        IsInteractable = flags.IsInteractable;

        this.boundingVolume = boundingVolume;

        TargetBuffer = targetBuffer;

        Debug.Assert(
            (TargetBuffer != TargetBuffer.Simple) ^ IsFull,
            $"TargetBuffer '{nameof(TargetBuffer.Simple)}' requires {nameof(IsFull)} to be {!IsFull}, all other target buffers cannot be full.");

        Debug.Assert(IsFull || !IsOpaque, "A block that is not full cannot be opaque.");
#pragma warning disable S3060 // "is" should not be used with "this"
        Debug.Assert(
            TargetBuffer == TargetBuffer.VaryingHeight == this is IHeightVariable,
            $"The target buffer should be {nameof(TargetBuffer.VaryingHeight)} if and only if the block implements {nameof(IHeightVariable)}.");
#pragma warning restore S3060 // "is" should not be used with "this"

        if (blockList.Count < BlockLimit)
        {
            blockList.Add(this);
            namedBlockDictionary.Add(namedId, this);

            Id = (uint) (blockList.Count - 1);
        }
        else
        {
            Debug.Fail($"Not more than {BlockLimit} blocks are allowed.");
        }
    }

    /// <inheritdoc />
    public uint Id { get; }

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string NamedId { get; }

    /// <inheritdoc />
    public bool RenderFaceAtNonOpaques { get; }

    /// <inheritdoc />
    public bool ReceiveCollisions { get; }

    /// <inheritdoc />
    public bool IsTrigger { get; }

    /// <inheritdoc />
    public bool IsReplaceable { get; }

    /// <inheritdoc />
    public bool IsInteractable { get; }

    /// <inheritdoc />
    public TargetBuffer TargetBuffer { get; }

    /// <inheritdoc />
    public bool IsFull { get; }

    /// <inheritdoc />
    public bool IsOpaque { get; }

    /// <inheritdoc />
    public bool IsSolid { get; }

    /// <inheritdoc />
    public bool IsSolidAndFull => IsSolid && IsFull;

    /// <summary>
    ///     Attempt to place the block in the world.
    /// </summary>
    /// <param name="world">The world in which to place the block.</param>
    /// <param name="position">The position at which to place the block.</param>
    /// <param name="entity">The entity that is placing the block.</param>
    /// <returns>True if placement was successful.</returns>
    public bool Place(World world, Vector3i position, PhysicsEntity? entity = null)
    {
        (BlockInstance, FluidInstance)? content = world.GetContent(position);

        if (content == null) return false;

        (BlockInstance block, FluidInstance fluid) = content.Value;

        bool canPlace = block.Block.IsReplaceable && CanPlace(world, position, entity);

        if (canPlace) DoPlace(world, position, entity);

        if (fluid.Fluid != Fluid.None && this is IFillable fillable)
            fillable.FluidChange(world, position, fluid.Fluid, fluid.Level);

        return canPlace;
    }

    /// <summary>
    ///     Attempt to destroy the block in the world.
    ///     Will always fail if there is a different block at the given position.
    /// </summary>
    /// <param name="world">The world in which to destroy the block.</param>
    /// <param name="position">The position at which to destroy to block.</param>
    /// <param name="entity">The entity destroying the block.</param>
    /// <returns>True if destruction was successful.</returns>
    public bool Destroy(World world, Vector3i position, PhysicsEntity? entity = null)
    {
        BlockInstance? potentialBlock = world.GetBlock(position);

        if (potentialBlock is not {} block) return false;
        if (block.Block != this || !CanDestroy(world, position, block.Data, entity)) return false;

        DoDestroy(world, position, block.Data, entity);

        return true;
    }

    string IIdentifiable<string>.Id => NamedId;

    uint IIdentifiable<uint>.Id => Id;

    /// <summary>
    ///     Called when loading blocks, meant to setup vertex data, indices etc.
    /// </summary>
    /// <param name="indexProvider"></param>
    protected virtual void Setup(ITextureIndexProvider indexProvider) {}

    /// <summary>
    ///     Returns the collider for a given position.
    /// </summary>
    /// <param name="world">The world in which the block is.</param>
    /// <param name="position">The position of the block.</param>
    /// <returns>The bounding volume.</returns>
    public BoxCollider GetCollider(World world, Vector3i position)
    {
        BlockInstance? potentialBlock = world.GetBlock(position);

        return (potentialBlock?.Block == this ? GetBoundingVolume(potentialBlock.Value.Data) : boundingVolume)
            .GetColliderAt(position);
    }

    /// <summary>
    ///     Override this to provide a custom bounding volume for this block, depending on the block data.
    ///     The bounding volume should be pre-calculated.
    /// </summary>
    /// <param name="data">The block data.</param>
    /// <returns>The bounding volume for the given data.</returns>
    protected virtual BoundingVolume GetBoundingVolume(uint data)
    {
        return boundingVolume;
    }

    /// <summary>
    ///     Override this to provide change the block placement checks.
    /// </summary>
    /// <param name="world">The world in which the placement occurs.</param>
    /// <param name="position">The position at which the placement is requested.</param>
    /// <param name="entity">The entity that performs placement.</param>
    /// <returns>True if placement is possible.</returns>
    public virtual bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        return true;
    }

    /// <summary>
    ///     Override this to change the block placement logic.
    ///     The block placement must always be successful. If checks are required, override <see cref="CanPlace" />.
    /// </summary>
    /// <param name="world">The world in which the placement occurs.</param>
    /// <param name="position">The position at which the placement is requested.</param>
    /// <param name="entity">The entity that performs placement.</param>
    protected virtual void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        world.SetBlock(this.AsInstance(), position);
    }

    /// <summary>
    ///     Override this to change the block destruction checks.
    /// </summary>
    /// <param name="world">The world in which the placement occurs.</param>
    /// <param name="position">The position at which the placement is requested.</param>
    /// <param name="data">The block data.</param>
    /// <param name="entity">The entity that performs placement.</param>
    /// <returns></returns>
    protected virtual bool CanDestroy(World world, Vector3i position, uint data, PhysicsEntity? entity)
    {
        return true;
    }

    /// <summary>
    ///     Override this to change the block destruction logic.
    ///     The block destruction must always be successful. If checks are required, override <see cref="CanDestroy" />.
    /// </summary>
    /// <param name="world">The world in which the placement occurs.</param>
    /// <param name="position">The position at which the placement is requested.</param>
    /// <param name="data">The block data.</param>
    /// <param name="entity">The entity that performs placement.</param>
    protected virtual void DoDestroy(World world, Vector3i position, uint data, PhysicsEntity? entity)
    {
        world.SetDefaultBlock(position);
    }

    /// <summary>
    ///     This method is called when an entity collides with this block.
    /// </summary>
    /// <param name="entity">The entity that caused the collision.</param>
    /// <param name="position">The block position.</param>
    public void EntityCollision(PhysicsEntity entity, Vector3i position)
    {
        BlockInstance? potentialBlock = entity.World.GetBlock(position);
        if (potentialBlock?.Block == this) EntityCollision(entity, position, potentialBlock.Value.Data);
    }

    /// <summary>
    ///     Override to provide custom entity collision logic.
    /// </summary>
    /// <param name="entity">The entity that collided with this block.</param>
    /// <param name="position">The position of the block.</param>
    /// <param name="data">The block data of this block.</param>
    protected virtual void EntityCollision(PhysicsEntity entity, Vector3i position, uint data) {}

    /// <summary>
    ///     Called when a block and an entity collide.
    /// </summary>
    /// <param name="entity">The entity that collided with the block.</param>
    /// <param name="position">The block position.</param>
    public void EntityInteract(PhysicsEntity entity, Vector3i position)
    {
        BlockInstance? potentialBlock = entity.World.GetBlock(position);
        if (potentialBlock?.Block == this) EntityInteract(entity, position, potentialBlock.Value.Data);
    }

    /// <summary>
    ///     Override to provide custom entity interaction logic.
    /// </summary>
    /// <param name="entity">The entity that interacted with this block.</param>
    /// <param name="position">The position of the block.</param>
    /// <param name="data">The block data of this block.</param>
    protected virtual void EntityInteract(PhysicsEntity entity, Vector3i position, uint data) {}

    /// <summary>
    ///     This method is called on blocks next to a position that was changed.
    /// </summary>
    /// <param name="world">The containing world.</param>
    /// <param name="position">The block position.</param>
    /// <param name="data">The data of the block next to the changed position.</param>
    /// <param name="side">The side of the block where the change happened.</param>
    public virtual void BlockUpdate(World world, Vector3i position, uint data, BlockSide side) {}

    /// <summary>
    ///     This method is called randomly on some blocks every update.
    /// </summary>
    public virtual void RandomUpdate(World world, Vector3i position, uint data) {}

    /// <summary>
    ///     This method is called for scheduled updates.
    /// </summary>
    protected virtual void ScheduledUpdate(World world, Vector3i position, uint data) {}

    /// <inheritdoc />
    public sealed override string ToString()
    {
        return NamedId;
    }
}
