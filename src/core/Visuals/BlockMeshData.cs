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
        private readonly uint[] indices;
        private readonly int[] textureIndices;
        private readonly float[] vertices;

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

        public uint VertexCount { get; }

        public int TextureIndex { get; }

        public bool IsTextureRotated { get; }

        public TintColor Tint { get; }

        public bool IsAnimated { get; }

        public bool HasUpper { get; }

        public bool IsLowered { get; }

        public bool IsUpper { get; }

        public bool IsDoubleCropPlant { get; }

        public float[] GetVertices()
        {
            return vertices;
        }

        public int[] GetTextureIndices()
        {
            return textureIndices;
        }

        public uint[] GetIndices()
        {
            return indices;
        }

        public int GetAnimationBit(int texture, int shift)
        {
            return IsAnimated && textureIndices[texture] != 0 ? 1 << shift : 0;
        }

        public int GetAnimationBit(int shift)
        {
            return IsAnimated && TextureIndex != 0 ? 1 << shift : 0;
        }

        public BlockMeshData Modified(TintColor tint)
        {
            return new(
                VertexCount,
                vertices,
                textureIndices,
                indices,
                TextureIndex,
                IsTextureRotated,
                tint,
                IsAnimated,
                HasUpper,
                IsLowered,
                IsUpper,
                IsDoubleCropPlant);
        }

        public BlockMeshData Modified(TintColor tint, bool isAnimated)
        {
            return new(
                VertexCount,
                vertices,
                textureIndices,
                indices,
                TextureIndex,
                IsTextureRotated,
                tint,
                isAnimated,
                HasUpper,
                IsLowered,
                IsUpper,
                IsDoubleCropPlant);
        }

        public BlockMeshData SwapTextureIndices(int[] newTextureIndices)
        {
            return new(
                VertexCount,
                vertices,
                newTextureIndices,
                indices,
                TextureIndex,
                IsTextureRotated,
                Tint,
                IsAnimated,
                HasUpper,
                IsLowered,
                IsUpper,
                IsDoubleCropPlant);
        }

        public BlockMeshData SwapTextureIndex(int newTextureIndex)
        {
            return new(
                VertexCount,
                vertices,
                textureIndices,
                indices,
                newTextureIndex,
                IsTextureRotated,
                Tint,
                IsAnimated,
                HasUpper,
                IsLowered,
                IsUpper,
                IsDoubleCropPlant);
        }

        public static BlockMeshData Basic(int textureIndex, bool isTextureRotated)
        {
            return new(vertexCount: 4, textureIndex: textureIndex, isTextureRotated: isTextureRotated);
        }

        public static BlockMeshData Complex(uint vertexCount, float[] vertices, int[] textureIndices, uint[] indices,
            TintColor? tint = null, bool isAnimated = false)
        {
            return new(
                vertexCount,
                vertices,
                textureIndices,
                indices,
                tint: tint,
                isAnimated: isAnimated);
        }

        public static BlockMeshData VaryingHeight(int textureIndex, TintColor tint)
        {
            return new(vertexCount: 4, textureIndex: textureIndex, tint: tint);
        }

        public static BlockMeshData CrossPlant(int textureIndex, TintColor tint, bool hasUpper, bool isLowered,
            bool isUpper)
        {
            return new(
                vertexCount: 8,
                textureIndex: textureIndex,
                tint: tint,
                hasUpper: hasUpper,
                isLowered: isLowered,
                isUpper: isUpper);
        }

        public static BlockMeshData CropPlant(int textureIndex, TintColor tint, bool isLowered, bool isUpper)
        {
            return new(
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
            return new(
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
            return new();
        }
    }
}