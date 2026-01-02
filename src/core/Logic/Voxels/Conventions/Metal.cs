// <copyright file="Metal.cs" company="VoxelGame">
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
using System.Collections.Generic;
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Conventions;

/// <summary>
///     A metal type, as defined by the <see cref="MetalConvention" />.
/// </summary>
public class Metal(CID contentID, BlockBuilder builder) : Convention<Metal>(contentID, builder)
{
    /// <summary>
    ///     Ores of this metal type which can be mined in the world.
    /// </summary>
    public required IEnumerable<Block> Ores { get; init; }

    /// <summary>
    ///     Blocks for native metals, which can be found in the world.
    /// </summary>
    public required IEnumerable<Block> NativeMetals { get; init; }
}

/// <summary>
///     A convention for metal types.
/// </summary>
public static class MetalConvention
{
    /// <summary>
    ///     Builds a new metal type.
    /// </summary>
    /// <param name="b">The block builder to use.</param>
    /// <param name="contentID">The content ID of the metal, used to create the block CIDs.</param>
    /// <param name="oreBlocks">The ore blocks, with their natural names and named IDs.</param>
    /// <param name="nativeMetalBlocks">The native metal blocks, with their natural names and named IDs.</param>
    /// <returns>The created metal type.</returns>
    public static Metal BuildMetal(this BlockBuilder b, CID contentID, IEnumerable<(CID contentID, String name)> oreBlocks, IEnumerable<(CID contentID, String name)> nativeMetalBlocks)
    {
        return b.BuildConvention<Metal>(builder =>
        {
            List<Block> ores = [];

            String texture = contentID.Identifier.PascalCaseToSnakeCase();

            foreach ((CID blockContentIDs, String blockName) in oreBlocks)
            {
                var blockTexture = $"{texture}_{blockContentIDs.Identifier.PascalCaseToSnakeCase()}";

                ores.Add(
                    builder.BuildSimpleBlock(blockContentIDs, blockName)
                        .WithTextureLayout(TextureLayout.Uniform(TID.Block(blockTexture)))
                        .Complete()
                );
            }

            List<Block> nativeMetals = [];

            foreach ((CID blockContentIDs, String blockName) in nativeMetalBlocks)
            {
                var blockTexture = $"{texture}_{blockContentIDs.Identifier.PascalCaseToSnakeCase()}";

                nativeMetals.Add(
                    builder.BuildSimpleBlock(blockContentIDs, blockName)
                        .WithTextureLayout(TextureLayout.Uniform(TID.Block(blockTexture)))
                        .Complete()
                );
            }

            return new Metal(contentID, builder)
            {
                Ores = ores,
                NativeMetals = nativeMetals
            };
        });
    }
}
