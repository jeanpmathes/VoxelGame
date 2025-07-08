// <copyright file="CustomModelBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Elements.Legacy;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Legacy.Blocks;

/// <summary>
///     A block that loads its complete model from a file. The block can only be placed on top of solid and full blocks.
///     Data bit usage: <c>------</c>
/// </summary>
public class CustomModelBlock : Block, IFillable, IComplex
{
    private readonly RID model;

    private BlockMesh mesh = null!;

    /// <summary>
    ///     Create a new <see cref="CustomModelBlock" />.
    /// </summary>
    /// <param name="name">The name of the block.</param>
    /// <param name="namedID">The named ID of the block.</param>
    /// <param name="flags">The block flags.</param>
    /// <param name="model">The resource ID of the model.</param>
    /// <param name="boundingVolume">The bounding box of the block.</param>
    internal CustomModelBlock(
        String name,
        String namedID,
        BlockFlags flags,
        RID model,
        BoundingVolume boundingVolume) :
        base(
            name,
            namedID,
            flags with {IsFull = false},
            boundingVolume)
    {
        this.model = model;
    }

    IComplex.MeshData IComplex.GetMeshData(BlockMeshInfo info)
    {
        return GetMeshData(info);
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider textureIndexProvider, IBlockModelProvider modelProvider, VisualConfiguration visuals)
    {
        mesh = modelProvider.GetModel(model).CreateMesh(textureIndexProvider);
    }

    /// <inheritdoc />
    public override Boolean CanPlace(World world, Vector3i position, Actor? actor)
    {
        return world.HasFullAndSolidGround(position, solidify: true);
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, UInt32 data, Side side)
    {
        if (side == Side.Bottom && !world.HasFullAndSolidGround(position)) Destroy(world, position);
    }

    /// <summary>
    ///     Override to return the custom mesh.
    /// </summary>
    protected virtual IComplex.MeshData GetMeshData(BlockMeshInfo info)
    {
        return mesh.GetMeshData();
    }
}
