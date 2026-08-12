// <copyright file="DisposableNativeObject.cs" company="VoxelGame">
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
using VoxelGame.Graphics.Core;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Graphics.Objects;

/// <summary>
/// Abstract base class for native objects that can be disposed of.
/// </summary>
/// <param name="nativePointer">The native pointer for this object.</param>
/// <param name="client">The native client.</param>
public class DisposableNativeObject<T>(IntPtr nativePointer, Client client) : NativeObject(nativePointer, client), IDisposable
    where T : DisposableNativeObject<T>
{
    private Action<T>? handler;

    /// <summary>
    /// Set a function to be called when this object is disposed of.
    /// </summary>
    /// <param name="newHandler">The function to call. Overrides any previous handler.</param>
    public void SetDisposeHandler(Action<T> newHandler)
    {
        handler = newHandler;
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Override this method to do any cleanup.
    /// </summary>
    /// <param name="disposing"><see langword="true"/> if called from <see cref="Dispose()"/>, <see langword="false"/> if called from the finalizer.</param>
    protected virtual void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            Deregister();

            handler?.Invoke((T) this);
        }
        else
            ExceptionTools.ThrowForMissedDispose(this);

        disposed = true;
    }

    /// <summary>
    ///     The finalizer.
    /// </summary>
    ~DisposableNativeObject()
    {
        Dispose(disposing: false);
    }

    #endregion
}
