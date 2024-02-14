// <copyright file="GrowingBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block that grows upwards and is destroyed if a certain ground block is not given.
///     Data bit usage: <c>---aaa</c>
/// </summary>
// a: age
public class GrowingBlock : BasicBlock, ICombustible
{
    private readonly int maxHeight;
    private readonly Block requiredGround;

    internal GrowingBlock(string name, string namedID, TextureLayout layout, Block ground, int maxHeight) :
        base(
            name,
            namedID,
            BlockFlags.Basic,
            layout)
    {
        requiredGround = ground;
        this.maxHeight = maxHeight;
    }

    /// <inheritdoc />
    public override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        Block down = world.GetBlock(position.Below())?.Block ?? Logic.Blocks.Instance.Air;

        return down == requiredGround || down == this;
    }

    /// <inheritdoc />
    public override void NeighborUpdate(World world, Vector3i position, uint data, BlockSide side)
    {
        if (side == BlockSide.Bottom)
        {
            Block below = world.GetBlock(position.Below())?.Block ?? Logic.Blocks.Instance.Air;

            if (below != requiredGround && below != this) ScheduleDestroy(world, position);
        }
    }

    /// <inheritdoc />
    public override void RandomUpdate(World world, Vector3i position, uint data)
    {
        var age = (int) (data & 0b00_0111);

        if (age < 7)
        {
            world.SetBlock(this.AsInstance((uint) (age + 1)), position);
        }
        else
        {
            if (!(world.GetBlock(position.Above())?.Block.IsReplaceable ?? false)) return;

            var height = 0;

            for (var offset = 0; offset < maxHeight; offset++)
                if (world.GetBlock(position.Below(offset))?.Block == this) height++;
                else break;

            if (height < maxHeight) Place(world, position.Above());
        }
    }
}
