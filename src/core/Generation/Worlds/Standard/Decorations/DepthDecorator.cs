// <copyright file="DepthDecorator.cs" company="VoxelGame">
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
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation.Worlds.Standard.Decorations;

/// <summary>
///     Selects positions between a minimum and maximum depth.
/// </summary>
public class DepthDecorator : Decorator
{
    private readonly Int32 maxDepth;
    private readonly Int32 minDepth;

    /// <summary>
    ///     Creates a new depth decorator.
    /// </summary>
    /// <param name="minDepth">The minimum depth.</param>
    /// <param name="maxDepth">The maximum depth, must be greater than the minimum depth.</param>
    public DepthDecorator(Int32 minDepth, Int32 maxDepth)
    {
        this.minDepth = minDepth;
        this.maxDepth = maxDepth;

        Debug.Assert(minDepth < maxDepth);
    }

    /// <inheritdoc />
    public override Boolean CanPlace(Vector3i position, in Decoration.PlacementContext context, IReadOnlyGrid grid)
    {
        return context.Depth >= minDepth && context.Depth <= maxDepth;
    }
}
