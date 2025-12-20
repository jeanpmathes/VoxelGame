// <copyright file="StoredMultiSided.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Siding;

/// <summary>
///     Behavior for <see cref="Sided" /> blocks that can have multiple main or front sides at once, and store them in the
///     block state.
/// </summary>
public partial class StoredMultiSided : BlockBehavior, IBehavior<StoredMultiSided, BlockBehavior, Block>
{
    [Constructible]
    private StoredMultiSided(Block subject) : base(subject)
    {
        var sided = subject.Require<Sided>();
        sided.Sides.ContributeFunction(GetSides);
        sided.SidedState.ContributeFunction(GetSidedState);
    }

    [LateInitialization] private partial IAttributeData<Sides> Sides { get; set; }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        Sides = builder.Define(nameof(Sides)).Flags<Sides>().Attribute();
    }

    private Sides GetSides(Sides original, State state)
    {
        return GetSides(state);
    }

    private State? GetSidedState(State? original, (State state, Sides sides) context)
    {
        (State state, Sides newSides) = context;

        return SetSides(state, newSides);
    }

    /// <summary>
    ///     Get the current sides of the block in the given state.
    /// </summary>
    /// <param name="state">The state to get the sides from.</param>
    /// <returns>The sides of the block in the given state.</returns>
    public Sides GetSides(State state)
    {
        return state.Get(Sides);
    }

    /// <summary>
    ///     Set the sides of the block in the given state.
    /// </summary>
    /// <param name="state">The state to set the sides in.</param>
    /// <param name="newSides">The sides to set.</param>
    /// <returns>The state with the updated sides.</returns>
    public State SetSides(State state, Sides newSides)
    {
        return state.With(Sides, newSides);
    }
}
