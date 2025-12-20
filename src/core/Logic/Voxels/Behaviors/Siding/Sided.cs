// <copyright file="Sided.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Voxels.Behaviors.Orienting;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Siding;

/// <summary>
///     Unifying behavior for all blocks that have one or more main or front sides that depend on the block state.
/// </summary>
public partial class Sided : BlockBehavior, IBehavior<Sided, BlockBehavior, Block>
{
    [Constructible]
    private Sided(Block subject) : base(subject)
    {
        subject.Require<Orientable>();

        Sides = Aspect<Sides, State>.New<Exclusive<Sides, State>>(nameof(Sides), this);
        SidedState = Aspect<State?, (State state, Sides side)>.New<Exclusive<State?, (State state, Sides side)>>(nameof(SidedState), this);
    }

    /// <summary>
    ///     Get the main or front sides of the block in a given state.
    /// </summary>
    public Aspect<Sides, State> Sides { get; }

    /// <summary>
    ///     Get a state set to the given sides, starting from a given other state.
    ///     May be <c>null</c> if the given side is not supported.
    /// </summary>
    public Aspect<State?, (State state, Sides side)> SidedState { get; }

    /// <summary>
    ///     Get the current main or front sides of the block in the given state.
    /// </summary>
    /// <param name="state">The state to get the sides from.</param>
    /// <returns>The main or front sides of the block in the given state.</returns>
    public Sides GetSides(State state)
    {
        return Sides.GetValue(Voxels.Sides.None, state);
    }

    /// <summary>
    ///     Set the main or front sides of the block in the given state.
    /// </summary>
    /// <param name="state">The state to set the sides in.</param>
    /// <param name="sides">The sides to set.</param>
    /// <returns>The state with the updated sides, or <c>null</c> if the sides are not supported.</returns>
    public State? SetSides(State state, Sides sides)
    {
        return SidedState.GetValue(original: null, (state, sides));
    }
}
