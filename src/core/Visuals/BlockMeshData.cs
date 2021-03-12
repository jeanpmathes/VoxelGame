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
        public uint VertexCount { get; }

        public float[] Vertices { get; }
        public int[] TextureIndices { get; }
        public uint[] Indices { get; }

        public TintColor Tint { get; }

        public bool IsAnimated { get; }

        public BlockMeshData(uint vertexCount, float[] vertices, int[] textureIndices, uint[] indices, bool isAnimated = false)
            : this(vertexCount, vertices, textureIndices, indices, TintColor.None, isAnimated) { }

        public BlockMeshData(uint vertexCount, float[] vertices, int[] textureIndices, uint[] indices, TintColor tint, bool isAnimated = false)
        {
            VertexCount = vertexCount;

            Vertices = vertices;
            TextureIndices = textureIndices;
            Indices = indices;

            Tint = tint;
            IsAnimated = isAnimated;
        }

        public BlockMeshData Modified(TintColor tint)
        {
            return new BlockMeshData(this.VertexCount, this.Vertices, this.TextureIndices, this.Indices, tint, this.IsAnimated);
        }

        public BlockMeshData Modified(TintColor tint, bool isAnimated)
        {
            return new BlockMeshData(this.VertexCount, this.Vertices, this.TextureIndices, this.Indices, tint, isAnimated);
        }

        public BlockMeshData SwapTextureIndices(int[] textureIndices)
        {
            return new BlockMeshData(this.VertexCount, this.Vertices, textureIndices, this.Indices, this.Tint, this.IsAnimated);
        }

        public static BlockMeshData Basic(float[] vertices, int[] textureIndices) => new BlockMeshData(4, vertices, textureIndices, Array.Empty<uint>());

        public static BlockMeshData Empty() => new BlockMeshData(0, Array.Empty<float>(), Array.Empty<int>(), Array.Empty<uint>());
    }
}