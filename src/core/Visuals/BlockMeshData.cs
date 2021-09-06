// <copyright file="BlockMeshData.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;

namespace VoxelGame.Core.Visuals
{
    public sealed class BlockMeshData
    {
        private readonly float[] vertices;
        private readonly int[] textureIndices;
        private readonly uint[] indices;

        public uint VertexCount { get; }

        public int TextureIndex { get; }

        public bool IsTextureRotated { get; }

        public TintColor Tint { get; }

        public bool IsAnimated { get; }

        public bool HasUpper { get; }

        public bool IsLowered { get; }

        public bool IsUpper { get; }

        public bool IsDoubleCropPlant { get; }

        private BlockMeshData(uint vertexCount = 0, float[]? vertices = null, int[]? textureIndices = null,
            uint[]? indices = null, int textureIndex = 0, bool isTextureRotated = false, TintColor? tint = null,
            bool isAnimated = false, bool isLowered = false, bool hasUpper = false, bool isUpper = false,
            bool isDoubleCropPlant = false)
        {
            VertexCount = vertexCount;

            this.vertices = vertices ?? Array.Empty<float>();
            this.textureIndices = textureIndices ?? Array.Empty<int>();
            this.indices = indices ?? Array.Empty<uint>();

            TextureIndex = textureIndex;
            IsTextureRotated = isTextureRotated;

            Tint = tint ?? TintColor.None;

            IsAnimated = isAnimated;
            HasUpper = hasUpper;
            IsLowered = isLowered;
            IsUpper = isUpper;
            IsDoubleCropPlant = isDoubleCropPlant;
        }

        private BlockMeshData(BlockMeshData original)
        {
            VertexCount = original.VertexCount;

            vertices = original.vertices;
            textureIndices = original.textureIndices;
            indices = original.indices;

            TextureIndex = original.TextureIndex;
            IsTextureRotated = original.IsTextureRotated;

            Tint = original.Tint;

            IsAnimated = original.IsAnimated;
            HasUpper = original.HasUpper;
            IsLowered = original.IsLowered;
            IsUpper = original.IsUpper;
            IsDoubleCropPlant = original.IsDoubleCropPlant;
        }

        public float[] GetVertices() => vertices;

        public int[] GetTextureIndices() => textureIndices;

        public uint[] GetIndices() => indices;

        public int GetAnimationBit(int texture, int shift)
        {
            return IsAnimated && textureIndices[texture] != 0 ? (1 << shift) : 0;
        }

        public int GetAnimationBit(int shift)
        {
            return IsAnimated && TextureIndex != 0 ? (1 << shift) : 0;
        }

        public BlockMeshData Modified(TintColor tint)
        {
            return new BlockMeshData(
                this.VertexCount,
                this.vertices,
                this.textureIndices,
                this.indices,
                this.TextureIndex,
                this.IsTextureRotated,
                tint,
                this.IsAnimated,
                this.HasUpper,
                this.IsLowered,
                this.IsUpper,
                this.IsDoubleCropPlant);
        }

        public BlockMeshData Modified(TintColor tint, bool isAnimated)
        {
            return new BlockMeshData(
                this.VertexCount,
                this.vertices,
                this.textureIndices,
                this.indices,
                this.TextureIndex,
                this.IsTextureRotated,
                tint,
                isAnimated,
                this.HasUpper,
                this.IsLowered,
                this.IsUpper,
                this.IsDoubleCropPlant);
        }

        public BlockMeshData SwapTextureIndices(int[] newTextureIndices)
        {
            return new BlockMeshData(
                this.VertexCount,
                this.vertices,
                newTextureIndices,
                this.indices,
                this.TextureIndex,
                this.IsTextureRotated,
                this.Tint,
                this.IsAnimated,
                this.HasUpper,
                this.IsLowered,
                this.IsUpper,
                this.IsDoubleCropPlant);
        }

        public BlockMeshData SwapTextureIndex(int newTextureIndex)
        {
            return new BlockMeshData(
                this.VertexCount,
                this.vertices,
                this.textureIndices,
                this.indices,
                newTextureIndex,
                this.IsTextureRotated,
                this.Tint,
                this.IsAnimated,
                this.HasUpper,
                this.IsLowered,
                this.IsUpper,
                this.IsDoubleCropPlant);
        }

        public static BlockMeshData Basic(int textureIndex, bool isTextureRotated)
        {
            return new BlockMeshData(vertexCount: 4, textureIndex: textureIndex, isTextureRotated: isTextureRotated);
        }

        public static BlockMeshData Complex(uint vertexCount, float[] vertices, int[] textureIndices, uint[] indices,
            TintColor? tint = null, bool isAnimated = false)
        {
            return new BlockMeshData(
                vertexCount: vertexCount,
                vertices: vertices,
                textureIndices: textureIndices,
                indices: indices,
                tint: tint,
                isAnimated: isAnimated);
        }

        public static BlockMeshData VaryingHeight(int textureIndex, TintColor tint)
        {
            return new BlockMeshData(vertexCount: 4, textureIndex: textureIndex, tint: tint);
        }

        public static BlockMeshData CrossPlant(int textureIndex, TintColor tint, bool hasUpper, bool isLowered,
            bool isUpper)
        {
            return new BlockMeshData(
                vertexCount: 8,
                textureIndex: textureIndex,
                tint: tint,
                hasUpper: hasUpper,
                isLowered: isLowered,
                isUpper: isUpper);
        }

        public static BlockMeshData CropPlant(int textureIndex, TintColor tint, bool isLowered, bool isUpper)
        {
            return new BlockMeshData(
                vertexCount: 0,
                textureIndex: textureIndex,
                tint: tint,
                hasUpper: false,
                isLowered: isLowered,
                isUpper: isUpper,
                isDoubleCropPlant: false);
        }

        public static BlockMeshData DoubleCropPlant(int textureIndex, TintColor tint, bool hasUpper, bool isLowered,
            bool isUpper)
        {
            return new BlockMeshData(
                vertexCount: 0,
                textureIndex: textureIndex,
                tint: tint,
                hasUpper: hasUpper,
                isLowered: isLowered,
                isUpper: isUpper,
                isDoubleCropPlant: true);
        }

        public static BlockMeshData Empty()
        {
            return new BlockMeshData();
        }
    }
}