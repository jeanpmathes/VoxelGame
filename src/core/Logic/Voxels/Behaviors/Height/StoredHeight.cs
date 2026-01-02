// <copyright file="StoredHeight.cs" company="VoxelGame">
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

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Height;

/// <summary>
///     Provides utilities to work with stored height attributes.
///     Behaviors that store height information can contribute to the aspect defined here.
/// </summary>
public partial class StoredHeight : BlockBehavior, IBehavior<StoredHeight, BlockBehavior, Block>
{
    [Constructible]
    private StoredHeight(Block subject) : base(subject)
    {
        HeightedState = Aspect<State, BlockHeight>.New<Exclusive<State, BlockHeight>>(nameof(HeightedState), this);
    }

    /// <summary>
    ///     Aspect used to retrieve the state for a given height.
    /// </summary>
    public Aspect<State, BlockHeight> HeightedState { get; }

    /// <summary>
    ///     Get the state with the given height applied.
    /// </summary>
    /// <param name="state">The original state.</param>
    /// <param name="height">The desired height.</param>
    /// <returns>The state with the desired height applied.</returns>
    public State SetHeight(State state, BlockHeight height)
    {
        return HeightedState.GetValue(state, height);
    }
}
