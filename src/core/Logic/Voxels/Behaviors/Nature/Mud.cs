// <copyright file="Mud.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature;

/// <summary>
///     When <see cref="Soil" /> gets filled with too much water, it turns into <see cref="Mud" />.
/// </summary>
public partial class Mud : BlockBehavior, IBehavior<Mud, BlockBehavior, Block>
{
    [Constructible]
    private Mud(Block subject) : base(subject)
    {
        subject.Require<Plantable>();
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Plantable.IGrowthAttemptMessage>(OnGrowthAttempt);
    }

    private static void OnGrowthAttempt(Plantable.IGrowthAttemptMessage message)
    {
        if (message.Fluid != Voxels.Fluids.Instance.FreshWater) return;
        
        FluidLevel remaining = FluidLevel.Full - message.Level;

        message.World.SetContent(remaining >= FluidLevel.One
                ? new Content(new State(Blocks.Instance.Environment.Soil), Voxels.Fluids.Instance.FreshWater.AsInstance(remaining))
                : new Content(Blocks.Instance.Environment.Soil),
            message.Position);

        message.MarkAsSuccessful();
    }
}
