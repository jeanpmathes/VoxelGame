// <copyright file="CrackedMud.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature;

/// <summary>
///     Behavior for cracked mud blocks, allowing them to absorb water and revert to mud.
/// </summary>
public partial class CrackedMud : BlockBehavior, IBehavior<CrackedMud, BlockBehavior, Block>
{
    [Constructible]
    private CrackedMud(Block subject) : base(subject) {}

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IStateUpdateMessage>(OnStateUpdate);
    }

    private static void OnStateUpdate(Block.IStateUpdateMessage message)
    {
        if (!message.NewState.Fluid.IsAnyWater)
            return;

        message.World.SetContent(Content.Create(Blocks.Instance.Environment.Mud), message.Position);
    }
}
