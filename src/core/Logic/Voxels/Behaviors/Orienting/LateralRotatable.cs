// <copyright file="LateralRotatable.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Voxels.Behaviors.Siding;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Orienting;

/// <summary>
///     Allows a block to be rotated in the four cardinal directions.
/// </summary>
public partial class LateralRotatable : BlockBehavior, IBehavior<LateralRotatable, BlockBehavior, Block>
{
    [Constructible]
    private LateralRotatable(Block subject) : base(subject)
    {
        subject.Require<Rotatable>().Turns.ContributeFunction(GetTurns);

        var siding = subject.Require<SingleSided>();
        siding.Side.ContributeFunction(GetSide);
        siding.SidedState.ContributeFunction(GetSidedState);
    }

    [LateInitialization] private partial IAttributeData<Orientation> Orientation { get; set; }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        Orientation = builder.Define(nameof(Orientation)).Enum<Orientation>()
            .Attribute(Utilities.Orientation.South, Utilities.Orientation.South);
    }

    private Int32 GetTurns(Int32 original, State state)
    {
        Orientation currentOrientation = GetOrientation(state);

        return currentOrientation switch
        {
            Utilities.Orientation.South => 0,
            Utilities.Orientation.West => 1,
            Utilities.Orientation.North => 2,
            Utilities.Orientation.East => 3,
            _ => throw Exceptions.UnsupportedEnumValue(currentOrientation)
        };
    }

    private Side GetSide(Side original, State state)
    {
        return GetOrientation(state).ToSide();
    }

    private State? GetSidedState(State? original, (State state, Side side) context)
    {
        (State state, Side side) = context;

        if (!side.IsLateral())
            return null;

        return SetOrientation(state, side.ToOrientation());
    }

    /// <summary>
    ///     Get the orientation of the block in the given state.
    /// </summary>
    /// <param name="state">The state to query.</param>
    /// <returns>The orientation of the block in the given state.</returns>
    public Orientation GetOrientation(State state)
    {
        return state.Get(Orientation);
    }

    /// <summary>
    ///     Set the orientation of the block in the given state.
    /// </summary>
    /// <param name="state">The state to start from.</param>
    /// <param name="newOrientation">The new orientation.</param>
    /// <returns>A new state with the updated orientation.</returns>
    public State SetOrientation(State state, Orientation newOrientation)
    {
        return state.With(Orientation, newOrientation);
    }
}
