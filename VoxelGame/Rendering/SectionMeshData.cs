// <copyright file="SectionMesh.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using System.Collections.Generic;

namespace VoxelGame.Rendering
{
    public class SectionMeshData
    {
        internal int[] simpleVertexData;
        internal uint[] simpleIndices;

        internal float[] complexVertexPositions;
        internal int[] complexVertexData;
        internal uint[] complexIndices;

        public SectionMeshData(ref List<int> simpleVertexData, ref List<uint> simpleIndices, ref List<float> complexVertexPositions, ref List<int> complexVertexData, ref List<uint> complexIndices)
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

            this.simpleVertexData = simpleVertexData.ToArray();
            this.simpleIndices = simpleIndices.ToArray();

            this.complexVertexPositions = complexVertexPositions.ToArray();
            this.complexVertexData = complexVertexData.ToArray();
            this.complexIndices = complexIndices.ToArray();
        }
    }
}
