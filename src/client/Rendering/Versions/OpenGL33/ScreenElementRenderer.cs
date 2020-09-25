// <copyright file="OverlayRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using OpenToolkit.Mathematics;
using System;
using VoxelGame.Core;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Rendering.Versions.OpenGL33
{
    public class ScreenElementRenderer : Rendering.ScreenElementRenderer
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<ScreenElementRenderer>();

        private readonly int vbo;
        private readonly int ebo;
        private readonly int vao;

        private int texUnit;
        private Vector3 color;

        public ScreenElementRenderer()
        {
            float[] vertices = new float[]
            {
                -0.5f, -0.5f, 0.0f, 0f, 0f,
                -0.5f,  0.5f, 0.0f, 0f, 1f,
                 0.5f,  0.5f, 0.0f, 1f, 1f,
                 0.5f, -0.5f, 0.0f, 1f, 0f
            };

            uint[] indices = new uint[]
            {
                0, 2, 1,
                0, 3, 2
            };

            // Vertex Buffer Object
            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Element Buffer Object
            ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            Client.ScreenElementShader.Use();

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

        public override void SetTexture(Rendering.Texture texture)
        {
            if (disposed)
            {
                return;
            }

            texUnit = texture.TextureUnit - TextureUnit.Texture0;
        }

        public override void SetColor(Vector3 color)
        {
            if (disposed)
            {
                return;
            }

            this.color = color;
        }

        public override void Draw(Vector3 position)
        {
            if (disposed)
            {
                return;
            }

            Vector2 screenSize = Screen.Size.ToVector2();
            Vector3 scale = new Vector3(position.Z, position.Z, 1f) * screenSize.Length;
            Vector3 translate = new Vector3((position.Xy - new Vector2(0.5f, 0.5f)) * screenSize);

            Matrix4 model = Matrix4.Identity * Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(translate);

            GL.BindVertexArray(vao);

            Client.ScreenElementShader.Use();

            Client.ScreenElementShader.SetMatrix4("model", model);
            Client.ScreenElementShader.SetVector3("color", color);
            Client.ScreenElementShader.SetInt("tex", texUnit);

            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

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
                logger.LogWarning(LoggingEvents.UndeletedBuffers, "A renderer has been disposed by GC, without deleting buffers.");
            }

            disposed = true;
        }

        #endregion IDisposable Support
    }
}