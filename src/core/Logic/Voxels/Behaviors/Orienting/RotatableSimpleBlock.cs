// <copyright file="RotatableSimpleBlock.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
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

        Axis axis = rotatable.GetAxis(state);
        Int32 turns = MathTools.Mod(rotatable.GetTurns(state), m: 4);

        if (turns == 0 || turns == 2 || axis == Axis.Y) return false;

        Boolean isLeftOrRightSide = side is Side.Left or Side.Right;
        Boolean onXAndRotated = axis == Axis.X && isLeftOrRightSide;
        Boolean onZAndRotated = axis == Axis.Z && !isLeftOrRightSide;

        return onXAndRotated || onZAndRotated;
    }

    private (Axis axis, Int32 turns) GetRotation((Axis axis, Int32 turns) original, State state)
    {
        Axis axis = rotatable.GetAxis(state);
        Int32 turns = MathTools.Mod(rotatable.GetTurns(state), m: 4);

        return (axis, turns);
    }
}
