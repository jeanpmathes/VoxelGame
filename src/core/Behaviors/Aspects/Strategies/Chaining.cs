// <copyright file="Chaining.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Behaviors.Aspects.Strategies;

/// <summary>
///     Chains together all contributions from contributors.
///     Note that this strategy should only be used when the contributions are not expected to conflict.
///     The order of contributions is not guaranteed, so the result may vary based on the order of contributors.
/// </summary>
/// <typeparam name="TValue">The type of the value being contributed to.</typeparam>
/// <typeparam name="TContext">The type of the context in which the contributions are made.</typeparam>
public class Chaining<TValue, TContext> : IContributionStrategy<TValue, TContext>
{
    /// <inheritdoc />
    public static Int32 MaxContributorCount => Int32.MaxValue;

    /// <inheritdoc />
    public TValue CombineContributions(TValue original, TContext context, Span<IContributor<TValue, TContext>> contributors)
    {
        TValue result = original;

        foreach (IContributor<TValue, TContext> contributor in contributors) result = contributor.Contribute(result, context);

        return result;
    }
}
