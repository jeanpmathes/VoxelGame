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
        private readonly int positionSize;
        private readonly int dataSize;

        private readonly int positionVbo;
        private readonly int dataVbo;
        private readonly int ebo;
        private readonly int vao;

        private int elementCount;

        private ElementPositionDataDrawGroup(int positionSize, int dataSize)
        {
            this.positionSize = positionSize;
            this.dataSize = dataSize;

            GL.CreateBuffers(1, out positionVbo);
            GL.CreateBuffers(1, out dataVbo);
            GL.CreateBuffers(1, out ebo);
            GL.CreateVertexArrays(1, out vao);
        }

        public static ElementPositionDataDrawGroup Create(int positionSize, int dataSize)
        {
            return new ElementPositionDataDrawGroup(positionSize, dataSize);
        }

        public bool IsFilled { get; private set; }

        public void SetData(int positionCount, float[] positions, int dataCount, int[] data, int indexCount, uint[] indices)
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
            GL.VertexArrayVertexBuffer(vao, 0, positionVbo, IntPtr.Zero, positionSize * sizeof(float));
            GL.VertexArrayVertexBuffer(vao, 1, dataVbo, IntPtr.Zero, dataSize * sizeof(int));
            GL.VertexArrayElementBuffer(vao, ebo);
        }

        public void VertexArrayAttributeBinding(int positionAttribute, int dataAttribute)
        {
            GL.EnableVertexArrayAttrib(vao, positionAttribute);
            GL.EnableVertexArrayAttrib(vao, dataAttribute);

            GL.VertexArrayAttribFormat(vao, positionAttribute, positionSize, VertexAttribType.Float, false, 0 * sizeof(float));
            GL.VertexArrayAttribIFormat(vao, dataAttribute, dataSize, VertexAttribType.Int, 0 * sizeof(int));

            GL.VertexArrayAttribBinding(vao, positionAttribute, 0);
            GL.VertexArrayAttribBinding(vao, dataAttribute, 1);
        }

        public void BindVertexArray()
        {
            GL.BindVertexArray(vao);
        }

        public void DrawElements()
        {
            GL.DrawElements(PrimitiveType.Triangles, elementCount, DrawElementsType.UnsignedInt, 0);
        }

        public void Draw() => DrawElements();

        public void Delete()
        {
            GL.DeleteBuffer(positionVbo);
            GL.DeleteBuffer(dataVbo);
            GL.DeleteBuffer(ebo);
            GL.DeleteVertexArray(vao);
        }
    }
}