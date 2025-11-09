// <copyright file="ICatalogEntry.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Utilities.Resources;

/// <summary>
///     An entry in a resource catalog.
///     All entries will be entered in the order they are added to the catalog.
///     When entered, they can load resources, provide further catalog entries or do other things.
///     As soon as all resources and entries of a catalog are processed, the catalog is exited.
/// </summary>
public interface ICatalogEntry : IIssueSource
{
    /// <summary>
    ///     Get the name of the catalog entry.
    /// </summary>
    public sealed String Name => Reflections.GetDecoratedName(Prefix, GetType(), Instance);

    /// <summary>
    ///     The prefix of the catalog entry. Will be used to create the full name of the entry.
    /// </summary>
    protected String Prefix { get; }

    /// <summary>
    ///     The optional instance name of the catalog entry. Will be used to create the full name of the entry if not null.
    /// </summary>
    protected String? Instance { get; }

    /// <inheritdoc />
    String? IIssueSource.InstanceName => Instance;

    /// <summary>
    ///     Enter this catalog entry. This allows the entry to load resources
    /// </summary>
    /// <param name="context">The context in which the resources are loaded.</param>
    /// <param name="resources">The resources loaded by this entry.</param>
    /// <param name="entries">The catalog entries provided by this entry.</param>
    public void Enter(IResourceContext context, out IEnumerable<IResource> resources, out IEnumerable<ICatalogEntry> entries);
}
