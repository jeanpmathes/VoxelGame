// <copyright file="DoubleCropBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block which grows on farmland and has multiple growth stages, of which some are two blocks tall.
///     Data bit usage: <c>-lhsss</c>
/// </summary>
// l: lowered
// s: stage
// h: height
public class DoubleCropBlock : Block, ICombustible, IFillable, ICropPlant
{
    private readonly string texture;

    private readonly List<BoundingVolume> volumes = new();

    private (
        int dead, int first, int second, int third,
        (int low, int top) fourth, (int low, int top) fifth, (int low, int top) sixth, (int low, int top) final
        ) stages;

    private int[] stageTextureIndicesLow = null!;
    private int[] stageTextureIndicesTop = null!;

    internal DoubleCropBlock(string name, string namedId, string texture, int dead, int first, int second,
        int third, (int low, int top) fourth, (int low, int top) fifth, (int low, int top) sixth,
        (int low, int top) final) :
        base(
            name,
            namedId,
            new BlockFlags(),
            BoundingVolume.Block)
    {
        this.texture = texture;

        stages = (dead, first, second, third, fourth, fifth, sixth, final);

        for (uint data = 0; data <= 0b01_1111; data++) volumes.Add(CreateVolume(data));
    }

    ICropPlant.MeshData ICropPlant.GetMeshData(BlockMeshInfo info)
    {
        var stageData = (int) (info.Data & 0b00_0111);

        bool isUpper = (info.Data & 0b00_1000) != 0;
        bool isLowered = (info.Data & 0b01_0000) != 0;
        bool hasUpper = (GrowthStage) stageData >= GrowthStage.Fourth;

        int textureIndex = !isUpper ? stageTextureIndicesLow[stageData] : stageTextureIndicesTop[stageData];

        return new ICropPlant.MeshData(textureIndex)
        {
            HasUpper = hasUpper,
            IsLowered = isLowered,
            IsUpper = isUpper,
            IsDoubleCropPlant = true
        };
    }

    /// <inheritdoc />
    public void OnFluidChange(World world, Vector3i position, Fluid fluid, FluidLevel level)
    {
        if (fluid.IsFluid && level > FluidLevel.Four) ScheduleDestroy(world, position);
    }

    /// <inheritdoc />
    protected override void OnSetup(ITextureIndexProvider indexProvider)
    {
        int baseIndex = indexProvider.GetTextureIndex(texture);

        if (baseIndex == 0) stages = (0, 0, 0, 0, (0, 0), (0, 0), (0, 0), (0, 0));

        stageTextureIndicesLow = new[]
        {
            baseIndex + stages.dead,
            baseIndex + stages.first,
            baseIndex + stages.second,
            baseIndex + stages.third,
            baseIndex + stages.fourth.low,
            baseIndex + stages.fifth.low,
            baseIndex + stages.sixth.low,
            baseIndex + stages.final.low
        };

        stageTextureIndicesTop = new[]
        {
            0,
            0,
            0,
            0,
            baseIndex + stages.fourth.top,
            baseIndex + stages.fifth.top,
            baseIndex + stages.sixth.top,
            baseIndex + stages.final.top
        };
    }

    private static BoundingVolume CreateVolume(uint data)
    {
        var stage = (GrowthStage) (data & 0b00_0111);

        bool isLowerAndStillGrowing = (data & 0b00_1000) == 0 && stage == GrowthStage.Initial;
        bool isUpperAndStillGrowing = (data & 0b00_1000) != 0 && stage is GrowthStage.Fourth or GrowthStage.Fifth;

        if (isLowerAndStillGrowing || isUpperAndStillGrowing)
            return BoundingVolume.BlockWithHeight(height: 7);

        return BoundingVolume.BlockWithHeight(height: 15);
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(uint data)
    {
        return volumes[(int) data & 0b01_1111];
    }

    /// <inheritdoc />
    public override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        return world.GetBlock(position.Below())?.Block is IPlantable;
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        bool isLowered = world.IsLowered(position);

        var data = (uint) GrowthStage.Initial;
        if (isLowered) data |= 0b01_0000;

        world.SetBlock(this.AsInstance(data), position);
    }

    /// <inheritdoc />
    protected override void DoDestroy(World world, Vector3i position, uint data, PhysicsEntity? entity)
    {
        world.SetDefaultBlock(position);

        bool isBase = (data & 0b00_1000) == 0;

        if ((data & 0b00_0111) >= (int) GrowthStage.Fourth)
            world.SetDefaultBlock(isBase ? position.Above() : position.Below());
    }

    /// <inheritdoc />
    public override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
    {
        // Check if this block is the lower part and if the ground supports plant growth.
        if (side == BlockSide.Bottom && (data & 0b00_1000) == 0 &&
            (world.GetBlock(position.Below())?.Block ?? Logic.Blocks.Instance.Air) is not IPlantable) Destroy(world, position);
    }

    /// <inheritdoc />
    public override void RandomUpdate(World world, Vector3i position, uint data)
    {
        var stage = (GrowthStage) (data & 0b00_0111);
        uint lowered = data & 0b01_0000;

        // If this block is the upper part, the random update is ignored.
        if ((data & 0b00_1000) != 0) return;

        if (world.GetBlock(position.Below())?.Block is not IPlantable plantable) return;
        if ((int) stage > 2 && !plantable.SupportsFullGrowth) return;
        if (stage is GrowthStage.Final or GrowthStage.Dead) return;

        if (stage >= GrowthStage.Third) GrowBothParts(world, position, plantable, lowered, stage);
        else world.SetBlock(this.AsInstance(lowered | (uint) (stage + 1)), position);
    }

    private void GrowBothParts(World world, Vector3i position, IPlantable plantable, uint lowered,
        GrowthStage stage)
    {
        if (world.GetFluid(position.Below())?.Fluid == Logic.Fluids.Instance.SeaWater)
        {
            world.SetBlock(this.AsInstance(lowered | (uint) GrowthStage.Dead), position);
            if (stage != GrowthStage.Third) world.SetDefaultBlock(position.Above());

            return;
        }

        BlockInstance? above = world.GetBlock(position.Above());
        bool growthPossible = above?.Block.IsReplaceable == true || above?.Block == this;

        if (!growthPossible || !plantable.TryGrow(world, position.Below(), Logic.Fluids.Instance.FreshWater, FluidLevel.One)) return;

        world.SetBlock(this.AsInstance(lowered | (uint) (stage + 1)), position);

        world.SetBlock(
            this.AsInstance(lowered | (uint) (0b00_1000 | ((int) stage + 1))),
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

        // Second

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

        // Sixth

        /// <summary>
        ///     Two blocks tall.
        /// </summary>
        Final = 7
    }
}

