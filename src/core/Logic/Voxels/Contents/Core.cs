// <copyright file="Core.cs" company="VoxelGame">
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

using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels.Behaviors;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Contents;

/// <summary>
///     These blocks are the most essential blocks in the game.
///     The game relies on these blocks to exist and on their IDs to be fixed.
/// </summary>
public class Core(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     The air block that fills the world. Could also be interpreted as "no block".
    /// </summary>
    public Block Air { get; } = builder
        .BuildUnmeshedBlock(new CID(nameof(Air)), Language.Air)
        .WithBehavior<Static>()
        .WithBehavior<Fillable>()
        .WithProperties(flags => flags.IsSolid.ContributeConstant(value: false))
        .WithProperties(flags => flags.IsEmpty.ContributeConstant(value: true))
        .WithBehavior<Replaceable>()
        .WithValidation((block, validator) =>
        {
            if (block.BlockID != 0)
                validator.ReportError($"Block {block} must have block ID 0");
        })
        .Complete();

    /// <summary>
    ///     An error block, used as fallback when structure operations fail.
    /// </summary>
    public Block Error { get; } = builder
        .BuildSimpleBlock(new CID(nameof(Error)), Language.Error)
        .WithTextureLayout(TextureLayout.Uniform(TID.MissingTexture))
        .Complete();

    /// <summary>
    ///     The core of the world, which is found at the lowest level.
    /// </summary>
    public Block CoreBlock { get; } = builder
        .BuildSimpleBlock(new CID(nameof(CoreBlock)), Language.Core)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("core")))
        .Complete();

    /// <summary>
    ///     A block that serves as a neutral choice for development purposes.
    /// </summary>
    public Block Dev { get; } = builder
        .BuildSimpleBlock(new CID(nameof(Dev)), Language.DevBlock)
        .WithTextureLayout(TextureLayout.Uniform(TID.Block("dev")))
        .Complete();
}
