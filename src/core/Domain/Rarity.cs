// <copyright file="Rarity.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Domain;

/// <summary>
///     Rarity levels for items, loot, etc.
/// </summary>
public enum Rarity
{
    /// <summary>
    ///     The lowest rarity level, not because of the probability of getting it, but because of its value.
    ///     It should be indicated by a grey color.
    /// </summary>
    Junk,

    /// <summary>
    ///     The most common rarity level.
    ///     It should be indicated by a white color.
    /// </summary>
    Common,

    /// <summary>
    ///     Uncommon rarity level.
    ///     It should be indicated by a green color.
    /// </summary>
    Uncommon,

    /// <summary>
    ///     Rare rarity level.
    ///     It should be indicated by a blue color.
    /// </summary>
    Rare,

    /// <summary>
    ///     Exceptional rarity level.
    ///     It should be indicated by a purple color.
    /// </summary>
    Exceptional,

    /// <summary>
    ///     The highest normal rarity level.
    ///     It should be indicated by a yellow color.
    /// </summary>
    Miraculous,

    /// <summary>
    ///     Essentially impossible to get.
    ///     Used for very special items that are tied to specific events or achievements.
    ///     Developer items should also be of this rarity.
    ///     It should be indicated by a dark color, showing the item in grayscale.
    /// </summary>
    Unreal
}
