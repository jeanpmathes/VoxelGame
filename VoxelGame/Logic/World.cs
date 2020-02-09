﻿// <copyright file="World.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using System;
using System.Collections.Generic;

namespace VoxelGame.Logic
{
    public class World
    {
        public const int chunkExtents = 4;

        public readonly int sectionSizeExp = (int)Math.Log(Section.SectionSize, 2);
        public readonly int chunkHeightExp = (int)Math.Log(Chunk.ChunkHeight, 2);

        private Dictionary<ValueTuple<int, int>, Chunk> activeChunks = new Dictionary<ValueTuple<int, int>, Chunk>();
        private List<Chunk> chunksToMesh = new List<Chunk>();
        private List<Chunk> chunksToRender = new List<Chunk>();

        public World()
        {
            for (int x = 0; x < chunkExtents; x++)
            {
                for (int z = 0; z < chunkExtents; z++)
                {
                    activeChunks.Add((x, z), new Chunk(x, z));
                }
            }

            chunksToMesh.AddRange(activeChunks.Values);

            // Mesh the listed chunks
            for (int i = 0; i < chunksToMesh.Count; i++)
            {
                chunksToMesh[i].CreateMesh();
            }

            chunksToMesh.Clear();
        }

        public void FrameUpdate()
        {
            // Mesh the listed chunks
            for (int i = 0; i < chunksToMesh.Count; i++)
            {
                chunksToMesh[i].CreateMesh();
            }

            chunksToMesh.Clear();

            // Collect all chunks to render
            chunksToRender.AddRange(activeChunks.Values);

            // Render the listed chunks
            for (int i = 0; i < chunksToRender.Count; i++)
            {
                chunksToRender[i].Render();
            }

            chunksToRender.Clear();
        }

        /// <summary>
        /// Returns the block at a given position in block coordinates. The block is only searched in active chunks.
        /// </summary>
        /// <param name="x">The x position in block coordinates.</param>
        /// <param name="y">The y position in block coordinates.</param>
        /// <param name="z">The z position in block coordinates.</param>
        /// <returns>The Block at x, y, z or null if the block was not found</returns>
        public Block GetBlock(int x, int y, int z)
        {
            return activeChunks[(x << sectionSizeExp, z << sectionSizeExp)]
                .GetSection(y << chunkHeightExp)
                .GetBlock(x & (Section.SectionSize - 1), y & (Section.SectionSize - 1), z & (Section.SectionSize - 1));
        }

        public void SetBlock(Block block, int x, int y, int z)
        {
            activeChunks[(x << sectionSizeExp, z << sectionSizeExp)]
                .GetSection(y << chunkHeightExp)
                .SetBlock(block, x & (Section.SectionSize - 1), y & (Section.SectionSize - 1), z & (Section.SectionSize - 1));
        }

        public Chunk GetChunk(int x, int z)
        {
            return activeChunks[(x, z)];
        }

        public Section GetSection(int x, int y, int z)
        {
            return activeChunks[(x, z)]
                .GetSection(y);
        }
    }
}