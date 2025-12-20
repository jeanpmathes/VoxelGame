// <copyright file="SingleSided.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Siding;

/// <summary>
///     Unifying behavior for <see cref="Sided" /> blocks that have only one main or front side.
/// </summary>
public partial class SingleSided : BlockBehavior, IBehavior<SingleSided, BlockBehavior, Block>
{
    [Constructible]
    private SingleSided(Block subject) : base(subject)
    {
        var sided = subject.Require<Sided>();
        sided.Sides.ContributeFunction(GetSides);
        sided.SidedState.ContributeFunction(GetSidedState);

        Side = Aspect<Side, State>.New<Exclusive<Side, State>>(nameof(Side), this);
        SidedState = Aspect<State?, (State state, Side side)>.New<Exclusive<State?, (State state, Side side)>>(nameof(SidedState), this);
    }

    /// <summary>
    ///     Get the main or front side of the block in a given state.
    /// </summary>
    public Aspect<Side, State> Side { get; }

    /// <summary>
    ///     Get a state set to the given side, starting from a given other state.
    ///     May be <c>null</c> if the given side is not supported.
    /// </summary>
    public Aspect<State?, (State state, Side side)> SidedState { get; }

    private Sides GetSides(Sides original, State state)
    {
        return GetSide(state).ToFlag();
    }

    private State? GetSidedState(State? original, (State state, Sides sides) context)
    {
        (State state, Sides sides) = context;

        return sides.Count() == 1 ? SetSide(state, sides.Single()) : null;
    }

    /// <summary>
    ///     Get the current main or front side of the block in the given state.
    /// </summary>
    /// <param name="state">The state to get the side from.</param>
    /// <returns>The main or front side of the block in the given state.</returns>
    public Side GetSide(State state)
    {
        return Side.GetValue(Voxels.Side.Front, state);
    }

    /// <summary>
    ///     Set the main or front side of the block in the given state.
    /// </summary>
    /// <param name="state">The state to set the side in.</param>
    /// <param name="side">The side to set.</param>
    /// <returns>The state with the updated side, or <c>null</c> if the side is not supported.</returns>
    public State? SetSide(State state, Side? side)
    {
        return side == null ? null : SidedState.GetValue(state, (state, side.Value));
    }
}
