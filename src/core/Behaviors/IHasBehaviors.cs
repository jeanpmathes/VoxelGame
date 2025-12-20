// <copyright file="IHasBehaviors.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
    IEnumerable<TBehavior> Behaviors { get; }

    /// <summary>
    ///     Checks if this subject has a behavior of the specified concrete type.
    /// </summary>
    /// <typeparam name="TConcreteBehavior">The concrete behavior type to check for.</typeparam>
    /// <returns><c>true</c> if the subject has the specified behavior; otherwise, <c>false</c>.</returns>
    Boolean Is<TConcreteBehavior>() where TConcreteBehavior : class, TBehavior, IBehavior<TConcreteBehavior, TBehavior, TSubject>;

    /// <summary>
    ///     Get the behavior of the specified concrete type if it exists, otherwise returns <c>null</c>.
    /// </summary>
    /// <typeparam name="TConcreteBehavior">The concrete behavior type to retrieve.</typeparam>
    /// <returns>The behavior of the specified type if it exists; otherwise, <c>null</c>.</returns>
    TConcreteBehavior? Get<TConcreteBehavior>() where TConcreteBehavior : class, TBehavior, IBehavior<TConcreteBehavior, TBehavior, TSubject>;

    /// <summary>
    ///     Requires the behavior of the specified concrete type, creating it if it does not exist.
    ///     This is only valid to call before the subject is baked.
    /// </summary>
    /// <typeparam name="TConcreteBehavior">The concrete behavior type to require.</typeparam>
    /// <returns>The behavior of the specified type, guaranteed to exist after this call.</returns>
    TConcreteBehavior Require<TConcreteBehavior>() where TConcreteBehavior : class, TBehavior, IBehavior<TConcreteBehavior, TBehavior, TSubject>;

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
    void RequireIfPresent<TConditionalConcreteBehavior, TConditionConcreteBehavior>(Action<TConditionalConcreteBehavior>? initializer = null)
        where TConditionalConcreteBehavior : class, TBehavior, IBehavior<TConditionalConcreteBehavior, TBehavior, TSubject>
        where TConditionConcreteBehavior : class, TBehavior, IBehavior<TConditionConcreteBehavior, TBehavior, TSubject>;

    /// <summary>
    ///     Bakes the behaviors of this subject into an array.
    ///     After baking, the subject's behaviors are immutable and cannot be modified.
    /// </summary>
    /// <param name="array">The array to bake the behaviors into.</param>
    void Bake(TBehavior?[] array);

    /// <summary>
    ///     Validates the behaviors of this subject.
    /// </summary>
    void Validate(IValidator validator);
}
