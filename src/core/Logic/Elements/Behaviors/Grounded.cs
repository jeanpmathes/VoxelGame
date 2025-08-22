// <copyright file="Grounded.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Elements.Behaviors.Height;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors;

/// <summary>
/// Makes a block require fully solid ground to be placed.
/// </summary>
public class Grounded : BlockBehavior, IBehavior<Grounded, BlockBehavior, Block>
{
    private Grounded(Block subject) : base(subject)
    {
        subject.IsPlacementAllowed.ContributeFunction(GetPlacementAllowed);
    }

    /// <inheritdoc/>
    public static Grounded Construct(Block input)
    {
        return new Grounded(input);
    }

    /// <inheritdoc/>
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.PlacementCompletedMessage>(OnPlacementCompleted);
        bus.Subscribe<Block.NeighborUpdateMessage>(OnNeighborUpdate);
    }

    private static Boolean GetPlacementAllowed(Boolean original, (World world, Vector3i position, Actor? actor) context)
    {
        (World world, Vector3i position, Actor? _) = context;

        return IsGrounded(world, position);
    }

    private static Boolean IsGrounded(World world, Vector3i position)
    {
        Vector3i positionBelow = position.Below();
        BlockInstance blockBelow = world.GetBlock(positionBelow) ?? BlockInstance.Default;

        return blockBelow.IsFullySolid || blockBelow.Block.Has<CompletableGround>();
    }
    
    private static void OnPlacementCompleted(Block.PlacementCompletedMessage message)
    {
        Vector3i positionBelow = message.Position.Below();
        BlockInstance blockBelow = message.World.GetBlock(positionBelow) ?? BlockInstance.Default;
        
        if (blockBelow.IsFullySolid) 
            return;

        if (blockBelow.Block.Get<CompletableGround>() is {} completableGround)
            completableGround.BecomeComplete(message.World, positionBelow);
    } 
    
    private void OnNeighborUpdate(Block.NeighborUpdateMessage message)
    {
        if (message.Side != Side.Bottom || IsGrounded(message.World, message.Position)) return;

        Subject.ScheduleDestroy(message.World, message.Position);
    }
}
