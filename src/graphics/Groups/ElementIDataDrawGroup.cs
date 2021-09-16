// <copyright file="ElementDrawGroup.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Graphics.OpenGL4;

namespace VoxelGame.Graphics.Groups
{
    public class ElementIDataDrawGroup : IDrawGroup
    {
        private readonly int ebo;
        private readonly int size;
        private readonly int vao;

        private readonly int vbo;

        private int elementCount;

        private ElementIDataDrawGroup(int size)
        {
            this.size = size;

            GL.CreateBuffers(n: 1, out vbo);
            GL.CreateBuffers(n: 1, out ebo);
            GL.CreateVertexArrays(n: 1, out vao);
        }

        public bool IsFilled { get; private set; }

        public void BindVertexArray()
        {
            GL.BindVertexArray(vao);
        }

        public void Draw()
        {
            DrawElements();
        }

        public static ElementIDataDrawGroup Create(int size)
        {
            return new(size);
        }

        public void SetData(int vertexDataCount, int[] vertexData, int elementDataCount, uint[] elementData)
        {
            elementCount = elementDataCount;

            if (elementCount == 0)
            {
                IsFilled = false;

                return;
            }

            IsFilled = true;

            GL.NamedBufferData(vbo, vertexDataCount * sizeof(int), vertexData, BufferUsageHint.DynamicDraw);
            GL.NamedBufferData(ebo, elementDataCount * sizeof(uint), elementData, BufferUsageHint.DynamicDraw);
        }

        public void VertexArrayBindBuffer()
        {
            GL.VertexArrayVertexBuffer(vao, bindingindex: 0, vbo, IntPtr.Zero, size * sizeof(int));
            GL.VertexArrayElementBuffer(vao, ebo);
        }

        public void VertexArrayAttributeBinding(int attribute)
        {
            GL.EnableVertexArrayAttrib(vao, attribute);
            GL.VertexArrayAttribIFormat(vao, attribute, size, VertexAttribType.Int, 0 * sizeof(int));
            GL.VertexArrayAttribBinding(vao, attribute, bindingindex: 0);
        }

        public void DrawElements()
        {
            GL.DrawElements(PrimitiveType.Triangles, elementCount, DrawElementsType.UnsignedInt, indices: 0);
        }

        public void Delete()
        {
            GL.DeleteBuffer(vbo);
            GL.DeleteBuffer(ebo);
            GL.DeleteVertexArray(vao);
        }
    }
}