// <copyright file="CrossPlant.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Physics;
using VoxelGame.Rendering;
using VoxelGame.Logic.Interfaces;

namespace VoxelGame.Logic.Blocks
{
    public class CrossPlantBlock : CrossBlock
    {
        /// <summary>
        /// Initializes a new instance of a cross plant; a plant made out of two intersecting planes. It is using a neutral tint.
        /// </summary>
        /// <param name="name">The name of this block and the texture file.</param>
        /// <param name="isReplaceable">Indicates whether this block will be replaceable.</param>
        /// <param name="boundingBox">The bounding box of this block.</param>
        public CrossPlantBlock(string name, string texture, bool isReplaceable, BoundingBox boundingBox) :
            base(
                name,
                texture,
                isReplaceable,
                recieveCollisions: false,
                isTrigger: false,
                boundingBox)
        {
        }

        public override bool Place(int x, int y, int z, Entities.PhysicsEntity entity)
        {
            // Check the block under the placement position.
            Block ground = (Game.World.GetBlock(x, y - 1, z, out _) ?? Block.AIR);

            if (ground is IPlantable)
            {
                return base.Place(x, y, z, entity);
            }
            else
            {
                return false;
            }
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            tint = TintColor.Neutral;

            return base.GetMesh(side, data, out vertices, out textureIndices, out indices, out _);
        }

        public override void BlockUpdate(int x, int y, int z, byte data)
        {
            // Check the block under this block
            Block ground = (Game.World.GetBlock(x, y - 1, z, out _) ?? Block.AIR);

            if (!(ground is IPlantable))
            {
                Destroy(x, y, z, null);
            }
        }
    }
}