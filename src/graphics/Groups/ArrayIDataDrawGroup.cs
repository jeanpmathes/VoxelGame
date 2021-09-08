// <copyright file="ArrayIDataDrawGroup.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Graphics.OpenGL4;

namespace VoxelGame.Graphics.Groups
{
    public class ArrayIDataDrawGroup : IDrawGroup
    {
        private readonly int size;
        private readonly int vao;

        private readonly int vbo;

        private int vertexCount;

        private ArrayIDataDrawGroup(int size)
        {
            this.size = size;

            GL.CreateBuffers(1, out vbo);
            GL.CreateVertexArrays(1, out vao);
        }

        public bool IsFilled { get; private set; }

        public void BindVertexArray()
        {
            GL.BindVertexArray(vao);
        }

        public void Draw()
        {
            DrawArrays();
        }

        public static ArrayIDataDrawGroup Create(int size)
        {
            return new(size);
        }

        public void SetData(int vertexDataCount, int[] vertexData)
        {
            vertexCount = vertexDataCount / size;

            if (vertexCount == 0)
            {
                IsFilled = false;

                return;
            }

            IsFilled = true;

            GL.NamedBufferData(vbo, vertexDataCount * sizeof(int), vertexData, BufferUsageHint.DynamicDraw);
        }

        public void VertexArrayBindBuffer()
        {
            GL.VertexArrayVertexBuffer(vao, 0, vbo, IntPtr.Zero, size * sizeof(int));
        }

        public void VertexArrayAttributeBinding(int attribute)
        {
            GL.EnableVertexArrayAttrib(vao, attribute);
            GL.VertexArrayAttribIFormat(vao, attribute, size, VertexAttribType.Int, 0 * sizeof(int));
            GL.VertexArrayAttribBinding(vao, attribute, 0);
        }

        public void DrawArrays()
        {
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);
        }

        public void Delete()
        {
            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
        }
    }
}