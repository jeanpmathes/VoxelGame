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
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex : IEquatable<Vertex>
    {
        /// <summary>
        ///     The position of the vertex.
        /// </summary>
        public readonly Vector2 Position;

        /// <summary>
        ///     The texture coordinate of the vertex.
        /// </summary>
        public readonly Vector2 TextureCoordinate;

        /// <summary>
        ///     The color of the vertex.
        /// </summary>
        public readonly Vector4 Color;

        /// <summary>
        ///     The equality operation.
        /// </summary>
        public bool Equals(Vertex other)
        {
            return Position.Equals(other.Position) && TextureCoordinate.Equals(other.TextureCoordinate) && Color.Equals(other.Color);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Vertex other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(Position, TextureCoordinate, Color);
        }

        /// <summary>
        ///     Whether two vertices are equal.
        /// </summary>
        public static bool operator ==(Vertex left, Vertex right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Whether two vertices are not equal.
        /// </summary>
        public static bool operator !=(Vertex left, Vertex right)
        {
            return !left.Equals(right);
        }
    }

    internal delegate void InitializeTexturesDelegate(IntPtr textures, uint textureCount, IntPtr ctx);

    internal delegate void DrawBufferDelegate(IntPtr vertices, uint vertexCount, uint textureIndex, [MarshalAs(UnmanagedType.Bool)] bool useTexture, IntPtr ctx);

    #pragma warning disable S3898 // No equality comparison used.
    internal struct Internal
    #pragma warning restore S3898 // No equality comparison used.
    {
#pragma warning disable CS0649 // Assigned by native code.
        internal readonly InitializeTexturesDelegate initializeTextures;
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
    /// </summary>
    /// <param name="textures">The textures to initialize.</param>
    public void InitializeTextures(ICollection<Texture> textures)
    {
        var textureCount = (uint) textures.Count;

        var pointers = new IntPtr[textureCount];
        for (var i = 0; i < textureCount; i++) pointers[i] = textures.ElementAt(i).Self;

        fixed (IntPtr* texturesPointer = pointers)
        {
            @internal.initializeTextures((IntPtr) texturesPointer, textureCount, @internal.ctx);
        }
    }

    /// <summary>
    ///     Draws a buffer of vertices.
    /// </summary>
    /// <param name="vertices">The vertices to draw.</param>
    /// <param name="textureIndex">The index of the texture to use.</param>
    /// <param name="useTexture">Whether to use a texture.</param>
    public void DrawBuffer(Vertex[] vertices, uint textureIndex, bool useTexture)
    {
        var vertexCount = (uint) vertices.Length;

        fixed (Vertex* verticesPointer = vertices)
        {
            @internal.drawBuffer((IntPtr) verticesPointer, vertexCount, textureIndex, useTexture, @internal.ctx);
        }
    }

    internal delegate void Callback(Internal @internal);
}
