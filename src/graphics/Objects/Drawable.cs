// <copyright file="IDrawable.cs" company="VoxelGame">
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
using VoxelGame.Graphics.Core;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Graphics.Objects;

/// <summary>
///     The common abstract base class of objects drawn in 3D space.
/// </summary>
[NativeMarshalling(typeof(DrawableMarshaller))]
public class Drawable : Spatial, IDisposable
{
    private Boolean enabled;

    /// <summary>
    ///     Create a new drawable object.
    /// </summary>
    /// <param name="nativePointer">The native pointer.</param>
    /// <param name="space">The space.</param>
    protected Drawable(IntPtr nativePointer, Space space) : base(nativePointer, space) {}

    /// <summary>
    ///     Set or get the enabled state of this object. If disabled, the object will not be rendered.
    /// </summary>
    public Boolean IsEnabled
    {
        get => enabled;
        set
        {
            if (value == enabled) return;

            ExceptionTools.ThrowIfDisposed(disposed);

            NativeMethods.SetDrawableEnabledState(this, value);
            enabled = value;
        }
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <summary>
    ///     Override to implement custom dispose logic.
    /// </summary>
    protected virtual void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            Deregister();
            NativeMethods.ReturnDrawable(this);
        }
        else
        {
            ExceptionTools.ThrowForMissedDispose(this);
        }

        disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     The finalizer.
    /// </summary>
    ~Drawable()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}

#pragma warning disable S3242
[CustomMarshaller(typeof(Drawable), MarshalMode.ManagedToUnmanagedIn, typeof(DrawableMarshaller))]
internal static class DrawableMarshaller
{
    internal static IntPtr ConvertToUnmanaged(Drawable managed)
    {
        return managed.Self;
    }

    internal static void Free(IntPtr unmanaged)
    {
        // Nothing to do here.
    }
}
#pragma warning restore S3242
