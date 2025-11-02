// <copyright file="BlockBehavior.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Voxels;

/// <summary>
///     Base class for behaviors that apply to blocks.
/// </summary>
public class BlockBehavior : Behavior<BlockBehavior, Block>
{
    /// <summary>
    ///     Creates a new instance of the <see cref="BlockBehavior" /> class.
    ///     Behaviors should be created through the methods on the subject, not directly.
    /// </summary>
    /// <param name="subject">The subject that this behavior applies to.</param>
    protected BlockBehavior(Block subject) : base(subject) {}

    /// <summary>
    ///     Override this method to perform work during block initialization.
    /// </summary>
    /// <param name="properties">
    ///     The properties that define the block's aspects.
    ///     Can be contributed to by the behaviors.
    /// </param>
    public virtual void OnInitialize(BlockProperties properties) {}

    /// <summary>
    ///     Override this method to define the states of the behavior.
    /// </summary>
    /// <param name="builder">The builder to create attributes with.</param>
    public virtual void DefineState(IStateBuilder builder) {}

    /// <summary>
    /// Validates that a predicate holds for all states of the block, reporting a warning if not.
    /// </summary>
    /// <param name="validator">The validator to report warnings to.</param>
    /// <param name="predicate">The predicate to validate.</param>
    /// <param name="message">The message to report if the predicate fails for any state.</param>
    protected void ValidateForAllStatesOrWarn(IValidator validator, Predicate<State> predicate, String message)
    {
        (Int32 count, State evidence) = ValidateForAllStates(predicate);
        
        if (count > 0)
            validator.ReportWarning($"{message} (failed for {count} states, e.g. {evidence})");
    }
    
    /// <summary>
    /// Validates that a predicate holds for all states of the block, reporting an error if not.
    /// </summary>
    /// <param name="validator">The validator to report errors to.</param>
    /// <param name="predicate">The predicate to validate.</param>
    /// <param name="message">The message to report if the predicate fails for any state.</param>
    protected void ValidateForAllStatesOrError(IValidator validator, Predicate<State> predicate, String message)
    {
        (Int32 count, State evidence) = ValidateForAllStates(predicate);
        
        if (count > 0)
            validator.ReportError($"{message} (failed for {count} states, e.g. {evidence})");
    }
    
    private (Int32, State) ValidateForAllStates(Predicate<State> predicate)
    {
        var count = 0;
        State evidence = Subject.States.Default;
        
        foreach (State state in Subject.States.GetAllStates())
        {
            if (predicate(state)) 
                continue;

            count++;
            evidence = state;
        }

        return (count, evidence);
    }
}
