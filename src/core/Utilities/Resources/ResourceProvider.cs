// <copyright file="GroupProvider.cs" company="VoxelGame">
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
using Microsoft.Extensions.Logging;
using VoxelGame.Logging;

namespace VoxelGame.Core.Utilities.Resources;

/// <summary>
///     Collects all loaded resources of a specific type and provides them in an easily accessible way.
/// </summary>
/// <typeparam name="T">The type of the resources to group.</typeparam>
public abstract partial class ResourceProvider<T> : IResourceProvider where T : class, IResource
{
    private readonly String name;

    private Dictionary<RID, T> resources = [];

    /// <summary>
    ///     Creates a new group provider.
    /// </summary>
    protected ResourceProvider()
    {
        name = typeof(T).Name;
    }

    /// <summary>
    ///     Get all resources managed by this provider.
    /// </summary>
    protected IReadOnlyDictionary<RID, T> Resources => resources;

    /// <inheritdoc />
    public IResourceContext? Context { get; set; }

    /// <inheritdoc />
    public void SetUp()
    {
        resources = Context?.GetAll<T>().ToDictionary(resource => resource.Identifier, resource => resource) ?? [];

        if (Context != null)
            OnSetUp(Context);
    }

    /// <summary>
    ///     Override to get requirements from the resource context.
    /// </summary>
    /// <param name="context">The resource context to get requirements from.</param>
    protected virtual void OnSetUp(IResourceContext context) {}

    /// <summary>
    ///     Override to implement fallback resource creation.
    ///     Will be called for each fallback creation and not cached.
    /// </summary>
    /// <returns>The fallback resource.</returns>
    protected abstract T CreateFallback();

    /// <summary>
    ///     Override to implement optional resource copying.
    ///     Will be called with each successful resource retrieval.
    /// </summary>
    /// <param name="resource">The retrieved resource.</param>
    /// <returns>A copy, or the resource itself if no copying is required.</returns>
    protected virtual T Copy(T resource)
    {
        return resource;
    }

    /// <summary>
    ///     Gets the resource for the given identifier, or a fallback resource if the resource is not found.
    /// </summary>
    /// <param name="identifier">The identifier of the resource to get.</param>
    /// <returns>The resource for the given identifier, or a fallback resource if the resource is not found.</returns>
    protected T GetResource(RID identifier)
    {
        if (Context == null)
        {
            LogLoadingDisabled(logger);

            return CreateFallback();
        }

        if (resources.TryGetValue(identifier, out T? resource))
            return Copy(resource);

        Context.ReportWarning(this, $"{name} resource '{identifier}' not found, using fallback instead");

        return CreateFallback();
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<ResourceProvider<T>>();

    [LoggerMessage(EventId = LogID.GroupProvider + 0, Level = LogLevel.Warning, Message = "Loading of resources is currently disabled, fallback will be used instead")]
    private static partial void LogLoadingDisabled(ILogger logger);

    #endregion LOGGING
}
