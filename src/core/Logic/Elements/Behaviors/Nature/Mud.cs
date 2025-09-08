// <copyright file="Mud.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Nature;

/// <summary>
/// When <see cref="Soil"/> gets filled with too much water, it turns into <see cref="Mud"/>.
/// </summary>
public class Mud : BlockBehavior, IBehavior<Mud, BlockBehavior, Block>
{
    private Mud(Block subject) : base(subject)
    {
        subject.Require<Plantable>();
    }
    
    /// <inheritdoc/>
    public static Mud Construct(Block input)
    {
        return new Mud(input);
    }

    /// <inheritdoc/>
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Plantable.GrowthAttemptMessage>(OnGrowthAttempt);
    }

    private static void OnGrowthAttempt(Plantable.GrowthAttemptMessage message)
    {
        if (message.Fluid != Elements.Fluids.Instance.FreshWater)
        {
            message.CanGrow = false;
            return;
        }

        FluidLevel remaining = FluidLevel.Eight - (Int32) message.Level; // todo: add a fluid level struct

        message.World.SetContent(remaining >= FluidLevel.One
                ? new Content(Blocks.Instance.Environment.Soil.AsInstance(), Elements.Fluids.Instance.FreshWater.AsInstance(remaining))
                : new Content(Blocks.Instance.Environment.Soil),
            message.Position);

        message.CanGrow = true;
    }
}
