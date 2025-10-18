// <copyright file="DestroyOnLiquid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

/// <summary>
///     Breaks when filled with more than a certain amount of liquid.
/// </summary>
public partial class DestroyOnLiquid : BlockBehavior, IBehavior<DestroyOnLiquid, BlockBehavior, Block>
{
    [Constructible]
    private DestroyOnLiquid(Block subject) : base(subject)
    {
    }

    /// <summary>
    ///     The threshold above which the block breaks when filled with liquid.
    /// </summary>
    public ResolvedProperty<FluidLevel> Threshold { get; } = ResolvedProperty<FluidLevel>.New<Minimum<FluidLevel, Void>>(nameof(Threshold), FluidLevel.One);

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
