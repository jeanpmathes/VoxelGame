// <copyright file="Chunk.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using System;
using System.Threading.Tasks;
using VoxelGame.WorldGeneration;

namespace VoxelGame.Logic
{
    public class Chunk
    {
        public const int ChunkHeight = 32;

        private const int maxMeshDataStep = 4;

        /// <summary>
        /// The X position of this chunk in chunk units
        /// </summary>
        public int X { get; }

        /// <summary>
        /// The Y position of this chunk in chunk units
        /// </summary>
        public int Z { get; }

        private readonly Section[] sections = new Section[ChunkHeight];

        private bool hasMeshData = false;
        private int meshDataIndex = 0;

        public Chunk(int x, int z)
        {
            X = x;
            Z = z;

            for (int y = 0; y < ChunkHeight; y++)
            {
                sections[y] = new Section();
            }
        }

        public void Generate(IWorldGenerator generator)
        {
            if (generator == null)
            {
                throw new ArgumentNullException(paramName: nameof(generator));
            }

            for (int x = 0; x < Section.SectionSize; x++)
            {
                for (int z = 0; z < Section.SectionSize; z++)
                {
                    int y = 0;

                    foreach (Block block in generator.GenerateColumn(x + (X * Section.SectionSize), z + (Z * Section.SectionSize)))
                    {
                        sections[y >> 5][x, y & (Section.SectionSize - 1), z] = block;

                        y++;
                    }
                }
            }
        }

        public Task GenerateAsync(IWorldGenerator generator)
        {
            return Task.Run(() => Generate(generator));
        }

        public void CreateMesh()
        {
            for (int y = 0; y < ChunkHeight; y++)
            {
                sections[y].CreateMesh(X, y, Z);
            }

            hasMeshData = true;
            meshDataIndex = 0;
        }

        public Task<(float[][] verticesData, uint[][] indicesData)> CreateMeshDataAsync()
        {
            return Task.Run(CreateMeshData);
        }

        private (float[][] verticesData, uint[][] indicesData) CreateMeshData()
        {
            float[][] verticesData = new float[ChunkHeight][];
            uint[][] indicesData = new uint[ChunkHeight][];

            for (int y = 0; y < ChunkHeight; y++)
            {
                sections[y].CreateMeshData(X, y, Z, out verticesData[y], out indicesData[y]);
            }

            meshDataIndex = 0;

            return (verticesData, indicesData);
        }

        public void SetMeshData(float[][] verticesData, uint[][] indicesData)
        {
            if (verticesData == null)
            {
                throw new ArgumentNullException(nameof(verticesData));
            }

            if (indicesData == null)
            {
                throw new ArgumentNullException(nameof(indicesData));
            }

            for (int y = 0; y < ChunkHeight; y++)
            {
                sections[y].SetMeshData(ref verticesData[y], ref indicesData[y]);
            }

            hasMeshData = true;
            meshDataIndex = 0;
        }

        public bool SetMeshDataStep(float[][] verticesData, uint[][] indicesData)
        {
            if (verticesData == null)
            {
                throw new ArgumentNullException(nameof(verticesData));
            }

            if (indicesData == null)
            {
                throw new ArgumentNullException(nameof(indicesData));
            }

            for (int i = 0; i < maxMeshDataStep; i++)
            {
                sections[meshDataIndex].SetMeshData(ref verticesData[meshDataIndex], ref indicesData[meshDataIndex]);

                // The index has reached the end, all sections have received their mesh data
                if (meshDataIndex == ChunkHeight - 1)
                {
                    hasMeshData = true;
                    meshDataIndex = 0;

                    return true;
                }
                else
                {
                    meshDataIndex++;
                }
            }

            return false;
        }

        public void CreateMesh(int y)
        {
            sections[y].CreateMesh(X, y, Z);
        }

        public void Render()
        {
            if (hasMeshData)
            {
                for (int y = 0; y < ChunkHeight; y++)
                {
                    sections[y].Render(new Vector3(X * Section.SectionSize, y * Section.SectionSize, Z * Section.SectionSize));
                }
            }
        }

        public Section GetSection(int y)
        {
            return sections[y];
        }
    }
}