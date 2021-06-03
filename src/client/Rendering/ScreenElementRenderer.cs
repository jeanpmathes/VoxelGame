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
    public class ScreenElementRenderer : Renderer
    {
        private static readonly ILogger Logger = LoggingHelper.CreateLogger<ScreenElementRenderer>();

        private readonly int vbo;
        private readonly int ebo;
        private readonly int vao;

        private int texUnit;
        private Vector3 color;

        public ScreenElementRenderer()
        {
            BlockModel.CreatePlaneModel(out float[] vertices, out uint[] indices);

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

            int vertexLocation = Client.ScreenElementShader.GetAttributeLocation("aPosition");
            int texCordLocation = Client.ScreenElementShader.GetAttributeLocation("aTexCoord");

            GL.EnableVertexArrayAttrib(vao, vertexLocation);
            GL.EnableVertexArrayAttrib(vao, texCordLocation);

            GL.VertexArrayAttribFormat(vao, vertexLocation, 3, VertexAttribType.Float, false, 0 * sizeof(float));
            GL.VertexArrayAttribFormat(vao, texCordLocation, 2, VertexAttribType.Float, false, 3 * sizeof(float));

            GL.VertexArrayAttribBinding(vao, vertexLocation, 0);
            GL.VertexArrayAttribBinding(vao, texCordLocation, 0);
        }

        public void SetTexture(Rendering.Texture texture)
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
                Logger.LogWarning(Events.UndeletedBuffers, "A renderer has been disposed by GC, without deleting buffers.");
            }

            disposed = true;
        }

        #endregion IDisposable Support
    }
}