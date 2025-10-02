// <copyright file="Pump.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Fluids;

/// <summary>
///     Elevates contained fluids upwards when interacted with.
/// </summary>
public class Pump : BlockBehavior, IBehavior<Pump, BlockBehavior, Block>
{
    private Pump(Block subject) : base(subject)
    {
        subject.Require<Fillable>().IsInflowAllowed.ContributeFunction(GetIsInflowAllowed);
        subject.Require<Fillable>().IsOutflowAllowed.ContributeFunction(GetIsOutflowAllowed);
    }

    /// <inheritdoc />
    public static Pump Construct(Block input)
    {
        return new Pump(input);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IActorInteractionMessage>(OnActorInteraction);
    }

    private static Boolean GetIsInflowAllowed(Boolean original, (World world, Vector3i position, State state, Side side, Fluid fluid) context)
    {
        (World _, Vector3i _, State _, Side side, Fluid _) = context;

        return side != Side.Top;
    }

    private static Boolean GetIsOutflowAllowed(Boolean original, (World world, Vector3i position, State state, Side side, Fluid fluid) context)
    {
        (World _, Vector3i _, State _, Side side, Fluid _) = context;

        return side == Side.Top;
    }

    private void OnActorInteraction(Block.IActorInteractionMessage message)
    {
        Fluid.Elevate(message.Actor.World, message.Position, distance: 16);
    }
}
