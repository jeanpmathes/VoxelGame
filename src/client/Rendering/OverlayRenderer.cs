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
    /// <summary>
    ///     A renderer for overlay textures. Any block or liquid texture can be used as an overlay.
    /// </summary>
    public sealed class OverlayRenderer : IDisposable
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<OverlayRenderer>();

        private readonly ElementDrawGroup drawGroup;
        private int samplerId;

        private int textureId;

        /// <summary>
        ///     Create a new overlay renderer.
        /// </summary>
        public OverlayRenderer()
        {
            (float[] vertices, uint[] indices) = BlockModels.CreatePlaneModel();

            drawGroup = ElementDrawGroup.Create();
            drawGroup.SetStorage(elements: 6, vertices.Length, vertices, indices.Length, indices);

            Shaders.Overlay.Use();

            drawGroup.VertexArrayBindBuffer(size: 5);

            int vertexLocation = Shaders.Overlay.GetAttributeLocation("aPosition");
            drawGroup.VertexArrayBindAttribute(vertexLocation, size: 3, offset: 0);

            int texCordLocation = Shaders.Overlay.GetAttributeLocation("aTexCoord");
            drawGroup.VertexArrayBindAttribute(texCordLocation, size: 2, offset: 3);
        }

        private static Shaders Shaders => Application.Client.Instance.Resources.Shaders;

        /// <summary>
        ///     Set the texture to a block texture.
        /// </summary>
        /// <param name="number">The number of the block texture.</param>
        public void SetBlockTexture(int number)
        {
            samplerId = number / ArrayTexture.UnitSize + 1;
            textureId = number % ArrayTexture.UnitSize;
        }

        /// <summary>
        ///     Set the texture to a liquid texture.
        /// </summary>
        /// <param name="number">The number of the liquid texture.</param>
        public void SetLiquidTexture(int number)
        {
            samplerId = 5;
            textureId = number;
        }

        /// <summary>
        ///     Draw the overlay.
        /// </summary>
        public void Draw()
        {
            if (disposed) return;

            GL.Enable(EnableCap.Blend);

            drawGroup.BindVertexArray();

            Shaders.Overlay.Use();

            Shaders.Overlay.SetInt("texId", textureId);
            Shaders.Overlay.SetInt("tex", samplerId);

            drawGroup.DrawElements(PrimitiveType.Triangles);

            GL.BindVertexArray(array: 0);
            GL.UseProgram(program: 0);

            GL.Disable(EnableCap.Blend);
        }

        #region IDisposable Support

        private bool disposed;

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing) drawGroup.Delete();
            else
                logger.LogWarning(
                    Events.UndeletedBuffers,
                    "Renderer disposed by GC without freeing storage");

            disposed = true;
        }

        /// <summary>
        ///     Finalizer.
        /// </summary>
        ~OverlayRenderer()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        ///     Dispose of the renderer.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
