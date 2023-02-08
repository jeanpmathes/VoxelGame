// <copyright file="DoubleCrossPlantBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
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
public class DoubleCrossPlantBlock : Block, ICombustible, IFillable, ICrossPlant
{
    private readonly string bottomTexture;
    private readonly int topTexOffset;

    private int bottomTextureIndex;
    private int topTextureIndex;

    internal DoubleCrossPlantBlock(string name, string namedId, string bottomTexture, int topTexOffset,
        BoundingVolume boundingVolume) :
        base(
            name,
            namedId,
            new BlockFlags(),
            boundingVolume)
    {
        this.bottomTexture = bottomTexture;
        this.topTexOffset = topTexOffset;
    }

    ICrossPlant.MeshData ICrossPlant.GetMeshData(BlockMeshInfo info)
    {
        bool isUpper = (info.Data & 0b01) != 0;
        bool isLowered = (info.Data & 0b10) != 0;

        return new ICrossPlant.MeshData(isUpper ? topTextureIndex : bottomTextureIndex)
        {
            Tint = TintColor.Neutral,
            HasUpper = true,
            IsLowered = isLowered,
            IsUpper = isUpper
        };
    }

    /// <inheritdoc />
    public void FluidChange(World world, Vector3i position, Fluid fluid, FluidLevel level)
    {
        if (fluid.IsFluid && level > FluidLevel.Five) ScheduleDestroy(world, position);
    }

    /// <inheritdoc />
    protected override void OnSetup(ITextureIndexProvider indexProvider)
    {
        bottomTextureIndex = indexProvider.GetTextureIndex(bottomTexture);
        topTextureIndex = bottomTextureIndex + topTexOffset;
    }

    /// <inheritdoc />
    public override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        return world.GetBlock(position.Above())?.Block.IsReplaceable == true &&
               (world.GetBlock(position.Below())?.Block ?? Logic.Blocks.Instance.Air) is IPlantable;
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        bool isLowered = world.IsLowered(position);

        uint data = (isLowered ? 1u : 0u) << 1;

        world.SetBlock(this.AsInstance(data), position);
        world.SetBlock(this.AsInstance(data | 1), position.Above());
    }

    /// <inheritdoc />
    protected override void DoDestroy(World world, Vector3i position, uint data, PhysicsEntity? entity)
    {
        bool isBase = (data & 0b1) == 0;

        world.SetDefaultBlock(position);
        world.SetDefaultBlock(isBase ? position.Above() : position.Below());
    }

    /// <inheritdoc />
    public override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
    {
        // Check if this block is the lower part and if the ground supports plant growth.
        if (side == BlockSide.Bottom && (data & 0b1) == 0 &&
            (world.GetBlock(position.Below())?.Block ?? Logic.Blocks.Instance.Air) is not IPlantable) Destroy(world, position);
    }
}

