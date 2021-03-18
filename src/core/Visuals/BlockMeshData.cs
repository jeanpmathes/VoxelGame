// <copyright file="BlockMeshData.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;

namespace VoxelGame.Core.Visuals
{
    public class BlockMeshData
    {
        private readonly float[] vertices;
        private readonly int[] textureIndices;
        private readonly uint[] indices;

        public uint VertexCount { get; }

        public int TextureIndex { get; }

        public TintColor Tint { get; }

        public bool IsAnimated { get; }

        public BlockMeshData(uint vertexCount, float[] vertices, int[] textureIndices, uint[] indices, bool isAnimated = false)
            : this(vertexCount, vertices, textureIndices, indices, TintColor.None, isAnimated) { }

        public BlockMeshData(uint vertexCount, float[] vertices, int[] textureIndices, uint[] indices, TintColor tint, bool isAnimated = false)
            : this(vertexCount, vertices, textureIndices, indices, 0, tint, isAnimated) { }

        private BlockMeshData(uint vertexCount, float[] vertices, int[] textureIndices, uint[] indices, int textureIndex, TintColor tint, bool isAnimated)
        {
            VertexCount = vertexCount;

            this.vertices = vertices;
            this.textureIndices = textureIndices;
            this.indices = indices;

            TextureIndex = textureIndex;

            Tint = tint;
            IsAnimated = isAnimated;
        }

        public float[] GetVertices() => vertices;

        public int[] GetTextureIndices() => textureIndices;

        public uint[] GetIndices() => indices;

        public BlockMeshData Modified(TintColor tint)
        {
            return new BlockMeshData(this.VertexCount, this.GetVertices(), this.GetTextureIndices(), this.GetIndices(), this.TextureIndex, tint, this.IsAnimated);
        }

        public BlockMeshData Modified(TintColor tint, bool isAnimated)
        {
            return new BlockMeshData(this.VertexCount, this.GetVertices(), this.GetTextureIndices(), this.GetIndices(), this.TextureIndex, tint, isAnimated);
        }

        public BlockMeshData SwapTextureIndices(int[] newTextureIndices)
        {
            return new BlockMeshData(this.VertexCount, this.GetVertices(), newTextureIndices, this.GetIndices(), this.TextureIndex, this.Tint, this.IsAnimated);
        }

        public BlockMeshData SwapTextureIndex(int newTextureIndex)
        {
            return new BlockMeshData(this.VertexCount, this.GetVertices(), Array.Empty<int>(), this.GetIndices(), newTextureIndex, this.Tint, this.IsAnimated);
        }

        public static BlockMeshData Basic(float[] vertices, int textureIndex)
        {
            return new BlockMeshData(4, vertices, Array.Empty<int>(), Array.Empty<uint>(), textureIndex, TintColor.None, false);
        }

        public static BlockMeshData Empty()
        {
            return new BlockMeshData(0, Array.Empty<float>(), Array.Empty<int>(), Array.Empty<uint>());
        }
    }
}