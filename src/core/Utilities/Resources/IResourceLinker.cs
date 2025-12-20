// <copyright file="IResourceLinker.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Utilities.Resources;

/// <summary>
///     A step in the loading process that links together previously loaded resources.
/// </summary>
public interface IResourceLinker : ICatalogEntry
{
    void ICatalogEntry.Enter(IResourceContext context, out IEnumerable<IResource> resources, out IEnumerable<ICatalogEntry> entries)
    {
        Link(context);

        resources = [];
        entries = [];
    }

    String ICatalogEntry.Prefix => "Linker";

    String? ICatalogEntry.Instance => null;

    /// <summary>
    ///     Links previously loaded resources together.
    /// </summary>
    /// <param name="context">The context in which the resources are loaded.</param>
    void Link(IResourceContext context);
}
