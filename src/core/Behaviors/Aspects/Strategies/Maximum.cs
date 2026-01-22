// <copyright file="Maximum.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Numerics;

namespace VoxelGame.Core.Behaviors.Aspects.Strategies;

/// <summary>
///     Uses the maximum contribution from multiple contributors to determine the final value.
/// </summary>
public class Maximum<TValue, TContext> : IContributionStrategy<TValue, TContext>
    where TValue : IComparisonOperators<TValue, TValue, Boolean>
{
    /// <inheritdoc />
    public static Int32 MaxContributorCount => Int32.MaxValue;

    /// <inheritdoc />
    public TValue CombineContributions(TValue original, TContext context, Span<IContributor<TValue, TContext>> contributors)
    {
        if (contributors.Length == 0) return original;

        TValue max = contributors[index: 0].Contribute(original, context);

        for (var index = 1; index < contributors.Length; index++)
        {
            TValue contribution = contributors[index].Contribute(original, context);
            if (contribution > max) max = contribution;
        }

        return max;
    }
}
