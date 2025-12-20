// <copyright file="OverlayRenderer.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Graphics;
using VoxelGame.Graphics.Objects;
using VoxelGame.Toolkit.Utilities.Constants;
using Image = VoxelGame.Core.Visuals.Image;

namespace VoxelGame.Client.Visuals;

/// <summary>
///     Renders a texture on the screen.
/// </summary>
public sealed class ScreenElementPipeline : IDisposable
{
    private readonly VoxelGame.Graphics.Core.Client client;
    private readonly ShaderBuffer<Data> data;
    private readonly Texture placeholder;
    private readonly Vector2d relativeScreenPosition;
    private ColorS color = ColorS.White;
    private IDisposable? disposable;
    private Boolean isTextureInitialized;
    private Boolean isVertexBufferUploaded;
    private (UInt32 start, UInt32 length) rangeOfVertexBuffer;
    private Single scaling = 1.0f;

    private Texture? texture;

    private ScreenElementPipeline(VoxelGame.Graphics.Core.Client client, Vector2d relativeScreenPosition, ShaderBuffer<Data> data)
    {
        this.client = client;
        this.relativeScreenPosition = relativeScreenPosition;
        this.data = data;

        placeholder = client.LoadTexture(Image.CreateFallback(size: 1));
    }

    /// <summary>
    ///     Whether the pipeline is enabled.
    /// </summary>
    public Boolean IsEnabled { get; set; }

    /// <summary>
    ///     Create a new <see cref="ScreenElementPipeline" />.
    /// </summary>
    /// <param name="client">The client instance.</param>
    /// <param name="factory">The factory creating the pipeline.</param>
    /// <param name="relativeScreenPosition">The position of the element on the screen, relative to the bottom left corner.</param>
    internal static ScreenElementPipeline? Create(VoxelGame.Graphics.Core.Client client, PipelineFactory factory, Vector2d relativeScreenPosition)
    {
        (RasterPipeline pipeline, ShaderBuffer<Data> buffer)? loaded
            = factory.LoadPipelineWithBuffer<Data>("ScreenElement", new ShaderPresets.Draw2D(Filter.Closest));

        if (loaded is not {pipeline: var pipeline, buffer: var buffer}) return null;

        ScreenElementPipeline created = new(client, relativeScreenPosition, buffer);

        created.disposable = client.AddDraw2dPipeline(pipeline, Draw2D.Background, created.Draw);

        return created;
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

    /// <summary>
    ///     Call each logic update.
    /// </summary>
    public void LogicUpdate()
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
    private readonly struct Data : IEquatable<Data>, IDefault<Data>
    {
        /// <inheritdoc />
        [UsedImplicitly] public static Data Default => new();

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

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing) disposable?.Dispose();

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
    ~ScreenElementPipeline()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
