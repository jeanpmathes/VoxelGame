// <copyright file="BehaviorExtensions.cs" company="VoxelGame">
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

using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Height;
using VoxelGame.Core.Logic.Voxels.Behaviors.Orienting;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors;

/// <summary>
///     Extensions that simplify working with behaviors.
/// </summary>
public static class BehaviorExtensions
{
    /// <summary>
    ///     Get the state with the given height applied, if the owning block supports stored height.
    /// </summary>
    /// <param name="state">The original state.</param>
    /// <param name="height">The desired height.</param>
    /// <returns>The state with the desired height if supported, otherwise the original state.</returns>
    public static State WithHeight(this State state, BlockHeight height)
    {
        return state.Block.Get<StoredHeight>() is {} storedHeight ? storedHeight.SetHeight(state, height) : state;
    }

    /// <summary>
    ///     Get the state with the given attachment applied, if the owning block supports attachments.
    /// </summary>
    /// <param name="state">The original state.</param>
    /// <param name="attachment">The desired attachment.</param>
    /// <returns>The state with the desired attachment if supported, otherwise the original state.</returns>
    public static State WithAttachment(this State state, Side attachment)
    {
        return state.Block.Get<Attached>() is {} attached ? attached.SetAttachment(state, attachment) : state;
    }

    /// <summary>
    ///     Get the state with the given axis applied, if the owning block supports axis rotation.
    /// </summary>
    /// <param name="state">The original state.</param>
    /// <param name="axis">The desired axis.</param>
    /// <returns>The state with the desired axis if supported, otherwise the original state.</returns>
    public static State WithAxis(this State state, Axis axis)
    {
        return state.Block.Get<AxisRotatable>() is {} rotatable ? rotatable.SetAxis(state, axis) : state;
    }
}
