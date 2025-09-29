// <copyright file="AxisRotatable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Orienting;

/// <summary>
/// Allows rotation of the block to align with each of the three axes.
/// </summary>
public partial class AxisRotatable : BlockBehavior, IBehavior<AxisRotatable, BlockBehavior, Block>
{
    [LateInitialization]
    private partial IAttribute<Axis> Axis { get; set; }
    
    private AxisRotatable(Block subject) : base(subject)
    {
        var rotatable = subject.Require<Rotatable>();
        rotatable.Axis.ContributeFunction(GetAxis);
        rotatable.Turns.ContributeFunction(GetTurns);
        
        subject.PlacementState.ContributeFunction(GetPlacementState);
    }

    /// <inheritdoc/>
    public static AxisRotatable Construct(Block input)
    {
        return new AxisRotatable(input);
    }

    /// <inheritdoc/>
    public override void DefineState(IStateBuilder builder)
    {
        Axis = builder.Define(nameof(Axis)).Enum<Axis>().Attribute();
    }
    
    private Axis GetAxis(Axis original, State state)
    {
        return state.Get(Axis) switch
        {
            Utilities.Axis.Y => Utilities.Axis.Y,
            Utilities.Axis.X => Utilities.Axis.Z,
            Utilities.Axis.Z => Utilities.Axis.X,
            _ => original
        };
    }
    
    private Int32 GetTurns(Int32 original, State state)
    {
        return state.Get(Axis) switch
        {
            Utilities.Axis.X => 1,
            Utilities.Axis.Y => 0,
            Utilities.Axis.Z => 1,
            _ => original
        };
    }
    
    private State GetPlacementState(State original, (World world, Vector3i position, Actor? actor) context)
    {
        (World _, Vector3i _, Actor? actor) = context;
        
        Side? side = actor?.GetTargetedSide()?.Opposite();
        
        return side == null ? original : SetAxis(original, side.Value.Axis());
    }

    /// <summary>
    /// Get the current axis in the given state.
    /// </summary>
    /// <param name="state">The state to get the axis in.</param>
    /// <returns>The current axis.</returns>
    public Axis GetAxis(State state)
    {
        return state.Get(Axis);
    }
    
    /// <summary>
    /// Set the axis in the given state to a new axis.
    /// </summary>
    /// <param name="state">The state to set the axis in.</param>
    /// <param name="newAxis">The new axis to set.</param>
    /// <returns>The new state with the updated axis.</returns>
    public State SetAxis(State state, Axis newAxis)
    {
        return state.With(Axis, newAxis);
    }
}
