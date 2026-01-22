// <copyright file="ChunkMeshData.cs" company="VoxelGame">
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
using System.Collections.Generic;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Mesh data for an entire chunk.
/// </summary>
/// <param name="SectionMeshData">The mesh data included for the sections.</param>
/// <param name="Sides">The sides to consider to be meshed after applying the mesh data.</param>
/// <param name="Indices">The indices of the included sections.</param>
public sealed record ChunkMeshData(SectionMeshData?[] SectionMeshData, Sides Sides, IReadOnlyCollection<Int32> Indices) : IDisposable
{
    #region DISPOSABLE

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
            foreach (SectionMeshData? mesh in SectionMeshData)
                mesh?.Dispose();

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
    ~ChunkMeshData()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}

/// <summary>
///     Contains the mesh data for a section.
/// </summary>
public sealed class SectionMeshData : IDisposable
{
    /// <summary>
    ///     Get whether this mesh data is empty.
    /// </summary>
    public Boolean IsFilled => GetTotalSize() > 0;

    /// <summary>
    ///     The basic mesh data.
    ///     It is created by the <see cref="SimpleBlock" />,
    ///     <see cref="ComplexBlock" />, and
    ///     <see cref="PartialHeightBlock" /> meshables.
    /// </summary>
    public required (IMeshing opaque, IMeshing transparent) BasicMeshing { get; init; }

    /// <summary>
    ///     The foliage mesh data.
    ///     It is created by the <see cref="FoliageBlock" /> meshable.
    /// </summary>
    public required IMeshing FoliageMeshing { get; init; }

    /// <summary>
    ///     The fluid mesh data.
    /// </summary>
    public required IMeshing FluidMeshing { get; init; }

    private Int32 GetTotalSize()
    {
        var size = 0;

        size += BasicMeshing.opaque.Count;
        size += BasicMeshing.transparent.Count;

        size += FoliageMeshing.Count;
        size += FluidMeshing.Count;

        return size;
    }

    #region DISPOSABLE

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            BasicMeshing.opaque.Dispose();
            BasicMeshing.transparent.Dispose();

            FoliageMeshing.Dispose();
            FluidMeshing.Dispose();
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
    ///     Finalizer.
    /// </summary>
    ~SectionMeshData()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
