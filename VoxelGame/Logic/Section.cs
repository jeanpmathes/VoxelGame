using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

using VoxelGame.Rendering;

namespace VoxelGame.Logic
{
    public class Section
    {
        public const int sectionSize = 32;

        private Block[,,] blocks;

        private int vertexBufferObject;
        private int elementBufferObject;
        private int vertexArrayObject;

        private uint[] indiciesAll;

        private bool hasChanged = true;

        public Section()
        {
            blocks = new Block[sectionSize, sectionSize, sectionSize];

            vertexBufferObject = GL.GenBuffer();
            elementBufferObject = GL.GenBuffer();
            vertexArrayObject = GL.GenVertexArray();

            Block current;
            for (int x = 0; x < sectionSize; x++)
            {
                for (int y = 0; y < sectionSize; y++)
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

                    for (int z = 0; z < sectionSize; z++)
                    {
                        blocks[x, y, z] = current;
                    }
                }
            }

            blocks[5, 26, 5] = Game.LOG;
            blocks[5, 27, 5] = Game.LOG;
            blocks[9, 26, 8] = Game.COBBLESTONE;
            blocks[9, 26, 7] = Game.COBBLESTONE;
            blocks[9, 26, 6] = Game.COBBLESTONE;
            blocks[12, 26, 12] = Game.ORE_IRON;
            blocks[13, 26, 12] = Game.STONE;
            blocks[14, 26, 12] = Game.ORE_GOLD;
            blocks[15, 26, 12] = Game.STONE;
            blocks[16, 26, 12] = Game.ORE_COAL;
        }

        public void Render(Vector3 position)
        {
            // Recalculate the mesh and set the buffers
            if (hasChanged)
            {
                List<float> vertecies = new List<float>();
                List<uint> indicies = new List<uint>();

                uint vertCount = 0;

                for (int x = 0; x < sectionSize; x++)
                {
                    for (int y = 0; y < sectionSize; y++)
                    {
                        for (int z = 0; z < sectionSize; z++)
                        {
                            if (blocks[x, y, z].IsSolid) // Check if this block is solid
                            {
                                // Check all six sides of this block
                                if (z + 1 >= sectionSize || !blocks[x, y, z + 1].IsSolid) // Front
                                {
                                    uint additionalVertCount = blocks[x, y, z].GetMesh(BlockSide.Front, out float[] sideVertecies, out uint[] sideIndicies);

                                    vertecies.AddRange(sideVertecies);
                                    indicies.AddRange(sideIndicies);

                                    for (int i = 0; i < sideVertecies.Length; i += 5) // Add the position to the vertecies
                                    {
                                        vertecies[(int)vertCount * 5 + i + 0] += x;
                                        vertecies[(int)vertCount * 5 + i + 1] += y;
                                        vertecies[(int)vertCount * 5 + i + 2] += z;
                                    }

                                    for (int i = 0; i < sideIndicies.Length; i++) // Add the additionalVertCount count to the indicies
                                    {
                                        indicies[indicies.Count - sideIndicies.Length + i] += vertCount;
                                    }

                                    vertCount += additionalVertCount;
                                }

                                if (z - 1 < 0 || !blocks[x, y, z - 1].IsSolid) // Back
                                {
                                    uint additionalVertCount = blocks[x, y, z].GetMesh(BlockSide.Back, out float[] sideVertecies, out uint[] sideIndicies);

                                    vertecies.AddRange(sideVertecies);
                                    indicies.AddRange(sideIndicies);

                                    for (int i = 0; i < sideVertecies.Length; i += 5) // Add the position to the vertecies
                                    {
                                        vertecies[(int)vertCount * 5 + i + 0] += x;
                                        vertecies[(int)vertCount * 5 + i + 1] += y;
                                        vertecies[(int)vertCount * 5 + i + 2] += z;
                                    }

                                    for (int i = 0; i < sideIndicies.Length; i++) // Add the additionalVertCount count to the indicies
                                    {
                                        indicies[indicies.Count - sideIndicies.Length + i] += vertCount;
                                    }

                                    vertCount += additionalVertCount;
                                }

                                if (x - 1 < 0 || !blocks[x - 1, y, z].IsSolid) // Left
                                {
                                    uint additionalVertCount = blocks[x, y, z].GetMesh(BlockSide.Left, out float[] sideVertecies, out uint[] sideIndicies);

                                    vertecies.AddRange(sideVertecies);
                                    indicies.AddRange(sideIndicies);

                                    for (int i = 0; i < sideVertecies.Length; i += 5) // Add the position to the vertecies
                                    {
                                        vertecies[(int)vertCount * 5 + i + 0] += x;
                                        vertecies[(int)vertCount * 5 + i + 1] += y;
                                        vertecies[(int)vertCount * 5 + i + 2] += z;
                                    }

                                    for (int i = 0; i < sideIndicies.Length; i++) // Add the additionalVertCount count to the indicies
                                    {
                                        indicies[indicies.Count - sideIndicies.Length + i] += vertCount;
                                    }

                                    vertCount += additionalVertCount;
                                }

                                if (x + 1 >= sectionSize || !blocks[x + 1, y, z].IsSolid) // Right
                                {
                                    uint additionalVertCount = blocks[x, y, z].GetMesh(BlockSide.Right, out float[] sideVertecies, out uint[] sideIndicies);

                                    vertecies.AddRange(sideVertecies);
                                    indicies.AddRange(sideIndicies);

                                    for (int i = 0; i < sideVertecies.Length; i += 5) // Add the position to the vertecies
                                    {
                                        vertecies[(int)vertCount * 5 + i + 0] += x;
                                        vertecies[(int)vertCount * 5 + i + 1] += y;
                                        vertecies[(int)vertCount * 5 + i + 2] += z;
                                    }

                                    for (int i = 0; i < sideIndicies.Length; i++) // Add the additionalVertCount count to the indicies
                                    {
                                        indicies[indicies.Count - sideIndicies.Length + i] += vertCount;
                                    }

                                    vertCount += additionalVertCount;
                                }

                                if (y - 1 < 0 || !blocks[x, y - 1, z].IsSolid) // Bottom
                                {
                                    uint additionalVertCount = blocks[x, y, z].GetMesh(BlockSide.Bottom, out float[] sideVertecies, out uint[] sideIndicies);

                                    vertecies.AddRange(sideVertecies);
                                    indicies.AddRange(sideIndicies);

                                    for (int i = 0; i < sideVertecies.Length; i += 5) // Add the position to the vertecies
                                    {
                                        vertecies[(int)vertCount * 5 + i + 0] += x;
                                        vertecies[(int)vertCount * 5 + i + 1] += y;
                                        vertecies[(int)vertCount * 5 + i + 2] += z;
                                    }

                                    for (int i = 0; i < sideIndicies.Length; i++) // Add the additionalVertCount count to the indicies
                                    {
                                        indicies[indicies.Count - sideIndicies.Length + i] += vertCount;
                                    }

                                    vertCount += additionalVertCount;
                                }

                                if (y + 1 >= sectionSize || !blocks[x, y + 1, z].IsSolid) // Top
                                {
                                    uint additionalVertCount = blocks[x, y, z].GetMesh(BlockSide.Top, out float[] sideVertecies, out uint[] sideIndicies);

                                    vertecies.AddRange(sideVertecies);
                                    indicies.AddRange(sideIndicies);

                                    for (int i = 0; i < sideVertecies.Length; i += 5) // Add the position to the vertecies
                                    {
                                        vertecies[(int)vertCount * 5 + i + 0] += x;
                                        vertecies[(int)vertCount * 5 + i + 1] += y;
                                        vertecies[(int)vertCount * 5 + i + 2] += z;
                                    }

                                    for (int i = 0; i < sideIndicies.Length; i++) // Add the additionalVertCount count to the indicies
                                    {
                                        indicies[indicies.Count - sideIndicies.Length + i] += vertCount;
                                    }

                                    vertCount += additionalVertCount;
                                }
                            }
                            else
                            {
                                uint additionalVertCount = blocks[x, y, z].GetMesh(BlockSide.All, out float[] sideVertecies, out uint[] sideIndicies);

                                if (additionalVertCount != 0)
                                {
                                    vertecies.AddRange(sideVertecies);
                                    indicies.AddRange(sideIndicies);

                                    for (int i = 0; i < sideVertecies.Length; i += 5) // Add the position to the vertecies
                                    {
                                        vertecies[(int)vertCount * 5 + i + 0] += x;
                                        vertecies[(int)vertCount * 5 + i + 1] += y;
                                        vertecies[(int)vertCount * 5 + i + 2] += z;
                                    }

                                    for (int i = 0; i < sideIndicies.Length; i++) // Add the additionalVertCount count to the indicies
                                    {
                                        indicies[indicies.Count - sideIndicies.Length + i] += vertCount;
                                    }

                                    vertCount += additionalVertCount;
                                }
                            }
                        }
                    }

                    hasChanged = false;
                }

                float[] verteciesAll = vertecies.ToArray();
                indiciesAll = indicies.ToArray();

                // Vertex Buffer Object
                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
                GL.BufferData(BufferTarget.ArrayBuffer, verteciesAll.Length * sizeof(float), verteciesAll, BufferUsageHint.StaticDraw);

                // Element Buffer Object
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indiciesAll.Length * sizeof(uint), indiciesAll, BufferUsageHint.StaticDraw);

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
            }
            
            GL.BindVertexArray(vertexArrayObject);

            Game.Shader.Use();
            Game.Atlas.Use();

            Matrix4 model = Matrix4.Identity * Matrix4.CreateTranslation(position);
            Game.Shader.SetMatrix4("model", model);
            Game.Shader.SetMatrix4("view", Game.MainCamera.GetViewMatrix());
            Game.Shader.SetMatrix4("projection", Game.MainCamera.GetProjectionMatrix());

            GL.DrawElements(PrimitiveType.Triangles, indiciesAll.Length, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }
    }
}
