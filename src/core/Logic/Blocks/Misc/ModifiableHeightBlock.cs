// <copyright file="ModifiableHeightBlock.cs" company="VoxelGame">
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
    ///     A block that allows to change its height by interacting.
    ///     Data bit usage: <c>--hhhh</c>
    /// </summary>
    public class ModifiableHeightBlock : VaryingHeightBlock
    {
        internal ModifiableHeightBlock(string name, string namedId, TextureLayout layout) :
            base(
                name,
                namedId,
                BlockFlags.Functional,
                layout) {}

        /// <inheritdoc />
        public override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            return world.HasSolidGround(position, solidify: true);
        }

        /// <inheritdoc />
        public override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom && !world.HasSolidGround(position))
            {
                if (GetHeight(data) == IHeightVariable.MaximumHeight)
                    ScheduleDestroy(world, position);
                else
                    Destroy(world, position);
            }
        }

        /// <inheritdoc />
        protected override void EntityInteract(PhysicsEntity entity, Vector3i position, uint data)
        {
            uint height = data & 0b00_1111;
            height++;

            if (height <= IHeightVariable.MaximumHeight) entity.World.SetBlock(this.AsInstance(height), position);
        }
    }
}
