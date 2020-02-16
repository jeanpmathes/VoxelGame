// <copyright file="Chunk.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;

namespace VoxelGame.Logic
{
    public class Chunk
    {
        public const int ChunkHeight = 32;

        /// <summary>
        /// The X position of this chunk in chunk units
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// The Y position of this chunk in chunk units
        /// </summary>
        public int Z { get; private set; }

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