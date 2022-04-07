// <copyright file="CrossPlantBlock.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Logic.Blocks;

/// <summary>
///     A plant made out of two intersecting planes. It uses neutral tint.
///     Data bit usage: <c>-----l</c>
/// </summary>
// l: lowered
public class CrossPlantBlock : Block, IFlammable, IFillable
{
    private readonly string texture;

    private int textureIndex;

    /// <summary>
    ///     Initializes a new instance of a cross plant.
    /// </summary>
    /// <param name="name">The name of this block.</param>
    /// <param name="namedId">The unique and unlocalized name of this block.</param>
    /// <param name="texture">The name of the texture of this block.</param>
    /// <param name="flags">The block flags.</param>
    /// <param name="boundingVolume">The bounding box of this block.</param>
    internal CrossPlantBlock(string name, string namedId, string texture, BlockFlags flags,
        BoundingVolume boundingVolume) :
        base(
            name,
            namedId,
            flags with { IsFull = false, IsOpaque = false },
            boundingVolume,
            TargetBuffer.CrossPlant)
    {
        this.texture = texture;
    }

    /// <inheritdoc />
    public void LiquidChange(World world, Vector3i position, Liquid liquid, LiquidLevel level)
    {
        if (liquid.IsLiquid && level > LiquidLevel.Four) Destroy(world, position);
    }

    /// <inheritdoc />
    protected override void Setup(ITextureIndexProvider indexProvider)
    {
        textureIndex = indexProvider.GetTextureIndex(texture);
    }

    /// <inheritdoc />
    public override BlockMeshData GetMesh(BlockMeshInfo info)
    {
        return BlockMeshData.CrossPlant(
            textureIndex,
            TintColor.Neutral,
            hasUpper: false,
            (info.Data & 0b1) == 1,
            isUpper: false);
    }

    /// <inheritdoc />
    public override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        return PlantBehaviour.CanPlace(world, position);
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        PlantBehaviour.DoPlace(this, world, position);
    }

    /// <inheritdoc />
    public override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
    {
        if (side == BlockSide.Bottom && (world.GetBlock(position.Below())?.Block ?? Air) is not IPlantable)
            Destroy(world, position);
    }
}
