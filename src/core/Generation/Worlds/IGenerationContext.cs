// <copyright file="IGenerationContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Sections;

namespace VoxelGame.Core.Generation.Worlds;

/// <summary>
///     Context in which a unit of generation work is executed.
/// </summary>
public interface IGenerationContext : IDisposable
{
    /// <summary>
    ///     The generator that created this context.
    /// </summary>
    public IWorldGenerator Generator { get; }

    /// <summary>
    ///     Generate a column of the world.
    /// </summary>
    /// <param name="x">The x position of the world.</param>
    /// <param name="z">The z position of the world.</param>
    /// <param name="heightRange">The height range (inclusive, exclusive) in which blocks should be generated.</param>
    /// <returns>The data in the column.</returns>
    IEnumerable<Content> GenerateColumn(Int32 x, Int32 z, (Int32 start, Int32 end) heightRange);

    /// <summary>
    ///     Generate all structures in a section.
    /// </summary>
    /// <param name="section">The section to generate structures in.</param>
    void GenerateStructures(Section section);

    /// <summary>
    ///     Generate a chunk in this context.
    /// </summary>
    /// <param name="chunk">The chunk to generate.</param>
    void Generate(Chunk chunk)
    {
        ChunkPosition chunkPosition = chunk.Position;

        (Int32 begin, Int32 end) range = (
            chunkPosition.Y * Chunk.BlockSize,
            (chunkPosition.Y + 1) * Chunk.BlockSize);

        for (var x = 0; x < Chunk.BlockSize; x++)
        for (var z = 0; z < Chunk.BlockSize; z++)
        {
            Int32 y = range.begin;

            foreach (Content content in GenerateColumn(
                         x + chunkPosition.X * Chunk.BlockSize,
                         z + chunkPosition.Z * Chunk.BlockSize,
                         range))
            {
                Vector3i blockPosition = (x, y, z);

                Content modifiedContent = content.Block.Block.GeneratorUpdate(content);

                UInt32 encodedContent = Section.Encode(
                    modifiedContent.Block.Block,
                    modifiedContent.Block.Data,
                    modifiedContent.Fluid.Fluid,
                    modifiedContent.Fluid.Level,
                    modifiedContent.Fluid.IsStatic);

                chunk.GetSection(blockPosition).SetContent(blockPosition, encodedContent);

                y++;
            }
        }

        for (var index = 0; index < Chunk.SectionCount; index++)
        {
            (Int32 x, Int32 y, Int32 z) = Chunk.IndexToLocalSection(index);

            Section section = chunk.GetLocalSection(x, y, z);

            GenerateStructures(section);
        }
    }
}
