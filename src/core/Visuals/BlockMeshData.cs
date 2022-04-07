// <copyright file="BlockMeshData.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Block mesh data, describing how a block should be meshed.
///     Many properties and methods are only meaningful for the correct meshing type.
/// </summary>
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

    /// <summary>
    ///     Get the vertex count.
    /// </summary>
    public uint VertexCount { get; }

    /// <summary>
    ///     Get the texture index.
    /// </summary>
    public int TextureIndex { get; }

    /// <summary>
    ///     Whether the texture is rotated.
    /// </summary>
    public bool IsTextureRotated { get; }

    /// <summary>
    ///     The block tint.
    /// </summary>
    public TintColor Tint { get; }

    /// <summary>
    ///     Whether the block is animated.
    /// </summary>
    public bool IsAnimated { get; }

    /// <summary>
    ///     Whether the plant has an upper part.
    /// </summary>
    public bool HasUpper { get; }

    /// <summary>
    ///     Whether the plant is lowered.
    /// </summary>
    public bool IsLowered { get; }

    /// <summary>
    ///     Whether the plant is an upper part.
    /// </summary>
    public bool IsUpper { get; }

    /// <summary>
    ///     Whether the plant is a double crop plant.
    /// </summary>
    public bool IsDoubleCropPlant { get; }

    /// <summary>
    ///     Get the vertices.
    /// </summary>
    public float[] GetVertices()
    {
        return vertices;
    }

    /// <summary>
    ///     Get the texture indices.
    /// </summary>
    public int[] GetTextureIndices()
    {
        return textureIndices;
    }

    /// <summary>
    ///     Get the indices.
    /// </summary>
    public uint[] GetIndices()
    {
        return indices;
    }

    /// <summary>
    ///     Get the animation bit.
    /// </summary>
    public int GetAnimationBit(int texture, int shift)
    {
        return IsAnimated && textureIndices[texture] != 0 ? 1 << shift : 0;
    }

    /// <summary>
    ///     Get the animation bit.
    /// </summary>
    public int GetAnimationBit(int shift)
    {
        return IsAnimated && TextureIndex != 0 ? 1 << shift : 0;
    }

    /// <summary>
    ///     Get a modified version of this mesh data.
    /// </summary>
    public BlockMeshData Modified(TintColor tint)
    {
        return new BlockMeshData(
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

    /// <summary>
    ///     Get a modified version of this mesh data.
    /// </summary>
    public BlockMeshData Modified(TintColor tint, bool isAnimated)
    {
        return new BlockMeshData(
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

    /// <summary>
    ///     Swap out the texture indices.
    /// </summary>
    public BlockMeshData SwapTextureIndices(int[] newTextureIndices)
    {
        return new BlockMeshData(
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

    /// <summary>
    ///     Swap out the texture index.
    /// </summary>
    public BlockMeshData SwapTextureIndex(int newTextureIndex)
    {
        return new BlockMeshData(
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

    /// <summary>
    ///     Get mesh data for a basic block.
    /// </summary>
    public static BlockMeshData Basic(int textureIndex, bool isTextureRotated)
    {
        return new BlockMeshData(vertexCount: 4, textureIndex: textureIndex, isTextureRotated: isTextureRotated);
    }

    /// <summary>
    ///     Get mesh data for a complex block.
    /// </summary>
    public static BlockMeshData Complex(uint vertexCount, float[] vertices, int[] textureIndices, uint[] indices,
        TintColor? tint = null, bool isAnimated = false)
    {
        return new BlockMeshData(
            vertexCount,
            vertices,
            textureIndices,
            indices,
            tint: tint,
            isAnimated: isAnimated);
    }

    /// <summary>
    ///     Get mesh data for a varying height block.
    /// </summary>
    public static BlockMeshData VaryingHeight(int textureIndex, TintColor tint)
    {
        return new BlockMeshData(vertexCount: 4, textureIndex: textureIndex, tint: tint);
    }

    /// <summary>
    ///     Get mesh data for a cross plant.
    /// </summary>
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

    /// <summary>
    ///     Get mesh data for a crop plant.
    /// </summary>
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

    /// <summary>
    ///     Get mesh data for a double crop plant.
    /// </summary>
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

    /// <summary>
    ///     Get empty mesh data.
    /// </summary>
    public static BlockMeshData Empty()
    {
        return new BlockMeshData();
    }
}
