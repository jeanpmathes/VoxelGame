// <copyright file="RotatableSimpleBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Meshables;
using VoxelGame.Core.Logic.Elements.Behaviors.Visuals;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Orienting;

/// <summary>
///     Glue behavior for blocks that are both <see cref="Rotatable" /> and meshed using <see cref="Simple" />.
/// </summary>
public class RotatableSimpleBlock : BlockBehavior, IBehavior<RotatableSimpleBlock, BlockBehavior, Block>
{
    private readonly Rotatable rotatable;

    private RotatableSimpleBlock(Block subject) : base(subject)
    {
        rotatable = subject.Require<Rotatable>();

        subject.Require<CubeTextured>().ActiveTexture.ContributeFunction(GetActiveTexture);
        subject.Require<Simple>().IsTextureRotated.ContributeFunction(GetIsTextureRotated);
    }


    /// <inheritdoc />
    public static RotatableSimpleBlock Construct(Block input)
    {
        return new RotatableSimpleBlock(input);
    }

    // todo: use conditionals to rotate texture 
    // Boolean isLeftOrRightSide = info.Side is Side.Left or Side.Right;
    // Boolean onXAndRotated = axis == Axis.X && !isLeftOrRightSide;
    // Boolean onZAndRotated = axis == Axis.Z && isLeftOrRightSide;
    // Boolean rotated = onXAndRotated || onZAndRotated;

    private Boolean GetIsTextureRotated(Boolean original, (State state, Side side) context)
    {
        (State state, Side side) = context;

        Axis axis = rotatable.GetCurrentAxis(state);
        Int32 turns = MathTools.Mod(rotatable.GetCurrentTurns(state), m: 4);

        if (turns == 0 || turns == 2 || axis == Axis.Y) return false;

        Boolean isLeftOrRightSide = side is Side.Left or Side.Right;
        Boolean onXAndRotated = axis == Axis.X && !isLeftOrRightSide;
        Boolean onZAndRotated = axis == Axis.Z && isLeftOrRightSide;

        return onXAndRotated || onZAndRotated;
    }

    private TextureLayout GetActiveTexture(TextureLayout original, State state)
    {
        Axis axis = rotatable.GetCurrentAxis(state);
        Int32 turns = MathTools.Mod(rotatable.GetCurrentTurns(state), m: 4);

        return original.Rotated(axis, turns);
    }
}
