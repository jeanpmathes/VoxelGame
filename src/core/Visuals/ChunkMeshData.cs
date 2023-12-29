// <copyright file="ChunkMeshData.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Mesh data for an entire chunk.
/// </summary>
/// <param name="SectionMeshData"></param>
/// <param name="Sides"></param>
public record ChunkMeshData(SectionMeshData[] SectionMeshData, BlockSides Sides)
{
    /// <summary>
    ///     Discard the mesh data.
    /// </summary>
    public void Discard()
    {
        foreach (SectionMeshData section in SectionMeshData) section.Discard();
    }

    /// <summary>
    ///     Return all pooled structures to their pools.
    /// </summary>
    public void ReturnPooled()
    {
        foreach (SectionMeshData section in SectionMeshData) section.Release();
    }
}

/// <summary>
///     Contains the mesh data for a section.
/// </summary>
public class SectionMeshData
{
    private bool isReturnedToPool;

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

    /// <summary>
    ///     Release all used resources. The data can only be returned once.
    /// </summary>
    public void Release()
    {
        Debug.Assert(!isReturnedToPool);

        BasicMeshing.opaque.Release();
        BasicMeshing.transparent.Release();

        FoliageMeshing.Release();
        FluidMeshing.Release();

        isReturnedToPool = true;
    }

    /// <summary>
    ///     Discard this mesh data.
    /// </summary>
    public void Discard()
    {
        if (isReturnedToPool) return;

        Release();
    }
}
