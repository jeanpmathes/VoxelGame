// <copyright file="IConcreteBehavior.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Behaviors;

#pragma warning disable S2436 // Three generic parameters needed for the system to work correctly.

/// <summary>
///     Defines the minimal behavior interface.
///     Mostly used for internal purposes, see <see cref="Behavior{TSelf,TSubject}" /> for the main functionality.
/// </summary>
public interface IBehavior : IIssueSource
{
    /// <summary>
    ///     The unknown ID for behaviors.
    /// </summary>
    const Int32 UnknownID = -1;

    /// <summary>
    ///     Get the subject that this behavior applies to.
    /// </summary>
    IHasBehaviors Subject { get; }

    /// <summary>
    ///     Validates the behavior.
    /// </summary>
    void Validate(IValidator validator);
}

/// <summary>
///     Specializations of <see cref="IBehavior" /> that include a subject.
/// </summary>
/// <typeparam name="TSubject">The subject type that the behavior applies to.</typeparam>
public interface IBehavior<out TSubject> : IBehavior, IEventSubject, IAspectable where TSubject : IHasBehaviors
{
    /// <summary>
    ///     Get the subject that this behavior applies to.
    /// </summary>
    new TSubject Subject { get; }
}

/// <summary>
///     The most specialized behavior interface.
/// </summary>
/// <typeparam name="TSelf">The type of the behavior itself.</typeparam>
/// <typeparam name="TBase">The base behavior type of the behavior hierarchy.</typeparam>
/// <typeparam name="TSubject">The subject type that the behavior applies to.</typeparam>
[SuppressMessage("ReSharper", "StaticMemberInGenericType", Justification = "Intentionally used.")]
public interface IBehavior<out TSelf, TBase, TSubject> : IBehavior<TSubject>, IConstructible<TSubject, TSelf>
    where TSelf : class, TBase, IBehavior<TSelf, TBase, TSubject>
    where TBase : class, IBehavior<TSubject>
    where TSubject : class, IHasBehaviors<TSubject, TBase>
{
    /// <summary>
    ///     The ID of the behavior.
    /// </summary>
    public static Int32 ID { get; private set; } = UnknownID;

    /// <summary>
    ///     Sets the ID of the behavior.
    ///     Is called internally by the behavior system.
    /// </summary>
    internal static void SetID(Int32 id)
    {
        if (ID != UnknownID)
            throw Exceptions.InvalidOperation("ID already set.");

        ID = id;
    }

    /// <summary>
    ///     Creates a new instance of the behavior for the given subject.
    /// </summary>
    internal static TSelf Create(TSubject subject)
    {
        var behavior = TSelf.Construct(subject);

        BehaviorSystem<TSubject, TBase>.Register(behavior);

        return behavior;
    }
}
