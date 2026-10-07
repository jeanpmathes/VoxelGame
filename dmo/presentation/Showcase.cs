// <copyright file="Showcase.cs" company="VoxelGame">
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
using VoxelGame.GUI.Controls.Templates;

namespace VoxelGame.Presentation.Demo;

/// <summary>
///     Define a new showcase by subclassing this class.
///     The subclass of this class will be the view model, use <see cref="GetTemplate" /> to define the view.
/// </summary>
public abstract class Showcase
{
    /// <summary>
    ///     The name of the showcase.
    /// </summary>
    public abstract String Name { get; }

    /// <summary>
    ///     Get the template of the showcase which defines the view.
    /// </summary>
    /// <returns>The content template of the showcase.</returns>
    public abstract ContentTemplate GetTemplate();
}
