// <copyright file="ChunkMeshData.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics;
using VoxelGame.Core.Collections;
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
        foreach (SectionMeshData section in SectionMeshData) section.ReturnPooled();
    }
}

/// <summary>
///     Contains the mesh data for a section.
/// </summary>
public class SectionMeshData
{
    private bool isReturnedToPool;

    internal SectionMeshData((PooledList<SpatialVertex>, PooledList<SpatialVertex>) basicMesh,
        PooledList<SpatialVertex> foliageMesh,
        PooledList<SpatialVertex> fluidMesh)
    {
        BasicMesh = basicMesh;
        FoliageMesh = foliageMesh;
        FluidMesh = fluidMesh;
    }

    private SectionMeshData()
    {
        BasicMesh = (new PooledList<SpatialVertex>(), new PooledList<SpatialVertex>());
        FoliageMesh = new PooledList<SpatialVertex>();
        FluidMesh = new PooledList<SpatialVertex>();
    }

    /// <summary>
    ///     Create an empty mesh data instance.
    /// </summary>
    public static SectionMeshData Empty => new();

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
    public (PooledList<SpatialVertex> opaque, PooledList<SpatialVertex> transparent) BasicMesh { get; }

    /// <summary>
    ///     The foliage mesh data.
    ///     It is created by the <see cref="VoxelGame.Core.Visuals.Meshables.IFoliage" /> meshable.
    /// </summary>
    public PooledList<SpatialVertex> FoliageMesh { get; }

    /// <summary>
    ///     The fluid mesh data.
    /// </summary>
    public PooledList<SpatialVertex> FluidMesh { get; }

    private int GetTotalSize()
    {
        var size = 0;

        size += BasicMesh.opaque.Count;
        size += BasicMesh.transparent.Count;

        size += FoliageMesh.Count;
        size += FluidMesh.Count;

        return size;
    }

    /// <summary>
    ///     Return all pooled lists to the pool. The data can only be returned once.
    /// </summary>
    public void ReturnPooled()
    {
        Debug.Assert(!isReturnedToPool);

        BasicMesh.opaque.ReturnToPool();
        BasicMesh.transparent.ReturnToPool();

        FoliageMesh.ReturnToPool();
        FluidMesh.ReturnToPool();

        isReturnedToPool = true;
    }

    /// <summary>
    ///     Discard this mesh data.
    /// </summary>
    public void Discard()
    {
        if (isReturnedToPool) return;

        ReturnPooled();
    }
}
