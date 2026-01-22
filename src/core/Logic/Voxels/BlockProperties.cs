// <copyright file="BlockProperties.cs" company="VoxelGame">
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
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;

namespace VoxelGame.Core.Logic.Voxels;

/// <summary>
///     Used to determine basic boolean aspects of a block.
/// </summary>
public class BlockProperties(Block subject)
{
    /// <summary>
    ///     Whether it is possible to see through this block.
    ///     Note that this only indicates whether the actual filled portion of the block is opaque.
    ///     If the block is not full, it is possible to see around the block.
    /// </summary>
    public Aspect<Boolean, Block> IsOpaque { get; } = Aspect<Boolean, Block>.New<LogicalAnd<Block>>(nameof(IsOpaque), subject);

    /// <summary>
    ///     This aspect is only relevant for non-opaque full blocks. It decides if their faces should be meshed next to
    ///     another non-opaque block.
    /// </summary>
    public Aspect<Boolean, Block> MeshFaceAtNonOpaques { get; } = Aspect<Boolean, Block>.New<ORing<Block>>(nameof(MeshFaceAtNonOpaques), subject);

    /// <summary>
    ///     Whether this block hinders movement.
    /// </summary>
    public Aspect<Boolean, Block> IsSolid { get; } = Aspect<Boolean, Block>.New<LogicalAnd<Block>>(nameof(IsSolid), subject);

    /// <summary>
    ///     Gets whether this block is unshaded, which means it does not receive shadows.
    /// </summary>
    public Aspect<Boolean, Block> IsUnshaded { get; } = Aspect<Boolean, Block>.New<ORing<Block>>(nameof(IsUnshaded), subject);

    /// <summary>
    ///     Whether this block is considered empty.
    /// </summary>
    public Aspect<Boolean, Block> IsEmpty { get; } = Aspect<Boolean, Block>.New<Exclusive<Boolean, Block>>(nameof(IsEmpty), subject);
}
