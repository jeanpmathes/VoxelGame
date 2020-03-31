// <copyright file="Section.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using System.Collections.Generic;

using VoxelGame.Rendering;
using VoxelGame.WorldGeneration;

namespace VoxelGame.Logic
{
    public class Section
    {
        public const int SectionSize = 32;

        private ushort[] blocks;

        private SectionRenderer renderer;

        public Section()
        {
            blocks = new ushort[SectionSize * SectionSize * SectionSize];

            renderer = new SectionRenderer();
        }

        public void Generate(IWorldGenerator generator, int xOffset, int yOffset, int zOffset)
        {
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
            // Get the sections next to this section
            Section frontNeighbour = Game.World.GetSection(sectionX, sectionY, sectionZ + 1);
            Section backNeighbour = Game.World.GetSection(sectionX, sectionY, sectionZ - 1);
            Section leftNeighbour = Game.World.GetSection(sectionX - 1, sectionY, sectionZ);
            Section rightNeighbour = Game.World.GetSection(sectionX + 1, sectionY, sectionZ);
            Section bottomNeighbour = Game.World.GetSection(sectionX, sectionY - 1, sectionZ);
            Section topNeighbour = Game.World.GetSection(sectionX, sectionY + 1, sectionZ);

            // Recalculate the mesh and set the buffers
            List<float> vertices = new List<float>();
            List<uint> indices = new List<uint>();

            uint vertCount = 0;

            for (int x = 0; x < SectionSize; x++)
            {
                for (int y = 0; y < SectionSize; y++)
                {
                    for (int z = 0; z < SectionSize; z++)
                    {
                        ushort currentBlockData = blocks[(x << 10) + (y << 5) + z];

                        Block currentBlock = Block.blockDictionary[(ushort)(currentBlockData & 0b0000_1111_1111)];
                        ushort currentData = (byte)((currentBlockData & 0b1111_0000_0000) >> 8);

                        if (currentBlock.IsFull) // Check if this block is sized 1x1x1
                        {
                            Block blockToCheck;

                            // Check all six sides of this block

                            // Front
                            if (z + 1 >= SectionSize && frontNeighbour != null)
                            {
                                blockToCheck = frontNeighbour[x, y, 0];
                            }
                            else if (z + 1 >= SectionSize)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = this[x, y, z + 1];
                            }

                            if (blockToCheck != null && (!blockToCheck.IsFull || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques))))
                            {
                                uint additionalVertCount = currentBlock.GetMesh(BlockSide.Front, currentData, out float[] sideVertices, out uint[] sideIndices);

                                vertices.AddRange(sideVertices);
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
                                blockToCheck = backNeighbour[x, y, SectionSize - 1];
                            }
                            else if (z - 1 < 0)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = this[x, y, z - 1];
                            }

                            if (blockToCheck != null && (!blockToCheck.IsFull || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques))))
                            {
                                uint additionalVertCount = currentBlock.GetMesh(BlockSide.Back, currentData, out float[] sideVertices, out uint[] sideIndices);

                                vertices.AddRange(sideVertices);
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
                                blockToCheck = leftNeighbour[SectionSize - 1, y, z];
                            }
                            else if (x - 1 < 0)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = this[x - 1, y, z];
                            }

                            if (blockToCheck != null && (!blockToCheck.IsFull || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques))))
                            {
                                uint additionalVertCount = currentBlock.GetMesh(BlockSide.Left, currentData, out float[] sideVertices, out uint[] sideIndices);

                                vertices.AddRange(sideVertices);
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
                                blockToCheck = rightNeighbour[0, y, z];
                            }
                            else if (x + 1 >= SectionSize)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = this[x + 1, y, z];
                            }

                            if (blockToCheck != null && (!blockToCheck.IsFull || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques))))
                            {
                                uint additionalVertCount = currentBlock.GetMesh(BlockSide.Right, currentData, out float[] sideVertices, out uint[] sideIndices);

                                vertices.AddRange(sideVertices);
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
                                blockToCheck = bottomNeighbour[x, SectionSize - 1, z];
                            }
                            else if (y - 1 < 0)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = this[x, y - 1, z];
                            }

                            if (blockToCheck?.IsFull != true || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques)))
                            {
                                uint additionalVertCount = currentBlock.GetMesh(BlockSide.Bottom, currentData, out float[] sideVertices, out uint[] sideIndices);

                                vertices.AddRange(sideVertices);
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
                                blockToCheck = topNeighbour[x, 0, z];
                            }
                            else if (y + 1 >= SectionSize)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = this[x, y + 1, z];
                            }

                            if (blockToCheck?.IsFull != true || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques)))
                            {
                                uint additionalVertCount = currentBlock.GetMesh(BlockSide.Top, currentData, out float[] sideVertices, out uint[] sideIndices);

                                vertices.AddRange(sideVertices);
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
                            uint additionalVertCount = currentBlock.GetMesh(BlockSide.All, currentData, out float[] sideVertices, out uint[] sideIndices);

                            if (additionalVertCount != 0)
                            {
                                vertices.AddRange(sideVertices);
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
                    }
                }
            }

            float[] verticesAll = vertices.ToArray();
            uint[] indicesAll = indices.ToArray();

            renderer.SetData(ref verticesAll, ref indicesAll);
        }

        public void Render(Vector3 position)
        {
            renderer.Draw(position);
        }

        /// <summary>
        /// Returns or sets the block at a section position.
        /// </summary>
        /// <param name="x">The x position of the block in this section.</param>
        /// <param name="y">The y position of the block in this section.</param>
        /// <param name="z">The z position of the block in this section.</param>
        /// <returns>The block at the given position.</returns>
        public Block this[int x, int y, int z]
        {
            get
            {
                return Block.blockDictionary[(ushort)(blocks[(x << 10) + (y << 5) + z] & 0b0000_1111_1111)];
            }

            set
            {
                blocks[(x << 10) + (y << 5) + z] = value.Id;
            }
        }
    }
}