// <copyright file="Minimum.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Numerics;

namespace VoxelGame.Core.Behaviors.Aspects.Strategies;

/// <summary>
/// Uses the minimum contribution from multiple contributors to determine the final value.
/// </summary>
public class Minimum<TValue, TContext> : IContributionStrategy<TValue, TContext> // todo: add maximum
    where TValue : IComparisonOperators<TValue, TValue, Boolean>
{
    /// <inheritdoc />
    public static Int32 MaxContributorCount => Int32.MaxValue;

    /// <inheritdoc />
    public TValue CombineContributions(TValue original, TContext context, Span<IContributor<TValue, TContext>> contributors)
    {
        if (contributors.Length == 0) return original;

        TValue min = contributors[0].Contribute(original, context);
        
        foreach (IContributor<TValue, TContext> contributor in contributors)
        {
            TValue contribution = contributor.Contribute(original, context);
            if (contribution < min) min = contribution;
        }

        return min;
    }
}
