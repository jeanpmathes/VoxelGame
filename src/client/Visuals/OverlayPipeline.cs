// <copyright file="OverlayRenderer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Graphics;
using VoxelGame.Graphics.Objects;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Visuals;

/// <summary>
///     A pipeline for rendering overlay textures. Any block or fluid texture can be used as an overlay.
///     This is a direct pipeline, meaning no instance objects are created and only a single overlay can be rendered at a
///     time.
/// </summary>
public sealed class OverlayPipeline : IDisposable
{
    private const Int32 BlockMode = 0;
    private const Int32 FluidMode = 1;

    private readonly VoxelGame.Graphics.Core.Client client;
    private readonly ShaderBuffer<Data> data;
    private readonly (TextureArray block, TextureArray fluid) textures;

    private IDisposable? disposable;
    private Int32 firstFluidTextureID;

    private Boolean isAnimated;
    private Boolean isTextureInitialized;
    private Boolean isVertexBufferUploaded;
    private Single lowerBound;

    private Int32 mode = BlockMode;
    private (UInt32 start, UInt32 length) rangeOfVertexBuffer;
    private Int32 textureID;
    private ColorS tint = ColorS.None;
    private Single upperBound;

    private OverlayPipeline(VoxelGame.Graphics.Core.Client client, ShaderBuffer<Data> data, (TextureArray, TextureArray) textures)
    {
        this.client = client;
        this.data = data;
        this.textures = textures;
    }

    /// <summary>
    ///     Whether rendering of the overlay is enabled.
    /// </summary>
    public Boolean IsEnabled { get; set; }

    /// <summary>
    ///     Create a new <see cref="OverlayPipeline" />.
    /// </summary>
    /// <param name="client">The client instance.</param>
    /// <param name="factory">The factory to create the pipeline.</param>
    /// <param name="textures">The texture arrays, containing block and fluid textures.</param>
    /// <returns>The new VFX.</returns>
    internal static OverlayPipeline? Create(VoxelGame.Graphics.Core.Client client, PipelineFactory factory, (TextureArray, TextureArray) textures)
    {
        (RasterPipeline pipeline, ShaderBuffer<Data> buffer)? loaded
            = factory.LoadPipelineWithBuffer<Data>("Overlay", new ShaderPresets.Draw2D(Filter.Closest));

        if (loaded is not {pipeline: var pipeline, buffer: var buffer}) return null;

        OverlayPipeline created = new(client, buffer, textures);

        created.disposable = client.AddDraw2dPipeline(pipeline, Draw2D.Background, created.Draw);

        return created;
    }

    /// <summary>
    ///     Set the texture to a block texture.
    /// </summary>
    /// <param name="overlay">The texture to use.</param>
    public void SetBlockTexture(OverlayTexture overlay)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        mode = BlockMode;

        SetAttributes(overlay);
    }

    /// <summary>
    ///     Set the texture to a fluid texture.
    /// </summary>
    /// <param name="overlay">The texture to use.</param>
    public void SetFluidTexture(OverlayTexture overlay)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        mode = FluidMode;

        SetAttributes(overlay);
    }

    /// <summary>
    ///     Set the bounds of the overlay.
    /// </summary>
    /// <param name="newLowerBound">The lower bound.</param>
    /// <param name="newUpperBound">The upper bound.</param>
    public void SetBounds(Double newLowerBound, Double newUpperBound)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        lowerBound = (Single) newLowerBound;
        upperBound = (Single) newUpperBound;
    }

    private void SetAttributes(OverlayTexture overlay)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        textureID = overlay.TextureIndex;
        tint = overlay.Tint;
        isAnimated = overlay.IsAnimated;
    }

    /// <summary>
    ///     Perform the logic update for the overlay pipeline.
    /// </summary>
    public void LogicUpdate()
    {
        Matrix4d model = Matrix4d.Identity;
        Matrix4d view = Matrix4d.Identity;
        var projection = Matrix4d.CreateOrthographic(width: 2.0, 2.0 / client.AspectRatio, depthNear: 0.0, depthFar: 1.0);

        (UInt32, UInt32, UInt32, UInt32) attributes = (0, 0, 0, 0);

        Meshing.SetTextureIndex(ref attributes, textureID);
        Meshing.SetTint(ref attributes, tint);
        Meshing.SetFlag(ref attributes, Meshing.QuadFlag.IsAnimated, isAnimated);

        Matrix4d mvp = model * view * projection;
        Matrix4 mvpF = new((Vector4) mvp.Row0, (Vector4) mvp.Row1, (Vector4) mvp.Row2, (Vector4) mvp.Row3);

        data.Data = new Data(mvpF, attributes, (lowerBound, upperBound), mode, firstFluidTextureID);
    }

    private void Draw(Draw2D drawer)
    {
        if (!IsEnabled) return;

        if (!isTextureInitialized)
        {
            firstFluidTextureID = textures.block.Count;

            using PooledList<Texture> list = new();
            list.AddRange(textures.block.AsSpan());
            list.AddRange(textures.fluid.AsSpan());

            drawer.InitializeTextures(list.AsSpan());
            isTextureInitialized = true;
        }

        if (!isVertexBufferUploaded)
        {
            drawer.UploadQuadBuffer(out rangeOfVertexBuffer);
            isVertexBufferUploaded = true;
        }

        // Because the shader indexes into the array itself, we don't need to pass the index here.
        // This is necessary to allow for animated overlays which use index offsets.
        drawer.DrawBuffer(rangeOfVertexBuffer, textureIndex: 0, useTexture: true);
    }

    /// <summary>
    ///     Data used by the shader.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private readonly struct Data : IEquatable<Data>
    {
        /// <summary>
        ///     The matrix used to transform the vertices.
        /// </summary>
        public readonly Matrix4 MVP;

        /// <summary>
        ///     Attributes encoded as binary data, including tint and animation.
        /// </summary>
        public readonly Vector4i Attributes;

        /// <summary>
        ///     The lower bound of the overlay.
        /// </summary>
        public readonly Single LowerBound;

        /// <summary>
        ///     The upper bound of the overlay.
        /// </summary>
        public readonly Single UpperBound;

        /// <summary>
        ///     The mode of the overlay, either block or fluid.
        /// </summary>
        public readonly Int32 Mode;

        /// <summary>
        ///     The index of the first fluid texture.
        /// </summary>
        public readonly Int32 FirstFluidTextureIndex;

        /// <summary>
        ///     Create a new <see cref="Data" /> instance.
        /// </summary>
        public Data(Matrix4 mvp, (UInt32, UInt32, UInt32, UInt32) attributes, (Single lower, Single upper) bounds, Int32 mode, Int32 firstFluidTextureIndex)
        {
            MVP = mvp;
            Attributes = EncodeAttributes(attributes);
            LowerBound = bounds.lower;
            UpperBound = bounds.upper;
            Mode = mode;
            FirstFluidTextureIndex = firstFluidTextureIndex;
        }

        private static Vector4i EncodeAttributes((UInt32 a, UInt32 b, UInt32 c, UInt32 d) attributes)
        {
            return new Vector4i((Int32) attributes.a, (Int32) attributes.b, (Int32) attributes.c, (Int32) attributes.d);
        }

        /// <summary>
        ///     Check equality.
        /// </summary>
        public Boolean Equals(Data other)
        {
            return (MVP, Attributes, Mode, LowerBound, UpperBound, FirstFluidTextureIndex)
                   == (other.MVP, other.Attributes, other.Mode, other.LowerBound, other.UpperBound, other.FirstFluidTextureIndex);
        }

        /// <inheritdoc />
        public override Boolean Equals(Object? obj)
        {
            return obj is Data other && Equals(other);
        }

        /// <inheritdoc />
        public override Int32 GetHashCode()
        {
            return HashCode.Combine(MVP, Attributes, Mode, LowerBound, UpperBound, FirstFluidTextureIndex);
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

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
            disposable?.Dispose();

        disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~OverlayPipeline()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
