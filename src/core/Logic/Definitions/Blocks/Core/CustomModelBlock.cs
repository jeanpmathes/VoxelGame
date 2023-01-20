﻿// <copyright file="CustomModelBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block that loads its complete model from a file. The block can only be placed on top of solid and full blocks.
///     Data bit usage: <c>------</c>
/// </summary>
public class CustomModelBlock : Block, IFillable, IComplex
{
    private readonly BlockMesh mesh;

    /// <summary>
    ///     Create a new <see cref="CustomModelBlock" />.
    /// </summary>
    /// <param name="name">The name of the block.</param>
    /// <param name="namedId">The named ID of the block.</param>
    /// <param name="flags">The block flags.</param>
    /// <param name="modelName">The name of the model to use for this block.</param>
    /// <param name="boundingVolume">The bounding box of the block.</param>
    internal CustomModelBlock(string name, string namedId, BlockFlags flags, string modelName,
        BoundingVolume boundingVolume) :
        base(
            name,
            namedId,
            flags with {IsFull = false, IsOpaque = false},
            boundingVolume)
    {
        mesh = BlockModel.Load(modelName).Mesh;
    }

    IComplex.MeshData IComplex.GetMeshData(BlockMeshInfo info)
    {
        return GetMeshData(info);
    }

    /// <inheritdoc />
    public override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        return world.HasFullAndSolidGround(position, solidify: true);
    }

    /// <inheritdoc />
    public override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
    {
        if (side == BlockSide.Bottom && !world.HasFullAndSolidGround(position)) Destroy(world, position);
    }

    /// <summary>
    ///     Override to return the custom mesh.
    /// </summary>
    protected virtual IComplex.MeshData GetMeshData(BlockMeshInfo info)
    {
        return mesh.GetMeshData();
    }
}
