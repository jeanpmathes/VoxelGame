// <copyright file="CoveredDirt.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Nature;

/// <summary>
/// Blocks with this behavior are dirt blocks that are covered with something.
/// Some conditions like placement of a block on top remove the covering, turning the block into a regular dirt block.
/// </summary>
public class CoveredDirt : BlockBehavior, IBehavior<CoveredDirt, BlockBehavior, Block>
{
    private CoveredDirt(Block subject) : base(subject)
    {
        subject.Require<Dirt>();
        
        subject.IsPlacementAllowed.ContributeFunction(GetPlacementAllowed);
    }

    /// <inheritdoc/>
    public static CoveredDirt Construct(Block input)
    {
        return new CoveredDirt(input);
    }

    /// <inheritdoc/>
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.PlacementMessage>(OnPlacement);
        bus.Subscribe<Block.NeighborUpdateMessage>(OnNeighborUpdate);
    }
    
    private static Boolean GetPlacementAllowed(Boolean original, (World world, Vector3i position, Actor? actor) context)
    {
        (World world, Vector3i position, Actor? actor) = context;
        
        return CanHaveCover(world, position) == false || Blocks.Instance.Environment.Dirt.CanPlace(world, position, actor);
    }

    private void OnPlacement(Block.PlacementMessage message)
    {
        if (CanHaveCover(message.World, message.Position) == false) 
            message.World.SetBlock(new BlockInstance(Subject.States.PlacementDefault), message.Position);
        else 
            Blocks.Instance.Environment.Dirt.Place(message.World, message.Position, message.Actor);
    }
    
    private void OnNeighborUpdate(Block.NeighborUpdateMessage message)
    {
        if (message.Side == Side.Top && CanHaveCover(message.World, message.Position) != false)
            RemoveCover(message.World, message.Position);
    }
    
    /// <summary>
    /// Check if a position can have cover on it.
    /// </summary>
    /// <param name="world">The world in which the position is located.</param>
    /// <param name="position">The position to check.</param>
    /// <returns><c>true</c> if the position can have cover, <c>false</c> if it cannot, and <c>null</c> if it is unknown.</returns>
    public static Boolean? CanHaveCover(World world, Vector3i position)
    {
        BlockInstance? top = world.GetBlock(position.Above());

        if (top is null) return null;
        
        // todo: add a new behavior for blocks that do not destroy cover despite being solid and opaque, e.g. snow

        return top.Value.Block is {IsOpaque: true, IsSolid: true} && top.Value.IsSideFull(Side.Bottom);
    }

    /// <summary>
    /// Remove the cover from a covered soil block, turning it into a regular soil block.
    /// </summary>
    /// <param name="world">The world in which the block is located.</param>
    /// <param name="position">The position of the block.</param>
    public void RemoveCover(World world, Vector3i position)
    {
        world.SetBlock(Blocks.Instance.Environment.Dirt.AsInstance(), position);
    }
}
