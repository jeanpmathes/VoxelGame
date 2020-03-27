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
        private HashSet<Chunk> chunksToGenerate = new HashSet<Chunk>();

        /// <summary>
        /// For newly created chunks.
        /// </summary>
        private HashSet<Chunk> chunksToMesh = new HashSet<Chunk>();

        /// <summary>
        /// For sections of already meshed chunks.
        /// </summary>
        private HashSet<(Chunk chunk, int index)> sectionsToMesh = new HashSet<(Chunk chunk, int index)>();

        private HashSet<Chunk> chunksToRender = new HashSet<Chunk>();

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

            chunksToGenerate.UnionWith(activeChunks.Values);
            chunksToMesh.UnionWith(activeChunks.Values);
        }

        public void FrameRender()
        {
            // Collect all chunks to generate

            // Generate all listed chunks
            foreach (Chunk chunk in chunksToGenerate)
            {
                chunk.Generate(generator);
            }

            chunksToGenerate.Clear();

            // Collect all chunks to mesh

            // Mesh the listed chunks
            foreach (Chunk chunk in chunksToMesh)
            {
                chunk.CreateMesh();
            }

            chunksToMesh.Clear();

            // Mesh all listed sections
            foreach ((Chunk chunk, int index) meshInstruction in sectionsToMesh)
            {
                meshInstruction.chunk.CreateMesh(meshInstruction.index);
            }

            sectionsToMesh.Clear();

            // Collect all chunks to render
            chunksToRender.UnionWith(activeChunks.Values);

            // Render the listed chunks
            foreach (Chunk chunk in chunksToRender)
            {
                chunk.Render();
            }

            chunksToRender.Clear();

            // Render the player
            Game.Player.Render();
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

                sectionsToMesh.Add((chunk, y >> chunkHeightExp));

                // Check if sections next to this section have to be changed

                // Next on y axis
                if ((y & (Section.SectionSize - 1)) == 0 && (y - 1 >> chunkHeightExp) >= 0)
                {
                    sectionsToMesh.Add((chunk, y - 1 >> chunkHeightExp));
                }
                else if ((y & (Section.SectionSize - 1)) == Section.SectionSize - 1 && (y + 1 >> chunkHeightExp) < Chunk.ChunkHeight)
                {
                    sectionsToMesh.Add((chunk, y + 1 >> chunkHeightExp));
                }

                // Next on x axis
                if ((x & (Section.SectionSize - 1)) == 0 && activeChunks.TryGetValue((x - 1 >> sectionSizeExp, z >> sectionSizeExp), out chunk))
                {
                    sectionsToMesh.Add((chunk, y >> chunkHeightExp));
                }
                else if ((x & (Section.SectionSize - 1)) == Section.SectionSize - 1 && activeChunks.TryGetValue((x + 1 >> sectionSizeExp, z >> sectionSizeExp), out chunk))
                {
                    sectionsToMesh.Add((chunk, y >> chunkHeightExp));
                }

                // Next on z axis
                if ((z & (Section.SectionSize - 1)) == 0 && activeChunks.TryGetValue((x >> sectionSizeExp, z - 1 >> sectionSizeExp), out chunk))
                {
                    sectionsToMesh.Add((chunk, y >> chunkHeightExp));
                }
                else if ((z & (Section.SectionSize - 1)) == Section.SectionSize - 1 && activeChunks.TryGetValue((x >> sectionSizeExp, z + 1 >> sectionSizeExp), out chunk))
                {
                    sectionsToMesh.Add((chunk, y >> chunkHeightExp));
                }
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