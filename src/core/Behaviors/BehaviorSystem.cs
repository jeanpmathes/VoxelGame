﻿// <copyright file="BehaviorSystem.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Behaviors;

#pragma warning disable S2743 // Intentionally used.

/// <summary>
///     The behavior system coordinates baking of behaviors for subjects.
///     All subjects associated with a system will have their behaviors baked into an array.
///     The behavior IDs are then uniformly assigned in the context of that system.
/// </summary>
/// <typeparam name="TSubject">The subject type managed by this system.</typeparam>
/// <typeparam name="TBehavior">The behavior base type managed by this system.</typeparam>
[SuppressMessage("ReSharper", "StaticMemberInGenericType", Justification = "Intentionally used.")]
public static class BehaviorSystem<TSubject, TBehavior>
    where TSubject : class, IHasBehaviors<TSubject, TBehavior>
    where TBehavior : class, IBehavior<TSubject>
{
    private static readonly HashSet<Type> knownTypes = [];
    private static readonly Dictionary<TSubject, List<(TBehavior, Int32)>> subjects = [];

    private static Boolean isBaked;

    /// <summary>
    ///     Register a concrete behavior with the system.
    /// </summary>
    /// <param name="behavior">The behavior to register.</param>
    /// <typeparam name="TConcreteBehavior">The concrete behavior type to register.</typeparam>
    public static void Register<TConcreteBehavior>(TConcreteBehavior behavior)
        where TConcreteBehavior : class, TBehavior, IBehavior<TConcreteBehavior, TBehavior, TSubject>
    {
        EnsureNotBaked();

        if (!subjects.ContainsKey(behavior.Subject))
            subjects[behavior.Subject] = [];

        Int32 id = IBehavior<TConcreteBehavior, TBehavior, TSubject>.ID;

        if (id == IBehavior.UnknownID)
        {
            knownTypes.Add(typeof(TConcreteBehavior));

            id = knownTypes.Count - 1;

            IBehavior<TConcreteBehavior, TBehavior, TSubject>.SetID(id);
        }

        subjects[behavior.Subject].Add((behavior, id));
    }

    /// <summary>
    ///     Bake the entire system. This may only be called once.
    /// </summary>
    /// <returns></returns>
    public static Int32 Bake()
    {
        EnsureNotBaked();
        isBaked = true;

        Int32 knownCount = knownTypes.Count;
        var array = new TBehavior?[knownCount];

        foreach ((TSubject subject, List<(TBehavior, Int32)> behaviors) in subjects)
        {
            Array.Fill(array, value: null);
            Int32 maxID = -1;

            foreach ((TBehavior behavior, Int32 id) in behaviors)
            {
                array[id] = behavior;
                maxID = Math.Max(maxID, id);
            }

            SetupEvents(subject);

            subject.Bake(array.Take(maxID + 1).ToArray());
        }

        return knownCount;
    }

    private static void SetupEvents(TSubject subject)
    {
        EventSystem eventSystem = new();

        subject.DefineEvents(eventSystem);

        foreach (TBehavior behavior in subject.Behaviors)
            behavior.DefineEvents(eventSystem);

        subject.SubscribeToEvents(eventSystem);

        foreach (TBehavior behavior in subject.Behaviors)
            behavior.SubscribeToEvents(eventSystem);
    }

    private static void EnsureNotBaked()
    {
        if (isBaked)
            throw Exceptions.InvalidOperation("Behavior system already baked.");
    }
}
