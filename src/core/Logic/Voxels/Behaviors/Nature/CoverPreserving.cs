// <copyright file="CoverPreserving.cs" company="VoxelGame">
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
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature;

/// <summary>
///     Marker behavior for blocks that can sit on covered soil without removing the cover.
/// </summary>
public partial class CoverPreserving : BlockBehavior, IBehavior<CoverPreserving, BlockBehavior, Block>
{
    [Constructible]
    private CoverPreserving(Block subject) : base(subject)
    {
        Preservation = Aspect<Boolean, State>.New<LogicalAnd<State>>(nameof(Preservation), this);
    }

    /// <summary>
    ///     Aspect that determines whether the block preserves the cover for a given state.
    /// </summary>
    public Aspect<Boolean, State> Preservation { get; }

    /// <summary>
    ///     Check whether the block preserves cover for the supplied state.
    /// </summary>
    /// <param name="state">The state to evaluate.</param>
    /// <returns><c>true</c> if the cover should remain, otherwise <c>false</c>.</returns>
    public Boolean IsPreserving(State state)
    {
        return Preservation.GetValue(original: true, state);
    }
}
