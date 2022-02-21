// <copyright file="GroundedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     A BasicBlock that can only be placed on top of blocks that are both solid and full or will become such blocks.
    ///     These blocks are also flammable.
    ///     Data bit usage: <c>------</c>
    /// </summary>
    public class GroundedBlock : BasicBlock, IFlammable
    {
        internal GroundedBlock(string name, string namedId, BlockFlags flags, TextureLayout layout) :
            base(
                name,
                namedId,
                flags,
                layout) {}

        /// <inheritdoc />
        public override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            return world.HasSolidGround(position, solidify: true);
        }

        /// <inheritdoc />
        public override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom && !world.HasSolidGround(position)) ScheduleDestroy(world, position);
        }
    }
}
