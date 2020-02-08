// <copyright file="World.cs" company="VoxelGame">
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

        public Block GetBlock(int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public void SetBlock(Block block, int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public Chunk GetChunk(int x, int y, int z)
        {
            throw new NotImplementedException();
        }

        public Section GetSection(int x, int y, int z)
        {
            throw new NotImplementedException();
        }
    }
}