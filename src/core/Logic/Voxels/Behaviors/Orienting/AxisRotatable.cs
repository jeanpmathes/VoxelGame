// <copyright file="AxisRotatable.cs" company="VoxelGame">
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
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Actors.Components;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Orienting;

/// <summary>
///     Allows rotation of the block to align with each of the three axes.
/// </summary>
public partial class AxisRotatable : BlockBehavior, IBehavior<AxisRotatable, BlockBehavior, Block>
{
    [Constructible]
    private AxisRotatable(Block subject) : base(subject)
    {
        var rotatable = subject.Require<Rotatable>();
        rotatable.Axis.ContributeFunction(GetAxis);
        rotatable.Turns.ContributeFunction(GetTurns);

        subject.PlacementState.ContributeFunction(GetPlacementState);
    }

    [LateInitialization] private partial IAttributeData<Axis> Axis { get; set; }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        Axis = builder.Define(nameof(Axis)).Enum<Axis>().Attribute();
    }

    private Axis GetAxis(Axis original, State state)
    {
        return state.Get(Axis) switch
        {
            Utilities.Axis.X => Utilities.Axis.Z,
            Utilities.Axis.Y => Utilities.Axis.Y,
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
    ///     Get the current axis in the given state.
    /// </summary>
    /// <param name="state">The state to get the axis in.</param>
    /// <returns>The current axis.</returns>
    public Axis GetAxis(State state)
    {
        return state.Get(Axis);
    }

    /// <summary>
    ///     Set the axis in the given state to a new axis.
    /// </summary>
    /// <param name="state">The state to set the axis in.</param>
    /// <param name="newAxis">The new axis to set.</param>
    /// <returns>The new state with the updated axis.</returns>
    public State SetAxis(State state, Axis newAxis)
    {
        return state.With(Axis, newAxis);
    }
}
