// <copyright file="ElementDrawGroup.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Graphics.OpenGL4;

namespace VoxelGame.Graphics.Groups
{
    public class ElementPositionDataDrawGroup : IDrawGroup
    {
        private readonly int dataSize;
        private readonly int dataVbo;
        private readonly int ebo;
        private readonly int positionSize;

        private readonly int positionVbo;
        private readonly int vao;

        private int elementCount;

        private ElementPositionDataDrawGroup(int positionSize, int dataSize)
        {
            this.positionSize = positionSize;
            this.dataSize = dataSize;

            GL.CreateBuffers(n: 1, out positionVbo);
            GL.CreateBuffers(n: 1, out dataVbo);
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

        public static ElementPositionDataDrawGroup Create(int positionSize, int dataSize)
        {
            return new(positionSize, dataSize);
        }

        public void SetData(int positionCount, float[] positions, int dataCount, int[] data, int indexCount,
            uint[] indices)
        {
            elementCount = indexCount;

            if (elementCount == 0)
            {
                IsFilled = false;

                return;
            }

            IsFilled = true;

            GL.NamedBufferData(positionVbo, positionCount * sizeof(float), positions, BufferUsageHint.DynamicDraw);
            GL.NamedBufferData(dataVbo, dataCount * sizeof(int), data, BufferUsageHint.DynamicDraw);
            GL.NamedBufferData(ebo, indexCount * sizeof(uint), indices, BufferUsageHint.DynamicDraw);
        }

        public void VertexArrayBindBuffer()
        {
            GL.VertexArrayVertexBuffer(vao, bindingindex: 0, positionVbo, IntPtr.Zero, positionSize * sizeof(float));
            GL.VertexArrayVertexBuffer(vao, bindingindex: 1, dataVbo, IntPtr.Zero, dataSize * sizeof(int));
            GL.VertexArrayElementBuffer(vao, ebo);
        }

        public void VertexArrayAttributeBinding(int positionAttribute, int dataAttribute)
        {
            GL.EnableVertexArrayAttrib(vao, positionAttribute);
            GL.EnableVertexArrayAttrib(vao, dataAttribute);

            GL.VertexArrayAttribFormat(
                vao,
                positionAttribute,
                positionSize,
                VertexAttribType.Float,
                normalized: false,
                0 * sizeof(float));

            GL.VertexArrayAttribIFormat(vao, dataAttribute, dataSize, VertexAttribType.Int, 0 * sizeof(int));

            GL.VertexArrayAttribBinding(vao, positionAttribute, bindingindex: 0);
            GL.VertexArrayAttribBinding(vao, dataAttribute, bindingindex: 1);
        }

        public void DrawElements()
        {
            GL.DrawElements(PrimitiveType.Triangles, elementCount, DrawElementsType.UnsignedInt, indices: 0);
        }

        public void Delete()
        {
            GL.DeleteBuffer(positionVbo);
            GL.DeleteBuffer(dataVbo);
            GL.DeleteBuffer(ebo);
            GL.DeleteVertexArray(vao);
        }
    }
}