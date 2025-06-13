// <copyright file="Mix.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Drawing;

namespace VoxelGame.Core.Behaviors.Aspects.Strategies;

#pragma warning disable S4017 // Intentionally used.

/// <summary>
///     Mixes multiple contributions of all contributors.
///     This strategy can only be used when the value type is <see cref="Color" />.
/// </summary>
public class Mix<TContext> : IContributionStrategy<Color, TContext>
{
    /// <inheritdoc />
    public static Int32 MaxContributorCount => Int32.MaxValue;

    /// <inheritdoc />
    public Color CombineContributions(Color original, TContext context, Span<IContributor<Color, TContext>> contributors)
    {
        UInt64 r = 0;
        UInt64 g = 0;
        UInt64 b = 0;
        UInt64 a = 0;

        foreach (IContributor<Color, TContext> contributor in contributors)
        {
            Color contribution = contributor.Contribute(original, context);

            r += contribution.R;
            g += contribution.G;
            b += contribution.B;
            a += contribution.A;
        }

        var count = (UInt64) contributors.Length;

        return Color.FromArgb(
            (Int32) (r / count),
            (Int32) (g / count),
            (Int32) (b / count),
            (Int32) (a / count)
        );
    }
}
