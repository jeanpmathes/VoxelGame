// <copyright file="ChunkMeshData.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Mesh data for an entire chunk.
/// </summary>
/// <param name="SectionMeshData">The mesh data included for the sections.</param>
/// <param name="Sides">The sides to consider to be meshed after applying the mesh data.</param>
/// <param name="Indices">The indices of the included sections.</param>
public sealed record ChunkMeshData(SectionMeshData?[] SectionMeshData, BlockSides Sides, IReadOnlyCollection<Int32> Indices) : IDisposable
{
    #region IDisposable Support

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

    #endregion IDisposable Support
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
    ///     It is created by the <see cref="VoxelGame.Core.Visuals.Meshables.ISimple" />,
    ///     <see cref="VoxelGame.Core.Visuals.Meshables.IComplex" />, and
    ///     <see cref="VoxelGame.Core.Visuals.Meshables.IVaryingHeight" /> meshables.
    /// </summary>
    public required (IMeshing opaque, IMeshing transparent) BasicMeshing { get; init; }

    /// <summary>
    ///     The foliage mesh data.
    ///     It is created by the <see cref="VoxelGame.Core.Visuals.Meshables.IFoliage" /> meshable.
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

    #region IDisposable Support

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

    #endregion IDisposable Support
}
