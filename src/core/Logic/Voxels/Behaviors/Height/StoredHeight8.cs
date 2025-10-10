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
    public const Int32 MinimumHeight = PartialHeight.MinimumHeight / 2;

    /// <summary>
    ///     The maximum height that can be stored in this behavior.
    /// </summary>
    public const Int32 MaximumHeight = (PartialHeight.MaximumHeight + 1) / 2;

    private StoredHeight8(Block subject) : base(subject)
    {
        subject.Require<PartialHeight>().Height.ContributeFunction((_, state) => state.Get(Height) * 2, exclusive: true);

        PlacementHeightInitializer = Aspect<Int32, Block>.New<Exclusive<Int32, Block>>(nameof(PlacementHeightInitializer), this);
    }

    [LateInitialization] private partial IAttribute<Int32> Height { get; set; }

    /// <summary>
    ///     The preferred height of the block at placement.
    /// </summary>
    public Int32 PlacementHeight { get; private set; }

    /// <summary>
    ///     Aspect used to initialize the <see cref="PlacementHeight" /> property.
    /// </summary>
    public Aspect<Int32, Block> PlacementHeightInitializer { get; }

    /// <inheritdoc />
    public static StoredHeight8 Construct(Block input)
    {
        return new StoredHeight8(input);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Modifiable.IModifyHeightMessage>(OnModifyHeight);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        PlacementHeight = PlacementHeightInitializer.GetValue(original: 0, Subject);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        Height = builder
            .Define(nameof(Height))
            .Int32(MinimumHeight, MaximumHeight + 1)
            .Attribute(generationDefault: MaximumHeight);
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (PlacementHeight is >= MinimumHeight and <= MaximumHeight)
            return;

        validator.ReportWarning("Placement height is out of bounds");
        PlacementHeight = MinimumHeight;
    }

    private void OnModifyHeight(Modifiable.IModifyHeightMessage message)
    {
        State newState = message.State.With(Height, (message.State.Get(Height) + 1) % MaximumHeight);
        message.World.SetBlock(newState, message.Position);
    }
}
