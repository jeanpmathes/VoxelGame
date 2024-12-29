// <copyright file="IResourceLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;

namespace VoxelGame.Core.Utilities.Resources;

/// <summary>
/// Can load resources. Allows to specify resource properties without creating the resource.
/// </summary>
public interface IResourceLoader : ICatalogEntry
{
    void ICatalogEntry.Enter(IResourceContext context, out IEnumerable<IResource> resources, out IEnumerable<ICatalogEntry> entries)
    {
        resources = Load(context);
        entries = [];
    }

    String ICatalogEntry.Prefix => "Loader";

    /// <summary>
    /// Loads the resources.
    /// </summary>
    /// <param name="context">The context in which the resources are loaded.</param>
    /// <returns>The loaded resources.</returns>
    IEnumerable<IResource> Load(IResourceContext context);
}
