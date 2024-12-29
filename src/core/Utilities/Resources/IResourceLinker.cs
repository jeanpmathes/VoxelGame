// <copyright file="IResourceLinker.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
    public void Link(IResourceContext context);
}
