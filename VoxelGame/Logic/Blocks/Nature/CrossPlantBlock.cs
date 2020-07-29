// <copyright file="CrossPlantBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Logic.Interfaces;
using VoxelGame.Physics;
using VoxelGame.Visuals;

namespace VoxelGame.Logic.Blocks
{
    public class CrossPlantBlock : CrossBlock, IFlammable
    {
        /// <summary>
        /// Initializes a new instance of a cross plant; a plant made out of two intersecting planes. It is using a neutral tint.
        /// Data bit usage: <c>-----</c>
        /// </summary>
        /// <param name="name">The name of this block and the texture file.</param>
        /// <param name="isReplaceable">Indicates whether this block will be replaceable.</param>
        /// <param name="boundingBox">The bounding box of this block.</param>
        public CrossPlantBlock(string name, string namedId, string texture, bool isReplaceable, BoundingBox boundingBox) :
            base(
                name,
                namedId,
                texture,
                recieveCollisions: false,
                isTrigger: false,
                isReplaceable,
                boundingBox)
        {
        }

        public override uint GetMesh(BlockSide side, uint data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated)
        {
            tint = TintColor.Neutral;

            return base.GetMesh(side, data, out vertices, out textureIndices, out indices, out _, out isAnimated);
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
    }
}