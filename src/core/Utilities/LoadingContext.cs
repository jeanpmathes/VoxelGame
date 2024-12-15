// <copyright file="LoadingContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Profiling;
using VoxelGame.Logging;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Describes a resource loading failure.
/// </summary>
/// <param name="MissingResources">The missing resources.</param>
/// <param name="IsCritical">Whether the failure is critical.</param>
public record ResourceLoadingFailure(Property MissingResources, Boolean IsCritical);

/// <summary>
///     A context in which loading operations can be performed.
/// </summary>
public partial class LoadingContext : ILoadingContext
{
    private readonly Timer? timer;

    private readonly Group steps;
    private readonly Stack<Entry> path;

    private Boolean isMissingAny;
    private Boolean isMissingCritical;

    /// <summary>
    ///     Creates a new loading context.
    /// </summary>
    public LoadingContext(Timer? timer)
    {
        this.timer = timer;

        steps = new Group("Base");
        path = new Stack<Entry>();

        path.Push(new Entry {group = steps, path = "Base"});
    }

    /// <summary>
    ///     The state of the resource loading. Is either null or an instance of <see cref="ResourceLoadingFailure" />.
    /// </summary>
    public ResourceLoadingFailure? State => isMissingAny ? new ResourceLoadingFailure(steps, isMissingCritical) : null;

    private String CurrentPath => path.Peek().path;

    private Group CurrentGroup => path.Peek().group;

    /// <inheritdoc />
    public IDisposable BeginStep(String name)
    {
        LogStartingLoadingStep(logger, name);

        Entry previous = path.Peek();

        Group current = new(name);
        previous.group.Add(current);

        Timer? subTimer = logger.BeginTimedSubScoped(name, timer);

        path.Push(new Entry {group = current, path = $"{previous.path} > {name}"});

        Step step = new(this, name, current, subTimer);

        return step;
    }

    /// <inheritdoc />
    public void ReportSuccess(String type, String resource)
    {
        LogLoadedResource(logger, CurrentPath, type, resource);
    }

    /// <inheritdoc />
    public void ReportFailure(String type, String resource, Exception exception, Boolean abort = false)
    {
        LogFailedToLoadResource(logger, exception, CurrentPath, type, resource);

        ReportMissing(type, resource, isCritical: true);

        if (abort) Abort();
    }

    /// <inheritdoc />
    public void ReportFailure(String type, String resource, String message, Boolean abort = false)
    {
        LogFailedToLoadResourceWithMessage(logger, CurrentPath, type, resource, message);

        ReportMissing(type, resource, isCritical: true);

        if (abort) Abort();
    }

    /// <inheritdoc />
    public void ReportWarning(String type, String resource, Exception exception)
    {
        LogWarningFailedToLoadResource(logger, exception, CurrentPath, type, resource);

        ReportMissing(type, resource, isCritical: false);
    }

    /// <inheritdoc />
    public void ReportWarning(String type, String resource, String message)
    {
        LogWarningFailedToLoadResourceWithMessage(logger, CurrentPath, type, resource, message);

        ReportMissing(type, resource, isCritical: false);
    }

    private void FinishStep(Step step)
    {
        if (step.Group == path.Peek().group) path.Pop();
        else Debug.Fail("Wrong step finished.");

        LogFinishedLoadingStep(logger, step.Name);
    }

    private void ReportMissing(String type, String resource, Boolean isCritical)
    {
        Error error = new(
            $"RES:{resource}",
            isCritical
                ? $"critical {type} resource missing"
                : $"{type} resource missing",
            isCritical);

        CurrentGroup.Add(error);

        isMissingAny = true;
        isMissingCritical |= isCritical;
    }

    private static void Abort()
    {
        throw new InvalidOperationException("Failed to load an absolute critical resource. See log for details.");
    }

    private sealed class Entry
    {
        public required Group group;
        public required String path;
    }

    private sealed record Step(LoadingContext Context, String Name, Group Group, IDisposable? Scope) : IDisposable
    {
        public void Dispose()
        {
            Context.FinishStep(this);
            Scope?.Dispose();
        }
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<LoadingContext>();

    [LoggerMessage(EventId = LogID.LoadingContext + 0, Level = LogLevel.Debug, Message = "Starting loading step '{StepName}'")]
    private static partial void LogStartingLoadingStep(ILogger logger, String stepName);

    [LoggerMessage(EventId = LogID.LoadingContext + 1, Level = LogLevel.Information, Message = "Finished loading step '{StepName}'")]
    private static partial void LogFinishedLoadingStep(ILogger logger, String stepName);

    [LoggerMessage(EventId = LogID.LoadingContext + 2, Level = LogLevel.Debug, Message = "{Step}: Loaded {Type} resource '{Resource}'")]
    private static partial void LogLoadedResource(ILogger logger, String step, String type, String resource);

    [LoggerMessage(EventId = LogID.LoadingContext + 3, Level = LogLevel.Error, Message = "{Step}: Failed to load {Type} resource '{Resource}'")]
    private static partial void LogFailedToLoadResource(ILogger logger, Exception exception, String step, String type, String resource);

    [LoggerMessage(EventId = LogID.LoadingContext + 4, Level = LogLevel.Error, Message = "{Step}: Failed to load {Type} resource '{Resource}': {Message}")]
    private static partial void LogFailedToLoadResourceWithMessage(ILogger logger, String step, String type, String resource, String message);

    [LoggerMessage(EventId = LogID.LoadingContext + 5, Level = LogLevel.Warning, Message = "{Step}: Failed to load {Type} resource '{Resource}'")]
    private static partial void LogWarningFailedToLoadResource(ILogger logger, Exception exception, String step, String type, String resource);

    [LoggerMessage(EventId = LogID.LoadingContext + 6, Level = LogLevel.Warning, Message = "{Step}: Failed to load {Type} resource '{Resource}': {Message}")]
    private static partial void LogWarningFailedToLoadResourceWithMessage(ILogger logger, String step, String type, String resource, String message);

    #endregion LOGGING
}
