// <copyright file="Validator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Behaviors;

/// <summary>
/// Tracks the validation of a behavior system.
/// </summary>
public interface IValidator
{
    /// <summary>
    /// Report a warning during validation.
    /// Will not abort creation and not fail the validation.
    /// </summary>
    void ReportWarning(String message);
    
    /// <summary>
    /// Report an error during validation.
    /// This might abort further creation of the system.
    /// </summary>
    void ReportError(String message);
}

/// <summary>
/// Implements <see cref="IValidator"/>.
/// </summary>
public class Validator(IResourceContext context) : IValidator
{
    private IIssueSource? source;
    private String sourceInfo = "";
    
    /// <summary>
    /// Whether there is at least one error reported.
    /// </summary>
    public Boolean HasError { get; private set; }
    
    /// <inheritdoc />
    public void ReportWarning(String message)
    {
        Debug.Assert(source != null);
        
        context.ReportWarning(source, $"{sourceInfo} {message}");
    }
    
    /// <inheritdoc />
    public void ReportError(String message)
    {
        Debug.Assert(source != null);
        
        context.ReportError(source, $"{sourceInfo} {message}");
        
        HasError = true;
    }

    /// <summary>
    /// Set the current scope of the validator.
    /// </summary>
    public void SetScope(IHasBehaviors behaviorContainer)
    {
        source = behaviorContainer;
        sourceInfo = $"[in {behaviorContainer}]";
    }
    
    /// <summary>
    /// Set the current scope of the validator.
    /// </summary>
    public void SetScope(IBehavior behavior)
    {
        source = behavior;
        sourceInfo = $"[in {behavior.Subject}]";
    }
}
