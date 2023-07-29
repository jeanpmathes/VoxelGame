// <copyright file="OverlayRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.Support.Graphics.Groups;
using VoxelGame.Support.Objects;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     A renderer for overlay textures. Any block or fluid texture can be used as an overlay.
/// </summary>
public sealed class OverlayRenderer : IDisposable
{
    // todo: maybe overlay renderer and screen element renderer can be merged (differences are in shader, data, and setup which can all be done in ctor)

    private const int BlockMode = 0;
    private const int FluidMode = 1;
    private static readonly ILogger logger = LoggingHelper.CreateLogger<OverlayRenderer>();

    private readonly ElementDrawGroup drawGroup;
    private bool isAnimated;

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
        // todo: port to DirectX

        (float[] vertices, uint[] indices) = BlockModels.CreatePlaneModel();

        drawGroup = ElementDrawGroup.Create();
        drawGroup.SetStorage(elements: 6, vertices.Length, vertices, indices.Length, indices);

        disposed = true; // todo: remove this line when porting to DirectX

        return; // todo: remove this line when porting to DirectX

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
        samplerId = texture.TextureIdentifier / Texture.MaxArrayTextureDepth + 1;
        textureId = texture.TextureIdentifier % Texture.MaxArrayTextureDepth;

        mode = BlockMode;

        SetGeneralAttributes(texture);
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

        SetGeneralAttributes(texture);
    }

    private void SetGeneralAttributes(OverlayTexture texture)
    {
        tint = texture.Tint;
        isAnimated = texture.IsAnimated;
    }

    /// <summary>
    ///     Draw the overlay.
    /// </summary>
    public void Draw()
    {
        if (disposed) return;

        // GL.Enable(EnableCap.Blend);
        // GL.Disable(EnableCap.DepthTest);

        drawGroup.BindVertexArray();

        Shaders.Overlay.Use();

        Shaders.Overlay.SetInt("textureId", textureId);
        Shaders.Overlay.SetInt("sampler", samplerId);
        Shaders.Overlay.SetInt("mode", mode);

        Shaders.Overlay.SetFloat("lowerBound", lowerBound);
        Shaders.Overlay.SetFloat("upperBound", upperBound);

        Shaders.Overlay.SetColor4("tint", tint);
        Shaders.Overlay.SetInt("isAnimated", isAnimated.ToInt());

        // drawGroup.DrawElements(PrimitiveType.Triangles);
        //
        // GL.BindVertexArray(array: 0);
        // GL.UseProgram(program: 0);
        //
        // GL.Enable(EnableCap.DepthTest);
        // GL.Disable(EnableCap.Blend);
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
