// <copyright file="Average.cs" company="VoxelGame">
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
