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
        internal PooledList<int> simpleVertexData;

        internal PooledList<float> complexVertexPositions;
        internal PooledList<int> complexVertexData;
        internal PooledList<uint> complexIndices;

        internal PooledList<int> varyingHeightVertexData;
        internal PooledList<uint> varyingHeightIndices;

        internal PooledList<int> opaqueLiquidVertexData;
        internal PooledList<uint> opaqueLiquidIndices;

        internal PooledList<int> transparentLiquidVertexData;
        internal PooledList<uint> transparentLiquidIndices;

        public SectionMeshData(ref PooledList<int> simpleVertexData,
            ref PooledList<float> complexVertexPositions, ref PooledList<int> complexVertexData, ref PooledList<uint> complexIndices,
            ref PooledList<int> varyingHeightVertexData, ref PooledList<uint> varyingHeightIndices,
            ref PooledList<int> opaqueLiquidVertexData, ref PooledList<uint> opaqueLiquidIndices,
            ref PooledList<int> transparentLiquidVertexData, ref PooledList<uint> transparentLiquidIndices)
        {
            this.simpleVertexData = simpleVertexData;

            this.complexVertexPositions = complexVertexPositions;
            this.complexVertexData = complexVertexData;
            this.complexIndices = complexIndices;

            this.varyingHeightVertexData = varyingHeightVertexData;
            this.varyingHeightIndices = varyingHeightIndices;

            this.opaqueLiquidVertexData = opaqueLiquidVertexData;
            this.opaqueLiquidIndices = opaqueLiquidIndices;

            this.transparentLiquidVertexData = transparentLiquidVertexData;
            this.transparentLiquidIndices = transparentLiquidIndices;
        }

        public void ReturnPooled()
        {
            simpleVertexData.ReturnToPool();

            complexVertexPositions.ReturnToPool();
            complexVertexData.ReturnToPool();
            complexIndices.ReturnToPool();

            varyingHeightVertexData.ReturnToPool();
            varyingHeightIndices.ReturnToPool();

            opaqueLiquidVertexData.ReturnToPool();
            opaqueLiquidIndices.ReturnToPool();

            transparentLiquidVertexData.ReturnToPool();
            transparentLiquidIndices.ReturnToPool();
        }
    }
}