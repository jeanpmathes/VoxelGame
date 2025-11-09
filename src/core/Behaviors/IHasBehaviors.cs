// <copyright file="IHasBehaviors.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Behaviors;

/// <summary>
///     Interface for a subject that carries behaviors.
/// </summary>
public interface IHasBehaviors : IEventSubject, IAspectable, IIssueSource;

/// <summary>
///     Interface for a subject that carries behaviors.
///     See <see cref="BehaviorContainer{TSelf, TBehavior}" /> for a base implementation.
/// </summary>
/// <typeparam name="TSubject">The subject type that holds behaviors.</typeparam>
/// <typeparam name="TBehavior">The behavior base class type that this subject holds.</typeparam>
public interface IHasBehaviors<TSubject, TBehavior> : IHasBehaviors
    where TSubject : class, IHasBehaviors<TSubject, TBehavior>
    where TBehavior : class, IBehavior<TSubject>
{
    /// <summary>
    ///     Gets all behaviors of this subject.
    /// </summary>
    public IEnumerable<TBehavior> Behaviors { get; }

    /// <summary>
    ///     Checks if this subject has a behavior of the specified concrete type.
    /// </summary>
    /// <typeparam name="TConcreteBehavior">The concrete behavior type to check for.</typeparam>
    /// <returns><c>true</c> if the subject has the specified behavior; otherwise, <c>false</c>.</returns>
    public Boolean Is<TConcreteBehavior>() where TConcreteBehavior : class, TBehavior, IBehavior<TConcreteBehavior, TBehavior, TSubject>;

    /// <summary>
    ///     Get the behavior of the specified concrete type if it exists, otherwise returns <c>null</c>.
    /// </summary>
    /// <typeparam name="TConcreteBehavior">The concrete behavior type to retrieve.</typeparam>
    /// <returns>The behavior of the specified type if it exists; otherwise, <c>null</c>.</returns>
    public TConcreteBehavior? Get<TConcreteBehavior>() where TConcreteBehavior : class, TBehavior, IBehavior<TConcreteBehavior, TBehavior, TSubject>;

    /// <summary>
    ///     Requires the behavior of the specified concrete type, creating it if it does not exist.
    ///     This is only valid to call before the subject is baked.
    /// </summary>
    /// <typeparam name="TConcreteBehavior">The concrete behavior type to require.</typeparam>
    /// <returns>The behavior of the specified type, guaranteed to exist after this call.</returns>
    public TConcreteBehavior Require<TConcreteBehavior>() where TConcreteBehavior : class, TBehavior, IBehavior<TConcreteBehavior, TBehavior, TSubject>;

    /// <summary>
    ///     Require a certain behavior under the condition that another behavior is present.
    ///     This means that as soon as the other behavior is present, this behavior will be created and the initializer will be
    ///     called.
    ///     If the other behavior is already present, the initializer will be called immediately.
    /// </summary>
    /// <param name="initializer">The optional initializer to call when the condition is met.</param>
    /// <typeparam name="TConditionalConcreteBehavior">The type of the behavior to add if the other behavior is present.</typeparam>
    /// <typeparam name="TConditionConcreteBehavior">
    ///     The type of the behavior that must be present for the conditional behavior
    ///     to be added.
    /// </typeparam>
    public void RequireIfPresent<TConditionalConcreteBehavior, TConditionConcreteBehavior>(Action<TConditionalConcreteBehavior>? initializer = null)
        where TConditionalConcreteBehavior : class, TBehavior, IBehavior<TConditionalConcreteBehavior, TBehavior, TSubject>
        where TConditionConcreteBehavior : class, TBehavior, IBehavior<TConditionConcreteBehavior, TBehavior, TSubject>;

    /// <summary>
    ///     Bakes the behaviors of this subject into an array.
    ///     After baking, the subject's behaviors are immutable and cannot be modified.
    /// </summary>
    /// <param name="array">The array to bake the behaviors into.</param>
    public void Bake(TBehavior?[] array);

    /// <summary>
    ///     Validates the behaviors of this subject.
    /// </summary>
    public void Validate(IValidator validator);
}
