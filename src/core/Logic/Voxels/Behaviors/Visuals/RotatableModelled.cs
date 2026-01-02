// <copyright file="RotatableModelled.cs" company="VoxelGame">
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
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Orienting;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     Rotates the models used by a <see cref="Modelled" /> block and changes the selection to match the rotation.
/// </summary>
public class RotatableModelled : BlockBehavior, IBehavior<RotatableModelled, BlockBehavior, Block>
{
    private readonly Rotatable rotatable;

    private RotatableModelled(Block subject) : base(subject)
    {
        RotationOverride = Aspect<(Axis axis, Int32 turns), State>
            .New<Exclusive<(Axis axis, Int32 turns), State>>(nameof(RotationOverride), this);

        rotatable = subject.Require<Rotatable>();

        subject.Require<Modelled>().Model.ContributeFunction(GetModel);
    }

    /// <summary>
    ///     Used to override the rotation as provided by the <see cref="Rotatable" /> behavior.
    /// </summary>
    public Aspect<(Axis axis, Int32 turns), State> RotationOverride { get; }

    /// <inheritdoc />
    public static RotatableModelled Construct(Block input)
    {
        return new RotatableModelled(input);
    }

    private Model GetModel(Model original, State state)
    {
        Axis axis = rotatable.GetAxis(state);
        Int32 turns = rotatable.GetTurns(state);

        (axis, turns) = RotationOverride.GetValue((axis, turns), state);

        return original.CreateModelForRotation(axis, turns);
    }
}
