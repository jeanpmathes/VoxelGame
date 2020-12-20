using System;
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using VoxelGame.Core;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Rendering.Versions.OpenGL33
{
    public class OverlayRenderer : Rendering.OverlayRenderer
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<OpenGL46.OverlayRenderer>();

        private readonly int vbo;
        private readonly int ebo;
        private readonly int vao;

        public OverlayRenderer()
        {
            BlockModel.CreatePlaneModel(out float[] vertices, out uint[] indices);

            // Vertex Buffer Object
            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Element Buffer Object
            ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            Client.OverlayShader.Use();

            // Vertex Array Object
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);

            int vertexLocation = Client.ScreenElementShader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            int texCordLocation = Client.ScreenElementShader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCordLocation);
            GL.VertexAttribPointer(texCordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            GL.BindVertexArray(0);
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