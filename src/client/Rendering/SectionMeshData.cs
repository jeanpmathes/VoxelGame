// <copyright file="SectionMesh.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics;
using VoxelGame.Core.Collections;

namespace VoxelGame.Client.Rendering
{
    public class SectionMeshData
    {
        public static SectionMeshData Empty => new SectionMeshData();

        internal readonly PooledList<int> simpleVertexData;

        internal readonly PooledList<float> complexVertexPositions;
        internal readonly PooledList<int> complexVertexData;
        internal readonly PooledList<uint> complexIndices;

        internal readonly PooledList<int> varyingHeightVertexData;
        internal readonly PooledList<uint> varyingHeightIndices;

        internal readonly PooledList<int> crossPlantVertexData;

        internal readonly PooledList<int> cropPlantVertexData;

        internal readonly PooledList<int> opaqueLiquidVertexData;
        internal readonly PooledList<uint> opaqueLiquidIndices;

        internal readonly PooledList<int> transparentLiquidVertexData;
        internal readonly PooledList<uint> transparentLiquidIndices;

        private bool isReturnedToPool;

        public SectionMeshData(PooledList<int> simpleVertexData,
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
            this.simpleVertexData = new PooledList<int>();

            this.complexVertexPositions = new PooledList<float>();
            this.complexVertexData = new PooledList<int>();
            this.complexIndices = new PooledList<uint>();

            this.varyingHeightVertexData = new PooledList<int>();
            this.varyingHeightIndices = new PooledList<uint>();

            this.crossPlantVertexData = new PooledList<int>();

            this.cropPlantVertexData = new PooledList<int>();

            this.opaqueLiquidVertexData = new PooledList<int>();
            this.opaqueLiquidIndices = new PooledList<uint>();

            this.transparentLiquidVertexData = new PooledList<int>();
            this.transparentLiquidIndices = new PooledList<uint>();
        }

        public bool IsFilled => complexVertexPositions.Count != 0 || simpleVertexData.Count != 0 || varyingHeightVertexData.Count != 0 || crossPlantVertexData.Count != 0 || cropPlantVertexData.Count != 0 || opaqueLiquidVertexData.Count != 0 || transparentLiquidVertexData.Count != 0;

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

        public void Discard()
        {
            if (isReturnedToPool) return;

            ReturnPooled();
        }
    }
}