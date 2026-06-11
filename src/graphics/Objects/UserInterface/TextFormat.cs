// <copyright file="TextFormat.cs" company="VoxelGame">
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
using System.Runtime.InteropServices.Marshalling;

namespace VoxelGame.Graphics.Objects.UserInterface;

/// <summary>
///     Native user-interface text-format wrapper.
/// </summary>
[NativeMarshalling(typeof(TextFormatMarshaller))]
public sealed class TextFormat : DisposableNativeObject<TextFormat>
{
    internal TextFormat(IntPtr nativePointer, Renderer renderer) : base(nativePointer, renderer.NativeClient)
    {
        Renderer = renderer;
    }

    internal Renderer Renderer { get; }

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc/>
    protected override void Dispose(Boolean disposing)
    {
        if (!disposed)
        {
            disposed = true;

            if (disposing)
                NativeMethods.ReturnUserInterfaceTextFormat(this);
        }

        base.Dispose(disposing);
    }

    #endregion DISPOSABLE
}

[CustomMarshaller(typeof(TextFormat), MarshalMode.ManagedToUnmanagedIn, typeof(TextFormatMarshaller))]
internal static class TextFormatMarshaller
{
    internal static IntPtr ConvertToUnmanaged(TextFormat managed)
    {
        return managed.Self;
    }

    internal static void Free(IntPtr unmanaged)
    {
        // Nothing to do here.
    }
}
