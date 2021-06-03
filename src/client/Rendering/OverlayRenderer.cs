// <copyright file="OverlayRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;

namespace VoxelGame.Client.Rendering
{
    public class OverlayRenderer : IDisposable
    {
        private static readonly ILogger Logger = LoggingHelper.CreateLogger<OverlayRenderer>();

        private readonly int vbo;
        private readonly int ebo;
        private readonly int vao;

        private int textureId;
        private int samplerId;

        public OverlayRenderer()
        {
            BlockModel.CreatePlaneModel(out float[] vertices, out uint[] indices);

            // Vertex Buffer Object
            GL.CreateBuffers(1, out vbo);
            GL.NamedBufferStorage(vbo, vertices.Length * sizeof(float), vertices, BufferStorageFlags.DynamicStorageBit);

            // Element Buffer Object
            GL.CreateBuffers(1, out ebo);
            GL.NamedBufferStorage(ebo, indices.Length * sizeof(uint), indices, BufferStorageFlags.DynamicStorageBit);

            Client.OverlayShader.Use();

            // Vertex Array Object
            GL.CreateVertexArrays(1, out vao);

            GL.VertexArrayVertexBuffer(vao, 0, vbo, IntPtr.Zero, 5 * sizeof(float));
            GL.VertexArrayElementBuffer(vao, ebo);

            int vertexLocation = Client.OverlayShader.GetAttributeLocation("aPosition");
            int texCordLocation = Client.OverlayShader.GetAttributeLocation("aTexCoord");

            GL.EnableVertexArrayAttrib(vao, vertexLocation);
            GL.EnableVertexArrayAttrib(vao, texCordLocation);

            GL.VertexArrayAttribFormat(vao, vertexLocation, 3, VertexAttribType.Float, false, 0 * sizeof(float));
            GL.VertexArrayAttribFormat(vao, texCordLocation, 2, VertexAttribType.Float, false, 3 * sizeof(float));

            GL.VertexArrayAttribBinding(vao, vertexLocation, 0);
            GL.VertexArrayAttribBinding(vao, texCordLocation, 0);
        }

        public void SetBlockTexture(int number)
        {
            samplerId = (number / 2048) + 1;
            textureId = number % 2048;
        }

        public void SetLiquidTexture(int number)
        {
            samplerId = 5;
            textureId = number;
        }

        public void Draw()
        {
            if (disposed)
            {
                return;
            }

            GL.Enable(EnableCap.Blend);

            GL.BindVertexArray(vao);

            Client.OverlayShader.Use();

            Client.OverlayShader.SetInt("texId", textureId);
            Client.OverlayShader.SetInt("tex", samplerId);

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
            GL.UseProgram(0);

            GL.Disable(EnableCap.Blend);
        }

        #region IDisposable Support

        private bool disposed;

        protected virtual void Dispose(bool disposing)
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
                Logger.LogWarning(Events.UndeletedBuffers, "A renderer has been disposed by GC, without deleting buffers.");
            }

            disposed = true;
        }

        ~OverlayRenderer()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}