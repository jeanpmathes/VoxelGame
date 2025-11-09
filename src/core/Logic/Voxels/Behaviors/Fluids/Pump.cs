// <copyright file="Pump.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

/// <summary>
///     Elevates contained fluids upwards when interacted with.
/// </summary>
public partial class Pump : BlockBehavior, IBehavior<Pump, BlockBehavior, Block>
{
    [Constructible]
    private Pump(Block subject) : base(subject)
    {
        subject.Require<Fillable>().IsInflowAllowed.ContributeFunction(GetIsInflowAllowed);
        subject.Require<Fillable>().IsOutflowAllowed.ContributeFunction(GetIsOutflowAllowed);
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

    private static void OnActorInteraction(Block.IActorInteractionMessage message)
    {
        Fluid.Elevate(message.Actor.World, message.Position, distance: 16);
    }
}
