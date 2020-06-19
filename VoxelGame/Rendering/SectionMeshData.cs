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

        public SectionMeshData(ref PooledList<int> simpleVertexData, ref PooledList<float> complexVertexPositions, ref PooledList<int> complexVertexData, ref PooledList<uint> complexIndices)
        {
            this.simpleVertexData = simpleVertexData;

            this.complexVertexPositions = complexVertexPositions;
            this.complexVertexData = complexVertexData;
            this.complexIndices = complexIndices;
        }

        public void ReturnPooled()
        {
            simpleVertexData.ReturnToPool();

            complexVertexPositions.ReturnToPool();
            complexVertexData.ReturnToPool();
            complexIndices.ReturnToPool();
        }
    }
}