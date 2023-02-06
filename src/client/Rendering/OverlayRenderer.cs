// <copyright file="OverlayRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL4;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Groups;
using VoxelGame.Logging;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     A renderer for overlay textures. Any block or fluid texture can be used as an overlay.
/// </summary>
public sealed class OverlayRenderer : IDisposable
{
    private const int BlockMode = 0;
    private const int FluidMode = 1;
    private static readonly ILogger logger = LoggingHelper.CreateLogger<OverlayRenderer>();

    private readonly ElementDrawGroup drawGroup;

    private float lowerBound;

    private int mode = BlockMode;
    private int samplerId;

    private int textureId;
    private TintColor tint = TintColor.None;
    private float upperBound;

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
    /// <param name="texture">The texture to use.</param>
    public void SetBlockTexture(OverlayTexture texture)
    {
        samplerId = texture.TextureIdentifier / ArrayTexture.UnitSize + 1;
        textureId = texture.TextureIdentifier % ArrayTexture.UnitSize;

        mode = BlockMode;

        tint = texture.Tint;
    }

    /// <summary>
    ///     Set the texture to a fluid texture.
    /// </summary>
    /// <param name="texture">The texture to use.</param>
    public void SetFluidTexture(OverlayTexture texture)
    {
        samplerId = 5;
        textureId = texture.TextureIdentifier;

        mode = FluidMode;

        tint = texture.Tint;
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

        Shaders.Overlay.SetInt("textureId", textureId);
        Shaders.Overlay.SetInt("sampler", samplerId);
        Shaders.Overlay.SetInt("mode", mode);

        Shaders.Overlay.SetFloat("lowerBound", lowerBound);
        Shaders.Overlay.SetFloat("upperBound", upperBound);

        Shaders.Overlay.SetColor4("tint", tint);

        GL.Disable(EnableCap.DepthTest);
        drawGroup.DrawElements(PrimitiveType.Triangles);
        GL.Enable(EnableCap.DepthTest);

        GL.BindVertexArray(array: 0);
        GL.UseProgram(program: 0);

        GL.Disable(EnableCap.Blend);
    }

    /// <summary>
    ///     Set the bounds of the overlay.
    /// </summary>
    /// <param name="newLowerBound">The lower bound.</param>
    /// <param name="newUpperBound">The upper bound.</param>
    public void SetBounds(double newLowerBound, double newUpperBound)
    {
        lowerBound = (float) newLowerBound;
        upperBound = (float) newUpperBound;
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
