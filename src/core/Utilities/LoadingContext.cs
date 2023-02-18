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
using VoxelGame.Logging;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     A context in which loading operations can be performed.
/// </summary>
public class LoadingContext
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<LoadingContext>();

    private readonly List<(Step step, string previous)> steps = new();

    private string CurrentStep { get; set; } = "Base";

    private void FinishStep(Step step)
    {
        if (steps[^1].step == step)
        {
            CurrentStep = steps[^1].previous;
            steps.RemoveAt(steps.Count - 1);
        }
        else
        {
            Debug.Fail("Step was not the last one.");
            steps.RemoveAll(s => s.step == step);
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

        Step step = new(this, id, name, logger.BeginScope(name));

        steps.Add((step, CurrentStep));
        CurrentStep = $"{CurrentStep} > {name}";

        return step;
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
        logger.LogDebug(id, "{Step}: Loaded {Type} resource '{Resource}'", CurrentStep, type, resource);
    }

    /// <summary>
    ///     Report a failed loading operation, which will make it impossible to start the game.
    /// </summary>
    public void ReportFailure(EventId id, string type, FileSystemInfo resource, Exception exception)
    {
        ReportFailure(id, type, resource.GetResourceRelativePath(), exception);
    }

    /// <summary>
    ///     Report a failed loading operation, which will make it impossible to start the game.
    /// </summary>
    public void ReportFailure(EventId id, string type, string resource, Exception exception)
    {
        logger.LogError(id, exception, "{Step}: Failed to load {Type} resource '{Resource}'", CurrentStep, type, resource);
    }

    /// <summary>
    ///     Report a failed loading operation, which will make it impossible to start the game.
    /// </summary>
    public void ReportFailure(EventId id, string type, FileSystemInfo resource, string message)
    {
        ReportFailure(id, type, resource.GetResourceRelativePath(), message);
    }

    /// <summary>
    ///     Report a failed loading operation, which will make it impossible to start the game.
    /// </summary>
    public void ReportFailure(EventId id, string type, string resource, string message)
    {
        logger.LogError(id, "{Step}: Failed to load {Type} resource '{Resource}': {Message}", CurrentStep, type, resource, message);
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
        logger.LogWarning(id, exception, "{Step}: Failed to load {Type} resource '{Resource}'", CurrentStep, type, resource);
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
        logger.LogWarning(id, "{Step}: Failed to load {Type} resource '{Resource}': {Message}", CurrentStep, type, resource, message);
    }

    private sealed record Step(LoadingContext Context, EventId ID, string Name, IDisposable Scope) : IDisposable
    {
        public void Dispose()
        {
            Context.FinishStep(this);
            Scope.Dispose();
        }
    }
}

