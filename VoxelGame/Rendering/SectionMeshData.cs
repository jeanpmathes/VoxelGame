// <copyright file="SectionMesh.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Collections;

namespace VoxelGame.Rendering
{
    public class SectionMeshData
    {
        internal PooledList<int> simpleVertexData;

        internal PooledList<float> complexVertexPositions;
        internal PooledList<int> complexVertexData;
        internal PooledList<uint> complexIndices;

        internal PooledList<float> liquidVertices;
        internal PooledList<int> liquidTextureIndices;
        internal PooledList<uint> liquidIndices;

        public SectionMeshData(ref PooledList<int> simpleVertexData, ref PooledList<float> complexVertexPositions, ref PooledList<int> complexVertexData, ref PooledList<uint> complexIndices, ref PooledList<float> liquidVertices, ref PooledList<int> liquidTextureIndices, ref PooledList<uint> liquidIndices)
        {
            this.simpleVertexData = simpleVertexData;

            this.complexVertexPositions = complexVertexPositions;
            this.complexVertexData = complexVertexData;
            this.complexIndices = complexIndices;

            this.liquidVertices = liquidVertices;
            this.liquidTextureIndices = liquidTextureIndices;
            this.liquidIndices = liquidIndices;
        }

        public void ReturnPooled()
        {
            simpleVertexData.ReturnToPool();

            complexVertexPositions.ReturnToPool();
            complexVertexData.ReturnToPool();
            complexIndices.ReturnToPool();

            liquidVertices.ReturnToPool();
            liquidTextureIndices.ReturnToPool();
            liquidIndices.ReturnToPool();
        }
    }
}