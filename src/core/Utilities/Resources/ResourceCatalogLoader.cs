// <copyright file="ResourceCatalogLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Profiling;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Utilities.Resources;

/// <summary>
///     A report detailing all issues that occurred during resource loading.
/// </summary>
/// <param name="Report">The report as properties.</param>
/// <param name="ErrorCount">The number of errors that occurred.</param>
/// <param name="WarningCount">The number of warnings that occurred.</param>
public record ResourceLoadingIssueReport(Property Report, Int32 ErrorCount, Int32 WarningCount)
{
    /// <summary>
    ///     Whether there are any errors in the report.
    ///     If yes, the game is not playable.
    /// </summary>
    public Boolean AnyErrors => ErrorCount > 0;

    /// <summary>
    ///     Merge multiple reports into one.
    /// </summary>
    /// <param name="name">The name of the merged report.</param>
    /// <param name="reports">The reports to merge.</param>
    /// <returns>The merged report or <c>null</c> if no reports are given.</returns>
    public static ResourceLoadingIssueReport? Merge(String name, params ResourceLoadingIssueReport?[] reports)
    {
        IList<ResourceLoadingIssueReport> validReports = reports.WhereNotNull().ToList();

        if (validReports.Count == 0)
            return null;

        if (validReports.Count == 1)
            return validReports[index: 0];

        Group report = new(name);

        var errorCount = 0;
        var warningCount = 0;

        foreach (ResourceLoadingIssueReport validReport in validReports)
        {
            report.Add(validReport.Report);

            errorCount += validReport.ErrorCount;
            warningCount += validReport.WarningCount;
        }

        return new ResourceLoadingIssueReport(report, errorCount, warningCount);
    }
}

/// <summary>
///     Loads resource catalogs.
/// </summary>
public sealed partial class ResourceCatalogLoader
{
    private readonly TypeKeyDictionary<RID> environment = new();

    /// <summary>
    ///     Add an object to the loading environment.
    ///     It will be available to all resources during the loading process.
    /// </summary>
    /// <param name="obj">The object to add.</param>
    public void AddToEnvironment(Object obj)
    {
        environment.Add(obj, key: null);
    }

    /// <summary>
    ///     Add the resources of another resource context to the loading environment.
    ///     Other objects that are not resources will be ignored.
    /// </summary>
    /// <param name="context">The context of which to add the resources.</param>
    public void AddToEnvironment(IResourceContext context)
    {
        // Generally, all non-keyed objects have been removed from the context anyway.

        foreach (IResource resource in context.GetAll<IResource>()) environment.Add(resource, resource.Identifier);
    }

    /// <summary>
    ///     Load the specified catalog.
    /// </summary>
    /// <param name="catalog">The catalog to load.</param>
    /// <param name="timer">A timer to use for profiling the loading operations.</param>
    /// <returns>The resource context containing all loaded resources and an optional error report.</returns>
    public (IResourceContext context, ResourceLoadingIssueReport? report) Load(ICatalogEntry catalog, Timer? timer)
    {
        Context context = new(environment);

        Group? report = LoadCatalogEntry(catalog, hierarchy: null, report: null, timer, context);
        Debug.Assert(report != null);

        context.OnComplete();

        return (context, context.BuildReport(report));
    }

    private static Group LoadCatalogEntry(ICatalogEntry entry, String? hierarchy, Group? report, Timer? timer, Context context)
    {
        LogStartingLoadingEntry(logger, entry.Name);

        String currentHierarchy = hierarchy != null ? $"{hierarchy} > {entry.Name}" : entry.Name;
        Group currentReport = new(entry.Name);
        Timer? currentTimer = logger.BeginTimedSubScoped(entry.Name, timer);

        entry.Enter(context, out IEnumerable<IResource> resources, out IEnumerable<ICatalogEntry> entries);

        LoadResources(resources, currentHierarchy, currentReport, context);
        LoadCatalogEntries(entries, currentHierarchy, currentReport, currentTimer, context);

        context.AddCatalogEntry(entry);

        currentTimer?.Dispose();
        report?.Add(currentReport);

        LogFinishedLoadingEntry(logger, entry.Name);

        return currentReport;
    }

    private static void LoadCatalogEntries(IEnumerable<ICatalogEntry> entries, String hierarchy, Group report, Timer? timer, Context context)
    {
        context.SetCurrentState(hierarchy, report);

        foreach (ICatalogEntry entry in entries) LoadCatalogEntry(entry, hierarchy, report, timer, context);
    }

    private static void LoadResources(IEnumerable<IResource> resources, String hierarchy, Group report, Context context)
    {
        context.SetCurrentState(hierarchy, report);

        foreach (IResource resource in resources)
        {
            ResourceIssue? error = resource.Issue;

            if (error == null)
            {
                context.AddResource(resource);

                LogLoadedResource(logger, hierarchy, resource.Type, resource.Identifier);
            }
            else
            {
                context.ReportIssue(resource, error);
            }
        }
    }

    private sealed class Context : IResourceContext
    {
        private readonly TypeKeyDictionary<RID> content = new();

        private String? currentHierarchy;
        private Group? currentReport;

        private Int32 errorCount;
        private Int32 warningCount;

        public Context(TypeKeyDictionary<RID> environment)
        {
            content.AddAll(environment);
        }

        public IEnumerable<IResource> Require<T>(Func<T, IEnumerable<IResource>> func) where T : class
        {
            return Require(func, identifier: null);
        }

        public IEnumerable<IResource> Require<T>(RID identifier, Func<T, IEnumerable<IResource>> func) where T : class
        {
            return Require(func, identifier);
        }

        public T? Get<T>(RID? identifier = null) where T : class
        {
            return identifier is {} id ? content.Get<T>(id) : content.Get<T>();
        }

        public IEnumerable<T> GetAll<T>() where T : class
        {
            return content.GetAll<T>();
        }

        public void ReportWarning(Object source, String message, Exception? exception = null, FileSystemInfo? path = null)
        {
            Debug.Assert(message[^1] is not '.' and not '!' and not '?' && message != exception?.Message);

            currentReport!.Add(new Error(Reflections.GetLongName(source.GetType()), message, isCritical: false));

            if (path == null) LogWarningForResource(logger, exception, currentHierarchy!, message);
            else LogWarningForResourceAtPath(logger, exception, currentHierarchy!, path, message);

            warningCount++;
        }

        public void ReportDiscovery(ResourceType type, RID identifier, Exception? error = null, String? errorMessage = null)
        {
            if (error == null && errorMessage == null)
            {
                LogDiscoveredSubResource(logger, currentHierarchy!, type, identifier);
            }
            else
            {
                String message = errorMessage ?? error!.Message;

                currentReport!.Add(new Error($"{identifier}", message, isCritical: false));

                LogWarningForSubResource(logger, error, currentHierarchy!, type, identifier, message);
            }
        }

        public event EventHandler? Completed;

        public void AddCatalogEntry(ICatalogEntry entry)
        {
            content.Add(entry, key: null);
        }

        public void AddResource(IResource resource)
        {
            content.Add(resource, resource.Identifier);
        }

        public void SetCurrentState(String hierarchy, Group report)
        {
            currentHierarchy = hierarchy;
            currentReport = report;
        }

        public ResourceLoadingIssueReport? BuildReport(Property report)
        {
            if (errorCount == 0 && warningCount == 0)
                return null;

            return new ResourceLoadingIssueReport(report, errorCount, warningCount);
        }

        private IEnumerable<IResource> Require<T>(Func<T, IEnumerable<IResource>> func, RID? identifier)
            where T : class
        {
            var obj = Get<T>(identifier);

            if (obj != null)
                return func(obj);

            String title = Reflections.GetLongName<T>();
            String message = identifier is {} id ? $"Required resource '{id}' not found" : $"Required object of type {title} not found";

            currentReport!.Add(new Error(title, message, isCritical: true));

            LogFailedRequirement(logger, currentHierarchy!, typeof(T).Name);

            errorCount++;

            return [];
        }

        public void ReportIssue(IResource resource, ResourceIssue issue)
        {
            Debug.Assert(issue.Message[^1] is not '.' and not '!' and not '?' || issue.Message == issue.Exception?.Message);

            if (issue.Level == Level.Error)
            {
                currentReport!.Add(new Error(
                    $"{resource.Identifier}",
                    $"{resource.Type} mandatory resource failed to load: {issue.Message}",
                    isCritical: true));

                LogFailedToLoadMandatoryResource(logger, issue.Exception, currentHierarchy!, resource.Type, resource.Identifier, issue.Message);

                errorCount++;
            }
            else
            {
                currentReport!.Add(new Error(
                    $"{resource.Identifier}",
                    $"{resource.Type} resource failed to load: {issue.Message}",
                    isCritical: false));

                LogFailedToLoadResource(logger, issue.Exception, currentHierarchy!, resource.Type, resource.Identifier, issue.Message);

                warningCount++;
            }
        }

        public void OnComplete()
        {
            content.Remove(key: null);

            Completed?.Invoke(this, EventArgs.Empty);
        }

        #region DISPOSING

        private Boolean disposed;

        private void Dispose(Boolean disposing)
        {
            if (disposed)
                return;

            if (disposing)
                foreach (IResource resource in content.GetAll<IResource>())
                    resource.Dispose();
            else Throw.ForMissedDispose(this);

            disposed = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        ~Context()
        {
            Dispose(disposing: false);
        }

        #endregion DISPOSING
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<ResourceCatalogLoader>();

    [LoggerMessage(EventId = LogID.ResourceLoader + 0, Level = LogLevel.Debug, Message = "Starting loading entry '{Entry}'")]
    private static partial void LogStartingLoadingEntry(ILogger logger, String entry);

    [LoggerMessage(EventId = LogID.ResourceLoader + 1,
        Level = LogLevel.Information,
        Message = "Finished loading entry '{Entry}'")]
    private static partial void LogFinishedLoadingEntry(ILogger logger, String entry);

    [LoggerMessage(EventId = LogID.ResourceLoader + 2,
        Level = LogLevel.Debug,
        Message = "{Entry}: Loaded '{Type}' resource '{Resource}'")]
    private static partial void LogLoadedResource(ILogger logger, String entry, ResourceType type, RID resource);

    [LoggerMessage(EventId = LogID.ResourceLoader + 3,
        Level = LogLevel.Warning,
        Message = "{Entry}: An issue occurred during loading - {Message}")]
    private static partial void LogWarningForResource(ILogger logger, Exception? exception, String entry, String message);

    [LoggerMessage(EventId = LogID.ResourceLoader + 4,
        Level = LogLevel.Warning,
        Message = "{Entry}: An issue occurred during loading at {Path} - {Message}")]
    private static partial void LogWarningForResourceAtPath(ILogger logger, Exception? exception, String entry, FileSystemInfo path, String message);

    [LoggerMessage(EventId = LogID.ResourceLoader + 5,
        Level = LogLevel.Warning,
        Message = "{Entry}: Failed to load {Type} resource '{Resource}' - {Message}")]
    private static partial void LogFailedToLoadResource(ILogger logger, Exception? exception, String entry, ResourceType type, RID resource, String message);

    [LoggerMessage(EventId = LogID.ResourceLoader + 6,
        Level = LogLevel.Error,
        Message = "{Entry}: Failed to load mandatory {Type} resource '{Resource}' - {Message}")]
    private static partial void LogFailedToLoadMandatoryResource(ILogger logger, Exception? exception, String entry, ResourceType type, RID resource, String message);

    [LoggerMessage(EventId = LogID.ResourceLoader + 7,
        Level = LogLevel.Debug,
        Message = "{Entry}: Discovered {Type} sub-resource '{Resource}'")]
    private static partial void LogDiscoveredSubResource(ILogger logger, String entry, ResourceType type, RID resource);

    [LoggerMessage(EventId = LogID.ResourceLoader + 8,
        Level = LogLevel.Warning,
        Message = "{Entry}: An issue occurred when loading {Type} sub-resource '{Resource}' - {Message}")]
    private static partial void LogWarningForSubResource(ILogger logger, Exception? exception, String entry, ResourceType type, RID resource, String message);

    [LoggerMessage(EventId = LogID.ResourceLoader + 9,
        Level = LogLevel.Error,
        Message = "{Entry}: Failed to find required object of type '{Type}'")]
    private static partial void LogFailedRequirement(ILogger logger, String entry, String type);

    #endregion LOGGING
}
