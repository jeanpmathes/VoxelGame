// <copyright file="DestroyOnLiquid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

/// <summary>
///     Breaks when filled with more than a certain amount of liquid.
/// </summary>
public class DestroyOnLiquid : BlockBehavior, IBehavior<DestroyOnLiquid, BlockBehavior, Block>
{
    private DestroyOnLiquid(Block subject) : base(subject)
    {
        ThresholdInitializer = Aspect<FluidLevel, Block>.New<Minimum<FluidLevel, Block>>(nameof(ThresholdInitializer), this);
    }

    /// <summary>
    ///     The threshold above which the block breaks when filled with liquid.
    /// </summary>
    public FluidLevel Threshold { get; private set; } = FluidLevel.One;

    /// <summary>
    ///     Aspect used to initialize the <see cref="Threshold" /> property.
    /// </summary>
    public Aspect<FluidLevel, Block> ThresholdInitializer { get; }

    /// <inheritdoc />
    public static DestroyOnLiquid Construct(Block input)
    {
        return new DestroyOnLiquid(input);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IStateUpdateMessage>(OnStateUpdate);
        bus.Subscribe<Block.IPlacementCompletedMessage>(OnPlacementCompleted);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Threshold = ThresholdInitializer.GetValue(FluidLevel.One, Subject);
    }

    private void OnStateUpdate(Block.IStateUpdateMessage message)
    {
        if (message.NewState.Fluid.Fluid.IsLiquid && message.NewState.Fluid.Level > Threshold)
            Subject.ScheduleDestroy(message.World, message.Position);
    }

    private void OnPlacementCompleted(Block.IPlacementCompletedMessage message)
    {
        Content? content = message.World.GetContent(message.Position);

        if (content is {Fluid.Fluid.IsLiquid: true} && content.Value.Fluid.Level > Threshold)
            Subject.ScheduleDestroy(message.World, message.Position);
    }
}
