﻿// <copyright file="BlockMeshContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     The context for block meshing.
/// </summary>
public class BlockMeshContext
{
    private readonly BlockMeshFaceHolder[] blockMeshFaceHolders;
    private readonly PooledList<uint> complexIndices = new(capacity: 16);
    private readonly PooledList<int> complexVertexData = new(capacity: 32);

    private readonly PooledList<float> complexVertexPositions = new(capacity: 64);

    private readonly PooledList<int> cropPlantVertexData = new(capacity: 16);
    private readonly PooledList<int> crossPlantVertexData = new(capacity: 16);
    private readonly Section current;
    private readonly Section?[] neighbors;
    private readonly VaryingHeightMeshFaceHolder[] varyingHeightMeshFaceHolders;

    public BlockMeshContext()
    {
        current = null!;
        neighbors = null!;
        blockMeshFaceHolders = null!;
        varyingHeightMeshFaceHolders = null!;
    }

    /// <summary>
    ///     Get or set the complex vertex count.
    /// </summary>
    public uint ComplexVertexCount { get; set; }

    /// <summary>
    ///     The current block tint, used when the tint is set to neutral.
    /// </summary>
    public TintColor BlockTint { get; }

    /// <summary>
    ///     The current fluid tint, used when the tint is set to neutral.
    /// </summary>
    public TintColor FluidTint { get; }

    /// <summary>
    ///     Get the lists that can be filled with complex mesh data.
    /// </summary>
    public (PooledList<float> positions, PooledList<int> data, PooledList<uint> indices) GetComplexMeshLists()
    {
        return (complexVertexPositions, complexVertexData, complexIndices);
    }

    /// <summary>
    ///     Get the block mesh face holder for simple faces, given a block side.
    /// </summary>
    public BlockMeshFaceHolder GetSimpleBlockMeshFaceHolder(BlockSide side)
    {
        return blockMeshFaceHolders[(int) side];
    }

    /// <summary>
    ///     Get the block mesh face holder for varying height faces, given a block side.
    /// </summary>
    public VaryingHeightMeshFaceHolder GetVaryingHeightMeshFaceHolder(BlockSide side)
    {
        return varyingHeightMeshFaceHolders[(int) side];
    }

    /// <summary>
    ///     Get the crop plant vertex data list.
    /// </summary>
    public PooledList<int> GetCropPlantVertexData()
    {
        return cropPlantVertexData;
    }

    /// <summary>
    ///     Get the cross plant vertex data list.
    /// </summary>
    public PooledList<int> GetCrossPlantVertexData()
    {
        return crossPlantVertexData;
    }

    /// <summary>
    ///     Get a block from the current section or one of its neighbors.
    /// </summary>
    /// <param name="position">The position, in section-local coordinates.</param>
    /// <param name="side">The block side giving the neighbor to use if necessary.</param>
    /// <returns>The block or null if there is no block.</returns>
    public Block? GetBlock(Vector3i position, BlockSide side)
    {
        Block? block;

        if (IsPositionOutOfSection(position))
        {
            position = position.Mod(Section.Size);

            Section? neighbor = neighbors[(int) side];
            block = neighbor?.GetBlock(position);
        }
        else
        {
            block = current.GetBlock(position);
        }

        return block;
    }

    /// <summary>
    ///     Get a block from the current section or one of its neighbors.
    /// </summary>
    /// <param name="position">The position, in section-local coordinates.</param>
    /// <param name="side">The block side giving the neighbor to use if necessary.</param>
    /// <param name="data">Will receive the data of the block.</param>
    /// <returns>The block or null if there is no block.</returns>
    public Block? GetBlock(Vector3i position, BlockSide side, out uint data)
    {
        Block? block;
        data = 0;

        if (IsPositionOutOfSection(position))
        {
            position = position.Mod(Section.Size);

            Section? neighbor = neighbors[(int) side];
            block = neighbor?.GetBlock(position, out data);
        }
        else
        {
            block = current.GetBlock(position, out data);
        }

        return block;
    }

    private static bool IsPositionOutOfSection(Vector3i position)
    {
        return position.X is < 0 or >= Section.Size ||
               position.Y is < 0 or >= Section.Size ||
               position.Z is < 0 or >= Section.Size;
    }
}
