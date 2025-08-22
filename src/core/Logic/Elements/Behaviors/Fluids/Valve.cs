﻿// <copyright file="Valve.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Elements.Behaviors.Visuals;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Fluids;

/// <summary>
/// Similar to a <see cref="Barrier"/>, but using <see cref="Modelled"/> blocks instead of textured ones.
/// </summary>
public class Valve : BlockBehavior, IBehavior<Valve, BlockBehavior, Block>
{
    // todo: valve and barrier do the same thing, merge both into a single behavior with conditonal require for Modelled or CubeTextured
    // todo: also fix that barrier does not change texture at all
    
    private IAttribute<Boolean> IsOpen => isOpen ?? throw Exceptions.NotInitialized(nameof(isOpen));
    private IAttribute<Boolean>? isOpen;
    
    private Valve(Block subject) : base(subject)
    {
        subject.Require<Modelled>().Selector.ContributeFunction(GetSelector);
        
        var fillable = subject.Require<Fillable>();
        fillable.IsInflowAllowed.ContributeFunction(GetIsInflowAllowed);
        fillable.IsOutflowAllowed.ContributeFunction(GetIsOutflowAllowed);
    }

    /// <inheritdoc />
    public static Valve Construct(Block input)
    {
        return new Valve(input);
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
    
    private Vector4i GetSelector(Vector4i original, State state)
    {
        Vector4i result = original;
        result.W = state.Get(IsOpen) ? 0 : 1;
        return result;
    }
    
    private Boolean GetIsInflowAllowed(Boolean original, (World world, Vector3i position, State state, Side side, Fluid fluid) context)
    {
        (World _, Vector3i _, State state, Side _, Fluid _) = context;

        return IsValveOpen(state);
    }
    
    private Boolean GetIsOutflowAllowed(Boolean original, (World world, Vector3i position, State state, Side side, Fluid fluid) context)
    {
        (World _, Vector3i _, State state, Side _, Fluid _) = context;

        return IsValveOpen(state);
    }
    
    private void OnActorInteraction(Block.ActorInteractionMessage message)
    {
        message.Actor.World.SetBlock(new BlockInstance(message.State.With(IsOpen, !message.State.Get(IsOpen))), message.Position);
    }
    
    /// <summary>
    /// Get whether the valve is open or closed. Only open valves allow fluids to pass through.
    /// </summary>
    /// <param name="state">The state of the block.</param>
    /// <returns><c>true</c> if the valve is open, <c>false</c> if it is closed.</returns>
    public Boolean IsValveOpen(State state)
    {
        return state.Get(IsOpen);
    }
}
