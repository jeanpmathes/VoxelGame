﻿// <copyright file="DoubleCropBlock.cs" company="VoxelGame">
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
///     A block which grows on farmland and has multiple growth stages, of which some are two blocks tall.
///     Data bit usage: <c>-lhsss</c>
/// </summary>
// l: lowered
// s: stage
// h: height
public class DoubleCropBlock : Block, ICombustible, IFillable, IFoliage
{
    private readonly TID texture;

    private readonly List<BlockMesh> meshes = [];
    private readonly List<BoundingVolume> volumes = [];

    internal DoubleCropBlock(String name, String namedID, TID texture) :
        base(
            name,
            namedID,
            new BlockFlags(),
            BoundingVolume.Block)
    {
        this.texture = texture;

        for (UInt32 data = 0; data <= 0b01_1111; data++) volumes.Add(CreateVolume(data));
    }

    IFoliage.MeshData IFoliage.GetMeshData(BlockMeshInfo info)
    {
        var stageData = (Int32) (info.Data & 0b00_0111);

        Boolean isUpper = (info.Data & 0b00_1000) != 0;
        Boolean hasUpper = (GrowthStage) stageData >= GrowthStage.Fourth;

        return new IFoliage.MeshData(meshes[(Int32) (info.Data & 0b01_1111)])
        {
            IsDoublePlant = hasUpper,
            IsUpperPart = isUpper
        };
    }

    /// <inheritdoc />
    public override void ContentUpdate(World world, Vector3i position, Content content)
    {
        if (content.Fluid.Fluid.IsFluid && content.Fluid.Level > FluidLevel.Four) ScheduleDestroy(world, position);
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider textureIndexProvider, IBlockModelProvider modelProvider, VisualConfiguration visuals)
    {
        var lowerTextureIndices = new Int32[(Int32) GrowthStage.Final + 1];
        var upperTextureIndices = new Int32[(Int32) GrowthStage.Final + 1];

        for (var stage = GrowthStage.Dead; stage <= GrowthStage.Final; stage++)
        {
            Byte xOffset = StageToTextureOffset(stage);
            Boolean isTwoBlocksTall = IsTwoBlocksTall(stage);

            lowerTextureIndices[(Int32) stage] = textureIndexProvider.GetTextureIndex(texture.Offset(xOffset));
            upperTextureIndices[(Int32) stage] = isTwoBlocksTall ? textureIndexProvider.GetTextureIndex(texture.Offset(xOffset, y: 1)) : 0;
        }

        for (UInt32 data = 0; data <= 0b01_1111; data++) meshes.Add(CreateMesh(data, lowerTextureIndices, upperTextureIndices, visuals));
    }

    private static Byte StageToTextureOffset(GrowthStage stage)
    {
        return stage switch
        {
            GrowthStage.Dead => 0,
            GrowthStage.Initial => 1,
            GrowthStage.Second => 2,
            GrowthStage.Third => 2,
            GrowthStage.Fourth => 3,
            GrowthStage.Fifth => 3,
            GrowthStage.Sixth => 4,
            GrowthStage.Final => 5,
            _ => throw Exceptions.UnsupportedEnumValue(stage)
        };
    }

    private static Boolean IsTwoBlocksTall(GrowthStage stage)
    {
        return stage is GrowthStage.Fourth or GrowthStage.Fifth or GrowthStage.Sixth or GrowthStage.Final;
    }

    private static BoundingVolume CreateVolume(UInt32 data)
    {
        var stage = (GrowthStage) (data & 0b00_0111);

        Boolean isLowerAndStillGrowing = (data & 0b00_1000) == 0 && stage == GrowthStage.Initial;
        Boolean isUpperAndStillGrowing = (data & 0b00_1000) != 0 && stage is GrowthStage.Fourth or GrowthStage.Fifth;

        if (isLowerAndStillGrowing || isUpperAndStillGrowing)
            return BoundingVolume.BlockWithHeight(height: 7);

        return BoundingVolume.BlockWithHeight(height: 15);
    }

    private static BlockMesh CreateMesh(UInt32 data, Int32[] stageTextureIndicesLow, Int32[] stageTextureIndicesTop, VisualConfiguration visuals)
    {
        var stageData = (Int32) (data & 0b00_0111);
        Boolean isUpper = (data & 0b00_1000) != 0;
        Boolean isLowered = (data & 0b01_0000) != 0;

        Int32 textureIndex = !isUpper ? stageTextureIndicesLow[stageData] : stageTextureIndicesTop[stageData];

        return BlockMeshes.CreateCropPlantMesh(visuals.FoliageQuality, createMiddlePiece: false, textureIndex, isLowered);
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(UInt32 data)
    {
        return volumes[(Int32) data & 0b01_1111];
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
        if (isLowered) data |= 0b01_0000;

        world.SetBlock(this.AsInstance(data), position);
    }

    /// <inheritdoc />
    protected override void DoDestroy(World world, Vector3i position, UInt32 data, PhysicsActor? actor)
    {
        world.SetDefaultBlock(position);

        Boolean isBase = (data & 0b00_1000) == 0;

        if ((data & 0b00_0111) >= (Int32) GrowthStage.Fourth)
            world.SetDefaultBlock(isBase ? position.Above() : position.Below());
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, UInt32 data, Side side)
    {
        // Check if this block is the lower part and if the ground supports plant growth.
        if (side == Side.Bottom && (data & 0b00_1000) == 0 &&
            (world.GetBlock(position.Below())?.Block ?? Elements.Blocks.Instance.Air) is not IPlantable) Destroy(world, position);
    }

    /// <inheritdoc />
    public override void RandomUpdate(World world, Vector3i position, UInt32 data)
    {
        var stage = (GrowthStage) (data & 0b00_0111);
        UInt32 lowered = data & 0b01_0000;

        // If this block is the upper part, the random update is ignored.
        if ((data & 0b00_1000) != 0) return;

        if (world.GetBlock(position.Below())?.Block is not IPlantable plantable) return;
        if ((Int32) stage > 2 && !plantable.SupportsFullGrowth) return;
        if (stage is GrowthStage.Final or GrowthStage.Dead) return;

        if (stage >= GrowthStage.Third) GrowBothParts(world, position, plantable, lowered, stage);
        else world.SetBlock(this.AsInstance(lowered | (UInt32) (stage + 1)), position);
    }

    private void GrowBothParts(World world, Vector3i position, IPlantable plantable, UInt32 lowered,
        GrowthStage stage)
    {
        if (world.GetFluid(position.Below())?.Fluid == Elements.Fluids.Instance.SeaWater)
        {
            world.SetBlock(this.AsInstance(lowered | (UInt32) GrowthStage.Dead), position);
            if (stage != GrowthStage.Third) world.SetDefaultBlock(position.Above());

            return;
        }

        BlockInstance? above = world.GetBlock(position.Above());
        Boolean growthPossible = above?.Block.IsReplaceable == true || above?.Block == this;

        if (!growthPossible || !plantable.TryGrow(world, position.Below(), Elements.Fluids.Instance.FreshWater, FluidLevel.One)) return;

        world.SetBlock(this.AsInstance(lowered | (UInt32) (stage + 1)), position);

        world.SetBlock(
            this.AsInstance(lowered | (UInt32) (0b00_1000 | ((Int32) stage + 1))),
            position.Above());
    }

    private enum GrowthStage
    {
        /// <summary>
        ///     One Block tall.
        /// </summary>
        Dead = 0,

        /// <summary>
        ///     One Block tall.
        /// </summary>
        Initial = 1,

        /// <summary>
        ///     One Block tall.
        /// </summary>
        Second = 2,

        /// <summary>
        ///     One Block tall.
        /// </summary>
        Third = 3,

        /// <summary>
        ///     Two blocks tall.
        /// </summary>
        Fourth = 4,

        /// <summary>
        ///     Two blocks tall.
        /// </summary>
        Fifth = 5,

        /// <summary>
        ///     Two blocks tall.
        /// </summary>
        Sixth = 6,

        /// <summary>
        ///     Two blocks tall.
        /// </summary>
        Final = 7
    }
}
