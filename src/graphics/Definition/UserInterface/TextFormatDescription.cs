// <copyright file="TextFormatDescription.cs" company="VoxelGame">
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
using VoxelGame.Annotations.Attributes;
using VoxelGame.Graphics.Interop;
using VoxelGame.GUI.Texts;

namespace VoxelGame.Graphics.Definition.UserInterface;

/// <summary>
///     Native UI text-format description mapping to <c>ui::TextFormatDescription</c>.
/// </summary>
[NativeMarshalling(typeof(TextFormatDescriptionMarshaller))]
[ValueSemantics]
[StructLayout(LayoutKind.Sequential)]
internal partial struct TextFormatDescription
{
    /// <summary>
    ///     The font family.
    /// </summary>
    internal String FontFamily;

    /// <summary>
    ///     The font weight.
    /// </summary>
    internal Int16 Weight;

    /// <summary>
    ///     The font style.
    /// </summary>
    internal Style Style;

    /// <summary>
    ///     The font stretch.
    /// </summary>
    internal Stretch Stretch;

    /// <summary>
    ///     The font size.
    /// </summary>
    internal Single Size;

    /// <summary>
    ///     The text wrapping mode.
    /// </summary>
    internal TextWrapping Wrapping;

    /// <summary>
    ///     The text alignment.
    /// </summary>
    internal TextAlignment Alignment;

    /// <summary>
    ///     The text trimming mode.
    /// </summary>
    internal TextTrimming Trimming;

    /// <summary>
    ///     The line height.
    /// </summary>
    internal Single LineHeight;
}

[CustomMarshaller(typeof(TextFormatDescription), MarshalMode.ManagedToUnmanagedIn, typeof(TextFormatDescriptionMarshaller))]
internal static class TextFormatDescriptionMarshaller
{
    internal static Unmanaged ConvertToUnmanaged(TextFormatDescription managed)
    {
        return new Unmanaged
        {
            fontFamily = UnicodeStringMarshaller.ConvertToUnmanaged(managed.FontFamily),
            weight = managed.Weight,
            style = (Byte) managed.Style,
            stretch = (Byte) managed.Stretch,
            size = managed.Size,
            wrapping = (Byte) managed.Wrapping,
            alignment = (Byte) managed.Alignment,
            trimming = (Byte) managed.Trimming,
            lineHeight = managed.LineHeight
        };
    }

    internal static void Free(Unmanaged unmanaged)
    {
        UnicodeStringMarshaller.Free(unmanaged.fontFamily);
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [StructLayout(LayoutKind.Sequential)]
    internal struct Unmanaged
    {
        internal IntPtr fontFamily;
        internal Int16 weight;
        internal Byte style;
        internal Byte stretch;
        internal Single size;
        internal Byte wrapping;
        internal Byte alignment;
        internal Byte trimming;
        internal Single lineHeight;
    }
}
