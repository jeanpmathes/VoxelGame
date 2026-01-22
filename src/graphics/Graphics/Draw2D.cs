// <copyright file="Draw2D.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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
using System.Runtime.InteropServices.Marshalling;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Graphics.Objects;
using VoxelGame.Toolkit.Interop;

namespace VoxelGame.Graphics.Graphics;

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

    internal delegate void DrawBufferDelegate(UInt32 firstVertex, UInt32 vertexCount, UInt32 textureIndex, Bool useTexture, IntPtr ctx);

    #pragma warning disable S3898 // No equality comparison used.
    [NativeMarshalling(typeof(InternalMarshaller))]
    [StructLayout(LayoutKind.Sequential)]
    internal struct Internal
    #pragma warning restore S3898 // No equality comparison used.
    {
        internal InitializeTexturesDelegate initializeTextures;
        internal UploadBufferDelegate uploadBuffer;
        internal DrawBufferDelegate drawBuffer;
        internal IntPtr ctx;
    }

    [CustomMarshaller(typeof(Internal), MarshalMode.ManagedToUnmanagedIn, typeof(InternalMarshaller))]
    [CustomMarshaller(typeof(Internal), MarshalMode.UnmanagedToManagedIn, typeof(InternalMarshaller))]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal static class InternalMarshaller
    {
        internal static Unmanaged ConvertToUnmanaged(Internal managed)
        {
            return new Unmanaged
            {
                initializeTextures = Marshal.GetFunctionPointerForDelegate(managed.initializeTextures),
                uploadBuffer = Marshal.GetFunctionPointerForDelegate(managed.uploadBuffer),
                drawBuffer = Marshal.GetFunctionPointerForDelegate(managed.drawBuffer),
                ctx = managed.ctx
            };
        }

        internal static Internal ConvertToManaged(Unmanaged unmanaged)
        {
            return new Internal
            {
                initializeTextures = Marshal.GetDelegateForFunctionPointer<InitializeTexturesDelegate>(unmanaged.initializeTextures),
                uploadBuffer = Marshal.GetDelegateForFunctionPointer<UploadBufferDelegate>(unmanaged.uploadBuffer),
                drawBuffer = Marshal.GetDelegateForFunctionPointer<DrawBufferDelegate>(unmanaged.drawBuffer),
                ctx = unmanaged.ctx
            };
        }

        internal static void Free(Unmanaged unmanaged)
        {
            // Nothing to free.
        }

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        internal struct Unmanaged
        {
            internal IntPtr initializeTextures;
            internal IntPtr uploadBuffer;
            internal IntPtr drawBuffer;
            internal IntPtr ctx;
        }
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

    internal delegate void Callback(InternalMarshaller.Unmanaged @internal);
}
