// <copyright file="DestroyOnFluid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

/// <summary>
///     Breaks when filled with any fluid, both liquid and gas.
/// </summary>
public partial class DestroyOnFluid : BlockBehavior, IBehavior<DestroyOnFluid, BlockBehavior, Block>
{
    [Constructible]
    private DestroyOnFluid(Block subject) : base(subject) {}

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IStateUpdateMessage>(OnStateUpdate);
        bus.Subscribe<Block.IPlacementCompletedMessage>(OnPlacementCompleted);
    }

    private void OnStateUpdate(Block.IStateUpdateMessage message)
    {
        if (!message.NewState.Fluid.IsEmpty)
            Subject.ScheduleDestroy(message.World, message.Position);
    }

    private void OnPlacementCompleted(Block.IPlacementCompletedMessage message)
    {
        Content? content = message.World.GetContent(message.Position);

        if (content is {Fluid.IsEmpty: false})
            Subject.ScheduleDestroy(message.World, message.Position);
    }
}
