﻿// <copyright file="ElementDrawGroup.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Graphics.OpenGL4;

namespace VoxelGame.Graphics.Groups
{
    public class ElementDrawGroup
    {
        private readonly int ebo;
        private readonly int vao;
        private readonly int vbo;

        private int elementCount;

        private ElementDrawGroup()
        {
            GL.CreateBuffers(n: 1, out vbo);
            GL.CreateBuffers(n: 1, out ebo);
            GL.CreateVertexArrays(n: 1, out vao);
        }

        public static ElementDrawGroup Create()
        {
            return new();
        }

        public void SetStorage(int elements, int vertexDataCount, float[] vertexData, int indexDataCount,
            uint[] indexData)
        {
            elementCount = elements;

            GL.NamedBufferStorage(
                vbo,
                vertexDataCount * sizeof(float),
                vertexData,
                BufferStorageFlags.DynamicStorageBit);

            GL.NamedBufferStorage(ebo, indexDataCount * sizeof(uint), indexData, BufferStorageFlags.DynamicStorageBit);
        }

        public void SetData(int elements, int vertexDataCount, float[] vertexData, int indexDataCount, uint[] indexData)
        {
            elementCount = elements;

            if (elementCount == 0) return;

            GL.NamedBufferData(vbo, vertexDataCount * sizeof(float), vertexData, BufferUsageHint.DynamicDraw);
            GL.NamedBufferData(ebo, indexDataCount * sizeof(uint), indexData, BufferUsageHint.DynamicDraw);
        }

        public void VertexArrayBindBuffer(int size)
        {
            GL.VertexArrayVertexBuffer(vao, bindingindex: 0, vbo, IntPtr.Zero, size * sizeof(float));
            GL.VertexArrayElementBuffer(vao, ebo);
        }

        public void VertexArrayBindAttribute(int attribute, int size, int offset)
        {
            GL.EnableVertexArrayAttrib(vao, attribute);

            GL.VertexArrayAttribFormat(
                vao,
                attribute,
                size,
                VertexAttribType.Float,
                normalized: false,
                offset * sizeof(float));

            GL.VertexArrayAttribBinding(vao, attribute, bindingindex: 0);
        }

        public void BindVertexArray()
        {
            GL.BindVertexArray(vao);
        }

        public void DrawElements(PrimitiveType type)
        {
            GL.DrawElements(type, elementCount, DrawElementsType.UnsignedInt, indices: 0);
        }

        public void Delete()
        {
            GL.DeleteBuffer(vbo);
            GL.DeleteBuffer(ebo);
            GL.DeleteVertexArray(vao);
        }
    }
}