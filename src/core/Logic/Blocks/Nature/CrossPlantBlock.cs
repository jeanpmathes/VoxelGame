// <copyright file="CrossPlantBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    public class CrossPlantBlock : CrossBlock, IFillable
    {
        /// <summary>
        /// Initializes a new instance of a cross plant; a plant made out of two intersecting planes. It is using a neutral tint.
        /// Data bit usage: <c>------</c>
        /// </summary>
        /// <param name="name">The name of this block.</param>
        /// <param name="namedId">The unique and unlocalized name of this block.</param>
        /// <param name="texture">The name of the texture of this block.</param>
        /// <param name="isReplaceable">Indicates whether this block will be replaceable.</param>
        /// <param name="boundingBox">The bounding box of this block.</param>
        internal CrossPlantBlock(string name, string namedId, string texture, bool isReplaceable, BoundingBox boundingBox) :
            base(
                name,
                namedId,
                texture,
                receiveCollisions: false,
                isTrigger: false,
                isReplaceable,
                boundingBox)
        {
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return base.GetMesh(info).Modified(TintColor.Neutral);
        }

        internal override bool CanPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            // Check the block under the placement position.
            Block ground = world.GetBlock(x, y - 1, z, out _) ?? Block.Air;
            return ground is IPlantable;
        }

        internal override void BlockUpdate(World world, int x, int y, int z, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom && !((world.GetBlock(x, y - 1, z, out _) ?? Block.Air) is IPlantable))
            {
                Destroy(world, x, y, z);
            }
        }

        public void LiquidChange(World world, int x, int y, int z, Liquid liquid, LiquidLevel level)
        {
            if (liquid.Direction > 0 && level > LiquidLevel.Four) Destroy(world, x, y, z);
        }
    }
}