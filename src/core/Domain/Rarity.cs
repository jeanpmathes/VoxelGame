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

using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Domain;

/// <summary>
///     Rarity levels for loot and such.
/// </summary>
public enum Rarity
{
    /// <summary>
    ///     The lowest rarity level, not because of the probability of getting it, but because of its value.
    /// </summary>
    Junk,

    /// <summary>
    ///     Very abundant, but not necessarily worthless.
    /// </summary>
    Abundant,

    /// <summary>
    ///     Common rarity level.
    /// </summary>
    Common,

    /// <summary>
    ///     Uncommon rarity level.
    /// </summary>
    Uncommon,

    /// <summary>
    ///     Rare rarity level.
    /// </summary>
    Rare,

    /// <summary>
    ///     Exceptional rarity level.
    /// </summary>
    Exceptional,

    /// <summary>
    ///     The highest normal rarity level.
    /// </summary>
    Miraculous,

    /// <summary>
    ///     Essentially impossible to get.
    ///     Used for very special items that are tied to specific events or achievements.
    ///     Developer items should also be of this rarity.
    /// </summary>
    Unreal
}

/// <summary>
///     Utilities for rarities.
/// </summary>
public static class Rarities
{
    /// <summary>
    ///     Get the tier of a rarity.
    /// </summary>
    public static Rank ToRank(this Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Junk => Rank.RankZ,
            Rarity.Abundant => Rank.Rank0,
            Rarity.Common => Rank.Rank1,
            Rarity.Uncommon => Rank.Rank2,
            Rarity.Rare => Rank.Rank3,
            Rarity.Exceptional => Rank.Rank4,
            Rarity.Miraculous => Rank.Rank5,
            Rarity.Unreal => Rank.RankX,
            _ => throw Exceptions.UnsupportedEnumValue(rarity)
        };
    }
}
