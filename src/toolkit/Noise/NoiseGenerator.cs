// <copyright file="NoiseGenerator.cs" company="VoxelGame">
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
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using VoxelGame.Toolkit.Collections;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Toolkit.Noise;

/// <summary>
///     Wraps around the noise generation library linked in the native code.
///     Use the <see cref="NoiseBuilder" /> to create a new noise generator.
/// </summary>
public sealed class NoiseGenerator : IDisposable
{
    private readonly IntPtr self;

    /// <summary>
    ///     Create a new noise generator.
    /// </summary>
    /// <param name="definition">The definition of the noise generator.</param>
    public NoiseGenerator(NoiseDefinition definition)
    {
        self = Native.CreateNoise(definition);
    }

    /// <summary>
    ///     Get the noise value at the given position.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Single GetNoise(Vector2d position)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        return Native.GetNoise2D(self, position);
    }

    /// <summary>
    ///     Get the noise value at the given position.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Single GetNoise(Vector3d position)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        return Native.GetNoise3D(self, position);
    }

    /// <summary>
    ///     Get the noise value for a grid of points, starting at the given position.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Array2D<Single> GetNoiseGrid(Vector2i position, Int32 size)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        Array2D<Single> result = new(size, transpose: true);
        Native.GetNoiseGrid2D(self, position, new Vector2i(size), result.AsSpan());

        return result;
    }

    /// <summary>
    ///     Get the noise value for a grid of points, starting at the given position.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Array3D<Single> GetNoiseGrid(Vector3i position, Int32 size)
    {
        ExceptionTools.ThrowIfDisposed(disposed);

        Array3D<Single> result = new(size, transpose: true);
        Native.GetNoiseGrid3D(self, position, new Vector3i(size), result.AsSpan());

        return result;
    }

    #region DISPOSABLE

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing) Native.DeleteNoise(self);
        else ExceptionTools.ThrowForMissedDispose(this);

        disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~NoiseGenerator()
    {
        Dispose(disposing: false);
    }

    #endregion
}
