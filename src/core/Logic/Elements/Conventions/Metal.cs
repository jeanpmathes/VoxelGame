// <copyright file="Metal.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements.Conventions;

/// <summary>
///     A metal type, as defined by the <see cref="MetalConvention" />.
/// </summary>
public class Metal(String namedID, BlockBuilder builder) : Convention<Metal>(namedID, builder)
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
    /// <param name="namedID">The named ID of the metal, used to create the block IDs.</param>
    /// <param name="oreBlocks">The ore blocks, with their natural names and named IDs.</param>
    /// <param name="nativeMetalBlocks">The native metal blocks, with their natural names and named IDs.</param>
    /// <returns>The created metal type.</returns>
    public static Metal BuildMetal(this BlockBuilder b, String namedID, IEnumerable<(String name, String namedID)> oreBlocks, IEnumerable<(String name, String namedID)> nativeMetalBlocks)
    {
        return b.BuildConvention<Metal>(builder =>
        {
            List<Block> ores = [];

            String texture = namedID.PascalCaseToSnakeCase();

            foreach ((String blockName, String blockNamedID) in oreBlocks)
            {
                var blockTexture = $"{texture}_{blockNamedID.PascalCaseToSnakeCase()}";

                ores.Add(
                    builder.BuildSimpleBlock(blockNamedID, blockName)
                        .WithTextureLayout(TextureLayout.Uniform(TID.Block(blockTexture)))
                        .Complete()
                );
            }

            List<Block> nativeMetals = [];

            foreach ((String blockName, String blockNamedID) in nativeMetalBlocks)
            {
                var blockTexture = $"{texture}_{blockNamedID.PascalCaseToSnakeCase()}";

                nativeMetals.Add(
                    builder.BuildSimpleBlock(blockNamedID, blockName)
                        .WithTextureLayout(TextureLayout.Uniform(TID.Block(blockTexture)))
                        .Complete()
                );
            }

            return new Metal(namedID, builder)
            {
                Ores = ores,
                NativeMetals = nativeMetals
            };
        });
    }
}
