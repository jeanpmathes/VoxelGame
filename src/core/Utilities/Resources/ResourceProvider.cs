// <copyright file="GroupProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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

    /// <summary>
    ///     Get all resources managed by this provider.
    /// </summary>
    protected IReadOnlyDictionary<RID, T> Resources => resources;

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<ResourceProvider<T>>();

    [LoggerMessage(EventId = LogID.GroupProvider + 0, Level = LogLevel.Warning, Message = "Loading of resources is currently disabled, fallback will be used instead")]
    private static partial void LogLoadingDisabled(ILogger logger);

    #endregion LOGGING
}
