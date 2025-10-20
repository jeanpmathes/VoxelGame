// <copyright file="StoredHeight8.cs" company="VoxelGame">
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
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Height;

/// <summary>
///     Defines the partial block height of a block as a stored attribute with 8 different states.
/// </summary>
/// <seealso cref="PartialHeight" />
public partial class StoredHeight8 : BlockBehavior, IBehavior<StoredHeight8, BlockBehavior, Block>
{
    /// <summary>
    ///     The minimum height that can be stored in this behavior.
    /// </summary>
    private static readonly Int32 minimumHeight = BlockHeight.Minimum.ToInt32() / 2;

    /// <summary>
    ///     The maximum height that can be stored in this behavior.
    /// </summary>
    private static readonly Int32 maximumHeight = BlockHeight.Maximum.ToInt32() / 2;

    [Constructible]
    private StoredHeight8(Block subject) : base(subject)
    {
        subject.Require<PartialHeight>().Height.ContributeFunction((_, state) => BlockHeight.FromInt32(state.Get(Height) * 2 + 1), exclusive: true);
        subject.Require<StoredHeight>().HeightedState.ContributeFunction(GetHeightedState);
    }

    [LateInitialization] private partial IAttribute<Int32> Height { get; set; }

    /// <summary>
    ///     The preferred height of the block at placement.
    /// </summary>
    public ResolvedProperty<BlockHeight> PlacementHeight { get; } = ResolvedProperty<BlockHeight>.New<Exclusive<BlockHeight, Void>>(nameof(PlacementHeight));
    
    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Modifiable.IModifyHeightMessage>(OnModifyHeight);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        PlacementHeight.Initialize(this);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        Height = builder
            .Define(nameof(Height))
            .Int32(minimumHeight, maximumHeight + 1)
            .Attribute(generationDefault: maximumHeight);
    }

    private void OnModifyHeight(Modifiable.IModifyHeightMessage message)
    {
        State newState = message.State.With(Height, (message.State.Get(Height) + 1) % (maximumHeight + 1));
        message.World.SetBlock(newState, message.Position);
    }

    private State GetHeightedState(State original, BlockHeight height)
    {
        return height.IsNone ? original : original.With(Height, height.ToInt32() / 2);
    }
}
