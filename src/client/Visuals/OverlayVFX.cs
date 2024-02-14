// <copyright file="OverlayRenderer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Graphics;
using VoxelGame.Support.Objects;

namespace VoxelGame.Client.Visuals;

#pragma warning disable S101

/// <summary>
///     A VFX for overlay textures. Any block or fluid texture can be used as an overlay.
/// </summary>
public sealed class OverlayVFX : VFX
{
    private const int BlockMode = 0;
    private const int FluidMode = 1;

    private readonly Support.Core.Client client;
    private readonly ShaderBuffer<Data> data;

    private readonly (TextureArray block, TextureArray fluid) textures;

    private IDisposable? disposable;

    private int mode = BlockMode;
    private TintColor tint = TintColor.None;
    private bool isAnimated;

    private float lowerBound;
    private float upperBound;

    private int textureID;
    private int firstFluidTextureID;
    private bool isTextureInitialized;

    private bool isVertexBufferUploaded;
    private (uint start, uint length) rangeOfVertexBuffer;

    private OverlayVFX(Support.Core.Client client, ShaderBuffer<Data> data, (TextureArray, TextureArray) textures)
    {
        this.client = client;
        this.data = data;
        this.textures = textures;
    }

    /// <inheritdoc />
    public override bool IsEnabled { get; set; }

    /// <summary>
    ///     Create a new <see cref="OverlayVFX" />.
    /// </summary>
    /// <param name="client">The client instance.</param>
    /// <param name="pipelines">The pipelines object used to load the pipeline.</param>
    /// <param name="textures">The texture arrays, containing block and fluid textures.</param>
    /// <returns>The new VFX.</returns>
    public static OverlayVFX? Create(Support.Core.Client client, Pipelines pipelines, (TextureArray, TextureArray) textures)
    {
        (RasterPipeline pipeline, ShaderBuffer<Data> buffer)? result
            = pipelines.LoadPipelineWithBuffer<Data>(client, "Overlay", new ShaderPresets.Draw2D(Filter.Closest));

        if (result is not {pipeline: var pipeline, buffer: var buffer}) return null;

        OverlayVFX vfx = new(client, buffer, textures);

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

    /// <summary>
    ///     Set the texture to a block texture.
    /// </summary>
    /// <param name="overlay">The texture to use.</param>
    public void SetBlockTexture(OverlayTexture overlay)
    {
        Throw.IfDisposed(disposed);

        mode = BlockMode;

        SetAttributes(overlay);
    }

    /// <summary>
    ///     Set the texture to a fluid texture.
    /// </summary>
    /// <param name="overlay">The texture to use.</param>
    public void SetFluidTexture(OverlayTexture overlay)
    {
        Throw.IfDisposed(disposed);

        mode = FluidMode;

        SetAttributes(overlay);
    }

    /// <summary>
    ///     Set the bounds of the overlay.
    /// </summary>
    /// <param name="newLowerBound">The lower bound.</param>
    /// <param name="newUpperBound">The upper bound.</param>
    public void SetBounds(double newLowerBound, double newUpperBound)
    {
        Throw.IfDisposed(disposed);

        lowerBound = (float) newLowerBound;
        upperBound = (float) newUpperBound;
    }

    private void SetAttributes(OverlayTexture overlay)
    {
        Throw.IfDisposed(disposed);

        textureID = overlay.TextureIdentifier;
        tint = overlay.Tint;
        isAnimated = overlay.IsAnimated;
    }

    /// <inheritdoc />
    protected override void OnUpdate()
    {
        Matrix4d model = Matrix4d.Identity;
        Matrix4d view = Matrix4d.Identity;
        var projection = Matrix4d.CreateOrthographic(width: 2.0, 2.0 / client.AspectRatio, depthNear: 0.0, depthFar: 1.0);

        (uint, uint, uint, uint) attributes = (0, 0, 0, 0);

        Meshing.SetTextureIndex(ref attributes, textureID);
        Meshing.SetTint(ref attributes, tint);
        Meshing.SetFlag(ref attributes, Meshing.QuadFlag.IsAnimated, isAnimated);

        Matrix4d mvp = model * view * projection;

        data.Data = new Data(mvp.ToMatrix4(), attributes, (lowerBound, upperBound), mode, firstFluidTextureID);
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

    private static Vector4i EncodeAttributes((uint a, uint b, uint c, uint d) attributes)
    {
        return new Vector4i((int) attributes.a, (int) attributes.b, (int) attributes.c, (int) attributes.d);
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
        public readonly float LowerBound;

        /// <summary>
        ///     The upper bound of the overlay.
        /// </summary>
        public readonly float UpperBound;

        /// <summary>
        ///     The mode of the overlay, either block or fluid.
        /// </summary>
        public readonly int Mode;

        /// <summary>
        ///     The index of the first fluid texture.
        /// </summary>
        public readonly int FirstFluidTextureIndex;

        /// <summary>
        ///     Create a new <see cref="Data" /> instance.
        /// </summary>
        public Data(Matrix4 mvp, (uint, uint, uint, uint) attributes, (float lower, float upper) bounds, int mode, int firstFluidTextureIndex)
        {
            MVP = mvp;
            Attributes = EncodeAttributes(attributes);
            LowerBound = bounds.lower;
            UpperBound = bounds.upper;
            Mode = mode;
            FirstFluidTextureIndex = firstFluidTextureIndex;
        }

        /// <summary>
        ///     Check equality.
        /// </summary>
        public bool Equals(Data other)
        {
            return (MVP, Attributes, Mode, LowerBound, UpperBound, FirstFluidTextureIndex)
                   == (other.MVP, other.Attributes, other.Mode, other.LowerBound, other.UpperBound, other.FirstFluidTextureIndex);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Data other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(MVP, Attributes, Mode, LowerBound, UpperBound, FirstFluidTextureIndex);
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

    #region IDisposable Support

    private bool disposed;

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposed) return;

        if (disposing) disposable?.Dispose();
        else Throw.ForMissedDispose(nameof(OverlayVFX));

        base.Dispose(disposing);

        disposed = true;
    }

    #endregion IDisposable Support
}
