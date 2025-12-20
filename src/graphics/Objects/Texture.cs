// <copyright file="Texture.cs" company="VoxelGame">
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
using System.Runtime.InteropServices.Marshalling;
using OpenTK.Mathematics;
using VoxelGame.Graphics.Core;

namespace VoxelGame.Graphics.Objects;

/// <summary>
///     A texture.
/// </summary>
[NativeMarshalling(typeof(TextureMarshaller))]
public class Texture : NativeObject
{
    private readonly Vector2i size;

    /// <summary>
    ///     Create a new texture from a native pointer.
    /// </summary>
    internal Texture(IntPtr nativePointer, Client client, Vector2i size) : base(nativePointer, client)
    {
        this.size = size;
    }

    /// <summary>
    ///     Gets the width of the texture.
    /// </summary>
    public Int32 Width => size.X;

    /// <summary>
    ///     Gets the height of the texture.
    /// </summary>
    public Int32 Height => size.Y;

    /// <summary>
    ///     Frees the texture. Not allowed in same frame as creation.
    /// </summary>
    public void Free()
    {
        Deregister();
        NativeMethods.FreeTexture(this);
    }
}

#pragma warning disable S3242
[CustomMarshaller(typeof(Texture), MarshalMode.ManagedToUnmanagedIn, typeof(TextureMarshaller))]
internal static class TextureMarshaller
{
    internal static IntPtr ConvertToUnmanaged(Texture managed)
    {
        return managed.Self;
    }

    internal static void Free(IntPtr unmanaged)
    {
        // Nothing to do here.
    }
}
#pragma warning restore S3242
