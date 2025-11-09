// <copyright file="Exclusive.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Behaviors.Aspects.Strategies;

/// <summary>
///     Allows only one contributor to contribute to the value.
/// </summary>
public class Exclusive<TValue, TContext> : IContributionStrategy<TValue, TContext>
{
    /// <inheritdoc />
    public static Int32 MaxContributorCount => 1;

    /// <inheritdoc />
    public TValue CombineContributions(TValue original, TContext context, Span<IContributor<TValue, TContext>> contributors)
    {
        return contributors[index: 0].Contribute(original, context);
    }
}
