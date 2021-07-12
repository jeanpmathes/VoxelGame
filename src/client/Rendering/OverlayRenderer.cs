// <copyright file="OverlayRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using Microsoft.Extensions.Logging;
using OpenToolkit.Graphics.OpenGL4;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Groups;
using VoxelGame.Logging;

namespace VoxelGame.Client.Rendering
{
    public class OverlayRenderer : IDisposable
    {
        private static readonly ILogger Logger = LoggingHelper.CreateLogger<OverlayRenderer>();

        private readonly ElementDrawGroup drawGroup;

        private int textureId;
        private int samplerId;

        public OverlayRenderer()
        {
            BlockModels.CreatePlaneModel(out float[] vertices, out uint[] indices);

            drawGroup = ElementDrawGroup.Create();
            drawGroup.SetStorage(6, vertices.Length, vertices, indices.Length, indices);

            Shaders.Overlay.Use();

            drawGroup.VertexArrayBindBuffer(5);

            int vertexLocation = Shaders.Overlay.GetAttributeLocation("aPosition");
            drawGroup.VertexArrayBindAttribute(vertexLocation, 3, 0);

            int texCordLocation = Shaders.Overlay.GetAttributeLocation("aTexCoord");
            drawGroup.VertexArrayBindAttribute(texCordLocation, 2, 3);
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

            drawGroup.BindVertexArray();

            Shaders.Overlay.Use();

            Shaders.Overlay.SetInt("texId", textureId);
            Shaders.Overlay.SetInt("tex", samplerId);

            drawGroup.DrawElements(PrimitiveType.Triangles);

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
                drawGroup.Delete();
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