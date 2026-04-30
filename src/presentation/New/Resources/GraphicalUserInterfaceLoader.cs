// <copyright file="GraphicalUserInterfaceLoader.cs" company="VoxelGame">
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
using System.Linq;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Graphics.Core;
using VoxelGame.GUI.Controls.Templates;
using VoxelGame.GUI.Styles;
using VoxelGame.GUI.Themes;
using VoxelGame.Presentation.New.Platform;

namespace VoxelGame.Presentation.New.Resources;

/// <summary>
///     Loads a <see cref="GraphicalUserInterface" />.
/// </summary>
public sealed class GraphicalUserInterfaceLoader : IResourceLoader
{
    String? ICatalogEntry.Instance => null;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context)
    {
        return context.Require<Client>(client =>
        {
            Theme theme = new("Base", context.GetAll<Style>().ToList(), context.GetAll<ContentTemplate>().ToList());

            GraphicalUserInterface gui = GraphicalUserInterface.Create(client, theme);

            return [gui];
        });
    }
}
