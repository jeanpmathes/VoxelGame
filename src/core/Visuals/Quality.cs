// <copyright file="Quality.cs" company="VoxelGame">
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
using System.Collections.Generic;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Different quality levels to select resources and routines.
/// </summary>
public enum Quality
{
    /// <summary>
    ///     Low quality. Performance is the most important.
    /// </summary>
    Low,

    /// <summary>
    ///     Medium quality. Performance and visual quality are equally important.
    /// </summary>
    Medium,

    /// <summary>
    ///     High quality. Visual quality is prioritized.
    /// </summary>
    High,

    /// <summary>
    ///     Ultra quality. Visual quality is the most important.
    /// </summary>
    Ultra
}

/// <summary>
///     Utility class for quality.
/// </summary>
public static class Qualities
{
    /// <summary>
    ///     The number of quality levels.
    /// </summary>
    public const Int32 Count = 4;

    /// <summary>
    ///     Get all quality levels.
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<Quality> All()
    {
        yield return Quality.Low;
        yield return Quality.Medium;
        yield return Quality.High;
        yield return Quality.Ultra;
    }

    /// <summary>
    ///     The name of the quality as string.
    /// </summary>
    public static String Name(this Quality quality)
    {
        return quality.ToStringFast();
    }
}
