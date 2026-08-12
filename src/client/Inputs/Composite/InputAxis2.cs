// <copyright file="InputAxis2.cs" company="VoxelGame">
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
using OpenTK.Mathematics;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Inputs.Composite;

/// <summary>
///     Combines two <see cref="InputAxis" /> values into a two-dimensional input value.
///     Dispose the two-dimensional axis to dispose both component axes.
/// </summary>
/// <param name="x">The axis that provides the horizontal component.</param>
/// <param name="y">The axis that provides the vertical component.</param>
public sealed class InputAxis2(InputAxis x, InputAxis y) : IDisposable
{
    private Boolean disposed;

    /// <summary>
    ///     Get the current horizontal and vertical input values.
    /// </summary>
    public Vector2 Value => new(x.Value, y.Value);

    #region DISPOSABLE

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            x.Dispose();
            y.Dispose();
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
    ~InputAxis2()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
