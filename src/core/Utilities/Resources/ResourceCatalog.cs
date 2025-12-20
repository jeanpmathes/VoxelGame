// <copyright file="ResourceCatalog.cs" company="VoxelGame">
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
using System.Linq;

namespace VoxelGame.Core.Utilities.Resources;

/// <summary>
///     The catalog contains resources to be loaded and defines how they are loaded.
/// </summary>
public class ResourceCatalog : ICatalogEntry
{
    private readonly List<ICatalogEntry> catalogEntries;

    /// <summary>
    ///     Create a new resource catalog with the given name and entries.
    /// </summary>
    /// <param name="entries">The entries in the catalog.</param>
    protected ResourceCatalog(IEnumerable<ICatalogEntry> entries)
    {
        catalogEntries = entries.ToList();
    }

    void ICatalogEntry.Enter(IResourceContext context, out IEnumerable<IResource> resources, out IEnumerable<ICatalogEntry> entries)
    {
        resources = [];
        entries = catalogEntries;
    }

    /// <inheritdoc />
    public String Prefix => "Catalog";

    /// <inheritdoc />
    public String? Instance => null;
}
