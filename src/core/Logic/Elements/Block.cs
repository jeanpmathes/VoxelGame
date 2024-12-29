// <copyright file="Block.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements;

/// <summary>
///     The basic block class. Blocks are used to construct the world.
/// </summary>
public partial class Block : IBlockMeshable, IIdentifiable<UInt32>, IIdentifiable<String>, IResource
{
    private const UInt32 InvalidID = UInt32.MaxValue;
    private readonly BoundingVolume boundingVolume;

    /// <summary>
    ///     Create a new block.
    /// </summary>
    /// <param name="name">The name of the block. Can be localized.</param>
    /// <param name="namedID">The named ID of the block. A unique and unlocalized identifier.</param>
    /// <param name="flags">The block flags setting specific options.</param>
    /// <param name="boundingVolume">The base bounding volume for this block. Is used for placement checks.</param>
    protected Block(String name, String namedID, BlockFlags flags, BoundingVolume boundingVolume)
    {
        Name = name;
        NamedID = namedID;
        Identifier = RID.Named<Block>(namedID);

        IsFull = flags.IsFull;
        IsOpaque = flags.IsOpaque;
        RenderFaceAtNonOpaques = flags.RenderFaceAtNonOpaques;
        IsSolid = flags.IsSolid;
        ReceiveCollisions = flags.ReceiveCollisions;
        IsTrigger = flags.IsTrigger;
        IsReplaceable = flags.IsReplaceable;
        IsInteractable = flags.IsInteractable;
        IsUnshaded = flags.IsUnshaded;

        this.boundingVolume = boundingVolume;

        IBlockMeshable meshable = this;
        meshable.Validate();
    }

    /// <summary>
    ///     Get this block as an implementor of the <see cref="IBlockBase" /> interface.
    /// </summary>
    public IBlockBase Base => this;

    /// <inheritdoc />
    public UInt32 ID { get; private set; } = InvalidID;

    /// <inheritdoc />
    public String Name { get; }

    /// <inheritdoc />
    public String NamedID { get; }

    /// <inheritdoc />
    public Boolean RenderFaceAtNonOpaques { get; }

    /// <inheritdoc />
    public Boolean ReceiveCollisions { get; }

    /// <inheritdoc />
    public Boolean IsTrigger { get; }

    /// <inheritdoc />
    public Boolean IsReplaceable { get; }

    /// <inheritdoc />
    public Boolean IsInteractable { get; }

    /// <inheritdoc />
    public Boolean IsUnshaded { get; }

    /// <inheritdoc />
    public Boolean IsFull { get; }

    /// <inheritdoc />
    public Boolean IsOpaque { get; }

    /// <inheritdoc />
    public Boolean IsSolid { get; }

    /// <summary>
    ///     Attempt to place the block in the world.
    /// </summary>
    /// <param name="world">The world in which to place the block.</param>
    /// <param name="position">The position at which to place the block.</param>
    /// <param name="actor"></param>
    /// <returns>True if placement was successful.</returns>
    public Boolean Place(World world, Vector3i position, PhysicsActor? actor = null)
    {
        Content? content = world.GetContent(position);

        if (content == null) return false;

        (BlockInstance block, FluidInstance _) = content.Value;

        Boolean canPlace = block.Block.IsReplaceable && CanPlace(world, position, actor);

        if (!canPlace) return canPlace;

        DoPlace(world, position, actor);

        return canPlace;
    }

    /// <summary>
    ///     Attempt to destroy the block in the world.
    ///     Will always fail if there is a different block at the given position.
    /// </summary>
    /// <param name="world">The world in which to destroy the block.</param>
    /// <param name="position">The position at which to destroy to block.</param>
    /// <param name="actor">The actor destroying the block.</param>1
    /// <returns>True if destruction was successful.</returns>
    public Boolean Destroy(World world, Vector3i position, PhysicsActor? actor = null)
    {
        BlockInstance? potentialBlock = world.GetBlock(position);

        if (potentialBlock is not {} block) return false;
        if (block.Block != this || !CanDestroy(world, position, block.Data, actor)) return false;

        DoDestroy(world, position, block.Data, actor);

        return true;
    }

    String IIdentifiable<String>.ID => NamedID;

    UInt32 IIdentifiable<UInt32>.ID => ID;

    /// <inheritdoc />
    public RID Identifier { get; }

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.Block;

    /// <summary>
    ///     Set up the block.
    /// </summary>
    /// <param name="id">The ID of the block.</param>
    /// <param name="indexProvider">The index provider for the block textures.</param>
    /// <param name="modelProvider">The model provider for the block models.</param>
    /// <param name="visuals">The visual configuration of the game.</param>
    public void SetUp(UInt32 id, ITextureIndexProvider indexProvider, IBlockModelProvider modelProvider, VisualConfiguration visuals)
    {
        Debug.Assert(ID == InvalidID);
        ID = id;

        OnSetUp(indexProvider, modelProvider, visuals);
    }

    /// <summary>
    ///     Called when loading blocks, meant to set up vertex data, indices etc.
    /// </summary>
    /// <param name="textureIndexProvider"></param>
    /// <param name="modelProvider">A model provider.</param>
    /// <param name="visuals">The visual configuration of the game.</param>
    protected virtual void OnSetUp(ITextureIndexProvider textureIndexProvider, IBlockModelProvider modelProvider, VisualConfiguration visuals) {}

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
    protected virtual BoundingVolume GetBoundingVolume(UInt32 data)
    {
        return boundingVolume;
    }

    /// <summary>
    ///     Override this to provide change the block placement checks.
    /// </summary>
    /// <param name="world">The world in which the placement occurs.</param>
    /// <param name="position">The position at which the placement is requested.</param>
    /// <param name="actor">The actor that performs placement.</param>
    /// <returns>True if placement is possible.</returns>
    public virtual Boolean CanPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        return true;
    }

    /// <summary>
    ///     Override this to change the block placement logic.
    ///     The block placement must always be successful. If checks are required, override <see cref="CanPlace" />.
    /// </summary>
    /// <param name="world">The world in which the placement occurs.</param>
    /// <param name="position">The position at which the placement is requested.</param>
    /// <param name="actor">The actor that performs placement.</param>
    protected virtual void DoPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        world.SetBlock(this.AsInstance(), position);
    }

    /// <summary>
    ///     Override this to change the block destruction checks.
    /// </summary>
    /// <param name="world">The world in which the placement occurs.</param>
    /// <param name="position">The position at which the placement is requested.</param>
    /// <param name="data">The block data.</param>
    /// <param name="actor">The actor that performs placement.</param>
    /// <returns></returns>
    protected virtual Boolean CanDestroy(World world, Vector3i position, UInt32 data, PhysicsActor? actor)
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
    /// <param name="actor">The actor that performs placement.</param>
    protected virtual void DoDestroy(World world, Vector3i position, UInt32 data, PhysicsActor? actor)
    {
        world.SetDefaultBlock(position);
    }

    /// <summary>
    ///     This method is called when an actor collides with this block.
    /// </summary>
    /// <param name="actor">The actor that caused the collision.</param>
    /// <param name="position">The block position.</param>
    public void ActorCollision(PhysicsActor actor, Vector3i position)
    {
        BlockInstance? potentialBlock = actor.World.GetBlock(position);
        if (potentialBlock?.Block == this) ActorCollision(actor, position, potentialBlock.Value.Data);
    }

    /// <summary>
    ///     Override to provide custom actor collision logic.
    /// </summary>
    /// <param name="actor">The actor that collided with this block.</param>
    /// <param name="position">The position of the block.</param>
    /// <param name="data">The block data of this block.</param>
    protected virtual void ActorCollision(PhysicsActor actor, Vector3i position, UInt32 data) {}

    /// <summary>
    ///     Called when a block and an actor collide.
    /// </summary>
    /// <param name="actor">The actor that collided with the block.</param>
    /// <param name="position">The block position.</param>
    public void ActorInteract(PhysicsActor actor, Vector3i position)
    {
        BlockInstance? potentialBlock = actor.World.GetBlock(position);
        if (potentialBlock?.Block == this) ActorInteract(actor, position, potentialBlock.Value.Data);
    }

    /// <summary>
    ///     Override to provide custom actor interaction logic.
    /// </summary>
    /// <param name="actor">The actor that interacted with this block.</param>
    /// <param name="position">The position of the block.</param>
    /// <param name="data">The block data of this block.</param>
    protected virtual void ActorInteract(PhysicsActor actor, Vector3i position, UInt32 data) {}

    /// <summary>
    ///     Called when the content at a position changes. This is called on the new block at that position.
    ///     While not very useful to react to block data changes the block caused itself,
    ///     it can be used to react to fluid changes at the position.
    /// </summary>
    /// <param name="world">The containing world.</param>
    /// <param name="position">The block position.</param>
    /// <param name="content">The new content at the position.</param>
    public virtual void ContentUpdate(World world, Vector3i position, Content content) {}

    /// <summary>
    ///     This method is called on blocks next to a position that was changed.
    /// </summary>
    /// <param name="world">The containing world.</param>
    /// <param name="position">The block position.</param>
    /// <param name="data">The data of the block next to the changed position.</param>
    /// <param name="side">The side of the block where the change happened.</param>
    public virtual void NeighborUpdate(World world, Vector3i position, UInt32 data, Side side) {}

    /// <summary>
    ///     This method is called randomly on some blocks every update.
    /// </summary>
    public virtual void RandomUpdate(World world, Vector3i position, UInt32 data) {}

    /// <summary>
    ///     This method is called for scheduled updates.
    /// </summary>
    protected virtual void ScheduledUpdate(World world, Vector3i position, UInt32 data) {}

    /// <summary>
    ///     This method is called on every block after the chunk the block is in has been generated.
    /// </summary>
    /// <param name="content">The content that is generated, containing this block.</param>
    /// <returns>Potentially modified content.</returns>
    public virtual Content GeneratorUpdate(Content content)
    {
        return content;
    }

    /// <inheritdoc />
    public sealed override String ToString()
    {
        return NamedID;
    }

    #region DISPOSING

    private Boolean disposed;

    /// <summary>
    /// Override to dispose resources.
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources.</param>
    protected virtual void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            // Nothing to dispose.
        }
        else Throw.ForMissedDispose(this);

        disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~Block()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSING
}
