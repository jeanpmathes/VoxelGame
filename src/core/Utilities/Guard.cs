// <copyright file="Guard.cs" company="VoxelGame">
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
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Guards any held resource and calls appropriate methods on it when the guard is disposed.
/// </summary>
public sealed class Guard : IDisposable
{
    private readonly Action release;
    private readonly Object resource;
    private readonly String? source;

    /// <summary>
    ///     Create a new guard.
    /// </summary>
    /// <param name="resource">The resource to guard.</param>
    /// <param name="source">Where the guard was created.</param>
    /// <param name="release">The method to call when the guard is disposed.</param>
    public Guard(Object resource, String source, Action release)
    {
        this.resource = resource;
        this.source = source;
        this.release = release;
    }

    /// <summary>
    ///     Check if the guard is guarding an object.
    /// </summary>
    /// <param name="object">The object to check.</param>
    /// <returns>True if the guard is guarding the resource.</returns>
    public Boolean IsGuarding(Object @object)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        return resource == @object;
    }

    #region DISPOSABLE

    private Boolean disposed;

    /// <summary>
    ///     Dispose of this guard.
    /// </summary>
    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing) release();
        else ExceptionTools.ThrowForMissedDispose(resource, source);

        disposed = true;
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~Guard()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    ///     Dispose of this chunk.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion DISPOSABLE
}
