// <copyright file="IResourceProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
