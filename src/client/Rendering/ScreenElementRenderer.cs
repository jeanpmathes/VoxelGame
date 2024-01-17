// <copyright file="OverlayRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Graphics;
using VoxelGame.Support.Objects;
using Image = VoxelGame.Core.Visuals.Image;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     Renders textures on the screen.
/// </summary>
public sealed class ScreenElementRenderer : Renderer
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<ScreenElementRenderer>();

    private readonly Support.Core.Client client;
    private readonly Vector2d relativeScreenPosition;
    private readonly ShaderBuffer<Data> data;

    private readonly Texture placeholder;

    private float scaling = 1.0f;

    private Color4 color = Color4.White;

    private Texture? texture;
    private bool isTextureInitialized;

    private bool isVertexBufferUploaded;
    private (uint start, uint length) rangeOfVertexBuffer;

    private ScreenElementRenderer(Support.Core.Client client, Vector2d relativeScreenPosition, ShaderBuffer<Data> data)
    {
        this.client = client;
        this.relativeScreenPosition = relativeScreenPosition;
        this.data = data;

        placeholder = client.LoadTexture(Image.CreateFallback(size: 1));
    }

    /// <inheritdoc />
    public override bool IsEnabled { get; set; }

    /// <summary>
    /// Create a new <see cref="ScreenElementRenderer"/>.
    /// </summary>
    /// <param name="client">The client instance.</param>
    /// <param name="pipelines">The pipelines object used to load the pipeline.</param>
    /// <param name="relativeScreenPosition">The position of the element on the screen, relative to the bottom left corner.</param>
    public static ScreenElementRenderer? Create(Support.Core.Client client, Pipelines pipelines, Vector2d relativeScreenPosition)
    {
        (RasterPipeline pipeline, ShaderBuffer<Data> buffer)? result
            = pipelines.LoadPipelineWithBuffer<Data>(client, "ScreenElement", new ShaderPresets.Draw2D(Filter.Closest));

        if (result is not {pipeline: var pipeline, buffer: var buffer}) return null;

        ScreenElementRenderer renderer = new(client, relativeScreenPosition, buffer);

        client.AddDraw2dPipeline(pipeline, Draw2D.Background, renderer.Draw);

        return renderer;
    }

    /// <inheritdoc />
    protected override void OnSetUp()
    {
        // Intentionally left empty.
    }

    /// <inheritdoc />
    protected override void OnTearDown()
    {
        // Intentionally left empty.
    }

    private void Draw(Draw2D drawer)
    {
        if (!IsEnabled) return;

        if (!isTextureInitialized)
        {
            drawer.InitializeTextures(new[] {texture ?? placeholder});
            isTextureInitialized = true;
        }

        if (!isVertexBufferUploaded)
        {
            drawer.UploadQuadBuffer(out rangeOfVertexBuffer);
            isVertexBufferUploaded = true;
        }

        drawer.DrawBuffer(rangeOfVertexBuffer, textureIndex: 0, texture != null);
    }

    /// <summary>
    /// Set the scale of the texture.
    /// </summary>
    /// <param name="newScaling">The new scale.</param>
    public void SetScale(float newScaling)
    {
        scaling = newScaling;
    }

    /// <summary>
    ///     Set the color to apply to the texture.
    /// </summary>
    /// <param name="newColor">The new color.</param>
    public void SetColor(Color newColor)
    {
        color = newColor;
    }

    /// <summary>
    ///     Set the texture to use for rendering.
    /// </summary>
    /// <param name="newTexture">The new texture.</param>
    public void SetTexture(Texture newTexture)
    {
        texture = newTexture;
        isTextureInitialized = false;
    }

    /// <inheritdoc />
    protected override void OnUpdate()
    {
        var screenSize = client.Size.ToVector2();

        Vector3d scale = new Vector3d(scaling, scaling, z: 1.0) * screenSize.Length * 0.5;

        Vector2d pixelOffset = (relativeScreenPosition - (0.5, 0.5)) * screenSize;
        Vector3d translation = new(pixelOffset);

        Matrix4d model = VMath.CreateScaleMatrix(scale) * Matrix4d.CreateTranslation(translation);
        Matrix4d view = Matrix4d.Identity;
        var projection = Matrix4d.CreateOrthographic(Screen.Size.X, Screen.Size.Y, depthNear: 0.0, depthFar: 1.0);

        data.Data = new Data
        {
            MVP = (model * view * projection).ToMatrix4(),
            Color = color
        };
    }

    #region IDisposable Support

    /// <inheritdoc />
    protected override void OnDispose(bool disposing)
    {
        if (disposing) ; // todo: dispose raster pipeline
        else
            logger.LogWarning(
                Events.LeakedNativeObject,
                "Renderer disposed by GC without freeing storage");
    }

    #endregion IDisposable Support

    /// <summary>
    ///     Data used by the shader.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct Data : IEquatable<Data>
    {
        /// <summary>
        ///     The model-view-projection matrix.
        /// </summary>
        public Matrix4 MVP;

        /// <summary>
        ///     The color to apply to the texture.
        ///     If no texture is used, the vertex color is used instead and this is ignored.
        /// </summary>
        public Color4 Color;

        /// <summary>
        ///     Check equality.
        /// </summary>
        public bool Equals(Data other)
        {
            return (MVP, Color) == (other.MVP, other.Color);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Data other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(MVP, Color);
        }

        /// <summary>
        ///     The equality operator.
        /// </summary>
        public static bool operator ==(Data left, Data right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     The inequality operator.
        /// </summary>
        public static bool operator !=(Data left, Data right)
        {
            return !left.Equals(right);
        }
    }
}
