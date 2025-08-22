// <copyright file="Plant.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Combustion;
using VoxelGame.Core.Logic.Elements.Behaviors.Fluids;
using VoxelGame.Core.Logic.Elements.Behaviors.Meshables;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;
using PartialHeight = VoxelGame.Core.Logic.Elements.Behaviors.Height.PartialHeight;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Nature.Plants;

/// <summary>
/// Basic behavior for plant blocks.
/// Assumes and supports <see cref="Foliage"/> rendering.
/// </summary>
public class Plant : BlockBehavior, IBehavior<Plant, BlockBehavior, Block>
{
    private IAttribute<Boolean> IsLowered => isLowered ?? throw Exceptions.NotInitialized(nameof(isLowered));
    private IAttribute<Boolean>? isLowered;
    
    private Plant(Block subject) : base(subject)
    {
        subject.Require<Combustible>();
        subject.Require<Fillable>();
        subject.Require<Foliage>().IsLowered.ContributeFunction((_, state) => state.Get(IsLowered));
        
        subject.PlacementState.ContributeFunction(GetPlacementState);
        subject.IsPlacementAllowed.ContributeFunction(GetPlacementAllowed);
    }

    /// <inheritdoc />
    public static Plant Construct(Block input)
    {
        return new Plant(input);
    }
    
    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        properties.IsSolid.ContributeConstant(value: false);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        isLowered = builder.Define(nameof(isLowered)).Boolean().Attribute();
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.NeighborUpdateMessage>(OnNeighborUpdate);
    }

    private static Boolean GetPlacementAllowed(Boolean original, (World world, Vector3i position, Actor? actor) context)
    {
        (World world, Vector3i position, Actor? _) = context;
        
        BlockInstance? ground = world.GetBlock(position.Below());
        
        return ground?.Block.Has<Plantable>() == true;
    }

    private State GetPlacementState(State state, (World world, Vector3i position, Actor? actor) context)
    {
        (World world, Vector3i position, _) = context;
        
        BlockInstance? below = world.GetBlock(position.Below());

        state.Set(IsLowered, below?.Block.Get<PartialHeight>() is {} partialHeight && partialHeight.GetHeight(below.Value.State) < PartialHeight.MaximumHeight);
        
        return state;
    }

    private void OnNeighborUpdate(Block.NeighborUpdateMessage message)
    {
        if (message.Side != Side.Bottom)
            return;
        
        if (message.World.GetBlock(message.Position.Below())?.Block.Has<Plantable>() != true)
            Subject.Destroy(message.World, message.Position);
    }
}
