// <copyright file="Quality.cs" company="VoxelGame">
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
///     Quality levels for items and such.
/// </summary>
public enum Quality
{
    /// <summary>
    ///     The lowest quality level and generally harmful instead of useful.
    ///     A corresponding scaling value would be negative.
    /// </summary>
    Terrible,

    /// <summary>
    ///     Deficient, but still usable.
    ///     A corresponding scaling value would be between zero and one.
    /// </summary>
    Deficient,

    /// <summary>
    ///     Normal quality level.
    ///     A corresponding scaling value would be exactly one.
    /// </summary>
    Normal,

    /// <summary>
    ///     Good quality level.
    ///     A corresponding scaling value would be greater than one.
    /// </summary>
    Good,

    /// <summary>
    ///     Excellent quality level.
    ///     A corresponding scaling value would be greater than one.
    /// </summary>
    Excellent,

    /// <summary>
    ///     Phenomenal quality level.
    ///     A corresponding scaling value would be greater than one.
    /// </summary>
    Phenomenal,

    /// <summary>
    ///     The highest normal quality level.
    ///     A corresponding scaling value would be greater than one.
    /// </summary>
    Masterwork,

    /// <summary>
    ///     Essentially impossible quality.
    ///     Used for very special items that go beyond natural possibility.
    ///     Developer items should also be of this quality.
    ///     It should be indicated by a dark color, showing the item in grayscale.
    /// </summary>
    Unreal
}

/// <summary>
///     Utilities for qualities.
/// </summary>
public static class Qualities
{
    /// <summary>
    ///     Get the tier of a quality.
    /// </summary>
    public static Rank ToRank(this Quality quality)
    {
        return quality switch
        {
            Quality.Terrible => Rank.RankZ,
            Quality.Deficient => Rank.Rank0,
            Quality.Normal => Rank.Rank1,
            Quality.Good => Rank.Rank2,
            Quality.Excellent => Rank.Rank3,
            Quality.Phenomenal => Rank.Rank4,
            Quality.Masterwork => Rank.Rank5,
            Quality.Unreal => Rank.RankX,
            _ => throw Exceptions.UnsupportedEnumValue(quality)
        };
    }
}
