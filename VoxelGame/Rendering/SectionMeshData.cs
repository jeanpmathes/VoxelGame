// <copyright file="SectionMesh.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using VoxelGame.Collections;

namespace VoxelGame.Rendering
{
    public class SectionMeshData
    {
        internal PooledList<int> simpleVertexData;
        internal PooledList<uint> simpleIndices;

        internal PooledList<float> complexVertexPositions;
        internal PooledList<int> complexVertexData;
        internal PooledList<uint> complexIndices;

        public SectionMeshData(ref PooledList<int> simpleVertexData, ref PooledList<uint> simpleIndices, ref PooledList<float> complexVertexPositions, ref PooledList<int> complexVertexData, ref PooledList<uint> complexIndices)
        {
            if (simpleVertexData == null)
            {
                throw new ArgumentNullException(nameof(simpleVertexData));
            }

            if (simpleIndices == null)
            {
                throw new ArgumentNullException(nameof(simpleIndices));
            }

            if (complexVertexPositions == null)
            {
                throw new ArgumentNullException(nameof(complexVertexPositions));
            }

            if (complexVertexData == null)
            {
                throw new ArgumentNullException(nameof(complexVertexData));
            }

            if (complexIndices == null)
            {
                throw new ArgumentNullException(nameof(complexIndices));
            }

            this.simpleVertexData = simpleVertexData;
            this.simpleIndices = simpleIndices;

            this.complexVertexPositions = complexVertexPositions;
            this.complexVertexData = complexVertexData;
            this.complexIndices = complexIndices;
        }

        public void ReturnPooled()
        {
            simpleVertexData.ReturnToPool();
            simpleIndices.ReturnToPool();

            complexVertexPositions.ReturnToPool();
            complexVertexData.ReturnToPool();
            complexIndices.ReturnToPool();
        }
    }
}
