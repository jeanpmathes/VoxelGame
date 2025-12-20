// <copyright file="Masking.cs" company="VoxelGame">
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
using System.Globalization;

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
        var result = Convert.ToInt64(original, CultureInfo.InvariantCulture);

        foreach (IContributor<TValue, TContext> contributor in contributors)
        {
            TValue contribution = contributor.Contribute(original, context);
            result &= Convert.ToInt64(contribution, CultureInfo.InvariantCulture);
        }

        return (TValue) Enum.ToObject(typeof(TValue), result);
    }
}
