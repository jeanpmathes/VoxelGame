﻿// <copyright file="Aspect.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Behaviors.Aspects;

/// <summary>
///     Aspects provide a way to let multiple contributors (i.e., behaviors) determine a value.
///     In contrast to the <see cref="IEvent{T}"/> system, aspects are meant to be side effect free.
/// </summary>
/// <typeparam name="TValue">The value type of the aspect.</typeparam>
/// <typeparam name="TContext">The type of context in which the aspect is evaluated.</typeparam>
public class Aspect<TValue, TContext>
{
    // todo: think about precomputing all / some aspects that use State as context, aspects could register them with a generic aspect registry

    private readonly String name;
    private readonly Int32 maxContributors;
    private readonly IContributionStrategy<TValue, TContext> strategy;
    private readonly IAspectable owner;

    private readonly List<IContributor<TValue, TContext>> usedContributors = [];
    private readonly List<IContributor<TValue, TContext>> rejectedContributors = [];

    private IContributor<TValue, TContext>? exclusiveContributor;

    private Aspect(String name, Int32 maxContributors, IContributionStrategy<TValue, TContext> strategy, IAspectable owner)
    {
        this.name = name;
        this.maxContributors = maxContributors;
        this.strategy = strategy;
        this.owner = owner;
        
        owner.Validation += OnValidation;
    }
    
    /// <summary>
    ///     Create a new aspect with the given strategy.
    /// </summary>
    /// <param name="name">The name of the aspect.</param>
    /// <param name="strategy">The contribution strategy to use for this aspect.</param>
    /// <param name="owner">The owner of this aspect.</param>
    /// <typeparam name="TStrategy">The type of contribution strategy to use.</typeparam>
    /// <returns>The new aspect.</returns>
    public static Aspect<TValue, TContext> New<TStrategy>(String name, TStrategy strategy, IAspectable owner) where TStrategy : IContributionStrategy<TValue, TContext>
    {
        return new Aspect<TValue, TContext>(
            name,
            TStrategy.MaxContributorCount,
            strategy,
            owner
        );
    }

    /// <summary>
    ///     Create a new aspect with the given strategy type.
    ///     This method can be used when the strategy has a parameterless constructor.
    /// </summary>
    /// <param name="name">The name of the aspect.</param>
    /// <param name="owner">The owner of this aspect.</param>
    /// <typeparam name="TStrategy">The type of contribution strategy to use.</typeparam>
    /// <returns>The new aspect.</returns>
    public static Aspect<TValue, TContext> New<TStrategy>(String name, IAspectable owner) where TStrategy : IContributionStrategy<TValue, TContext>, new()
    {
        return new Aspect<TValue, TContext>(
            name,
            TStrategy.MaxContributorCount,
            new TStrategy(),
            owner
        );
    }

    /// <summary>
    ///     Add a contributor to the aspect.
    /// </summary>
    /// <param name="contributor">The contributor to add.</param>
    /// <param name="exclusive">Whether the contributor wishes to be the only contributor to this aspect.</param>
    public void Add(IContributor<TValue, TContext> contributor, Boolean exclusive = false)
    {
        if (exclusiveContributor != null)
        {
            rejectedContributors.Add(contributor);

            return;
        }

        if (exclusive)
        {
            if (usedContributors.Count > 0)
            {
                rejectedContributors.Add(contributor);

                return;
            }

            exclusiveContributor = contributor;
        }
        else
        {
            if (usedContributors.Count > maxContributors)
            {
                rejectedContributors.Add(contributor);

                return;
            }

            usedContributors.Add(contributor);
        }
    }

    private void OnValidation(Object? sender, IAspectable.ValidationEventArgs e)
    {
        if (usedContributors.Count == maxContributors && rejectedContributors.Count > 0)
        {
            e.Context.ReportWarning(this, $"Cannot add more than {maxContributors} contributors to aspect");
        }
        else if (exclusiveContributor != null && rejectedContributors.Count > 0)
        {
            e.Context.ReportWarning(this, "Cannot add more than one contributor when an exclusive contributor is set");
        }
    }

    /// <summary>
    ///     Calculate the value of the aspect.
    /// </summary>
    /// <param name="original">The original value the contributors will contribute to.</param>
    /// <param name="context">The context in which the aspect is evaluated.</param>
    /// <returns>The calculated value of the aspect.</returns>
    public TValue GetValue(TValue original, TContext context)
    {
        TValue value = original;

        if (exclusiveContributor != null)
            value = exclusiveContributor.Contribute(value, context);
        else
            value = usedContributors.Count switch
            {
                1 => usedContributors[index: 0].Contribute(value, context),
                > 1 => strategy.CombineContributions(value, context, CollectionsMarshal.AsSpan(usedContributors)),
                _ => value
            };

        return value;
    }

    /// <inheritdoc/>
    public override String ToString()
    {
        return Reflections.GetDecoratedName(owner.ToString() ?? "unknown", GetType(), name);
    }
}
