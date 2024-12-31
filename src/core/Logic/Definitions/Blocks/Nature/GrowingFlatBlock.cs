// <copyright file="GrowingFlatBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block that grows downwards and can hang freely. This block is affected by neutral tint.
///     Data bit usage: <c>-aaaoo</c>
/// </summary>
// o: orientation
// a: age
public class GrowingFlatBlock : FlatBlock, ICombustible
{
    internal GrowingFlatBlock(String name, String namedID, TID texture, Single climbingVelocity,
        Single slidingVelocity) :
        base(
            name,
            namedID,
            texture,
            climbingVelocity,
            slidingVelocity) {}

    /// <inheritdoc />
    public override void ContentUpdate(World world, Vector3i position, Content content)
    {
        if (content.Fluid.Fluid.IsFluid && content.Fluid.Level > FluidLevel.Two) ScheduleDestroy(world, position);
    }

    /// <inheritdoc />
    protected override IComplex.MeshData GetMeshData(BlockMeshInfo info)
    {
        return base.GetMeshData(info) with {Tint = TintColor.Neutral};
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, UInt32 data, Side side)
    {
        var orientation = (Orientation) (data & 0b00_0011);

        (Block block, UInt32 dataAbove) = world.GetBlock(position.Above()) ?? BlockInstance.Default;

        // If another block of this type is above, no solid block is required to hold.
        if (block == this &&
            orientation == (Orientation) (dataAbove & 0b00_0011)) return;

        if (side == Side.Top) side = orientation.Opposite().ToSide();

        CheckBack(world, position, side, orientation, schedule: true);
    }

    /// <inheritdoc />
    public override void RandomUpdate(World world, Vector3i position, UInt32 data)
    {
        var orientation = (Orientation) (data & 0b00_0011);
        var age = (Int32) ((data & 0b1_1100) >> 2);

        if (age < 7) world.SetBlock(this.AsInstance((UInt32) (((age + 1) << 2) | (Int32) orientation)), position);
        else if (world.GetBlock(position.Below())?.Block == Elements.Blocks.Instance.Air)
            world.SetBlock(this.AsInstance((UInt32) orientation), position.Below());
    }
}
