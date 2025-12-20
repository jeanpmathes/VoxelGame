// <copyright file="Chaining.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
