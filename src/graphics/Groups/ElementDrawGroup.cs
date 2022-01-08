// <copyright file="ElementDrawGroup.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Graphics.OpenGL4;

namespace VoxelGame.Graphics.Groups
{
    /// <summary>
    ///     A draw group for meshes defined as elements.
    /// </summary>
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

        /// <summary>
        ///     Create a <see cref="ElementDrawGroup" />.
        /// </summary>
        /// <returns>The created draw group.</returns>
        public static ElementDrawGroup Create()
        {
            return new ElementDrawGroup();
        }

        /// <summary>
        ///     Set the data for the draw group, using immutable storage. The data can only be set once with this method.
        /// </summary>
        /// <param name="elements">The number of elements.</param>
        /// <param name="vertexDataCount">The length of the vertex data.</param>
        /// <param name="vertexData">The vertex data. Must be at least as long as the given length.</param>
        /// <param name="indexDataCount">The length of the index data.</param>
        /// <param name="indexData">The index data. Must be at least as long as the given length.</param>
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

        /// <summary>
        ///     Set the data for the draw group. This method allows the data to be re-set.
        /// </summary>
        /// <param name="elements">The number of elements.</param>
        /// <param name="vertexDataCount">The length of the vertex data.</param>
        /// <param name="vertexData">The vertex data. Must be at least as long as the given length.</param>
        /// <param name="indexDataCount">The length of the index data.</param>
        /// <param name="indexData">The index data. Must be at least as long as the given length.</param>
        public void SetData(int elements, int vertexDataCount, float[] vertexData, int indexDataCount, uint[] indexData)
        {
            elementCount = elements;

            if (elementCount == 0) return;

            GL.NamedBufferData(vbo, vertexDataCount * sizeof(float), vertexData, BufferUsageHint.DynamicDraw);
            GL.NamedBufferData(ebo, indexDataCount * sizeof(uint), indexData, BufferUsageHint.DynamicDraw);
        }

        /// <summary>
        ///     Bind the vertex array.
        /// </summary>
        /// <param name="size">The size of a data unit per vertex.</param>
        public void VertexArrayBindBuffer(int size)
        {
            GL.VertexArrayVertexBuffer(vao, bindingindex: 0, vbo, IntPtr.Zero, size * sizeof(float));
            GL.VertexArrayElementBuffer(vao, ebo);
        }

        /// <summary>
        ///     Bind an attribute.
        /// </summary>
        /// <param name="attribute">The attribute location to bind to.</param>
        /// <param name="size">The size of the data to be bound to the attribute.</param>
        /// <param name="offset">The offset to the vertex data unit start.</param>
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

        /// <summary>
        ///     Bind the vertex array.
        /// </summary>
        public void BindVertexArray()
        {
            GL.BindVertexArray(vao);
        }

        /// <summary>
        ///     Draw the elements.
        /// </summary>
        /// <param name="type">The primitive type to use for drawing.</param>
        public void DrawElements(PrimitiveType type)
        {
            GL.DrawElements(type, elementCount, DrawElementsType.UnsignedInt, indices: 0);
        }

        /// <summary>
        ///     Delete the used resources.
        /// </summary>
        public void Delete()
        {
            GL.DeleteBuffer(vbo);
            GL.DeleteBuffer(ebo);
            GL.DeleteVertexArray(vao);
        }
    }
}