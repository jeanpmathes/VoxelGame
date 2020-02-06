using System;
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

        private Section[] sections = new Section[32];
        
        public Chunk(int x, int z)
        {
            X = x;
            Z = z;

            for (int i = 0; i < ChunkHeight; i++)
            {
                sections[i] = new Section();
            }
        }

        public void CreateMesh()
        {
            for (int i = 0; i < ChunkHeight; i++)
            {
                sections[i].CreateMesh(new Vector3(X * Section.SectionSize, i * Section.SectionSize, Z * Section.SectionSize));
            }
        }

        public void Render()
        {
            for (int i = 0; i < ChunkHeight; i++)
            {
                sections[i].Render(new Vector3(X * Section.SectionSize, i * Section.SectionSize, Z * Section.SectionSize));
            }
        }
    }
}
