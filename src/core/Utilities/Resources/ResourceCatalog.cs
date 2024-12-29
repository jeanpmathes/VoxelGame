// <copyright file="ResourceCatalog.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
