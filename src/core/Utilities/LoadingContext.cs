// <copyright file="LoadingContext.cs" company="VoxelGame">
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
using VoxelGame.Logging;

namespace VoxelGame.Core.Utilities;

using Tree = Tree<string>;
using Node = Tree<string>.INode;

/// <summary>
///     Describes a resource loading failure.
/// </summary>
/// <param name="MissingResources">The missing resources.</param>
/// <param name="IsCritical">Whether the failure is critical.</param>
public record ResourceLoadingFailure(Tree<string> MissingResources, bool IsCritical);

/// <summary>
///     A context in which loading operations can be performed.
/// </summary>
public class LoadingContext
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<LoadingContext>();

    /// <summary>
    ///     Get the steps and all missing resources as a tree.
    /// </summary>
    private readonly Tree steps;

    private string currentPath;
    private Node currentStep;

    private bool isMissingAny;
    private bool isMissingCritical;

    /// <summary>
    ///     Creates a new loading context.
    /// </summary>
    public LoadingContext()
    {
        steps = new Tree("Base");

        currentPath = string.Empty;
        currentStep = steps.Root;

        RecalculatePath();
    }

    /// <summary>
    ///     The state of the resource loading. Is either null or an instance of <see cref="ResourceLoadingFailure" />.
    /// </summary>
    public ResourceLoadingFailure? State => isMissingAny ? new ResourceLoadingFailure(steps, isMissingCritical) : null;

    private void RecalculatePath()
    {
        List<string> path = new();
        Node? node = currentStep;

        while (node != null)
        {
            path.Add(node.Value);
            node = node.Parent;
        }

        currentPath = path.AsEnumerable().Reverse().Aggregate((a, b) => $"{a} > {b}");
    }

    private void FinishStep(Step step)
    {
        if (step.Node == currentStep)
        {
            currentStep = currentStep.Parent ?? steps.Root;
            RecalculatePath();
        }
        else
        {
            Debug.Fail("Step was not the last one.");
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

        currentStep = currentStep.AddChild(name);
        RecalculatePath();

        Step step = new(this, id, name, currentStep, logger.BeginScope(name));

        return step;
    }

    private void ReportMissing(string type, string resource, bool isCritical)
    {
        currentStep.AddChild(isCritical
            ? $"Missing critical {type} resource 'RES:{resource}'"
            : $"Missing {type} resource 'RES:{resource}'");

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
        logger.LogDebug(id, "{Step}: Loaded {Type} resource '{Resource}'", currentPath, type, resource);
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
        logger.LogError(id, exception, "{Step}: Failed to load {Type} resource '{Resource}'", currentPath, type, resource);
        ReportMissing(type, resource, isCritical: true);
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
        logger.LogError(id, "{Step}: Failed to load {Type} resource '{Resource}': {Message}", currentPath, type, resource, message);
        ReportMissing(type, resource, isCritical: true);
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
        logger.LogWarning(id, exception, "{Step}: Failed to load {Type} resource '{Resource}'", currentPath, type, resource);
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
        logger.LogWarning(id, "{Step}: Failed to load {Type} resource '{Resource}': {Message}", currentPath, type, resource, message);
        ReportMissing(type, resource, isCritical: false);
    }

    private sealed record Step(LoadingContext Context, EventId ID, string Name, Node Node, IDisposable Scope) : IDisposable
    {
        public void Dispose()
        {
            Context.FinishStep(this);
            Scope.Dispose();
        }
    }
}
