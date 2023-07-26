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
        public Vector4 Color;
    }

    internal delegate void InitializeTexturesDelegate(IntPtr textures, uint textureCount, IntPtr ctx);

    internal delegate void UploadBufferDelegate(IntPtr vertices, uint vertexCount, IntPtr ctx);

    internal delegate void DrawBufferDelegate(uint firstVertex, uint vertexCount, uint textureIndex, [MarshalAs(UnmanagedType.Bool)] bool useTexture, IntPtr ctx);

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
        var textureCount = (uint) textures.Length;

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
        var vertexCount = (uint) vertices.Length;

        fixed (Vertex* verticesPointer = vertices)
        {
            @internal.uploadBuffer((IntPtr) verticesPointer, vertexCount, @internal.ctx);
        }
    }

    /// <summary>
    ///     Draw parts of the current buffer.
    /// </summary>
    /// <param name="range">The range of vertices to draw, given as a tuple of the first vertex and the number of vertices.</param>
    /// <param name="textureIndex">The index of the texture to use. Is ignored if <paramref name="useTexture" /> is false.</param>
    /// <param name="useTexture">Whether to use a texture.</param>
    public void DrawBuffer((uint first, uint lenght) range, uint textureIndex, bool useTexture)
    {
        @internal.drawBuffer(range.first, range.lenght, textureIndex, useTexture, @internal.ctx);
    }

    internal delegate void Callback(Internal @internal);
}
