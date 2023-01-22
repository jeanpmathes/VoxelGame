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
    private const int ModeBlock = 0;
    private const int ModeFluid = 1;
    private static readonly ILogger logger = LoggingHelper.CreateLogger<OverlayRenderer>();

    private readonly ElementDrawGroup drawGroup;

    private float lowerBound;

    private int mode = ModeBlock;
    private int samplerId;

    private int textureId;
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
    /// <param name="number">The number of the block texture.</param>
    public void SetBlockTexture(int number)
    {
        samplerId = number / ArrayTexture.UnitSize + 1;
        textureId = number % ArrayTexture.UnitSize;

        mode = ModeBlock;
    }

    /// <summary>
    ///     Set the texture to a fluid texture.
    /// </summary>
    /// <param name="number">The number of the fluid texture.</param>
    public void SetFluidTexture(int number)
    {
        samplerId = 5;
        textureId = number;

        mode = ModeFluid;
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
        Shaders.Overlay.SetInt("mode", mode);

        Shaders.Overlay.SetFloat("lowerBound", lowerBound);
        Shaders.Overlay.SetFloat("upperBound", upperBound);

        drawGroup.DrawElements(PrimitiveType.Triangles);

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

