// <copyright file="LoadingContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
public record ResourceLoadingFailure(Property MissingResources, bool IsCritical);

/// <summary>
///     A context in which loading operations can be performed.
/// </summary>
public class LoadingContext
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<LoadingContext>();

    private readonly Timer? timer;

    private readonly Group steps;
    private readonly Stack<Entry> path;

    private bool isMissingAny;
    private bool isMissingCritical;

    /// <summary>
    ///     Creates a new loading context.
    /// </summary>
    public LoadingContext(Timer? timer)
    {
        this.timer = timer;

        steps = new Group("Base");
        path = new Stack<Entry>();

        path.Push(new Entry {group = steps, path = "Base", timer = timer});
    }

    /// <summary>
    ///     The state of the resource loading. Is either null or an instance of <see cref="ResourceLoadingFailure" />.
    /// </summary>
    public ResourceLoadingFailure? State => isMissingAny ? new ResourceLoadingFailure(steps, isMissingCritical) : null;

    private string CurrentPath => path.Peek().path;

    private Group CurrentGroup => path.Peek().group;

    private void FinishStep(Step step)
    {
        if (step.Group == path.Peek().group)
        {
            path.Pop();
        }
        else
        {
            Debug.Fail("Wrong step finished.");
        }

        logger.LogInformation(step.ID, "Finished loading step '{StepName}'", step.Name);
    }

    /// <summary>
    ///     Begin a loading step.
    /// </summary>
    /// <param name="id">The event id of general step-related events.</param>
    /// <param name="name">The name of the step.</param>
    public IDisposable BeginStep(EventId id, string name)
    {
        logger.LogDebug(id, "Starting loading step '{StepName}'", name);

        Entry previous = path.Peek();

        Group current = new(name);
        previous.group.Add(current);

        Timer? subTimer = timer?.StartSub(name);

        path.Push(new Entry {group = current, path = $"{previous.path} > {name}", timer = subTimer});

        Step step = new(this, id, name, current, subTimer);

        return step;
    }

    private void ReportMissing(string type, string resource, bool isCritical)
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

    /// <summary>
    ///     Report a successful loading operation.
    /// </summary>
    public void ReportSuccess(EventId id, string type, FileSystemInfo resource)
    {
        ReportSuccess(id, type, resource.GetResourceRelativePath());
    }

    /// <summary>
    ///     Report a successful loading operation.
    /// </summary>
    public void ReportSuccess(EventId id, string type, string resource)
    {
        logger.LogDebug(id, "{Step}: Loaded {Type} resource '{Resource}'", CurrentPath, type, resource);
    }

    /// <summary>
    ///     Report a failed loading operation, which will make it impossible to start the game.
    /// </summary>
    public void ReportFailure(EventId id, string type, FileSystemInfo resource, Exception exception, bool abort = false)
    {
        ReportFailure(id, type, resource.GetResourceRelativePath(), exception, abort);
    }

    /// <summary>
    ///     Report a failed loading operation, which will make it impossible to start the game.
    /// </summary>
    public void ReportFailure(EventId id, string type, string resource, Exception exception, bool abort = false)
    {
        logger.LogError(id, exception, "{Step}: Failed to load {Type} resource '{Resource}'", CurrentPath, type, resource);
        ReportMissing(type, resource, isCritical: true);

        if (abort) Abort();
    }

    /// <summary>
    ///     Report a failed loading operation, which will make it impossible to start the game.
    /// </summary>
    public void ReportFailure(EventId id, string type, FileSystemInfo resource, string message, bool abort = false)
    {
        ReportFailure(id, type, resource.GetResourceRelativePath(), message, abort);
    }

    /// <summary>
    ///     Report a failed loading operation, which will make it impossible to start the game.
    /// </summary>
    public void ReportFailure(EventId id, string type, string resource, string message, bool abort = false)
    {
        logger.LogError(id, "{Step}: Failed to load {Type} resource '{Resource}': {Message}", CurrentPath, type, resource, message);
        ReportMissing(type, resource, isCritical: true);

        if (abort) Abort();
    }

    /// <summary>
    ///     Warn about a failed loading operation. The game is still able to start.
    /// </summary>
    public void ReportWarning(EventId id, string type, FileSystemInfo resource, Exception exception)
    {
        ReportWarning(id, type, resource.GetResourceRelativePath(), exception);
    }

    /// <summary>
    ///     Warn about a failed loading operation. The game is still able to start.
    /// </summary>
    public void ReportWarning(EventId id, string type, string resource, Exception exception)
    {
        logger.LogWarning(id, exception, "{Step}: Failed to load {Type} resource '{Resource}'", CurrentPath, type, resource);
        ReportMissing(type, resource, isCritical: false);
    }

    /// <summary>
    ///     Warn about a failed loading operation. The game is still able to start.
    /// </summary>
    public void ReportWarning(EventId id, string type, FileSystemInfo resource, string message)
    {
        ReportWarning(id, type, resource.GetResourceRelativePath(), message);
    }

    /// <summary>
    ///     Warn about a failed loading operation. The game is still able to start.
    /// </summary>
    public void ReportWarning(EventId id, string type, string resource, string message)
    {
        logger.LogWarning(id, "{Step}: Failed to load {Type} resource '{Resource}': {Message}", CurrentPath, type, resource, message);
        ReportMissing(type, resource, isCritical: false);
    }

    private static void Abort()
    {
        throw new InvalidOperationException("Failed to load an absolute critical resource. See log for details.");
    }

    private sealed class Entry
    {
        public required Group group;
        public required string path;
        public required Timer? timer;
    }

    private sealed record Step(LoadingContext Context, EventId ID, string Name, Group Group, IDisposable? Scope) : IDisposable
    {
        public void Dispose()
        {
            Context.FinishStep(this);
            Scope?.Dispose();
        }
    }
}
