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
        PooledList<int> varyingHeightVertexData, PooledList<uint> varyingHeightIndices,
        PooledList<int> crossPlantVertexData,
        PooledList<int> cropPlantVertexData,
        PooledList<int> opaqueFluidVertexData, PooledList<uint> opaqueFluidIndices,
        PooledList<int> transparentFluidVertexData, PooledList<uint> transparentFluidIndices)
    {
        BasicMesh = basicMesh;

        this.varyingHeightVertexData = varyingHeightVertexData;
        this.varyingHeightIndices = varyingHeightIndices;

        this.crossPlantVertexData = crossPlantVertexData;

        this.cropPlantVertexData = cropPlantVertexData;

        this.opaqueFluidVertexData = opaqueFluidVertexData;
        this.opaqueFluidIndices = opaqueFluidIndices;

        this.transparentFluidVertexData = transparentFluidVertexData;
        this.transparentFluidIndices = transparentFluidIndices;
    }

    private SectionMeshData()
    {
        BasicMesh = (new PooledList<SpatialVertex>(), new PooledList<SpatialVertex>());

        varyingHeightVertexData = new PooledList<int>();
        varyingHeightIndices = new PooledList<uint>();

        crossPlantVertexData = new PooledList<int>();

        cropPlantVertexData = new PooledList<int>();

        opaqueFluidVertexData = new PooledList<int>();
        opaqueFluidIndices = new PooledList<uint>();

        transparentFluidVertexData = new PooledList<int>();
        transparentFluidIndices = new PooledList<uint>();
    }

    /// <summary>
    ///     Create an empty mesh data instance.
    /// </summary>
    public static SectionMeshData Empty => new();

    /// <summary>
    ///     Get whether this mesh data is empty.
    /// </summary>
    public bool IsFilled => BasicMesh.opaque.Count != 0 || BasicMesh.transparent.Count != 0 ||
                            varyingHeightVertexData.Count != 0 || crossPlantVertexData.Count != 0 ||
                            cropPlantVertexData.Count != 0 || opaqueFluidVertexData.Count != 0 ||
                            transparentFluidVertexData.Count != 0;

    /// <summary>
    ///     The basic mesh data.
    ///     It is created by the <see cref="VoxelGame.Core.Visuals.Meshables.ISimple"/>,
    ///     <see cref="VoxelGame.Core.Visuals.Meshables.IComplex"/>, and
    ///     <see cref="VoxelGame.Core.Visuals.Meshables.IVaryingHeight"/> meshables.
    /// </summary>
    public (PooledList<SpatialVertex> opaque, PooledList<SpatialVertex> transparent) BasicMesh { get; }

    /// <summary>
    ///     Return all pooled lists to the pool. The data can only be returned once.
    /// </summary>
    public void ReturnPooled()
    {
        Debug.Assert(!isReturnedToPool);

        BasicMesh.opaque.ReturnToPool();
        BasicMesh.transparent.ReturnToPool();

        varyingHeightVertexData.ReturnToPool();
        varyingHeightIndices.ReturnToPool();

        crossPlantVertexData.ReturnToPool();

        cropPlantVertexData.ReturnToPool();

        opaqueFluidVertexData.ReturnToPool();
        opaqueFluidIndices.ReturnToPool();

        transparentFluidVertexData.ReturnToPool();
        transparentFluidIndices.ReturnToPool();

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

    #pragma warning disable

    public readonly PooledList<int> cropPlantVertexData;

    public readonly PooledList<int> crossPlantVertexData;
    public readonly PooledList<uint> opaqueFluidIndices;

    public readonly PooledList<int> opaqueFluidVertexData;

    public readonly PooledList<uint> transparentFluidIndices;

    public readonly PooledList<int> transparentFluidVertexData;
    public readonly PooledList<uint> varyingHeightIndices;

    public readonly PooledList<int> varyingHeightVertexData;
    #pragma warning restore
}
