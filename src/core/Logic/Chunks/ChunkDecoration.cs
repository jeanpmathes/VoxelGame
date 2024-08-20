﻿// <copyright file="ChunkDecoration.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Chunks;

/// <summary>
///     Indicates what parts of the chunk have been decorated.
/// </summary>
[Flags]
public enum DecorationLevels
{
    /// <summary>
    ///     No decoration has been applied.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Only the center of the chunk has been decorated.
    /// </summary>
    Center = 1 << 0,

    /// <summary />
    Corner000 = 1 << 1,

    /// <summary />
    Corner001 = 1 << 2,

    /// <summary />
    Corner010 = 1 << 3,

    /// <summary />
    Corner011 = 1 << 4,

    /// <summary />
    Corner100 = 1 << 5,

    /// <summary />
    Corner101 = 1 << 6,

    /// <summary />
    Corner110 = 1 << 7,

    /// <summary />
    Corner111 = 1 << 8,

    /// <summary>
    ///     Indicates that all parts of the chunk have been decorated.
    /// </summary>
    All = Center | Corner000 | Corner001 | Corner010 | Corner011 | Corner100 | Corner101 | Corner110 | Corner111
}

/// <summary>
///     Helpers defining the decoration system for chunks.
///     Decoration is the step of chunk creation where it is filled with elements like trees.
///     It follows the step of generation where the terrain is created.
/// </summary>
public static class ChunkDecoration
{
    // todo: check if decoration only acquires chunks that are needed (considering both sides) - not the case, as the neighborhood is not nullable
    // todo: try looking for the wrong section access bug

    private static readonly Vector3i[] corners = VMath.Range3(x: 2, y: 2, z: 2).Select(corner => (Vector3i) corner).ToArray();

    private static readonly (Int32, Int32, Int32)[] centerSectionOffsets = VMath.Range3((1, 1, 1), (2, 2, 2)).ToArray();
    private static readonly (Int32, Int32, Int32)[] cornerSectionOffsets = VMath.Range3(x: 4, y: 4, z: 4).Where(IsNotCubeTip).ToArray();

    private static readonly (Vector3i, DecorationLevels)[][] cornerPositions =
        VMath.Range3(x: 2, y: 2, z: 2)
            .Select(corner => (Vector3i) corner)
            .OrderBy(GetCornerIndex)
            .Select(corner =>
                VMath.Range3(x: 2, y: 2, z: 2)
                    .Select(offset => (Vector3i) offset)
                    .Select(offset => (
                        corner + offset, // Localized position of the chunk.
                        GetFlagForCorner(Vector3i.One - offset))) // Flag - inverse is necessary, e.g. chunk at 0,0,0 decorates the corner at 1,1,1 .
                    .ToArray())
            .ToArray();

    /// <summary>
    ///     Decorate a chunk and parts of its neighbors, except the center of the chunk.
    ///     Assumes that the chunk has been generated and the center already decorated.
    /// </summary>
    /// <param name="neighbors">The neighborhood of chunks around the chunk to decorate.</param>
    public static void Decorate(Neighborhood<Chunk> neighbors)
    {
        Debug.Assert(neighbors.Center.Decoration.HasFlag(DecorationLevels.Center));

        IWorldGenerator generator = neighbors.Center.Context.Generator;

        foreach (Vector3i corner in corners)
        {
            if (IsCornerDecorated(corner, neighbors)) continue;

            DecorateCorner(generator, neighbors, corner);
        }
    }

    /// <summary>
    ///     Decorate the center of the chunk.
    ///     Assumes that the chunk has been generated but not decorated yet.
    /// </summary>
    /// <param name="chunk">The chunk to decorate.</param>
    public static void DecorateCenter(Chunk chunk)
    {
        Debug.Assert(chunk.Decoration == DecorationLevels.None);

        chunk.AddDecorationLevel(DecorationLevels.Center);

        var neighbors = new Neighborhood<Section>();

        foreach ((Int32 x, Int32 y, Int32 z) in centerSectionOffsets)
        {
            foreach ((Int32 dx, Int32 dy, Int32 dz) in Neighborhood.Indices)
                neighbors[dx, dy, dz] = chunk.GetLocalSection(x + dx - 1, y + dy - 1, z + dz - 1);

            chunk.Context.Generator.DecorateSection(SectionPosition.From(chunk.Position, (x, y, z)), neighbors);
        }
    }

    /// <summary>
    ///     Decorate a corner of the given local neighborhood of chunks.
    /// </summary>
    /// <param name="generator">The generator to use for decoration.</param>
    /// <param name="chunks">The local neighborhood of chunks.</param>
    /// <param name="corner">The corner to decorate.</param>
    private static void DecorateCorner(IWorldGenerator generator, Neighborhood<Chunk> chunks, Vector3i corner)
    {
        Neighborhood<Boolean> decorated = new();

        foreach ((Vector3i position, DecorationLevels flag) in GetCornerPositions(corner))
        {
            decorated[position] = chunks[position].Decoration.HasFlag(flag);
            chunks[position].AddDecorationLevel(flag);
        }

        // Go through all sections on the selected corner.
        // We want to decorate 56 of them, which is a cube of 4x4x4 without the tips (corners).
        // The tips of this cube are the centers of the chunks - the cube overlaps with multiple chunks.

        ChunkPosition firstLocalChunk = chunks.Center.Position.Offset(-Vector3i.One);
        SectionPosition firstSectionInCorner = SectionPosition.From(chunks.GetAt(corner).Position, (2, 2, 2));

        Neighborhood<Section> sections = new();

        foreach ((Int32 dx, Int32 dy, Int32 dz) in cornerSectionOffsets)
        {
            SectionPosition currentSectionInCorner = firstSectionInCorner.Offset(dx, dy, dz);
            Vector3i currentChunkOffset = firstLocalChunk.OffsetTo(currentSectionInCorner.Chunk);

            if (decorated[currentChunkOffset])
                continue;

            FillSectionNeighborhood(sections, firstLocalChunk, currentSectionInCorner, chunks);
            generator.DecorateSection(currentSectionInCorner, sections);
        }

        Debug.Assert(IsCornerDecorated(corner, chunks));
    }

    private static void FillSectionNeighborhood(
        Array3D<Section> neighborhood,
        ChunkPosition firstLocalChunk, SectionPosition centerSection,
        Array3D<Chunk> chunks)
    {
        foreach ((Int32 dx, Int32 dy, Int32 dz) in Neighborhood.Indices)
        {
            SectionPosition currentSection = centerSection.Offset(dx - 1, dy - 1, dz - 1);
            Vector3i currentChunkOffset = firstLocalChunk.OffsetTo(currentSection.Chunk);

            neighborhood[dx, dy, dz] = chunks[currentChunkOffset].GetSection(currentSection);
        }
    }

    /// <summary>
    ///     Whether a position is NOT a tip (corner) of a 4x4x4 cube.
    /// </summary>
    private static Boolean IsNotCubeTip((Int32, Int32, Int32) position)
    {
        return position switch
        {
            (0, 0, 0) => false,
            (0, 0, 3) => false,
            (0, 3, 0) => false,
            (0, 3, 3) => false,
            (3, 0, 0) => false,
            (3, 0, 3) => false,
            (3, 3, 0) => false,
            (3, 3, 3) => false,
            _ => true
        };
    }

    /// <summary>
    ///     Check if a corner is fully decorated, meaning all chunks that are part of the corner have the corresponding flag
    ///     set.
    /// </summary>
    /// <param name="corner">The corner to check.</param>
    /// <param name="chunks">All local chunks.</param>
    /// <returns>True if the corner is fully decorated, false otherwise.</returns>
    private static Boolean IsCornerDecorated(Vector3i corner, Array3D<Chunk> chunks)
    {
        foreach ((Vector3i position, DecorationLevels flag) in GetCornerPositions(corner))
            if (!chunks.GetAt(position).Decoration.HasFlag(flag))
                return false;

        return true;
    }

    private static Int32 GetCornerIndex(Vector3i corner)
    {
        return corner.X + corner.Y * 2 + corner.Z * 2 * 2;
    }

    /// <summary>
    ///     Get the localized positions of the chunks that make up a corner.
    ///     Additionally, get the flag each chunk gains from decorating that corner.
    /// </summary>
    /// <param name="corner">The corner to get the positions for.</param>
    /// <returns>The positions and corresponding decoration levels.</returns>
    private static (Vector3i, DecorationLevels)[] GetCornerPositions(Vector3i corner)
    {
        return cornerPositions[GetCornerIndex(corner)];
    }

    /// <summary>
    ///     Get the decoration level flag for a given corner.
    /// </summary>
    private static DecorationLevels GetFlagForCorner(Vector3i corner)
    {
        return (corner.X, corner.Y, corner.Z) switch
        {
            (0, 0, 0) => DecorationLevels.Corner000,
            (0, 0, 1) => DecorationLevels.Corner001,
            (0, 1, 0) => DecorationLevels.Corner010,
            (0, 1, 1) => DecorationLevels.Corner011,
            (1, 0, 0) => DecorationLevels.Corner100,
            (1, 0, 1) => DecorationLevels.Corner101,
            (1, 1, 0) => DecorationLevels.Corner110,
            (1, 1, 1) => DecorationLevels.Corner111,
            _ => throw new ArgumentOutOfRangeException(nameof(corner), corner, message: null)
        };
    }
}