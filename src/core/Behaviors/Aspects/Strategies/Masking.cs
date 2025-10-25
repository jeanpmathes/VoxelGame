// <copyright file = "Masking.cs" company = "VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Behaviors.Aspects.Strategies;

/// <summary>
///     Used for flag-based aspects, providing only the flags set by all contributors.
/// </summary>
public class Masking<TValue, TContext> : IContributionStrategy<TValue, TContext>
    where TValue : Enum
{
    /// <inheritdoc />
    public static Int32 MaxContributorCount => Int32.MaxValue;
    
    /// <inheritdoc />
    public TValue CombineContributions(TValue original, TContext context, Span<IContributor<TValue, TContext>> contributors)
    {
        var result = Convert.ToInt64(original);

        foreach (IContributor<TValue, TContext> contributor in contributors)
        {
            TValue contribution = contributor.Contribute(original, context);
            result &= Convert.ToInt64(contribution);
        }

        return (TValue)Enum.ToObject(typeof(TValue), result);
    }
}
