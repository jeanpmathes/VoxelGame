// <copyright file="ThemeBuilder.cs" company="VoxelGame">
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
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Controls.Templates;
using VoxelGame.GUI.Styles;

namespace VoxelGame.GUI.Themes;

/// <summary>
///     Utility to help with the creation of <see cref="Theme" />s.
/// </summary>
public class ThemeBuilder
{
    private readonly List<Style> styles = [];
    private readonly List<ContentTemplate> contentTemplates = [];

    /// <summary>
    ///     Create a style for a specific element type and register it in the registry.
    /// </summary>
    /// <param name="name">The name of the style to add.</param>
    /// <param name="builder">The builder for the style.</param>
    /// <typeparam name="TControl">The element type this style is for.</typeparam>
    /// <returns>A new instance of the <see cref="Style{TElement}" /> class.</returns>
    public Style<TControl> AddStyle<TControl>(String name, Action<Styling.IBuilder<TControl>> builder) where TControl : IControl
    {
        Style<TControl> style = Styling.Create(name, builder);

        styles.Add(style);

        return style;
    }

    /// <summary>
    ///     Add a content template to the registry.
    /// </summary>
    /// <param name="name">The name of the content template to add.</param>
    /// <param name="function">The function that creates the control structure for the content.</param>
    /// <typeparam name="TContent">The type of the content.</typeparam>
    /// <returns>The created content template.</returns>
    public ContentTemplate<TContent> AddContentTemplate<TContent>(String name, Func<TContent, Control> function) where TContent : class
    {
        ContentTemplate<TContent> template = ContentTemplate.Create(name, function);

        contentTemplates.Add(template);

        return template;
    }

    /// <summary>
    ///     Create a theme containing all styles and content templates currently in this builder.
    /// </summary>
    /// <returns>The created theme.</returns>
    public Theme BuildTheme()
    {
        return new Theme([..styles], [..contentTemplates]);
    }
}
