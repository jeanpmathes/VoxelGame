// <copyright file="Aspect.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Behaviors.Aspects;

/// <summary>
///     Aspects provide a way to let multiple contributors (i.e., behaviors) deter
/// </summary>
/// <typeparam name="TValue">The value type of the aspect.</typeparam>
/// <typeparam name="TContext">The type of context in which the aspect is evaluated.</typeparam>
public class Aspect<TValue, TContext>
{
    private readonly IContributionStrategy<TValue, TContext> strategy;
    private readonly Int32 maxContributors;

    private readonly List<IContributor<TValue, TContext>> contributors = [];

    private IContributor<TValue, TContext>? exclusiveContributor;

    private Aspect(IContributionStrategy<TValue, TContext> strategy, Int32 maxContributors)
    {
        this.strategy = strategy;
        this.maxContributors = maxContributors;
    }

    /// <summary>
    ///     Create a new aspect with the given strategy.
    /// </summary>
    /// <param name="strategy">The contribution strategy to use for this aspect.</param>
    /// <typeparam name="TStrategy">The type of contribution strategy to use.</typeparam>
    /// <returns>The new aspect.</returns>
    public static Aspect<TValue, TContext> New<TStrategy>(TStrategy strategy) where TStrategy : IContributionStrategy<TValue, TContext>
    {
        return new Aspect<TValue, TContext>(strategy, TStrategy.MaxContributorCount);
    }

    /// <summary>
    ///     Create a new aspect with the given strategy type.
    ///     This method can be used when the strategy has a parameterless constructor.
    /// </summary>
    /// <typeparam name="TStrategy">The type of contribution strategy to use.</typeparam>
    /// <returns>The new aspect.</returns>
    public static Aspect<TValue, TContext> New<TStrategy>() where TStrategy : IContributionStrategy<TValue, TContext>, new()
    {
        return new Aspect<TValue, TContext>(new TStrategy(), TStrategy.MaxContributorCount);
    }

    /// <summary>
    ///     Add a contributor to the aspect.
    /// </summary>
    /// <param name="contributor">The contributor to add.</param>
    /// <param name="exclusive">Whether the contributor wishes to be the only contributor to this aspect.</param>
    /// <param name="context">The resource context in which the contributor is added.</param>
    public void Add(IContributor<TValue, TContext> contributor, Boolean exclusive, IResourceContext context)
    {
        if (exclusiveContributor != null)
        {
            context.ReportWarning(this, "Cannot add contributors to an aspect with an exclusive contributor");

            return;
        }

        if (exclusive)
        {
            if (contributors.Count > 0)
            {
                context.ReportWarning(this, "Cannot add an exclusive contributor to an aspect with multiple contributors");

                return;
            }

            exclusiveContributor = contributor;
        }
        else
        {
            if (contributors.Count > maxContributors)
            {
                context.ReportWarning(this, $"Cannot add more than {maxContributors} contributors to this aspect");

                return;
            }

            contributors.Add(contributor);
        }
    }

    /// <summary>
    ///     Add a contributor to the aspect.
    /// </summary>
    /// <param name="contributor">The contributor to add.</param>
    /// <param name="context">The resource context in which the contributor is added.</param>
    public void Add(IContributor<TValue, TContext> contributor, IResourceContext context)
    {
        Add(contributor, exclusive: false, context);
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
            value = contributors.Count switch
            {
                1 => contributors[index: 0].Contribute(value, context),
                > 1 => strategy.CombineContributions(value, context, CollectionsMarshal.AsSpan(contributors)),
                _ => value
            };

        return value;
    }
}
