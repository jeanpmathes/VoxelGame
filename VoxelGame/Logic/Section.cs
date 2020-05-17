// <copyright file="Section.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using VoxelGame.Rendering;
using VoxelGame.WorldGeneration;

namespace VoxelGame.Logic
{
    [Serializable]
    public class Section : IDisposable
    {
        public const int SectionSize = 32;
        public const int TickBatchSize = 4;

        public const int BlockMask = 0b0000_0000_0000_0000_0000_0111_1111_1111;
        public const int DataMask = 0b0000_0000_0000_0000_1111_1000_0000_0000;

        private readonly ushort[] blocks;

        [NonSerialized] private bool isEmpty;
        [NonSerialized] private SectionRenderer renderer;

        public Section()
        {
            blocks = new ushort[SectionSize * SectionSize * SectionSize];

            Setup();
        }

        /// <summary>
        /// Sets up all non serialized members.
        /// </summary>
        public void Setup()
        {
            renderer = new SectionRenderer();
        }

        public void Generate(IWorldGenerator generator, int xOffset, int yOffset, int zOffset)
        {
            if (generator == null)
            {
                throw new ArgumentNullException(paramName: nameof(generator));
            }

            for (int x = 0; x < SectionSize; x++)
            {
                for (int y = 0; y < SectionSize; y++)
                {
                    for (int z = 0; z < SectionSize; z++)
                    {
                        blocks[(x << 10) + (y << 5) + z] = generator.GenerateBlock(x + xOffset, y + yOffset, z + zOffset).Id;
                    }
                }
            }
        }

        public void CreateMesh(int sectionX, int sectionY, int sectionZ)
        {
            CreateMeshData(sectionX, sectionY, sectionZ, out float[] vertices, out int[] textureIndices, out uint[] indices);
            SetMeshData(ref vertices, ref textureIndices, ref indices);
        }

        public void CreateMeshData(int sectionX, int sectionY, int sectionZ, out float[] verticesData, out int[] textureIndicesData, out uint[] indicesData)
        {
            // Get the sections next to this section
            Section frontNeighbour = Game.World.GetSection(sectionX, sectionY, sectionZ + 1);
            Section backNeighbour = Game.World.GetSection(sectionX, sectionY, sectionZ - 1);
            Section leftNeighbour = Game.World.GetSection(sectionX - 1, sectionY, sectionZ);
            Section rightNeighbour = Game.World.GetSection(sectionX + 1, sectionY, sectionZ);
            Section bottomNeighbour = Game.World.GetSection(sectionX, sectionY - 1, sectionZ);
            Section topNeighbour = Game.World.GetSection(sectionX, sectionY + 1, sectionZ);

            // Recalculate the mesh and set the buffers
            List<float> vertices = new List<float>(4096);
            List<int> textureIndices = new List<int>(512);
            List<uint> indices = new List<uint>(1024);

            uint vertCount = 0;

            for (int x = 0; x < SectionSize; x++)
            {
                for (int y = 0; y < SectionSize; y++)
                {
                    for (int z = 0; z < SectionSize; z++)
                    {
                        ushort currentBlockData = blocks[(x << 10) + (y << 5) + z];

                        Block currentBlock = Block.TranslateID((ushort)(currentBlockData & BlockMask));
                        byte currentData = (byte)((currentBlockData & DataMask) >> 11);

                        if (currentBlock.IsFull) // Check if this block is sized 1x1x1
                        {
                            Block blockToCheck;

                            // Check all six sides of this block

                            // Front
                            if (z + 1 >= SectionSize && frontNeighbour != null)
                            {
                                blockToCheck = frontNeighbour.GetBlock(x, y, 0);
                            }
                            else if (z + 1 >= SectionSize)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = GetBlock(x, y, z + 1);
                            }

                            if (blockToCheck != null && (!blockToCheck.IsFull || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques))))
                            {
                                uint additionalVertCount = currentBlock.GetMesh(BlockSide.Front, currentData, out float[] sideVertices, out int[] sideTextureIndices, out uint[] sideIndices);

                                vertices.AddRange(sideVertices);
                                textureIndices.AddRange(sideTextureIndices);
                                indices.AddRange(sideIndices);

                                for (int i = 0; i < sideVertices.Length; i += 5) // Add the position to the vertices
                                {
                                    vertices[((int)vertCount * 5) + i + 0] += x;
                                    vertices[((int)vertCount * 5) + i + 1] += y;
                                    vertices[((int)vertCount * 5) + i + 2] += z;
                                }

                                for (int i = 0; i < sideIndices.Length; i++) // Add the additionalVertCount count to the indices
                                {
                                    indices[indices.Count - sideIndices.Length + i] += vertCount;
                                }

                                vertCount += additionalVertCount;
                            }

                            // Back
                            if (z - 1 < 0 && backNeighbour != null)
                            {
                                blockToCheck = backNeighbour.GetBlock(x, y, SectionSize - 1);
                            }
                            else if (z - 1 < 0)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = GetBlock(x, y, z - 1);
                            }

                            if (blockToCheck != null && (!blockToCheck.IsFull || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques))))
                            {
                                uint additionalVertCount = currentBlock.GetMesh(BlockSide.Back, currentData, out float[] sideVertices, out int[] sideTextureIndices, out uint[] sideIndices);

                                vertices.AddRange(sideVertices);
                                textureIndices.AddRange(sideTextureIndices);
                                indices.AddRange(sideIndices);

                                for (int i = 0; i < sideVertices.Length; i += 5) // Add the position to the vertices
                                {
                                    vertices[((int)vertCount * 5) + i + 0] += x;
                                    vertices[((int)vertCount * 5) + i + 1] += y;
                                    vertices[((int)vertCount * 5) + i + 2] += z;
                                }

                                for (int i = 0; i < sideIndices.Length; i++) // Add the additionalVertCount count to the indices
                                {
                                    indices[indices.Count - sideIndices.Length + i] += vertCount;
                                }

                                vertCount += additionalVertCount;
                            }

                            // Left
                            if (x - 1 < 0 && leftNeighbour != null)
                            {
                                blockToCheck = leftNeighbour.GetBlock(SectionSize - 1, y, z);
                            }
                            else if (x - 1 < 0)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = GetBlock(x - 1, y, z);
                            }

                            if (blockToCheck != null && (!blockToCheck.IsFull || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques))))
                            {
                                uint additionalVertCount = currentBlock.GetMesh(BlockSide.Left, currentData, out float[] sideVertices, out int[] sideTextureIndices, out uint[] sideIndices);

                                vertices.AddRange(sideVertices);
                                textureIndices.AddRange(sideTextureIndices);
                                indices.AddRange(sideIndices);

                                for (int i = 0; i < sideVertices.Length; i += 5) // Add the position to the vertices
                                {
                                    vertices[((int)vertCount * 5) + i + 0] += x;
                                    vertices[((int)vertCount * 5) + i + 1] += y;
                                    vertices[((int)vertCount * 5) + i + 2] += z;
                                }

                                for (int i = 0; i < sideIndices.Length; i++) // Add the additionalVertCount count to the indices
                                {
                                    indices[indices.Count - sideIndices.Length + i] += vertCount;
                                }

                                vertCount += additionalVertCount;
                            }

                            // Right
                            if (x + 1 >= SectionSize && rightNeighbour != null)
                            {
                                blockToCheck = rightNeighbour.GetBlock(0, y, z);
                            }
                            else if (x + 1 >= SectionSize)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = GetBlock(x + 1, y, z);
                            }

                            if (blockToCheck != null && (!blockToCheck.IsFull || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques))))
                            {
                                uint additionalVertCount = currentBlock.GetMesh(BlockSide.Right, currentData, out float[] sideVertices, out int[] sideTextureIndices, out uint[] sideIndices);

                                vertices.AddRange(sideVertices);
                                textureIndices.AddRange(sideTextureIndices);
                                indices.AddRange(sideIndices);

                                for (int i = 0; i < sideVertices.Length; i += 5) // Add the position to the vertices
                                {
                                    vertices[((int)vertCount * 5) + i + 0] += x;
                                    vertices[((int)vertCount * 5) + i + 1] += y;
                                    vertices[((int)vertCount * 5) + i + 2] += z;
                                }

                                for (int i = 0; i < sideIndices.Length; i++) // Add the additionalVertCount count to the indices
                                {
                                    indices[indices.Count - sideIndices.Length + i] += vertCount;
                                }

                                vertCount += additionalVertCount;
                            }

                            // Bottom
                            if (y - 1 < 0 && bottomNeighbour != null)
                            {
                                blockToCheck = bottomNeighbour.GetBlock(x, SectionSize - 1, z);
                            }
                            else if (y - 1 < 0)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = GetBlock(x, y - 1, z);
                            }

                            if (blockToCheck?.IsFull != true || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques)))
                            {
                                uint additionalVertCount = currentBlock.GetMesh(BlockSide.Bottom, currentData, out float[] sideVertices, out int[] sideTextureIndices, out uint[] sideIndices);

                                vertices.AddRange(sideVertices);
                                textureIndices.AddRange(sideTextureIndices);
                                indices.AddRange(sideIndices);

                                for (int i = 0; i < sideVertices.Length; i += 5) // Add the position to the vertices
                                {
                                    vertices[((int)vertCount * 5) + i + 0] += x;
                                    vertices[((int)vertCount * 5) + i + 1] += y;
                                    vertices[((int)vertCount * 5) + i + 2] += z;
                                }

                                for (int i = 0; i < sideIndices.Length; i++) // Add the additionalVertCount count to the indices
                                {
                                    indices[indices.Count - sideIndices.Length + i] += vertCount;
                                }

                                vertCount += additionalVertCount;
                            }

                            // Top
                            if (y + 1 >= SectionSize && topNeighbour != null)
                            {
                                blockToCheck = topNeighbour.GetBlock(x, 0, z);
                            }
                            else if (y + 1 >= SectionSize)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = GetBlock(x, y + 1, z);
                            }

                            if (blockToCheck?.IsFull != true || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques)))
                            {
                                uint additionalVertCount = currentBlock.GetMesh(BlockSide.Top, currentData, out float[] sideVertices, out int[] sideTextureIndices, out uint[] sideIndices);

                                vertices.AddRange(sideVertices);
                                textureIndices.AddRange(sideTextureIndices);
                                indices.AddRange(sideIndices);

                                for (int i = 0; i < sideVertices.Length; i += 5) // Add the position to the vertices
                                {
                                    vertices[((int)vertCount * 5) + i + 0] += x;
                                    vertices[((int)vertCount * 5) + i + 1] += y;
                                    vertices[((int)vertCount * 5) + i + 2] += z;
                                }

                                for (int i = 0; i < sideIndices.Length; i++) // Add the additionalVertCount count to the indices
                                {
                                    indices[indices.Count - sideIndices.Length + i] += vertCount;
                                }

                                vertCount += additionalVertCount;
                            }
                        }
                        else
                        {
                            uint additionalVertCount = currentBlock.GetMesh(BlockSide.All, currentData, out float[] addVertices, out int[] addTextureIndices, out uint[] addIndices);

                            if (additionalVertCount != 0)
                            {
                                vertices.AddRange(addVertices);
                                textureIndices.AddRange(addTextureIndices);
                                indices.AddRange(addIndices);

                                for (int i = 0; i < addVertices.Length; i += 5) // Add the position to the vertices
                                {
                                    vertices[((int)vertCount * 5) + i + 0] += x;
                                    vertices[((int)vertCount * 5) + i + 1] += y;
                                    vertices[((int)vertCount * 5) + i + 2] += z;
                                }

                                for (int i = 0; i < addIndices.Length; i++) // Add the additionalVertCount count to the indices
                                {
                                    indices[indices.Count - addIndices.Length + i] += vertCount;
                                }

                                vertCount += additionalVertCount;
                            }
                        }
                    }
                }
            }

            isEmpty = (vertices.Count == 0);

            verticesData = vertices.ToArray();
            textureIndicesData = textureIndices.ToArray();
            indicesData = indices.ToArray();
        }

        public void SetMeshData(ref float[] vertices, ref int[] textureIndices, ref uint[] indices)
        {
            renderer.SetData(ref vertices, ref textureIndices, ref indices);
        }

        public void Render(Vector3 position)
        {
            if (!isEmpty)
            {
                renderer.Draw(position);
            }
        }

        public void Tick()
        {
            for (int i = 0; i < TickBatchSize; i++)
            {
                int index = Game.Random.Next(0, SectionSize * SectionSize * SectionSize);
                ushort val = blocks[index];

                int z = index & 31;
                index = (index - z) >> 5;
                int y = index & 31;
                index = (index - y) >> 5;
                int x = index;

                Block.TranslateID((ushort)(val & BlockMask))?.RandomUpdate(x, y, z, (byte)((val & DataMask) >> 11));
            }
        }

        /// <summary>
        /// Gets or sets the block at a section position.
        /// </summary>
        /// <param name="x">The x position of the block in this section.</param>
        /// <param name="y">The y position of the block in this section.</param>
        /// <param name="z">The z position of the block in this section.</param>
        /// <returns>The block at the given position.</returns>
        public ushort this[int x, int y, int z]
        {
            get
            {
                return blocks[(x << 10) + (y << 5) + z];
            }

            set
            {
                blocks[(x << 10) + (y << 5) + z] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Block GetBlock(int x, int y, int z)
        {
            return Block.TranslateID((ushort)(this[x, y, z] & BlockMask));
        }

        #region IDisposable Support

        [NonSerialized] private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    renderer?.Dispose();
                }

                disposed = true;
            }
        }

        ~Section()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}