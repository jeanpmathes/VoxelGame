// <copyright file="DoubleCrossPlantBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     Similar to <see cref="CrossPlantBlock" />, but is two blocks high.
///     Data bit usage: <c>----lh</c>
/// </summary>
// l: lowered
// h: height
public class DoubleCrossPlantBlock : Block, ICombustible, IFillable, IFoliage
{
    private readonly String bottomTexture;
    private readonly Int32 topTexOffset;

    private readonly List<BlockMesh> meshes = [];

    internal DoubleCrossPlantBlock(String name, String namedID, String bottomTexture, Int32 topTexOffset,
        BoundingVolume boundingVolume) :
        base(
            name,
            namedID,
            new BlockFlags(),
            boundingVolume)
    {
        this.bottomTexture = bottomTexture;
        this.topTexOffset = topTexOffset;
    }

    IFoliage.MeshData IFoliage.GetMeshData(BlockMeshInfo info)
    {
        Boolean isUpper = (info.Data & 0b01) != 0;

        return new IFoliage.MeshData(meshes[(Int32) (info.Data & 0b00_0011)])
        {
            Tint = TintColor.Neutral,
            IsDoublePlant = true,
            IsUpperPart = isUpper
        };
    }

    /// <inheritdoc />
    public override void ContentUpdate(World world, Vector3i position, Content content)
    {
        if (content.Fluid.Fluid.IsFluid && content.Fluid.Level > FluidLevel.Five) ScheduleDestroy(world, position);
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider indexProvider, VisualConfiguration visuals)
    {
        Int32 bottomTextureIndex = indexProvider.GetTextureIndex(bottomTexture);
        Int32 topTextureIndex = bottomTextureIndex + topTexOffset;

        for (UInt32 data = 0; data <= 0b00_0011; data++) meshes.Add(CreateMesh(data, bottomTextureIndex, topTextureIndex, visuals));
    }

    private static BlockMesh CreateMesh(UInt32 data, Int32 bottomTextureIndex, Int32 topTextureIndex, VisualConfiguration visuals)
    {
        Boolean isUpper = (data & 0b01) != 0;
        Boolean isLowered = (data & 0b10) != 0;

        return BlockMeshes.CreateCrossPlantMesh(visuals.FoliageQuality, isUpper ? topTextureIndex : bottomTextureIndex, isLowered);
    }

    /// <inheritdoc />
    public override Boolean CanPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        return world.GetBlock(position.Above())?.Block.IsReplaceable == true &&
               (world.GetBlock(position.Below())?.Block ?? Elements.Blocks.Instance.Air) is IPlantable;
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        Boolean isLowered = world.IsLowered(position);

        UInt32 data = (isLowered ? 1u : 0u) << 1;

        world.SetBlock(this.AsInstance(data), position);
        world.SetBlock(this.AsInstance(data | 1), position.Above());
    }

    /// <inheritdoc />
    protected override void DoDestroy(World world, Vector3i position, UInt32 data, PhysicsActor? actor)
    {
        Boolean isBase = (data & 0b1) == 0;

        world.SetDefaultBlock(position);
        world.SetDefaultBlock(isBase ? position.Above() : position.Below());
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, UInt32 data, Side side)
    {
        // Check if this block is the lower part and if the ground supports plant growth.
        if (side == Side.Bottom && (data & 0b1) == 0 &&
            (world.GetBlock(position.Below())?.Block ?? Elements.Blocks.Instance.Air) is not IPlantable) Destroy(world, position);
    }
}
