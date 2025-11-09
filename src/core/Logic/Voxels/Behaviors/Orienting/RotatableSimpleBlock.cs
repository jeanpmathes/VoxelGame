// <copyright file="RotatableSimpleBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Orienting;

/// <summary>
///     Glue behavior for blocks that are both <see cref="Rotatable" /> and meshed using <see cref="Simple" />.
/// </summary>
public partial class RotatableSimpleBlock : BlockBehavior, IBehavior<RotatableSimpleBlock, BlockBehavior, Block>
{
    private readonly Rotatable rotatable;

    [Constructible]
    private RotatableSimpleBlock(Block subject) : base(subject)
    {
        rotatable = subject.Require<Rotatable>();

        subject.Require<CubeTextured>().Rotation.ContributeFunction(GetRotation);
        subject.Require<Simple>().IsTextureRotated.ContributeFunction(GetIsTextureRotated);
    }

    private Boolean GetIsTextureRotated(Boolean original, (State state, Side side) context)
    {
        (State state, Side side) = context;

        Axis axis = rotatable.GetCurrentAxis(state);
        Int32 turns = MathTools.Mod(rotatable.GetCurrentTurns(state), m: 4);

        if (turns == 0 || turns == 2 || axis == Axis.Y) return false;

        Boolean isLeftOrRightSide = side is Side.Left or Side.Right;
        Boolean onXAndRotated = axis == Axis.X && isLeftOrRightSide;
        Boolean onZAndRotated = axis == Axis.Z && !isLeftOrRightSide;

        return onXAndRotated || onZAndRotated;
    }
    
    private (Axis axis, Int32 turns) GetRotation((Axis axis, Int32 turns) original, State state)
    {
        Axis axis = rotatable.GetCurrentAxis(state);
        Int32 turns = MathTools.Mod(rotatable.GetCurrentTurns(state), m: 4);
        
        return (axis, turns);
    }
}
