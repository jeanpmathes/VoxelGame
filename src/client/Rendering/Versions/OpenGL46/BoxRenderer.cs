// <copyright file="BoxRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System;
using VoxelGame.Core;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Rendering.Versions.OpenGL46
{
    /// <summary>
    /// A renderer that renders instances of the <see cref="BoundingBox"/> struct.
    /// </summary>
    public class BoxRenderer : Rendering.BoxRenderer
    {
        private static readonly ILogger Logger = LoggingHelper.CreateLogger<BoxRenderer>();

        private readonly int vbo;
        private readonly int ebo;
        private readonly int vao;

        private BoundingBox currentBoundingBox;

        private int elements;

        public BoxRenderer()
        {
            GL.CreateBuffers(1, out vbo);
            GL.CreateBuffers(1, out ebo);
            GL.CreateVertexArrays(1, out vao);
        }

        public override void SetBoundingBox(BoundingBox boundingBox)
        {
            if (disposed)
            {
                return;
            }

            if (currentBoundingBox == boundingBox)
            {
                return;
            }

            currentBoundingBox = boundingBox;

            elements = BuildMeshData(currentBoundingBox, boundingBox, out float[] vertices, out uint[] indices);

            // Vertex Buffer Object
            GL.NamedBufferData(vbo, vertices.Length * sizeof(float), vertices, BufferUsageHint.DynamicDraw);

            // Element Buffer Object
            GL.NamedBufferData(ebo, indices.Length * sizeof(uint), indices, BufferUsageHint.DynamicDraw);

            Client.SelectionShader.Use();

            // Vertex Array Object
            GL.VertexArrayVertexBuffer(vao, 0, vbo, IntPtr.Zero, 3 * sizeof(float));
            GL.VertexArrayElementBuffer(vbo, ebo);

            int vertexLocation = Client.SelectionShader.GetAttribLocation("aPosition");

            GL.EnableVertexArrayAttrib(vao, vertexLocation);
            GL.VertexArrayAttribFormat(vao, vertexLocation, 3, VertexAttribType.Float, false, 0 * sizeof(float));
            GL.VertexArrayAttribBinding(vao, vertexLocation, 0);
        }

        public override void Draw(Vector3 position)
        {
            if (disposed)
            {
                return;
            }

            GL.BindVertexArray(vao);

            Client.SelectionShader.Use();

            Matrix4 model = Matrix4.Identity * Matrix4.CreateTranslation(position);
            Client.SelectionShader.SetMatrix4("model", model);
            Client.SelectionShader.SetMatrix4("view", Client.Player.GetViewMatrix());
            Client.SelectionShader.SetMatrix4("projection", Client.Player.GetProjectionMatrix());

            GL.DrawElements(PrimitiveType.Lines, elements, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        #region IDisposable Support

        private bool disposed;

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                GL.DeleteBuffer(vbo);
                GL.DeleteBuffer(ebo);
                GL.DeleteVertexArray(vao);
            }
            else
            {
                Logger.LogWarning(LoggingEvents.UndeletedBuffers, "A renderer has been disposed by GC, without deleting buffers.");
            }

            disposed = true;
        }

        #endregion IDisposable Support
    }
}