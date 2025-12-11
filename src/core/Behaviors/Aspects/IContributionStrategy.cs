// <copyright file="IContributionStrategy.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Behaviors.Aspects;

/// <summary>
///     Defines how multiple contributions from contributors should be combined.
/// </summary>
/// <typeparam name="TValue">The type of the value being contributed to.</typeparam>
/// <typeparam name="TContext">The type of the context in which contributions are made.</typeparam>
public interface IContributionStrategy<TValue, TContext>
{
    /// <summary>
    ///     The maximum number of contributors that can be combined by this strategy.
    ///     This number should be greater than or equal to 1.
    /// </summary>
    static abstract Int32 MaxContributorCount { get; }

    /// <summary>
    ///     Apply the strategy to determine the final value.
    /// </summary>
    /// <param name="original">The original value to which contributions are made.</param>
    /// <param name="context">The context in which the aspect is evaluated.</param>
    /// <param name="contributors">The contributors providing contributions to the original value.</param>
    /// <returns>The final value as determined by the strategy.</returns>
    TValue CombineContributions(TValue original, TContext context, Span<IContributor<TValue, TContext>> contributors);
}
