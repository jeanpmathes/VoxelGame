// <copyright file="BlockMesh.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core.Visuals
{
    public class BlockMesh
    {
        private readonly uint[] indices;
        private readonly int[] textureIndices;
        private readonly uint vertexCount;
        private readonly float[] vertices;

        public BlockMesh(uint vertexCount, float[] vertices, int[] textureIndices, uint[] indices)
        {
            this.vertexCount = vertexCount;
            this.vertices = vertices;
            this.textureIndices = textureIndices;
            this.indices = indices;
        }

        public BlockMeshData GetComplexMeshData(TintColor? tint = null, bool isAnimated = false)
        {
            return BlockMeshData.Complex(vertexCount, vertices, textureIndices, indices, tint, isAnimated);
        }
    }
}