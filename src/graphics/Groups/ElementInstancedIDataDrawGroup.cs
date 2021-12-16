﻿// <copyright file="ElementInstancedIDataDrawGroup.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Graphics.OpenGL4;

namespace VoxelGame.Graphics.Groups
{
    public class ElementInstancedIDataDrawGroup : IDrawGroup
    {
        private const int ModelPositionBindingIndex = 0;
        private const int InstanceBindingIndex = 1;

        private readonly int instanceSize;

        private readonly int instanceVBO;
        private readonly int modelEBO;
        private readonly int modelElementCount;

        private readonly int modelPositionVBO;

        private readonly int vao;

        private int instanceCount;

        private ElementInstancedIDataDrawGroup((float[] vertices, uint[] indices) model, int instanceSize)
        {
            this.instanceSize = instanceSize;

            GL.CreateBuffers(n: 1, out modelPositionVBO);
            GL.CreateBuffers(n: 1, out modelEBO);
            GL.CreateBuffers(n: 1, out instanceVBO);
            GL.CreateVertexArrays(n: 1, out vao);

            modelElementCount = model.indices.Length;
            SetModelData(model);
        }

        public bool IsFilled { get; private set; }

        public void BindVertexArray()
        {
            GL.BindVertexArray(vao);
        }

        public void Draw()
        {
            DrawElementsInstanced();
        }

        public static ElementInstancedIDataDrawGroup Create((float[] vertices, uint[] indices) model, int instanceSize)
        {
            return new ElementInstancedIDataDrawGroup(model, instanceSize);
        }

        private void SetModelData((float[] vertices, uint[] indices) model)
        {
            GL.NamedBufferData(
                modelPositionVBO,
                model.vertices.Length * sizeof(float),
                model.vertices,
                BufferUsageHint.StaticDraw);

            GL.NamedBufferData(
                modelEBO,
                model.indices.Length * sizeof(uint),
                model.indices,
                BufferUsageHint.StaticDraw);
        }

        public void SetInstanceData(int instanceDataCount, int[] instanceData)
        {
            instanceCount = instanceDataCount / instanceSize;

            if (instanceCount == 0)
            {
                IsFilled = false;

                return;
            }

            IsFilled = true;

            GL.NamedBufferData(instanceVBO, instanceDataCount * sizeof(int), instanceData, BufferUsageHint.DynamicDraw);
        }

        public void VertexArrayBindBuffer(int modelSize)
        {
            GL.VertexArrayVertexBuffer(
                vao,
                ModelPositionBindingIndex,
                modelPositionVBO,
                IntPtr.Zero,
                modelSize * sizeof(float));

            GL.VertexArrayElementBuffer(vao, modelEBO);
            GL.VertexArrayVertexBuffer(vao, InstanceBindingIndex, instanceVBO, IntPtr.Zero, instanceSize * sizeof(int));
        }

        public void VertexArrayModelAttributeBinding(int modelAttribute, int size, int offset)
        {
            GL.EnableVertexArrayAttrib(vao, modelAttribute);

            GL.VertexArrayAttribFormat(
                vao,
                modelAttribute,
                size,
                VertexAttribType.Float,
                normalized: false,
                offset * sizeof(float));

            GL.VertexArrayAttribBinding(vao, modelAttribute, ModelPositionBindingIndex);

        }

        public void VertexArrayInstanceAttributeBinding(int instanceAttribute)
        {

            GL.EnableVertexArrayAttrib(vao, instanceAttribute);
            GL.VertexArrayAttribIFormat(vao, instanceAttribute, instanceSize, VertexAttribType.Int, 0 * sizeof(int));
            GL.VertexArrayAttribBinding(vao, instanceAttribute, InstanceBindingIndex);
            GL.VertexArrayBindingDivisor(vao, InstanceBindingIndex, divisor: 1);
        }

        public void DrawElementsInstanced()
        {
            GL.DrawElementsInstanced(
                PrimitiveType.Triangles,
                modelElementCount,
                DrawElementsType.UnsignedInt,
                IntPtr.Zero,
                instanceCount);
        }

        public void Delete()
        {
            GL.DeleteBuffer(modelPositionVBO);
            GL.DeleteBuffer(modelEBO);
            GL.DeleteBuffer(instanceVBO);
            GL.DeleteVertexArray(vao);
        }
    }
}