// <copyright file="Barrier.cs" company="VoxelGame">
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
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Fluids;

/// <summary>
/// Can be opened and closed, allowing fluids to pass through.
/// </summary>
public class Barrier : BlockBehavior, IBehavior<Barrier, BlockBehavior, Block>
{
    private IAttribute<Boolean> IsOpen => isOpen ?? throw Exceptions.NotInitialized(nameof(isOpen));
    private IAttribute<Boolean>? isOpen;

    private Barrier(Block subject) : base(subject)
    {
        var fillable = subject.Require<Fillable>();
        fillable.IsInflowAllowed.ContributeFunction(GetIsInflowAllowed);
        fillable.IsOutflowAllowed.ContributeFunction(GetIsOutflowAllowed);
    }

    /// <inheritdoc />
    public static Barrier Construct(Block input)
    {
        return new Barrier(input);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        isOpen = builder.Define(nameof(isOpen)).Boolean().Attribute();
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.ActorInteractionMessage>(OnActorInteraction);
    }

    private Boolean GetIsInflowAllowed(Boolean original, (World world, Vector3i position, State state, Side side, Fluid fluid) context)
    {
        (World _, Vector3i _, State state, Side _, Fluid _) = context;

        return IsBarrierOpen(state);
    }
    
    private Boolean GetIsOutflowAllowed(Boolean original, (World world, Vector3i position, State state, Side side, Fluid fluid) context)
    {
        (World _, Vector3i _, State state, Side _, Fluid _) = context;

        return IsBarrierOpen(state);
    }
    
    private void OnActorInteraction(Block.ActorInteractionMessage message)
    {
        message.Actor.World.SetBlock(new BlockInstance(message.State.With(IsOpen, !message.State.Get(IsOpen))), message.Position);
    }
    
    /// <summary>
    /// Get whether the barrier is open or closed. Only open barriers allow fluids to pass through.
    /// </summary>
    /// <param name="state">The state of the block.</param>
    /// <returns><c>true</c> if the barrier is open, <c>false</c> if it is closed.</returns>
    public Boolean IsBarrierOpen(State state)
    {
        return state.Get(IsOpen);
    }
}
