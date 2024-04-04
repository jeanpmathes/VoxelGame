// <copyright file="Draw2D.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using VoxelGame.Support.Objects;

namespace VoxelGame.Support.Graphics;

/// <summary>
///     Wraps the draw 2D functionality.
/// </summary>
#pragma warning disable S3898 // No equality comparison used.
public readonly unsafe struct Draw2D
#pragma warning restore S3898 // No equality comparison used.
{
    /// <summary>
    ///     Use this as a priority to add a pipeline that will be rendered before all other pipelines, thus in the background.
    /// </summary>
    public const Int32 Background = Int32.MinValue;

    /// <summary>
    ///     Use this as a priority to add a pipeline that will be rendered after all other pipelines, thus in the foreground
    ///     and on top of everything.
    /// </summary>
    public const Int32 Foreground = Int32.MaxValue;

    /// <summary>
    ///     A single vertex.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    #pragma warning disable S3898 // No equality comparison used.
    public struct Vertex
    #pragma warning restore S3898 // No equality comparison used.
    {
        /// <summary>
        ///     The position of the vertex.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        ///     The texture coordinate of the vertex.
        /// </summary>
        public Vector2 TextureCoordinate;

        /// <summary>
        ///     The color of the vertex.
        /// </summary>
        public Color4 Color;
    }

    internal delegate void InitializeTexturesDelegate(IntPtr textures, UInt32 textureCount, IntPtr ctx);

    internal delegate void UploadBufferDelegate(IntPtr vertices, UInt32 vertexCount, IntPtr ctx);

    internal delegate void DrawBufferDelegate(UInt32 firstVertex, UInt32 vertexCount, UInt32 textureIndex, [MarshalAs(UnmanagedType.Bool)] Boolean useTexture, IntPtr ctx);

    #pragma warning disable S3898 // No equality comparison used.
    internal struct Internal
    #pragma warning restore S3898 // No equality comparison used.
    {
#pragma warning disable CS0649 // Assigned by native code.
        internal readonly InitializeTexturesDelegate initializeTextures;
        internal readonly UploadBufferDelegate uploadBuffer;
        internal readonly DrawBufferDelegate drawBuffer;
        internal readonly IntPtr ctx;
#pragma warning restore CS0649 // Assigned by native code.
    }

    private readonly Internal @internal;

    internal Draw2D(Internal @internal)
    {
        this.@internal = @internal;
    }

    /// <summary>
    ///     Initializes all textures that can be used in subsequent draw calls.
    ///     This must be called at least once before any draw calls.
    ///     At least one texture must be provided.
    /// </summary>
    /// <param name="textures">The textures to initialize.</param>
    public void InitializeTextures(Span<Texture> textures)
    {
        var textureCount = (UInt32) textures.Length;

        var pointers = new IntPtr[textureCount];
        for (var i = 0; i < textureCount; i++) pointers[i] = textures[i].Self;

        fixed (IntPtr* texturesPointer = pointers)
        {
            @internal.initializeTextures((IntPtr) texturesPointer, textureCount, @internal.ctx);
        }
    }

    /// <summary>
    ///     Uploads a buffer of vertices to the GPU.
    ///     This replaces the current buffer, meaning that during one frame, only one buffer can be used.
    /// </summary>
    /// <param name="vertices">The vertices to upload.</param>
    public void UploadBuffer(Span<Vertex> vertices)
    {
        var vertexCount = (UInt32) vertices.Length;

        fixed (Vertex* verticesPointer = vertices)
        {
            @internal.uploadBuffer((IntPtr) verticesPointer, vertexCount, @internal.ctx);
        }
    }

    /// <summary>
    ///     Upload a buffer that contains exactly one quad.
    ///     The quad fills the whole space between <c>-1</c> and <c>1</c> in both dimensions.
    /// </summary>
    /// <param name="range">The range of vertices that were uploaded.</param>
    /// <param name="color">The color of the quad.</param>
    public void UploadQuadBuffer(out (UInt32, UInt32) range, Color4? color = null)
    {
        Color4 c = color ?? Color4.Black;

        Vertex bottomLeft = new()
        {
            Position = (-1, -1),
            TextureCoordinate = (0, 1),
            Color = c
        };

        Vertex topLeft = new()
        {
            Position = (-1, 1),
            TextureCoordinate = (0, 0),
            Color = c
        };

        Vertex topRight = new()
        {
            Position = (1, 1),
            TextureCoordinate = (1, 0),
            Color = c
        };

        Vertex bottomRight = new()
        {
            Position = (1, -1),
            TextureCoordinate = (1, 1),
            Color = c
        };

        Vertex[] vertices =
        [
            bottomLeft, topLeft, topRight,
            bottomLeft, topRight, bottomRight
        ];

        UploadBuffer(vertices);

        range = (0, (UInt32) vertices.Length);
    }

    /// <summary>
    ///     Draw parts of the current buffer.
    /// </summary>
    /// <param name="range">The range of vertices to draw, given as a tuple of the first vertex and the number of vertices.</param>
    /// <param name="textureIndex">
    ///     The index of the texture to use. Is ignored if <paramref name="useTexture" /> is false. When
    ///     using manual indexing in the shader, supply <c>0</c> here.
    /// </param>
    /// <param name="useTexture">Whether to use a texture.</param>
    public void DrawBuffer((UInt32 first, UInt32 lenght) range, UInt32 textureIndex, Boolean useTexture)
    {
        @internal.drawBuffer(range.first, range.lenght, textureIndex, useTexture, @internal.ctx);
    }

    internal delegate void Callback(Internal @internal);
}
