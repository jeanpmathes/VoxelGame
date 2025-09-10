// <copyright file="IResourceContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;

namespace VoxelGame.Core.Utilities.Resources;

/// <summary>
///     The context in which resources are loaded.
/// </summary>
public interface IResourceContext : IDisposable
{
    /// <summary>
    ///     Require any resource, catalog entry or external object to proceed.
    /// </summary>
    /// <param name="func">The func that requires the object.</param>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <returns>The resources loaded by <paramref name="func" /> or an empty enumerable if the requirement is not met.</returns>
    public IEnumerable<IResource> Require<T>(Func<T, IEnumerable<IResource>> func) where T : class;

    /// <summary>
    ///     Require a resource to proceed.
    /// </summary>
    /// <param name="identifier">The identifier of the required resource.</param>
    /// <param name="func">The func that requires the resource.</param>
    /// <typeparam name="T">The type of the resource.</typeparam>
    /// <returns>The resources loaded by <paramref name="func" /> or an empty enumerable if the requirement is not met.</returns>
    public IEnumerable<IResource> Require<T>(RID identifier, Func<T, IEnumerable<IResource>> func) where T : class;

    /// <summary>
    ///     Get a resource or any other catalog entry if it is loaded.
    /// </summary>
    /// <param name="identifier">An optional identifier of the resource.</param>
    /// <typeparam name="T">The type of the resource or catalog entry.</typeparam>
    /// <returns>The resource or catalog if it is loaded, otherwise <c>null</c>.</returns>
    public T? Get<T>(RID? identifier = null) where T : class;

    /// <summary>
    ///     Get all objects of a certain type.
    /// </summary>
    /// <typeparam name="T">The type of the objects.</typeparam>
    /// <returns>The objects of the specified type.</returns>
    public IEnumerable<T> GetAll<T>() where T : class;

    /// <summary>
    ///     Report a warning for the loading of the current resource.
    /// </summary>
    /// <param name="source">The source of the warning.</param>
    /// <param name="message">The warning message.</param>
    /// <param name="exception">An optional exception that caused the warning.</param>
    /// <param name="path">An optional path associated with the warning.</param>
    public void ReportWarning(IIssueSource source, String message, Exception? exception = null, FileSystemInfo? path = null);
    
    /// <summary>
    /// Report an error for the loading of the current resource.
    /// Using an error resource is generally preferred to using this method.
    /// </summary>
    /// <param name="source">The source of the error.</param>
    /// <param name="message">The error message.</param>
    /// <param name="exception">An optional exception that caused the error.</param>
    /// <param name="path">An optional path associated with the error.</param>
    public void ReportError(IIssueSource source, String message, Exception? exception = null, FileSystemInfo? path = null);

    /// <summary>
    ///     Report the discovery (and potential load) of a sub-resource when loading the current resource.
    ///     By providing either <paramref name="error" /> or <paramref name="errorMessage" />, the sub-resource load is
    ///     considered failed.
    ///     A sub-resource failure will always be reported as a warning.
    /// </summary>
    /// <param name="type">The type of the sub-resource.</param>
    /// <param name="identifier">The identifier of the sub-resource.</param>
    /// <param name="error">
    ///     Any exception that might have occurred with the sub-resource.
    /// </param>
    /// <param name="errorMessage">
    ///     An optional error message describing a failure to load the sub-resource.
    /// </param>
    public void ReportDiscovery(ResourceType type, RID identifier, Exception? error = null, String? errorMessage = null);

    /// <summary>
    ///     Invoked when the loading process is completed.
    /// </summary>
    public event EventHandler Completed;
}
