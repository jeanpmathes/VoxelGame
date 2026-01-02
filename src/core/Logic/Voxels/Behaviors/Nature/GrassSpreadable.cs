// <copyright file="GrassSpreadable.cs" company="VoxelGame">
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
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature;

/// <summary>
///     Blocks which can receive grass spread from a <see cref="Grass" /> block.
/// </summary>
public partial class GrassSpreadable : BlockBehavior, IBehavior<GrassSpreadable, BlockBehavior, Block>
{
    [Constructible]
    private GrassSpreadable(Block subject) : base(subject) {}

    /// <summary>
    ///     Spreads grass on the block. This operation does not always succeed.
    /// </summary>
    /// <param name="world">The world the block is in.</param>
    /// <param name="position">The position of the block.</param>
    /// <param name="grass">The grass block that is spreading.</param>
    /// <returns>True when the grass block successfully spread.</returns>
    public Boolean SpreadGrass(World world, Vector3i position, Block grass)
    {
        if (world.GetBlock(position)?.Block != Subject || CoveredSoil.CanHaveCover(world, position) != true) return false;

        world.SetBlock(new State(grass), position);

        return true;
    }
}
