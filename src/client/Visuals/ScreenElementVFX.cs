// <copyright file="OverlayRenderer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Graphics;
using VoxelGame.Graphics.Objects;
using VoxelGame.Toolkit.Utilities;
using Image = VoxelGame.Core.Visuals.Image;

namespace VoxelGame.Client.Visuals;

#pragma warning disable S101 // Naming.

/// <summary>
///     Renders textures on the screen.
/// </summary>
public sealed class ScreenElementVFX : VFX
{
    private readonly VoxelGame.Graphics.Core.Client client;
    private readonly ShaderBuffer<Data> data;
    private readonly Vector2d relativeScreenPosition;
    private readonly Texture placeholder;
    private Texture? texture;
    private Single scaling = 1.0f;
    private ColorS color = ColorS.White;
    private Boolean isTextureInitialized;
    private Boolean isVertexBufferUploaded;
    private (UInt32 start, UInt32 length) rangeOfVertexBuffer;
    private IDisposable? disposable;

    private ScreenElementVFX(VoxelGame.Graphics.Core.Client client, Vector2d relativeScreenPosition, ShaderBuffer<Data> data)
    {
        this.client = client;
        this.relativeScreenPosition = relativeScreenPosition;
        this.data = data;

        placeholder = client.LoadTexture(Image.CreateFallback(size: 1));
    }

    /// <inheritdoc />
    public override Boolean IsEnabled { get; set; }

    /// <summary>
    ///     Create a new <see cref="ScreenElementVFX" />.
    /// </summary>
    /// <param name="client">The client instance.</param>
    /// <param name="factory">The factory creating the pipeline.</param>
    /// <param name="relativeScreenPosition">The position of the element on the screen, relative to the bottom left corner.</param>
    internal static ScreenElementVFX? Create(VoxelGame.Graphics.Core.Client client, PipelineFactory factory, Vector2d relativeScreenPosition)
    {
        (RasterPipeline pipeline, ShaderBuffer<Data> buffer)? result
            = factory.LoadPipelineWithBuffer<Data>("ScreenElement", new ShaderPresets.Draw2D(Filter.Closest));

        if (result is not {pipeline: var pipeline, buffer: var buffer}) return null;

        ScreenElementVFX vfx = new(client, relativeScreenPosition, buffer);

        vfx.disposable = client.AddDraw2dPipeline(pipeline, Draw2D.Background, vfx.Draw);

        return vfx;
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
    ///     Set the scale of the texture.
    /// </summary>
    /// <param name="newScaling">The new scale.</param>
    public void SetScale(Single newScaling)
    {
        scaling = newScaling;
    }

    /// <summary>
    ///     Set the color to apply to the texture.
    /// </summary>
    /// <param name="newColor">The new color.</param>
    public void SetColor(ColorS newColor)
    {
        color = newColor;
    }

    /// <summary>
    ///     Set the texture to display.
    /// </summary>
    /// <param name="newTexture">The new texture.</param>
    public void SetTexture(Texture newTexture)
    {
        texture = newTexture;
        isTextureInitialized = false;
    }

    /// <inheritdoc />
    protected override void OnLogicUpdate()
    {
        var screenSize = client.Size.ToVector2();

        Vector3d scale = new Vector3d(scaling, scaling, z: 1.0) * screenSize.Length * 0.5;

        Vector2d pixelOffset = (relativeScreenPosition - (0.5, 0.5)) * screenSize;
        Vector3d translation = new(pixelOffset);

        Matrix4d model = MathTools.CreateScaleMatrix(scale) * Matrix4d.CreateTranslation(translation);
        Matrix4d view = Matrix4d.Identity;
        var projection = Matrix4d.CreateOrthographic(client.Size.X, client.Size.Y, depthNear: 0.0, depthFar: 1.0);

        Matrix4d mvp = model * view * projection;
        Matrix4 mvpF = new((Vector4) mvp.Row0, (Vector4) mvp.Row1, (Vector4) mvp.Row2, (Vector4) mvp.Row3);

        data.Data = new Data(mvpF, color.ToColor4());
    }

    /// <summary>
    ///     Data used by the shader.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private readonly struct Data : IEquatable<Data>
    {
        /// <summary>
        ///     The model-view-projection matrix.
        /// </summary>
        public readonly Matrix4 MVP;

        /// <summary>
        ///     The color to apply to the texture.
        ///     If no texture is used, the vertex color is used instead and this is ignored.
        /// </summary>
        public readonly Color4 Color;

        /// <summary>
        ///     Create a new <see cref="Data" /> instance.
        /// </summary>
        public Data(Matrix4 mvp, Color4 color)
        {
            MVP = mvp;
            Color = color;
        }

        /// <summary>
        ///     Check equality.
        /// </summary>
        public Boolean Equals(Data other)
        {
            return (MVP, Color) == (other.MVP, other.Color);
        }

        /// <inheritdoc />
        public override Boolean Equals(Object? obj)
        {
            return obj is Data other && Equals(other);
        }

        /// <inheritdoc />
        public override Int32 GetHashCode()
        {
            return HashCode.Combine(MVP, Color);
        }

        /// <summary>
        ///     The equality operator.
        /// </summary>
        public static Boolean operator ==(Data left, Data right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     The inequality operator.
        /// </summary>
        public static Boolean operator !=(Data left, Data right)
        {
            return !left.Equals(right);
        }
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing) disposable?.Dispose();
        else Throw.ForMissedDispose(this);

        base.Dispose(disposing);

        disposed = true;
    }

    #endregion DISPOSABLE
}
