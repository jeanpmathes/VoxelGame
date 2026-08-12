// <copyright file="InputAxis.cs" company="VoxelGame">
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
using VoxelGame.Client.Inputs.Actions;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Inputs.Composite;

/// <summary>
///     Combines two <see cref="Button" /> actions into a value between -1 and 1.
///     Dispose the axis to dispose both actions.
/// </summary>
public sealed class InputAxis : IDisposable
{
    private readonly Button negative;
    private readonly Button positive;
    private Boolean disposed;

    /// <summary>
    ///     Create an axis whose value increases with one action and decreases with the other.
    /// </summary>
    /// <param name="positive">The action that contributes 1 while active.</param>
    /// <param name="negative">The action that contributes -1 while active.</param>
    public InputAxis(Button positive, Button negative)
    {
        this.positive = positive;
        this.negative = negative;
    }

    /// <summary>
    ///     Get the combined value. Opposing active actions cancel each other out.
    /// </summary>
    public Single Value => (positive.IsActive ? 1 : 0) - (negative.IsActive ? 1 : 0);

    #region DISPOSABLE

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            positive.Dispose();
            negative.Dispose();
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
    ~InputAxis()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
