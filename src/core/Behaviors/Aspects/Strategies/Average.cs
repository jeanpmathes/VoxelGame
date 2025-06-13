// <copyright file="Average.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Numerics;

namespace VoxelGame.Core.Behaviors.Aspects.Strategies;

#pragma warning disable S2743 // Intentionally used.

/// <summary>
///     Combines multiple contributors by calculating the average of their contributions.
///     This requires the value type to support addition, additive identity, and division by an integer.
/// </summary>
public class Average<TValue, TContext> : IContributionStrategy<TValue, TContext>
    where TValue : IAdditionOperators<TValue, TValue, TValue>, IAdditiveIdentity<TValue, TValue>, IDivisionOperators<TValue, Int32, TValue>
{
    /// <inheritdoc />
    public static Int32 MaxContributorCount => Int32.MaxValue;

    /// <inheritdoc />
    public TValue CombineContributions(TValue original, TContext context, Span<IContributor<TValue, TContext>> contributors)
    {
        TValue sum = TValue.AdditiveIdentity;

        foreach (IContributor<TValue, TContext> contributor in contributors) sum += contributor.Contribute(original, context);

        return sum / contributors.Length;
    }
}
