﻿// <copyright file="CropBlock.cs" company="VoxelGame">
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
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block which grows on farmland and has multiple growth stages.
///     Data bit usage: <c>--lsss</c>
/// </summary>
// l: lowered
// s: stage
public class CropBlock : Block, ICombustible, IFillable, IFoliage
{
    private readonly TID texture;

    private readonly List<BoundingVolume> volumes = [];
    private readonly List<BlockMesh> meshes = [];

    internal CropBlock(String name, String namedID, TID texture) :
        base(
            name,
            namedID,
            new BlockFlags(),
            BoundingVolume.Block)
    {
        this.texture = texture;

        for (UInt32 data = 0; data <= 0b00_1111; data++) volumes.Add(CreateVolume(data));
    }

    IFoliage.MeshData IFoliage.GetMeshData(BlockMeshInfo info)
    {
        return new IFoliage.MeshData(meshes[(Int32) info.Data & 0b00_1111]);
    }

    /// <inheritdoc />
    public override void ContentUpdate(World world, Vector3i position, Content content)
    {
        if (content.Fluid.Fluid.IsFluid && content.Fluid.Level > FluidLevel.Three) ScheduleDestroy(world, position);
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider textureIndexProvider, IBlockModelProvider modelProvider, VisualConfiguration visuals)
    {
        var textureIndices = new Int32[(Int32) GrowthStage.Dead + 1];

        for (var stage = GrowthStage.Initial; stage <= GrowthStage.Dead; stage++)
            textureIndices[(Int32) stage] = textureIndexProvider.GetTextureIndex(texture.Offset(GetStateOffset(stage)));

        for (UInt32 data = 0; data <= 0b00_1111; data++) meshes.Add(CreateMesh(visuals.FoliageQuality, textureIndices, data));
    }

    private static Byte GetStateOffset(GrowthStage stage)
    {
        return stage switch
        {
            GrowthStage.Initial => 0,
            GrowthStage.Second => 1,
            GrowthStage.Third => 1,
            GrowthStage.Fourth => 2,
            GrowthStage.Fifth => 2,
            GrowthStage.Sixth => 3,
            GrowthStage.Final => 4,
            GrowthStage.Dead => 5,
            _ => throw Exceptions.UnsupportedEnumValue(stage)
        };
    }

    private static BoundingVolume CreateVolume(UInt32 data)
    {
        var stage = (GrowthStage) (data & 0b00_0111);

        switch (stage)
        {
            case GrowthStage.Initial:
            case GrowthStage.Dead:
                return BoundingVolume.BlockWithHeight(height: 3);

            case GrowthStage.Second:
                return BoundingVolume.BlockWithHeight(height: 5);

            case GrowthStage.Third:
                return BoundingVolume.BlockWithHeight(height: 7);

            case GrowthStage.Fourth:
                return BoundingVolume.BlockWithHeight(height: 9);

            case GrowthStage.Fifth:
                return BoundingVolume.BlockWithHeight(height: 11);

            case GrowthStage.Sixth:
                return BoundingVolume.BlockWithHeight(height: 13);

            case GrowthStage.Final:
                return BoundingVolume.BlockWithHeight(height: 15);

            default: throw Exceptions.UnsupportedEnumValue(stage);
        }
    }

    private static BlockMesh CreateMesh(Quality quality, Int32[] stageTextureIndices, UInt32 data)
    {
        Int32 textureIndex = stageTextureIndices[data & 0b00_0111];
        Boolean isLowered = (data & 0b00_1000) != 0;

        return BlockMeshes.CreateCropPlantMesh(quality, createMiddlePiece: true, textureIndex, isLowered);
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(UInt32 data)
    {
        return volumes[(Int32) data & 0b00_1111];
    }

    /// <inheritdoc />
    public override Boolean CanPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        return world.GetBlock(position.Below())?.Block is IPlantable;
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        Boolean isLowered = world.IsLowered(position);

        var data = (UInt32) GrowthStage.Initial;
        if (isLowered) data |= 0b00_1000;

        world.SetBlock(this.AsInstance(data), position);
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, UInt32 data, Side side)
    {
        if (side == Side.Bottom && world.GetBlock(position.Below())?.Block is not IPlantable)
            Destroy(world, position);
    }

    /// <inheritdoc />
    public override void RandomUpdate(World world, Vector3i position, UInt32 data)
    {
        var stage = (GrowthStage) (data & 0b00_0111);
        UInt32 lowered = data & 0b00_1000;

        if (stage is GrowthStage.Final or GrowthStage.Dead ||
            world.GetBlock(position.Below())?.Block is not IPlantable plantable) return;

        if ((Int32) stage > 2)
        {
            if (world.GetFluid(position.Below())?.Fluid == Elements.Fluids.Instance.SeaWater)
            {
                world.SetBlock(this.AsInstance(lowered | (UInt32) GrowthStage.Dead), position);

                return;
            }

            if (!plantable.SupportsFullGrowth) return;
            if (!plantable.TryGrow(world, position.Below(), Elements.Fluids.Instance.FreshWater, FluidLevel.One)) return;
        }

        world.SetBlock(this.AsInstance(lowered | (UInt32) (stage + 1)), position);
    }

    private enum GrowthStage
    {
        Initial,
        Second,
        Third,
        Fourth,
        Fifth,
        Sixth,
        Final,
        Dead
    }
}
