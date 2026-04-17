// <copyright file="Theme.cs" company="VoxelGame">
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

using System.Collections.Generic;
using VoxelGame.GUI.Controls.Templates;
using VoxelGame.GUI.Styles;

namespace VoxelGame.GUI.Themes;

/// <summary>
///     A theme combines styles and content templates.
/// </summary>
public class Theme(List<Style> styles, List<ContentTemplate> contentTemplates)
{
    /// <summary>
    ///     Create a new empty <see cref="Theme" />.
    /// </summary>
    public Theme() : this([], []) {}

    /// <summary>
    ///     Get all styles that are part of this theme.
    /// </summary>
    public IReadOnlyList<Style> Styles => styles;

    /// <summary>
    ///     Get all content templates that are part of this theme.
    /// </summary>
    public IReadOnlyList<ContentTemplate> ContentTemplates => contentTemplates;
}
