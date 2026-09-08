// <copyright file="Brush.cs" company="VoxelGame">
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
///     User-interface brush wrapper.
/// </summary>
[NativeMarshalling(typeof(BrushMarshaller))]
public sealed class Brush : DisposableNativeObject<Brush>
{
    internal Brush(IntPtr nativePointer, Renderer renderer) : base(nativePointer, renderer.NativeClient)
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
                NativeMethods.ReturnUserInterfaceBrush(this);
        }

        base.Dispose(disposing);
    }

    #endregion DISPOSABLE
}

[CustomMarshaller(typeof(Brush), MarshalMode.ManagedToUnmanagedIn, typeof(BrushMarshaller))]
internal static class BrushMarshaller
{
    internal static IntPtr ConvertToUnmanaged(Brush managed)
    {
        return managed.Self;
    }

    internal static void Free(IntPtr unmanaged)
    {
        // Nothing to do here.
    }
}
