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
        private readonly int dataVBO;
        private readonly int ebo;
        private readonly int positionSize;

        private readonly int positionVBO;
        private readonly int vao;

        private int elementCount;

        private ElementPositionDataDrawGroup(int positionSize, int dataSize)
        {
            this.positionSize = positionSize;
            this.dataSize = dataSize;

            GL.CreateBuffers(n: 1, out positionVBO);
            GL.CreateBuffers(n: 1, out dataVBO);
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
            return new ElementPositionDataDrawGroup(positionSize, dataSize);
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

            GL.NamedBufferData(positionVBO, positionCount * sizeof(float), positions, BufferUsageHint.DynamicDraw);
            GL.NamedBufferData(dataVBO, dataCount * sizeof(int), data, BufferUsageHint.DynamicDraw);
            GL.NamedBufferData(ebo, indexCount * sizeof(uint), indices, BufferUsageHint.DynamicDraw);
        }

        public void VertexArrayBindBuffer()
        {
            GL.VertexArrayVertexBuffer(vao, bindingindex: 0, positionVBO, IntPtr.Zero, positionSize * sizeof(float));
            GL.VertexArrayVertexBuffer(vao, bindingindex: 1, dataVBO, IntPtr.Zero, dataSize * sizeof(int));
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
            GL.DeleteBuffer(positionVBO);
            GL.DeleteBuffer(dataVBO);
            GL.DeleteBuffer(ebo);
            GL.DeleteVertexArray(vao);
        }
    }
}