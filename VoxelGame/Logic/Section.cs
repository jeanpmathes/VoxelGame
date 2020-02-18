// <copyright file="Section.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using VoxelGame.Rendering;

namespace VoxelGame.Logic
{
    public class Section
    {
        public const int SectionSize = 32;

        private ushort[] blocks;

        private int vertexBufferObject;
        private int elementBufferObject;
        private int vertexArrayObject;

        private uint[] indicesAll;

        public Section()
        {
            blocks = new ushort[SectionSize * SectionSize * SectionSize];

            vertexBufferObject = GL.GenBuffer();
            elementBufferObject = GL.GenBuffer();
            vertexArrayObject = GL.GenVertexArray();

            Block current;
            for (int x = 0; x < SectionSize; x++)
            {
                for (int y = 0; y < SectionSize; y++)
                {
                    if (y > 25)
                    {
                        current = Game.AIR;
                    }
                    else if (y == 25)
                    {
                        if (x > 16)
                        {
                            current = Game.SAND;
                        }
                        else
                        {
                            current = Game.GRASS;
                        }
                    }
                    else if (y > 20)
                    {
                        current = Game.DIRT;
                    }
                    else
                    {
                        current = Game.STONE;
                    }

                    for (int z = 0; z < SectionSize; z++)
                    {
                        this[x, y, z] = current;
                    }
                }
            }

            this[5, 26, 5] = Game.LOG;
            this[5, 27, 5] = Game.LOG;
            this[5, 28, 5] = Game.LOG;
            this[5, 29, 5] = Game.LOG;
            this[5, 30, 5] = Game.LEAVES;
            this[4, 29, 5] = Game.LEAVES;
            this[6, 29, 5] = Game.LEAVES;
            this[4, 29, 4] = Game.LEAVES;
            this[6, 29, 4] = Game.LEAVES;
            this[4, 29, 6] = Game.LEAVES;
            this[6, 29, 6] = Game.LEAVES;

            this[9, 26, 8] = Game.COBBLESTONE;
            this[9, 26, 7] = Game.COBBLESTONE;
            this[9, 26, 6] = Game.COBBLESTONE;
            this[9, 27, 8] = Game.GLASS;
            this[9, 27, 7] = Game.GLASS;
            this[9, 27, 6] = Game.GLASS;

            this[10, 27, 8] = Game.GLASS;
            this[10, 27, 7] = Game.GLASS;
            this[10, 27, 6] = Game.GLASS;

            this[9, 28, 6] = Game.LEAVES;
            this[12, 26, 12] = Game.ORE_IRON;
            this[13, 26, 12] = Game.STONE;
            this[14, 26, 12] = Game.ORE_GOLD;
            this[15, 26, 12] = Game.STONE;
            this[16, 26, 12] = Game.ORE_COAL;
            this[12, 26, 13] = Game.TALL_GRASS;
            this[13, 26, 13] = Game.TALL_GRASS;
            this[14, 26, 13] = Game.TALL_GRASS;
            this[15, 26, 13] = Game.TALL_GRASS;
            this[16, 26, 13] = Game.TALL_GRASS;

            this[12, 25, 16] = Game.AIR;
            this[13, 25, 16] = Game.TALL_GRASS;
            this[14, 25, 16] = Game.AIR;
            this[15, 25, 16] = Game.TALL_GRASS;
            this[16, 25, 16] = Game.AIR;

            this[12, 25, 14] = Game.AIR;
            this[13, 25, 14] = Game.TALL_GRASS;
            this[14, 25, 14] = Game.AIR;
            this[15, 25, 14] = Game.AIR;
            this[16, 25, 14] = Game.AIR;

            this[12, 25, 15] = Game.AIR;
            this[13, 25, 15] = Game.AIR;
            this[14, 25, 15] = Game.AIR;
            this[15, 25, 15] = Game.TALL_GRASS;
            this[16, 25, 15] = Game.AIR;

            this[17, 17, 31] = Game.AIR;
            this[17, 18, 31] = Game.AIR;
            this[17, 16, 31] = Game.AIR;
            this[18, 17, 31] = Game.AIR;
            this[17, 17, 30] = Game.AIR;
            this[16, 17, 31] = Game.AIR;

            this[29, 25, 31] = Game.AIR;
            this[28, 25, 31] = Game.AIR;
            this[29, 24, 31] = Game.AIR;
            this[29, 23, 31] = Game.AIR;
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

                        Block currentBlock = Game.blockDictionary[(ushort)(currentBlockData & 0b0000_1111_1111)];
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

                            if (blockToCheck?.IsFull != true || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques)))
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

                            if (blockToCheck?.IsFull != true || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques)))
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

                            if (blockToCheck?.IsFull != true || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques)))
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

                            if (blockToCheck?.IsFull != true || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques)))
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

            float[] verteciesAll = vertices.ToArray();
            indicesAll = indices.ToArray();

            // Vertex Buffer Object
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, verteciesAll.Length * sizeof(float), verteciesAll, BufferUsageHint.StaticDraw);

            // Element Buffer Object
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indicesAll.Length * sizeof(uint), indicesAll, BufferUsageHint.StaticDraw);

            Game.Shader.Use();

            // Vertex Array Object
            GL.BindVertexArray(vertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);

            int vertexLocation = Game.Shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            int texCoordLocation = Game.Shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            GL.BindVertexArray(0);
        }

        public void Render(Vector3 position)
        {
            GL.BindVertexArray(vertexArrayObject);

            Game.Shader.Use();
            Game.Atlas.Use();

            Matrix4 model = Matrix4.Identity * Matrix4.CreateTranslation(position);
            Game.Shader.SetMatrix4("model", model);
            Game.Shader.SetMatrix4("view", Game.MainCamera.GetViewMatrix());
            Game.Shader.SetMatrix4("projection", Game.MainCamera.GetProjectionMatrix());

            GL.DrawElements(PrimitiveType.Triangles, indicesAll.Length, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
            GL.UseProgram(0);
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
                return Game.blockDictionary[(ushort)(blocks[(x << 10) + (y << 5) + z] & 0b0000_1111_1111)];
            }

            set
            {
                blocks[(x << 10) + (y << 5) + z] = value.Id;
            }
        }
    }
}