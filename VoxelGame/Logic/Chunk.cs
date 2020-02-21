// <copyright file="Chunk.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;

using VoxelGame.WorldGeneration;

namespace VoxelGame.Logic
{
    public class Chunk
    {
        public const int ChunkHeight = 32;

        /// <summary>
        /// The X position of this chunk in chunk units
        /// </summary>
        public int X { get; }

        /// <summary>
        /// The Y position of this chunk in chunk units
        /// </summary>
        public int Z { get; }

        private Section[] sections = new Section[ChunkHeight];

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
            for (int x = 0; x < Section.SectionSize; x++)
            {
                for (int z = 0; z < Section.SectionSize; z++)
                {
                    int y = 0;

                    foreach (Block block in generator.GenerateColumn(x + X * Section.SectionSize, z + Z * Section.SectionSize))
                    {
                        sections[y >> 5][x, y & (Section.SectionSize - 1), z] = block;

                        y++;
                    }
                }
            }
        }

        public void CreateMesh()
        {
            for (int y = 0; y < ChunkHeight; y++)
            {
                sections[y].CreateMesh(X, y, Z);
            }
        }

        public void Render()
        {
            for (int y = 0; y < ChunkHeight; y++)
            {
                sections[y].Render(new Vector3(X * Section.SectionSize, y * Section.SectionSize, Z * Section.SectionSize));
            }
        }

        public Section GetSection(int y)
        {
            return sections[y];
        }
    }
}