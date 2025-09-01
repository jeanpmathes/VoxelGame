// <copyright file="AxisRotatable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Orienting;

/// <summary>
/// Allows rotation of the block to align with each of the three axes.
/// </summary>
public class AxisRotatable : BlockBehavior, IBehavior<AxisRotatable, BlockBehavior, Block>
{
    private IAttribute<Axis> Axis => axis ?? throw Exceptions.NotInitialized(nameof(axis));
    private IAttribute<Axis>? axis;
    
    private AxisRotatable(Block subject) : base(subject)
    {
        subject.Require<Rotatable>().Rotation.ContributeFunction(GetRotation);
    }
    
    // todo: use conditionals to rotate texture 
    // Boolean isLeftOrRightSide = info.Side is Side.Left or Side.Right;
    // Boolean onXAndRotated = axis == Axis.X && !isLeftOrRightSide;
    // Boolean onZAndRotated = axis == Axis.Z && isLeftOrRightSide;
    // Boolean rotated = onXAndRotated || onZAndRotated;
    
    /// <inheritdoc/>
    public static AxisRotatable Construct(Block input)
    {
        return new AxisRotatable(input);
    }

    /// <inheritdoc/>
    public override void DefineState(IStateBuilder builder)
    {
        axis = builder.Define(nameof(axis)).Enum<Axis>().Attribute();
    }
    
    private Side GetRotation(Side original, State state)
    {
        return TranslateSide(original, GetAxis(state));
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
    
    private static Side TranslateSide(Side side, Axis axis)
    {
        return axis switch
        {
            Utilities.Axis.Y => side,
            Utilities.Axis.X =>
                side.Rotate(Utilities.Axis.Z),
            Utilities.Axis.Z =>
                side.Rotate(Utilities.Axis.X),
            _ => throw Exceptions.UnsupportedEnumValue(axis)
        };
    }
}
