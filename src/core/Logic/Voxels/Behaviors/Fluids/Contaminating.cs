// <copyright file="Contaminating.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

/// <summary>
///     Contaminates water when the block is destroyed.
/// </summary>
public partial class Contaminating : BlockBehavior, IBehavior<Contaminating, BlockBehavior, Block>
{
    [Constructible]
    private Contaminating(Block subject) : base(subject)
    {
        subject.Require<Fillable>();
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IDestructionCompletedMessage>(OnDestructionCompleted);
    }
    
    private static void OnDestructionCompleted(Block.IDestructionCompletedMessage message)
    {
        Content? content = message.World.GetContent(message.Position);
        
        if (content is {Fluid: var fluid} && fluid.Fluid == Voxels.Fluids.Instance.FreshWater)
        {
            message.World.SetFluid(
                Voxels.Fluids.Instance.WasteWater.AsInstance(fluid.Level, fluid.IsStatic),
                message.Position);
        }
    }
}
