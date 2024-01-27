// <copyright file="ChunkMeshData.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Mesh data for an entire chunk.
/// </summary>
/// <param name="SectionMeshData"></param>
/// <param name="Sides"></param>
public sealed record ChunkMeshData(SectionMeshData[] SectionMeshData, BlockSides Sides) : IDisposable
{
    #region IDisposable Support

    private bool disposed;

    private void Dispose(bool disposing)
    {
        if (disposed) return;

        if (disposing)
            foreach (SectionMeshData mesh in SectionMeshData)
                mesh.Dispose();

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
    internal SectionMeshData((IMeshing, IMeshing) basicMeshing,
        IMeshing foliageMeshing,
        IMeshing fluidMeshing)
    {
        BasicMeshing = basicMeshing;
        FoliageMeshing = foliageMeshing;
        FluidMeshing = fluidMeshing;
    }

    /// <summary>
    ///     Get whether this mesh data is empty.
    /// </summary>
    public bool IsFilled => GetTotalSize() > 0;

    /// <summary>
    ///     The basic mesh data.
    ///     It is created by the <see cref="VoxelGame.Core.Visuals.Meshables.ISimple"/>,
    ///     <see cref="VoxelGame.Core.Visuals.Meshables.IComplex"/>, and
    ///     <see cref="VoxelGame.Core.Visuals.Meshables.IVaryingHeight"/> meshables.
    /// </summary>
    public (IMeshing opaque, IMeshing transparent) BasicMeshing { get; }

    /// <summary>
    ///     The foliage mesh data.
    ///     It is created by the <see cref="VoxelGame.Core.Visuals.Meshables.IFoliage" /> meshable.
    /// </summary>
    public IMeshing FoliageMeshing { get; }

    /// <summary>
    ///     The fluid mesh data.
    /// </summary>
    public IMeshing FluidMeshing { get; }

    private int GetTotalSize()
    {
        var size = 0;

        size += BasicMeshing.opaque.Count;
        size += BasicMeshing.transparent.Count;

        size += FoliageMeshing.Count;
        size += FluidMeshing.Count;

        return size;
    }

    #region IDisposable Support

    private bool disposed;

    private void Dispose(bool disposing)
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
