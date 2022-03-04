// <copyright file="SectionMesh.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics;
using VoxelGame.Core.Collections;

namespace VoxelGame.Client.Rendering
{
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
        internal readonly PooledList<uint> opaqueLiquidIndices;

        internal readonly PooledList<int> opaqueLiquidVertexData;

        internal readonly PooledList<int> simpleVertexData;
        internal readonly PooledList<uint> transparentLiquidIndices;

        internal readonly PooledList<int> transparentLiquidVertexData;
        internal readonly PooledList<uint> varyingHeightIndices;

        internal readonly PooledList<int> varyingHeightVertexData;

        private bool isReturnedToPool;

        internal SectionMeshData(PooledList<int> simpleVertexData,
            PooledList<float> complexVertexPositions, PooledList<int> complexVertexData,
            PooledList<uint> complexIndices,
            PooledList<int> varyingHeightVertexData, PooledList<uint> varyingHeightIndices,
            PooledList<int> crossPlantVertexData,
            PooledList<int> cropPlantVertexData,
            PooledList<int> opaqueLiquidVertexData, PooledList<uint> opaqueLiquidIndices,
            PooledList<int> transparentLiquidVertexData, PooledList<uint> transparentLiquidIndices)
        {
            this.simpleVertexData = simpleVertexData;

            this.complexVertexPositions = complexVertexPositions;
            this.complexVertexData = complexVertexData;
            this.complexIndices = complexIndices;

            this.varyingHeightVertexData = varyingHeightVertexData;
            this.varyingHeightIndices = varyingHeightIndices;

            this.crossPlantVertexData = crossPlantVertexData;

            this.cropPlantVertexData = cropPlantVertexData;

            this.opaqueLiquidVertexData = opaqueLiquidVertexData;
            this.opaqueLiquidIndices = opaqueLiquidIndices;

            this.transparentLiquidVertexData = transparentLiquidVertexData;
            this.transparentLiquidIndices = transparentLiquidIndices;
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

            opaqueLiquidVertexData = new PooledList<int>();
            opaqueLiquidIndices = new PooledList<uint>();

            transparentLiquidVertexData = new PooledList<int>();
            transparentLiquidIndices = new PooledList<uint>();
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
                                cropPlantVertexData.Count != 0 || opaqueLiquidVertexData.Count != 0 ||
                                transparentLiquidVertexData.Count != 0;

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

            opaqueLiquidVertexData.ReturnToPool();
            opaqueLiquidIndices.ReturnToPool();

            transparentLiquidVertexData.ReturnToPool();
            transparentLiquidIndices.ReturnToPool();

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
}
