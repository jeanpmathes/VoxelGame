// <copyright file="ANDing.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Behaviors.Aspects.Strategies;

/// <summary>
/// Combines contributions by ANDing them together.
/// </summary>
/// <typeparam name="TContext">The context in which the aspect is evaluated.</typeparam>
public class ANDing<TContext> : IContributionStrategy<Boolean, TContext>
{
    /// <inheritdoc />
    public static Int32 MaxContributorCount => Int32.MaxValue;
    
    /// <inheritdoc />
    public Boolean CombineContributions(Boolean original, TContext context, Span<IContributor<Boolean, TContext>> contributors)
    {
        Boolean result = original;
        
        foreach (IContributor<Boolean, TContext> contributor in contributors)
        {
            result &= contributor.Contribute(result, context);
        }
        
        return result;
    }
}
