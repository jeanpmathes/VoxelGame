// <copyright file="IResourceProvider.cs" company="VoxelGame">
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
///     Provides previously loaded resources in an easily accessible and usable way.
///     These can for example index and cache a set of resources.
/// </summary>
public interface IResourceProvider : ICatalogEntry
{
    /// <summary>
    ///     The loading context. Will be set and unset by this interface.
    /// </summary>
    public IResourceContext? Context { get; protected set; }

    void ICatalogEntry.Enter(IResourceContext context, out IEnumerable<IResource> resources, out IEnumerable<ICatalogEntry> entries)
    {
        Context = context;

        SetUp();

        context.Completed += OnCompleted;

        resources = [];
        entries = [];

        void OnCompleted(Object? sender, EventArgs e)
        {
            Context = null;

            context.Completed -= OnCompleted;
        }
    }

    String ICatalogEntry.Prefix => "Provider";

    String? ICatalogEntry.Instance => null;

    /// <summary>
    ///     Called once during the loading process of the containing catalog.
    /// </summary>
    void SetUp();
}
