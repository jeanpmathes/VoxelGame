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
public partial class ResourceProvider<T> : IResourceProvider where T : class, IResource
{
    private readonly String name;

    private readonly Func<T> createFallback;
    private readonly Func<T, T> copy;

    private Dictionary<RID, T> models = [];


    /// <summary>
    ///     Creates a new group provider.
    /// </summary>
    /// <param name="createFallback">Function to create a fallback resource.</param>
    /// <param name="copy">Function to copy a retrieved resource, may also return the resource itself.</param>
    public ResourceProvider(Func<T> createFallback, Func<T, T> copy)
    {
        name = typeof(T).Name;

        this.createFallback = createFallback;
        this.copy = copy;
    }

    /// <inheritdoc />
    public IResourceContext? Context { get; set; }

    /// <inheritdoc />
    public void SetUp()
    {
        models = Context?.GetAll<T>().ToDictionary(resource => resource.Identifier, resource => resource) ?? [];
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

            return createFallback();
        }

        if (models.TryGetValue(identifier, out T? resource))
            return copy(resource);

        Context.ReportWarning(this, $"{name} resource '{identifier}' not found, using fallback instead");

        return createFallback();
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<ResourceProvider<T>>();

    [LoggerMessage(EventId = LogID.GroupProvider + 0, Level = LogLevel.Warning, Message = "Loading of resources is currently disabled, fallback will be used instead")]
    private static partial void LogLoadingDisabled(ILogger logger);

    #endregion LOGGING
}
