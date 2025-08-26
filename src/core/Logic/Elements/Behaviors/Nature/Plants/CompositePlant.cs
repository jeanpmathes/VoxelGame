// <copyright file="CompositePlant.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Logic.Elements.Behaviors.Nature.Plants;

/// <summary>
/// Glue behavior for plants that are made up of multiple parts.
/// </summary>
public class CompositePlant : BlockBehavior, IBehavior<CompositePlant, BlockBehavior, Block>
{
    private CompositePlant(Block subject) : base(subject)
    {
        subject.Require<Composite>().IsPlacementAllowed.ContributeFunction(GetIsPlacementAllowed);
    }

    /// <inheritdoc />
    public static CompositePlant Construct(Block input)
    {
        return new CompositePlant(input);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Composite.NeighborUpdateMessage>(OnNeighborUpdate);
    }

    private static Boolean GetIsPlacementAllowed(Boolean original, (World world, Vector3i position, Vector3i part, Actor? actor) context)
    {
        (World world, Vector3i position, Vector3i part, Actor? _) = context;
        
        if (part.Y != 0) return true;
        
        BlockInstance? ground = world.GetBlock(position.Below());
        
        return ground?.Block.Has<Plantable>() == true;
    }
    
    private void OnNeighborUpdate(Composite.NeighborUpdateMessage message)
    {
        if (message.Part.Y != 0)
            return;
        
        if (message.Side != Side.Bottom)
            return;
        
        if (message.World.GetBlock(message.Position.Below())?.Block.Has<Plantable>() != true)
            Subject.Destroy(message.World, message.Position);
    }
}
