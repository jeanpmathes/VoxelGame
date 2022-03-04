// <copyright file="ArrayIDataDrawGroup.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Graphics.OpenGL4;

namespace VoxelGame.Graphics.Groups
{
    /// <summary>
    ///     Draw meshes defined as integer data arrays.
    /// </summary>
    public class ArrayIDataDrawGroup : IDrawGroup
    {
        private readonly int size;
        private readonly int vao;

        private readonly int vbo;

        private int vertexCount;

        private ArrayIDataDrawGroup(int size)
        {
            this.size = size;

            GL.CreateBuffers(n: 1, out vbo);
            GL.CreateVertexArrays(n: 1, out vao);
        }

        /// <inheritdoc />
        public bool IsFilled { get; private set; }

        /// <inheritdoc />
        public void BindVertexArray()
        {
            GL.BindVertexArray(vao);
        }

        /// <inheritdoc />
        public void Draw()
        {
            DrawArrays();
        }

        /// <summary>
        ///     Create an <see cref="ArrayIDataDrawGroup" />.
        /// </summary>
        /// <param name="size">The size of each data unit.</param>
        /// <returns>The created draw group.</returns>
        public static ArrayIDataDrawGroup Create(int size)
        {
            return new ArrayIDataDrawGroup(size);
        }

        /// <summary>
        ///     Set the vertex data.
        /// </summary>
        /// <param name="vertexDataCount">The number of integers that make up the data. Must be a multiple of the size.</param>
        /// <param name="vertexData">The vertex array. Can be larger than the vertexDataCount.</param>
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

        /// <summary>
        ///     Bind the vertex array.
        /// </summary>
        public void VertexArrayBindBuffer()
        {
            GL.VertexArrayVertexBuffer(vao, bindingindex: 0, vbo, IntPtr.Zero, size * sizeof(int));
        }

        /// <summary>
        ///     Bind the data attribute.
        /// </summary>
        /// <param name="attribute">The attribute location.</param>
        public void VertexArrayAttributeBinding(int attribute)
        {
            GL.EnableVertexArrayAttrib(vao, attribute);
            GL.VertexArrayAttribIFormat(vao, attribute, size, VertexAttribType.Int, 0 * sizeof(int));
            GL.VertexArrayAttribBinding(vao, attribute, bindingindex: 0);
        }

        /// <summary>
        ///     Draw the array.
        /// </summary>
        public void DrawArrays()
        {
            GL.DrawArrays(PrimitiveType.Triangles, first: 0, vertexCount);
        }

        /// <summary>
        ///     Delete the used resources.
        /// </summary>
        public void Delete()
        {
            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
        }
    }
}
