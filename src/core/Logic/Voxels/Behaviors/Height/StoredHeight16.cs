// <copyright file="StoredHeight16.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Toolkit.Utilities;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Height;

/// <summary>
///     Defines the partial block height of a block as a stored attribute with 16 different states.
/// </summary>
/// <seealso cref="PartialHeight" />
public partial class StoredHeight16 : BlockBehavior, IBehavior<StoredHeight16, BlockBehavior, Block>
{
    [Constructible]
    private StoredHeight16(Block subject) : base(subject)
    {
        subject.Require<PartialHeight>().Height.ContributeFunction((_, state) => state.Get(Height), exclusive: true);
        subject.Require<StoredHeight>().HeightedState.ContributeFunction(GetHeightedState);
    }

    [LateInitialization] private partial IAttribute<Int32> Height { get; set; }

    /// <summary>
    ///     The preferred height of the block at placement.
    /// </summary>
    public ResolvedProperty<Int32> PlacementHeight { get; } = ResolvedProperty<Int32>.New<Exclusive<Int32, Void>>(nameof(PlacementHeight));

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Modifiable.IModifyHeightMessage>(OnModifyHeight);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        Height = builder
            .Define(nameof(Height))
            .Int32(PartialHeight.MinimumHeight, PartialHeight.MaximumHeight + 1)
            .Attribute(generationDefault: PartialHeight.MaximumHeight);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        PlacementHeight.Initialize(this);
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (PlacementHeight.Get() is >= PartialHeight.MinimumHeight and <= PartialHeight.MaximumHeight)
            return;

        validator.ReportWarning("Placement height is out of bounds");
        PlacementHeight.Override(PartialHeight.MinimumHeight);
    }

    private void OnModifyHeight(Modifiable.IModifyHeightMessage message)
    {
        State state = message.State;

        Int32 newHeight = (state.Get(Height) + 1) % (PartialHeight.MaximumHeight + 1);

        message.World.SetBlock(state.With(Height, newHeight), message.Position);
    }

    private State GetHeightedState(State original, Int32 height)
    {
        Int32 clampedHeight = Math.Clamp(height, PartialHeight.MinimumHeight, PartialHeight.MaximumHeight);

        return original.With(Height, clampedHeight);
    }
}
