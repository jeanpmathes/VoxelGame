// <copyright file="Mix.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Behaviors.Aspects.Strategies;

/// <summary>
///     Mixes multiple contributions of all contributors.
///     This strategy can only be used when the value type is <see cref="ColorS" />.
///     This performs special handling of <see cref="ColorS.Neutral" /> contributions.
/// </summary>
public class Mix<TContext> : IContributionStrategy<ColorS, TContext>
{
    /// <inheritdoc />
    public static Int32 MaxContributorCount => Int32.MaxValue;

    /// <inheritdoc />
    public ColorS CombineContributions(ColorS original, TContext context, Span<IContributor<ColorS, TContext>> contributors)
    {
        if (contributors.Length == 0)
            return original;

        var r = 0.0;
        var g = 0.0;
        var b = 0.0;
        var a = 0.0;

        foreach (IContributor<ColorS, TContext> contributor in contributors)
        {
            ColorS contribution = contributor.Contribute(original, context);

            if (contribution.IsNeutral)
                return contribution;

            r += contribution.R;
            g += contribution.G;
            b += contribution.B;
            a += contribution.A;
        }

        Single count = contributors.Length;

        return ColorS.FromRGBA(
            (Single) (r / count),
            (Single) (g / count),
            (Single) (b / count),
            (Single) (a / count));
    }
}
