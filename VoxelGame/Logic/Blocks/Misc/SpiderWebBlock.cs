// <copyright file="SpiderWeb.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Entities;
using VoxelGame.Logic.Interfaces;
using VoxelGame.Physics;
using VoxelGame.Utilities;

namespace VoxelGame.Logic.Blocks
{
    public class SpiderWebBlock : CrossBlock, IFlammable
    {
        private protected readonly float maxVelocity;

        /// <summary>
        /// Creates a SpiderWeb block, a block that slows down entities that collide with it.
        /// </summary>
        /// <param name="name">The name of the block.</param>
        /// <param name="maxVelocity">The maximum velocity of entities colliding with this block.</param>
        public SpiderWebBlock(string name, string namedId, string texture, float maxVelocity) :
        base(
            name,
            namedId,
            texture,
            recieveCollisions: true,
            isTrigger: true,
            isReplaceable: false,
            BoundingBox.Block)
        {
            this.maxVelocity = maxVelocity;
        }

        protected override void EntityCollision(PhysicsEntity entity, int x, int y, int z, byte data)
        {
            entity.Velocity = VMath.Clamp(entity.Velocity, -1f, maxVelocity);
        }
    }
}