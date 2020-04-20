// <copyright file="CrossPlant.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using VoxelGame.Physics;
using VoxelGame.Rendering;

namespace VoxelGame.Logic.Blocks
{
    public class CrossPlant : CrossBlock
    {
        /// <summary>
        /// Initializes a new instance of a cross plant; a plant made out of two intersecting planes.
        /// </summary>
        /// <param name="name">The name of this block and the texture file.</param>
        /// <param name="isReplaceable">Indicates whether this block will be replaceable.</param>
        /// <param name="requiredGround">The block on which this block can be placed.</param>
        /// <param name="boundingBox">The bounding box of this block.</param>
        public CrossPlant(string name, bool isReplaceable, BoundingBox boundingBox) :
            base(
                name,
                isReplaceable,
                boundingBox)
        {
#pragma warning disable CA2214 // Do not call overridable methods in constructors
            Setup();
#pragma warning restore CA2214 // Do not call overridable methods in constructors
        }

        public override bool Place(int x, int y, int z, Entities.PhysicsEntity entity)
        {
            // Check the block under the placement position.
            Block ground = (Game.World.GetBlock(x, y - 1, z) ?? Block.AIR);

            if (ground != Block.DIRT && ground != Block.GRASS)
            {
                return false;
            }

            return base.Place(x, y, z, entity);
        }

        public override void BlockUpdate(int x, int y, int z)
        {
            // Check the block under this block
            Block ground = (Game.World.GetBlock(x, y - 1, z) ?? Block.AIR);

            if (ground != Block.DIRT && ground != Block.GRASS)
            {
                Destroy(x, y, z, null);
            }
        }
    }
}
