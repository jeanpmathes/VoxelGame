// <copyright file="Disposer.cs" company="VoxelGame">
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
///     Utility class to create a <see cref="IDisposable" /> object from an action.
/// </summary>
public sealed class Disposer : IDisposable
{
    private readonly Action dispose;

    private Boolean disposed;

    /// <summary>
    ///     Create a new <see cref="Disposer" />.
    /// </summary>
    /// <param name="dispose">The dispose action, will only be called if the dispose method is called.</param>
    public Disposer(Action dispose)
    {
        this.dispose = dispose;
    }

    /// <summary>
    ///     Create a new <see cref="Disposer" /> with an empty dispose action.
    /// </summary>
    public Disposer()
    {
        dispose = DoNothing;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private static void DoNothing()
    {
        // Intentionally does nothing.
    }

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing) dispose();
        else ExceptionTools.ThrowForMissedDispose(this);

        disposed = true;
    }

    /// <summary>
    ///     The finalizer.
    /// </summary>
    ~Disposer()
    {
        Dispose(disposing: false);
    }
}
