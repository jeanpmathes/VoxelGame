// <copyright file="DestroyOnFluid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Fluids;

/// <summary>
/// Breaks when filled with any fluid, both liquid and gas.
/// </summary>
public class DestroyOnFluid : BlockBehavior, IBehavior<DestroyOnFluid, BlockBehavior, Block>
{
    private DestroyOnFluid(Block subject) : base(subject) {}
    
    /// <inheritdoc />
    public static DestroyOnFluid Construct(Block input)
    {
        return new DestroyOnFluid(input);
    }
    
    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.StateUpdateMessage>(OnStateUpdate);
        bus.Subscribe<Block.PlacementCompletedMessage>(OnPlacementCompleted);
    }

    private void OnStateUpdate(Block.StateUpdateMessage message)
    {
        if (!message.NewContent.Fluid.IsEmpty) 
            Subject.ScheduleDestroy(message.World, message.Position);
    }
    
    private void OnPlacementCompleted(Block.PlacementCompletedMessage message)
    {
        Content? content = message.World.GetContent(message.Position);
        
        if (content is {Fluid.IsEmpty: false})
            Subject.ScheduleDestroy(message.World, message.Position);
    }
}
