﻿// <copyright file="DestroyOnFluid.cs" company="VoxelGame">
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
        bus.Subscribe<Block.ContentUpdateMessage>(OnContentUpdate);
    }

    private void OnContentUpdate(Block.ContentUpdateMessage message)
    {
        if (!message.NewContent.Fluid.IsEmpty) 
            Subject.ScheduleDestroy(message.World, message.Position);
    }
}
