// <copyright file="FruitCropBlock.cs" company="VoxelGame">
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
///     A block that places a fruit block when reaching the final growth stage.
///     Data bit usage: <c>--sssl</c>
/// </summary>
// l: lowered
// s: stage
public class FruitCropBlock : Block, ICombustible, IFillable, IFoliage
{
    private readonly Block fruit;
    private readonly String texture;

    private readonly List<BoundingVolume> volumes = [];
    private readonly List<BlockMesh> meshes = [];

    internal FruitCropBlock(String name, String namedID, String texture, Block fruit) :
        base(
            name,
            namedID,
            new BlockFlags(),
            new BoundingVolume(new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3d(x: 0.175f, y: 0.5f, z: 0.175f)))
    {
        this.texture = texture;
        this.fruit = fruit;

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
    protected override void OnSetUp(ITextureIndexProvider indexProvider, VisualConfiguration visuals)
    {
        Int32 baseTextureIndex = indexProvider.GetTextureIndex(texture);

        (Int32 dead, Int32 initial, Int32 last) textureIndices =
        (
            baseTextureIndex + 0,
            baseTextureIndex + 1,
            baseTextureIndex + 2
        );

        for (UInt32 data = 0; data <= 0b00_1111; data++) meshes.Add(CreateMesh(data, textureIndices, visuals));
    }

    private static BoundingVolume CreateVolume(UInt32 data)
    {
        var stage = (GrowthStage) ((data >> 1) & 0b111);

        return stage <= GrowthStage.First
            ? new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.25f, z: 0.5f),
                new Vector3d(x: 0.175f, y: 0.25f, z: 0.175f))
            : new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f),
                new Vector3d(x: 0.175f, y: 0.5f, z: 0.175f));
    }

    private static BlockMesh CreateMesh(UInt32 data, (Int32 dead, Int32 initial, Int32 last) textureIndices, VisualConfiguration visuals)
    {
        var stage = (GrowthStage) ((data >> 1) & 0b111);
        Boolean isLowered = (data & 0b1) != 0;

        Int32 textureIndex = stage switch
        {
            GrowthStage.Initial => textureIndices.initial,
            GrowthStage.First => textureIndices.initial,
            GrowthStage.Second => textureIndices.last,
            GrowthStage.Third => textureIndices.last,
            GrowthStage.Fourth => textureIndices.last,
            GrowthStage.Fifth => textureIndices.last,
            GrowthStage.Ready => textureIndices.last,
            GrowthStage.Dead => textureIndices.dead,
            _ => 0
        };

        return BlockMeshes.CreateCropPlantMesh(visuals.FoliageQuality, createMiddlePiece: true, textureIndex, isLowered);
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(UInt32 data)
    {
        return volumes[(Int32) data & 0b00_1111];
    }

    /// <inheritdoc />
    public override Boolean CanPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        return PlantBehaviour.CanPlace(world, position);
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        PlantBehaviour.DoPlace(this, world, position);
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, UInt32 data, BlockSide side)
    {
        PlantBehaviour.NeighborUpdate(world, this, position, side);
    }

    /// <inheritdoc />
    public override void RandomUpdate(World world, Vector3i position, UInt32 data)
    {
        if (world.GetBlock(position.Below())?.Block is not IPlantable ground) return;

        var stage = (GrowthStage) ((data >> 1) & 0b111);
        UInt32 isLowered = data & 0b1;

        switch (stage)
        {
            case < GrowthStage.Ready:
                world.SetBlock(this.AsInstance((UInt32) ((Int32) (stage + 1) << 1) | isLowered), position);

                break;

            case GrowthStage.Ready when ground.SupportsFullGrowth && world.GetFluid(position.Below())?.Fluid == Elements.Fluids.Instance.SeaWater:
                world.SetBlock(this.AsInstance(((UInt32) GrowthStage.Dead << 1) | isLowered), position);

                break;

            case GrowthStage.Ready when ground.SupportsFullGrowth && ground.TryGrow(
                world,
                position.Below(),
                Elements.Fluids.Instance.FreshWater,
                FluidLevel.Two):
            {
                foreach (Orientation orientation in Orientations.ShuffledStart(position))
                {
                    if (!fruit.Place(world, orientation.Offset(position))) continue;

                    world.SetBlock(this.AsInstance(((UInt32) GrowthStage.Second << 1) | isLowered), position);

                    break;
                }

                break;
            }
                #pragma warning disable // Suppress that the default is redundant. It is needed so that all cases are covered.
            default:
                // Ground does not support full growth.
                break;
                #pragma warning restore
        }
    }

    private enum GrowthStage
    {
        Initial,
        First,
        Second,
        Third,
        Fourth,
        Fifth,
        Ready,
        Dead
    }
}
