// <copyright file="FruitCropBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Blocks;

/// <summary>
///     A block that places a fruit block when reaching the final growth stage.
///     Data bit usage: <c>--sssl</c>
/// </summary>
// l: lowered
// s: stage
public class FruitCropBlock : Block, IFlammable, IFillable, ICrossPlant
{
    private readonly Block fruit;
    private readonly string texture;

    private readonly List<BoundingVolume> volumes = new();

    private (int dead, int initial, int last) textureIndex;

    internal FruitCropBlock(string name, string namedId, string texture, Block fruit) :
        base(
            name,
            namedId,
            new BlockFlags(),
            new BoundingVolume(new Vector3(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3(x: 0.175f, y: 0.5f, z: 0.175f)))
    {
        this.texture = texture;
        this.fruit = fruit;

        for (uint data = 0; data <= 0b00_1111; data++) volumes.Add(CreateVolume(data));
    }

    ICrossPlant.MeshData ICrossPlant.GetMeshData(BlockMeshInfo info)
    {
        var stage = (GrowthStage) ((info.Data >> 1) & 0b111);

        int index = stage switch
        {
            GrowthStage.Initial => textureIndex.initial,
            GrowthStage.First => textureIndex.initial,
            GrowthStage.Second => textureIndex.last,
            GrowthStage.Third => textureIndex.last,
            GrowthStage.Fourth => textureIndex.last,
            GrowthStage.Fifth => textureIndex.last,
            GrowthStage.Ready => textureIndex.last,
            GrowthStage.Dead => textureIndex.dead,
            _ => 0
        };

        return new ICrossPlant.MeshData
        {
            TextureIndex = index,
            Tint = TintColor.None,
            HasUpper = false,
            IsLowered = (info.Data & 0b1) == 1,
            IsUpper = false
        };
    }

    /// <inheritdoc />
    public void FluidChange(World world, Vector3i position, Fluid fluid, FluidLevel level)
    {
        if (fluid.IsFluid && level > FluidLevel.Three) ScheduleDestroy(world, position);
    }

    /// <inheritdoc />
    protected override void Setup(ITextureIndexProvider indexProvider)
    {
        int baseTextureIndex = indexProvider.GetTextureIndex(texture);

        textureIndex =
        (
            baseTextureIndex + 0,
            baseTextureIndex + 1,
            baseTextureIndex + 2
        );
    }

    private static BoundingVolume CreateVolume(uint data)
    {
        var stage = (GrowthStage) ((data >> 1) & 0b111);

        return stage <= GrowthStage.First
            ? new BoundingVolume(
                new Vector3(x: 0.5f, y: 0.25f, z: 0.5f),
                new Vector3(x: 0.175f, y: 0.25f, z: 0.175f))
            : new BoundingVolume(
                new Vector3(x: 0.5f, y: 0.5f, z: 0.5f),
                new Vector3(x: 0.175f, y: 0.5f, z: 0.175f));
    }

    /// <inheritdoc />
    protected override BoundingVolume GetBoundingVolume(uint data)
    {
        return volumes[(int) data & 0b00_1111];
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

    /// <inheritdoc />
    public override void RandomUpdate(World world, Vector3i position, uint data)
    {
        if (world.GetBlock(position.Below())?.Block is not IPlantable ground) return;

        var stage = (GrowthStage) ((data >> 1) & 0b111);
        uint isLowered = data & 0b1;

        switch (stage)
        {
            case < GrowthStage.Ready:
                world.SetBlock(this.AsInstance((uint) ((int) (stage + 1) << 1) | isLowered), position);

                break;
            case GrowthStage.Ready when ground.SupportsFullGrowth && ground.TryGrow(
                world,
                position.Below(),
                Fluid.Water,
                FluidLevel.Two):
            {
                foreach (Orientation orientation in Orientations.ShuffledStart(position))
                {
                    if (!fruit.Place(world, orientation.Offset(position))) continue;
                    world.SetBlock(this.AsInstance(((uint) GrowthStage.Second << 1) | isLowered), position);

                    break;
                }

                break;
            }
            case GrowthStage.Ready when ground.SupportsFullGrowth:
                world.SetBlock(this.AsInstance(((uint) GrowthStage.Dead << 1) | isLowered), position);

                break;

                #pragma warning disable
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
