// <copyright file="DestroyOnLiquid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

/// <summary>
///     Breaks when filled with more than a certain amount of liquid, by default any amount causes breaking.
/// </summary>
public partial class DestroyOnLiquid : BlockBehavior, IBehavior<DestroyOnLiquid, BlockBehavior, Block>
{
    [Constructible]
    private DestroyOnLiquid(Block subject) : base(subject)
    {
        subject.Require<Fillable>();
    }

    /// <summary>
    ///     The threshold above which the block breaks when filled with liquid.
    /// </summary>
    public ResolvedProperty<FluidLevel> Threshold { get; } = ResolvedProperty<FluidLevel>.New<Minimum<FluidLevel, Void>>(nameof(Threshold), FluidLevel.None);

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IStateUpdateMessage>(OnStateUpdate);
        bus.Subscribe<Block.IPlacementCompletedMessage>(OnPlacementCompleted);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Threshold.Initialize(this);
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (Threshold.Get().IsFull)
            validator.ReportWarning("The threshold is set to full fluid level, rendering the behavior useless");
    }

    private void OnStateUpdate(Block.IStateUpdateMessage message)
    {
        if (message.NewState.Fluid.Fluid.IsLiquid && message.NewState.Fluid.Level > Threshold.Get())
            Subject.ScheduleDestroy(message.World, message.Position);
    }

    private void OnPlacementCompleted(Block.IPlacementCompletedMessage message)
    {
        Content? content = message.World.GetContent(message.Position);

        if (content is {Fluid.Fluid.IsLiquid: true} && content.Value.Fluid.Level > Threshold.Get())
            Subject.ScheduleDestroy(message.World, message.Position);
    }
}
