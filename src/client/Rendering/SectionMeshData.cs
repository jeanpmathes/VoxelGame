// <copyright file="SectionMesh.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics;
using VoxelGame.Core.Collections;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     Contains the mesh data for a section.
/// </summary>
public class SectionMeshData
{
    internal readonly PooledList<uint> complexIndices;
    internal readonly PooledList<int> complexVertexData;

    internal readonly PooledList<float> complexVertexPositions;

    internal readonly PooledList<int> cropPlantVertexData;

    internal readonly PooledList<int> crossPlantVertexData;
    internal readonly PooledList<uint> opaqueFluidIndices;

    internal readonly PooledList<int> opaqueFluidVertexData;

    internal readonly PooledList<int> simpleVertexData;
    internal readonly PooledList<uint> transparentFluidIndices;

    internal readonly PooledList<int> transparentFluidVertexData;
    internal readonly PooledList<uint> varyingHeightIndices;

    internal readonly PooledList<int> varyingHeightVertexData;

    private bool isReturnedToPool;

    internal SectionMeshData(PooledList<int> simpleVertexData,
        PooledList<float> complexVertexPositions, PooledList<int> complexVertexData,
        PooledList<uint> complexIndices,
        PooledList<int> varyingHeightVertexData, PooledList<uint> varyingHeightIndices,
        PooledList<int> crossPlantVertexData,
        PooledList<int> cropPlantVertexData,
        PooledList<int> opaqueFluidVertexData, PooledList<uint> opaqueFluidIndices,
        PooledList<int> transparentFluidVertexData, PooledList<uint> transparentFluidIndices)
    {
        this.simpleVertexData = simpleVertexData;

        this.complexVertexPositions = complexVertexPositions;
        this.complexVertexData = complexVertexData;
        this.complexIndices = complexIndices;

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
        simpleVertexData = new PooledList<int>();

        complexVertexPositions = new PooledList<float>();
        complexVertexData = new PooledList<int>();
        complexIndices = new PooledList<uint>();

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
    public bool IsFilled => complexVertexPositions.Count != 0 || simpleVertexData.Count != 0 ||
                            varyingHeightVertexData.Count != 0 || crossPlantVertexData.Count != 0 ||
                            cropPlantVertexData.Count != 0 || opaqueFluidVertexData.Count != 0 ||
                            transparentFluidVertexData.Count != 0;

    /// <summary>
    ///     Return all pooled lists to the pool. The data can only be returned once.
    /// </summary>
    public void ReturnPooled()
    {
        Debug.Assert(!isReturnedToPool);

        simpleVertexData.ReturnToPool();

        complexVertexPositions.ReturnToPool();
        complexVertexData.ReturnToPool();
        complexIndices.ReturnToPool();

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
}
