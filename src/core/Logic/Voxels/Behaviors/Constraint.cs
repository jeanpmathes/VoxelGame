// <copyright file="Constraint.cs" company="VoxelGame">
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

using System;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Voxels.Behaviors;

/// <summary>
///     A special behavior that allows marking certain states as invalid, thus constraining the set of valid states.
///     Note aspects cannot generally assume that provided states are valid, so they must not fail on invalid states.
///     The main benefit of this behavior is that nice placeholder meshes and collisions are used for the invalid states.
///     This behavior can be seen as a remedy for lacking functionality in the state definition system.
///     But addressing those shortcomings would drastically increase the complexity of that system and would only be rarely
///     used.
/// </summary>
public partial class Constraint : BlockBehavior, IBehavior<Constraint, BlockBehavior, Block>
{
    [Constructible]
    private Constraint(Block subject) : base(subject)
    {
        IsValid = Aspect<Boolean, State>.New<Exclusive<Boolean, State>>(nameof(IsValid), this);
    }

    /// <summary>
    ///     Whether a certain state is valid.
    /// </summary>
    public Aspect<Boolean, State> IsValid { get; }

    /// <summary>
    ///     Check whether a given state is valid.
    ///     A state is invalid if there is no way to place the block in that state or have it change into that state through
    ///     behavior.
    /// </summary>
    /// <param name="state">The state to check.</param>
    /// <returns><c>true</c> if the state is valid, <c>false</c> otherwise.</returns>
    public static Boolean IsStateValid(State state)
    {
        return state.Block.Get<Constraint>()?.IsValid.GetValue(original: true, state) ?? true;
    }
}
