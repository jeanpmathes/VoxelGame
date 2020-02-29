// <copyright file="World.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using System;
using System.Collections.Generic;

using VoxelGame.WorldGeneration;

namespace VoxelGame.Logic
{
    public class World
    {
        public const int ChunkExtents = 5;

        private readonly int sectionSizeExp = (int)Math.Log(Section.SectionSize, 2);
        private readonly int chunkHeightExp = (int)Math.Log(Chunk.ChunkHeight, 2);

        private IWorldGenerator generator;

        private Dictionary<ValueTuple<int, int>, Chunk> activeChunks = new Dictionary<ValueTuple<int, int>, Chunk>();
        private List<Chunk> chunksToGenerate = new List<Chunk>();
        private List<Chunk> chunksToMesh = new List<Chunk>();
        private List<Chunk> chunksToRender = new List<Chunk>();

        public World(IWorldGenerator generator)
        {
            this.generator = generator;

            for (int x = ChunkExtents / -2; x < ChunkExtents / 2 + 1; x++)
            {
                for (int z = ChunkExtents / -2; z < ChunkExtents / 2 + 1; z++)
                {
                    activeChunks.Add((x, z), new Chunk(x, z));
                }
            }

            chunksToGenerate.AddRange(activeChunks.Values);
            chunksToMesh.AddRange(activeChunks.Values);
        }

        public void FrameRender()
        {
            // Collect all chunks to generate

            // Generate all listed chunks
            for (int i = 0; i < chunksToGenerate.Count; i++)
            {
                chunksToGenerate[i].Generate(generator);
            }

            chunksToGenerate.Clear();

            // Collect all chunks to mesh

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

        public void FrameUpdate(float deltaTime)
        {
            Game.Player.Tick(deltaTime);
        }

        /// <summary>
        /// Returns the block at a given position in block coordinates. The block is only searched in active chunks.
        /// </summary>
        /// <param name="x">The x position in block coordinates.</param>
        /// <param name="y">The y position in block coordinates.</param>
        /// <param name="z">The z position in block coordinates.</param>
        /// <returns>The Block at x, y, z or null if the block was not found.</returns>
        public Block GetBlock(int x, int y, int z)
        {
            if (activeChunks.TryGetValue((x >> sectionSizeExp, z >> sectionSizeExp), out Chunk chunk) && y >= 0 && y < Chunk.ChunkHeight * Section.SectionSize)
            {
                return chunk.GetSection(y >> chunkHeightExp)
                    [x & (Section.SectionSize - 1), y & (Section.SectionSize - 1), z & (Section.SectionSize - 1)];
            }
            else
            {
                return null;
            }
        }

        public void SetBlock(Block block, int x, int y, int z)
        {
            if (activeChunks.TryGetValue((x >> sectionSizeExp, z >> sectionSizeExp), out Chunk chunk) && y >= 0 && y < Chunk.ChunkHeight * Section.SectionSize)
            {
                chunk.GetSection(y >> chunkHeightExp)
                    [x & (Section.SectionSize - 1), y & (Section.SectionSize - 1), z & (Section.SectionSize - 1)] = block;
            }
        }

        public Chunk GetChunk(int x, int z)
        {
            activeChunks.TryGetValue((x, z), out Chunk chunk);
            return chunk;
        }

        public Section GetSection(int x, int y, int z)
        {
            if (activeChunks.TryGetValue((x, z), out Chunk chunk) && y >= 0 && y < Chunk.ChunkHeight)
            {
                return chunk.GetSection(y);
            }
            else
            {
                return null;
            }
        }
    }
}