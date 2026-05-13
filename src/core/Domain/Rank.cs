// <copyright file="Rank.cs" company="VoxelGame">
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
///     A rank, the common denominator of both <see cref="Quality" /> and <see cref="Rarity" />.
/// </summary>
public enum Rank
{
    /// <summary>
    ///     The lowest rank, with special semantics.
    ///     It should be indicated by a dull brown color.
    /// </summary>
    RankZ,

    /// <summary>
    ///     A very low rank.
    ///     It should be indicated by a gray color.
    /// </summary>
    Rank0,

    /// <summary>
    ///     The ordinary baseline rank.
    ///     It should be indicated by a white color.
    /// </summary>
    Rank1,

    /// <summary>
    ///     A slightly elevated rank.
    ///     It should be indicated by a green color.
    /// </summary>
    Rank2,

    /// <summary>
    ///     A high rank.
    ///     It should be indicated by a blue color.
    /// </summary>
    Rank3,

    /// <summary>
    ///     An exceptional rank.
    ///     It should be indicated by a purple color.
    /// </summary>
    Rank4,

    /// <summary>
    ///     The highest normal rank.
    ///     It should be indicated by a yellow color.
    /// </summary>
    Rank5,

    /// <summary>
    ///     An essentially impossible rank.
    ///     It should be indicated by a dark color, showing the item in grayscale.
    /// </summary>
    RankX
}
