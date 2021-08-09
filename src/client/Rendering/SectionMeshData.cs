// <copyright file="SectionMesh.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Collections;

namespace VoxelGame.Client.Rendering
{
    public class SectionMeshData
    {
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

        public SectionMeshData(
            ref PooledList<int> simpleVertexData,
            ref PooledList<float> complexVertexPositions, ref PooledList<int> complexVertexData, ref PooledList<uint> complexIndices,
            ref PooledList<int> varyingHeightVertexData, ref PooledList<uint> varyingHeightIndices,
            ref PooledList<int> crossPlantVertexData,
            ref PooledList<int> cropPlantVertexData,
            ref PooledList<int> opaqueLiquidVertexData, ref PooledList<uint> opaqueLiquidIndices,
            ref PooledList<int> transparentLiquidVertexData, ref PooledList<uint> transparentLiquidIndices)
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

        public bool IsFilled => complexVertexPositions.Count != 0 || simpleVertexData.Count != 0 || varyingHeightVertexData.Count != 0 || crossPlantVertexData.Count != 0 || cropPlantVertexData.Count != 0 || opaqueLiquidVertexData.Count != 0 || transparentLiquidVertexData.Count != 0;

        public void ReturnPooled()
        {
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
        }
    }
}