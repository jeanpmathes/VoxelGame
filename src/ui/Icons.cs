// <copyright file="Icons.cs" company="VoxelGame">
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
using System.Collections.Generic;
using VoxelGame.Core.Utilities;

namespace VoxelGame.UI;

/// <summary>
///     Defines all icons used in the GUI.
/// </summary>
public class Icons(Registry<String> icons, Registry<String> images)
{
    /// <summary>
    ///     The singleton instance of this class.
    /// </summary>
    public static Icons Instance { get; } = new(new Registry<String>(e => e), new Registry<String>(e => e));

    /// <summary>
    ///     Get a sequence of all icon names.
    /// </summary>
    public IEnumerable<String> IconNames => icons.Values;

    /// <summary>
    ///     Get a sequence of all image names.
    /// </summary>
    public IEnumerable<String> ImageNames => images.Values;

    internal String Reset { get; } = icons.Register("reset");
    internal String Load { get; } = icons.Register("load");
    internal String Delete { get; } = icons.Register("delete");
    internal String Warning { get; } = icons.Register("warning");
    internal String Error { get; } = icons.Register("error");
    internal String Info { get; } = icons.Register("info");
    internal String Rename { get; } = icons.Register("rename");
    internal String Search { get; } = icons.Register("search");
    internal String Clear { get; } = icons.Register("clear");
    internal String Duplicate { get; } = icons.Register("duplicate");
    internal String StarFilled { get; } = icons.Register("star_filled");
    internal String StarEmpty { get; } = icons.Register("star_empty");
    internal String Close { get; } = icons.Register("close");
    internal String Check { get; } = icons.Register("check");

    internal String StartImage { get; } = images.Register("start");
}
