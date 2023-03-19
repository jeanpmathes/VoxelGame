// <copyright file="GrowingFlatBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
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
public class GrowingFlatBlock : FlatBlock, ICombustible, IFillable
{
    internal GrowingFlatBlock(string name, string namedId, string texture, float climbingVelocity,
        float slidingVelocity) :
        base(
            name,
            namedId,
            texture,
            climbingVelocity,
            slidingVelocity) {}

    /// <inheritdoc />
    public void OnFluidChange(World world, Vector3i position, Fluid fluid, FluidLevel level)
    {
        if (fluid.IsFluid && level > FluidLevel.Two) ScheduleDestroy(world, position);
    }

    /// <inheritdoc />
    protected override IComplex.MeshData GetMeshData(BlockMeshInfo info)
    {
        return base.GetMeshData(info) with {Tint = TintColor.Neutral};
    }

    /// <inheritdoc />
    public override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
    {
        var orientation = (Orientation) (data & 0b00_0011);

        (Block block, uint dataAbove) = world.GetBlock(position.Above()) ?? BlockInstance.Default;

        // If another block of this type is above, no solid block is required to hold.
        if (block == this &&
            orientation == (Orientation) (dataAbove & 0b00_0011)) return;

        if (side == BlockSide.Top) side = orientation.Opposite().ToBlockSide();

        CheckBack(world, position, side, orientation, schedule: true);
    }

    /// <inheritdoc />
    public override void RandomUpdate(World world, Vector3i position, uint data)
    {
        var orientation = (Orientation) (data & 0b00_0011);
        var age = (int) ((data & 0b1_1100) >> 2);

        if (age < 7) world.SetBlock(this.AsInstance((uint) (((age + 1) << 2) | (int) orientation)), position);
        else if (world.GetBlock(position.Below())?.Block == Logic.Blocks.Instance.Air)
            world.SetBlock(this.AsInstance((uint) orientation), position.Below());
    }
}


