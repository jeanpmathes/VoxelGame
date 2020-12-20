using System;
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using VoxelGame.Core;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Rendering.Versions.OpenGL46
{
    public class OverlayRenderer : Rendering.OverlayRenderer
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<OverlayRenderer>();

        private readonly int vbo;
        private readonly int ebo;
        private readonly int vao;

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

            int vertexLocation = Client.OverlayShader.GetAttribLocation("aPosition");
            int texCordLocation = Client.OverlayShader.GetAttribLocation("aTexCoord");

            GL.EnableVertexArrayAttrib(vao, vertexLocation);
            GL.EnableVertexArrayAttrib(vao, texCordLocation);

            GL.VertexArrayAttribFormat(vao, vertexLocation, 3, VertexAttribType.Float, false, 0 * sizeof(float));
            GL.VertexArrayAttribFormat(vao, texCordLocation, 2, VertexAttribType.Float, false, 3 * sizeof(float));

            GL.VertexArrayAttribBinding(vao, vertexLocation, 0);
            GL.VertexArrayAttribBinding(vao, texCordLocation, 0);
        }

        public override void Draw(Vector3 position)
        {
            Draw();
        }

        public override void Draw()
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
                logger.LogWarning(LoggingEvents.UndeletedBuffers, "A renderer has been disposed by GC, without deleting buffers.");
            }

            disposed = true;
        }

        #endregion IDisposable Support
    }
}