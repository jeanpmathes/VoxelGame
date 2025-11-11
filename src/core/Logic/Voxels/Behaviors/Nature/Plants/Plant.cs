// <copyright file="Plant.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Combustion;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Utilities;
using PartialHeight = VoxelGame.Core.Logic.Voxels.Behaviors.Height.PartialHeight;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature.Plants;

/// <summary>
///     Basic behavior for plant blocks.
///     Assumes and supports <see cref="Foliage" /> rendering.
/// </summary>
public partial class Plant : BlockBehavior, IBehavior<Plant, BlockBehavior, Block>
{
    private Boolean isComposite;

    [Constructible]
    private Plant(Block subject) : base(subject)
    {
        subject.Require<Combustible>();
        subject.Require<Fillable>();
        subject.Require<Foliage>().IsLowered.ContributeFunction((_, state) => state.Get(IsLowered));

        subject.RequireIfPresent<CompositePlant, Composite>(_ => isComposite = true);

        subject.PlacementState.ContributeFunction(GetPlacementState);
        subject.IsPlacementAllowed.ContributeFunction(GetPlacementAllowed);
    }

    [LateInitialization] private partial IAttributeData<Boolean> IsLowered { get; set; }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        if (!isComposite)
            bus.Subscribe<Block.INeighborUpdateMessage>(OnNeighborUpdate);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        properties.IsSolid.ContributeConstant(value: false);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        IsLowered = builder.Define(nameof(IsLowered)).Boolean().Attribute();
    }

    private Boolean GetPlacementAllowed(Boolean original, (World world, Vector3i position, Actor? actor) context)
    {
        if (isComposite) return true;

        (World world, Vector3i position, Actor? _) = context;

        State? ground = world.GetBlock(position.Below());

        return ground?.Block.Is<Plantable>() == true;
    }

    private State GetPlacementState(State state, (World world, Vector3i position, Actor? actor) context)
    {
        (World world, Vector3i position, _) = context;

        State? below = world.GetBlock(position.Below());

        state.Set(IsLowered, below?.Block.Get<PartialHeight>() is {} partialHeight && !partialHeight.GetHeight(below.Value).IsFull);

        return state;
    }

    private void OnNeighborUpdate(Block.INeighborUpdateMessage message)
    {
        if (message.Side != Side.Bottom)
            return;

        if (message.World.GetBlock(message.Position.Below())?.Block.Is<Plantable>() != true)
            Subject.Destroy(message.World, message.Position);
    }
}
