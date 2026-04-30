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

using System;
using System.Collections.Generic;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.GUI.Controls.Templates;
using VoxelGame.GUI.Styles;

namespace VoxelGame.GUI.Themes;

/// <summary>
///     A theme combines styles and content templates.
/// </summary>
public sealed class Theme(RID id, List<Style> styles, List<ContentTemplate> contentTemplates) : IResource
{
    /// <summary>
    ///     Create a new empty <see cref="Theme" />.
    /// </summary>
    public Theme() : this([], []) {}

    /// <summary>
    ///     Create a new virtual <see cref="Theme" />.
    ///     This should be used only outside the resource loading phase.
    /// </summary>
    public Theme(List<Style> styles, List<ContentTemplate> contentTemplates) : this(RID.Virtual, styles, contentTemplates) {}

    /// <summary>
    ///     Create a new named <see cref="Theme" />.
    ///     Use this during the resource loading phase.
    /// </summary>
    public Theme(String name, List<Style> styles, List<ContentTemplate> contentTemplates) : this(RID.Named<Theme>(name), styles, contentTemplates) {}

    /// <summary>
    ///     Get all styles that are part of this theme.
    /// </summary>
    public IReadOnlyList<Style> Styles => styles;

    /// <summary>
    ///     Get all content templates that are part of this theme.
    /// </summary>
    public IReadOnlyList<ContentTemplate> ContentTemplates => contentTemplates;

    /// <inheritdoc />
    public RID Identifier { get; } = id;

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.Theme;

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose of, as styles and content templates are owned by the resource context.
    }
}
