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

namespace VoxelGame.Client.Rendering
{
    public class ScreenElementRenderer : Renderer
    {
        private static readonly ILogger logger = Logging.CreateLogger<ScreenElementRenderer>();

        private readonly int vbo;
        private readonly int ebo;
        private readonly int vao;

        private int texUnit;
        private Vector3 color;

        public ScreenElementRenderer()
        {
            vao = GL.GenVertexArray();

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
            GL.CreateBuffers(1, out vbo);
            GL.NamedBufferStorage(vbo, vertices.Length * sizeof(float), vertices, BufferStorageFlags.DynamicStorageBit);

            // Element Buffer Object
            GL.CreateBuffers(1, out ebo);
            GL.NamedBufferStorage(ebo, indices.Length * sizeof(uint), indices, BufferStorageFlags.DynamicStorageBit);

            Client.ScreenElementShader.Use();

            // Vertex Array Object
            GL.CreateVertexArrays(1, out vao);

            GL.VertexArrayVertexBuffer(vao, 0, vbo, IntPtr.Zero, 5 * sizeof(float));
            GL.VertexArrayElementBuffer(vao, ebo);

            int vertexLocation = Client.ScreenElementShader.GetAttribLocation("aPosition");
            int texCordLocation = Client.ScreenElementShader.GetAttribLocation("aTexCoord");

            GL.EnableVertexArrayAttrib(vao, vertexLocation);
            GL.EnableVertexArrayAttrib(vao, texCordLocation);

            GL.VertexArrayAttribFormat(vao, vertexLocation, 3, VertexAttribType.Float, false, 0 * sizeof(float));
            GL.VertexArrayAttribFormat(vao, texCordLocation, 2, VertexAttribType.Float, false, 3 * sizeof(float));

            GL.VertexArrayAttribBinding(vao, vertexLocation, 0);
            GL.VertexArrayAttribBinding(vao, texCordLocation, 0);
        }

        public void SetTexture(Texture texture)
        {
            if (disposed)
            {
                return;
            }

            texUnit = texture.TextureUnit - TextureUnit.Texture0;
        }

        public void SetColor(Vector3 color)
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

            Vector2 screenSize = Client.Instance.Size.ToVector2();
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