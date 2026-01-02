// <copyright file="TextureDescription.cs" company="VoxelGame">
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
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Graphics.Definition;

#pragma warning disable S3898 // No equality comparison used.

/// <summary>
///     Describes a texture.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct TextureDescription
{
    /// <summary>
    ///     The width of the texture.
    /// </summary>
    public UInt32 Width;

    /// <summary>
    ///     The height of the texture.
    /// </summary>
    public UInt32 Height;

    /// <summary>
    ///     The number of mip-levels in the texture.
    /// </summary>
    public UInt32 MipLevels;

    /// <summary>
    ///     The used color format.
    /// </summary>
    public ColorFormat ColorFormat;
}

/// <summary>
///     Supported texture color formats.
/// </summary>
public enum ColorFormat : byte
{
    /// <summary>
    ///     The red, green, blue and alpha channels are stored in 8 bits each.
    /// </summary>
    RGBA,

    /// <summary>
    ///     The blue, green, red and alpha channels are stored in 8 bits each.
    /// </summary>
    BGRA
}

/// <summary>
///     Utility to work with color formats.
/// </summary>
public static class ColorFormats
{
    /// <summary>
    ///     Get a color format from an image format.
    /// </summary>
    /// <param name="format">The image format.</param>
    /// <returns>The corresponding color format.</returns>
    public static ColorFormat ToNative(this Image.Format format)
    {
        if (format == Image.Format.RGBA) return ColorFormat.RGBA;
        if (format == Image.Format.BGRA) return ColorFormat.BGRA;

        throw Exceptions.UnsupportedValue(format);
    }
}
