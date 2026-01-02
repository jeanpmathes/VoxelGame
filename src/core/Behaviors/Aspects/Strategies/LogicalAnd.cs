// <copyright file="LogicalAnd.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Behaviors.Aspects.Strategies;

/// <summary>
///     Combines contributions by ANDing them together.
/// </summary>
/// <typeparam name="TContext">The context in which the aspect is evaluated.</typeparam>
public class LogicalAnd<TContext> : IContributionStrategy<Boolean, TContext>
{
    /// <inheritdoc />
    public static Int32 MaxContributorCount => Int32.MaxValue;

    /// <inheritdoc />
    public Boolean CombineContributions(Boolean original, TContext context, Span<IContributor<Boolean, TContext>> contributors)
    {
        Boolean result = original;

        foreach (IContributor<Boolean, TContext> contributor in contributors) result &= contributor.Contribute(result, context);

        return result;
    }
}
