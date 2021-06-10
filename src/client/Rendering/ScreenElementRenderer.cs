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
using VoxelGame.Graphics.Groups;
using VoxelGame.Graphics.Objects;
using VoxelGame.Logging;

namespace VoxelGame.Client.Rendering
{
    public class ScreenElementRenderer : IDisposable
    {
        private static readonly ILogger Logger = LoggingHelper.CreateLogger<ScreenElementRenderer>();

        private readonly ElementDrawGroup drawGroup;

        private int texUnit;
        private Vector3 color;

        public ScreenElementRenderer()
        {
            BlockModel.CreatePlaneModel(out float[] vertices, out uint[] indices);

            drawGroup = ElementDrawGroup.Create();
            drawGroup.SetStorage(6, vertices.Length, vertices, indices.Length, indices);

            Client.ScreenElementShader.Use();

            drawGroup.VertexArrayBindBuffer(5);

            int vertexLocation = Client.ScreenElementShader.GetAttributeLocation("aPosition");
            drawGroup.VertexArrayBindAttribute(vertexLocation, 3, 0);

            int texCordLocation = Client.ScreenElementShader.GetAttributeLocation("aTexCoord");
            drawGroup.VertexArrayBindAttribute(texCordLocation, 2, 3);
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

        public void Draw(Vector2 offset, float scaling)
        {
            if (disposed)
            {
                return;
            }

            var screenSize = Screen.Size.ToVector2();
            Vector3 scale = new Vector3(scaling, scaling, 1f) * screenSize.Length;
            Vector3 translate = new Vector3((offset - new Vector2(0.5f, 0.5f)) * screenSize);

            Matrix4 model = Matrix4.Identity * Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(translate);

            drawGroup.BindVertexArray();

            Client.ScreenElementShader.Use();

            Client.ScreenElementShader.SetMatrix4("model", model);
            Client.ScreenElementShader.SetVector3("color", color);
            Client.ScreenElementShader.SetInt("tex", texUnit);

            drawGroup.DrawElements(PrimitiveType.Triangles);

            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        #region IDisposable Support

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                drawGroup.Delete();
            }
            else
            {
                Logger.LogWarning(Events.UndeletedBuffers, "A renderer has been disposed by GC, without deleting buffers.");
            }

            disposed = true;
        }

        ~ScreenElementRenderer()
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