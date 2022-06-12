// <copyright file="OverlayRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Drawing;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Groups;
using VoxelGame.Graphics.Objects;
using VoxelGame.Logging;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     Renders textures on the screen.
/// </summary>
public sealed class ScreenElementRenderer : IDisposable
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<ScreenElementRenderer>();

    private readonly ElementDrawGroup drawGroup;
    private Vector3 color;

    private int texUnit;

    /// <summary>
    ///     Create a new <see cref="ScreenElementRenderer" />.
    /// </summary>
    public ScreenElementRenderer()
    {
        (float[] vertices, uint[] indices) = BlockModels.CreatePlaneModel();

        drawGroup = ElementDrawGroup.Create();
        drawGroup.SetStorage(elements: 6, vertices.Length, vertices, indices.Length, indices);

        Shaders.ScreenElement.Use();

        drawGroup.VertexArrayBindBuffer(size: 5);

        int vertexLocation = Shaders.ScreenElement.GetAttributeLocation("aPosition");
        drawGroup.VertexArrayBindAttribute(vertexLocation, size: 3, offset: 0);

        int texCordLocation = Shaders.ScreenElement.GetAttributeLocation("aTexCoord");
        drawGroup.VertexArrayBindAttribute(texCordLocation, size: 2, offset: 3);
    }

    private static Shaders Shaders => Application.Client.Instance.Resources.Shaders;

    /// <summary>
    ///     Set the texture to use for rendering.
    /// </summary>
    /// <param name="texture">The texture.</param>
    public void SetTexture(Texture texture)
    {
        if (disposed) return;

        texUnit = texture.TextureUnit - TextureUnit.Texture0;
    }

    /// <summary>
    ///     Set the color to apply to the texture.
    /// </summary>
    /// <param name="newColor">The color.</param>
    public void SetColor(Color newColor)
    {
        if (disposed) return;

        color = newColor.ToVector3();
    }

    /// <summary>
    ///     Draw the screen element.
    /// </summary>
    /// <param name="offset">The relative position on the screen.</param>
    /// <param name="scaling">The scale of the screen element.</param>
    public void Draw(Vector2 offset, float scaling)
    {
        if (disposed) return;

        var screenSize = Screen.Size.ToVector2();
        Vector3d scale = new Vector3d(scaling, scaling, z: 1.0) * screenSize.Length;
        var translate = new Vector3d((offset - new Vector2d(x: 0.5, y: 0.5)) * screenSize);

        Matrix4d model = Matrix4d.Identity * VMath.CreateScaleMatrix(scale) * Matrix4d.CreateTranslation(translate);

        drawGroup.BindVertexArray();

        Shaders.ScreenElement.Use();

        Shaders.ScreenElement.SetMatrix4("model", model.ToMatrix4());
        Shaders.ScreenElement.SetVector3("color", color);
        Shaders.ScreenElement.SetInt("tex", texUnit);

        drawGroup.DrawElements(PrimitiveType.Triangles);

        GL.BindVertexArray(array: 0);
        GL.UseProgram(program: 0);
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
    ~ScreenElementRenderer()
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
