// <copyright file="CrossPlantBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
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
        /// <param name="name">The name of this block and the texture file.</param>
        /// <param name="isReplaceable">Indicates whether this block will be replaceable.</param>
        /// <param name="boundingBox">The bounding box of this block.</param>
        public CrossPlantBlock(string name, string namedId, string texture, bool isReplaceable, BoundingBox boundingBox) :
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

        protected override bool Place(Entities.PhysicsEntity? entity, int x, int y, int z)
        {
            // Check the block under the placement position.
            Block ground = Game.World.GetBlock(x, y - 1, z, out _) ?? Block.Air;

            if (ground is IPlantable)
            {
                Game.World.SetBlock(this, 0, x, y, z);

                return true;
            }
            else
            {
                return false;
            }
        }

        internal override void BlockUpdate(int x, int y, int z, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom && !((Game.World.GetBlock(x, y - 1, z, out _) ?? Block.Air) is IPlantable))
            {
                Destroy(x, y, z);
            }
        }

        public void LiquidChange(int x, int y, int z, Liquid liquid, LiquidLevel level)
        {
            if (liquid.Direction > 0 && level > LiquidLevel.Four) Destroy(x, y, z);
        }
    }
}