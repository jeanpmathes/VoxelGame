// <copyright file="SpiderWeb.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Entities;
using VoxelGame.Physics;
using VoxelGame.Utilities;

namespace VoxelGame.Logic.Blocks
{
    public class SpiderWebBlock : CrossBlock
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        protected readonly float maxVelocity;
#pragma warning restore CA1051 // Do not declare visible instance fields

        /// <summary>
        /// Creates a SpiderWeb block, a block that slows down entities that collide with it.
        /// </summary>
        /// <param name="name">The name of the block.</param>
        /// <param name="maxVelocity">The maximum velocity of entities colliding with this block.</param>
        public SpiderWebBlock(string name, string texture, float maxVelocity) :
        base(
            name,
            texture,
            recieveCollisions: true,
            isTrigger: true,
            isReplaceable: false,
            BoundingBox.Block)
        {
            this.maxVelocity = maxVelocity;
        }

        public override void EntityCollision(PhysicsEntity entity, int x, int y, int z)
        {
            if (entity == null)
            {
                throw new System.ArgumentNullException(nameof(entity));
            }

            entity.Velocity = VMath.Clamp(entity.Velocity, -1f, maxVelocity);
        }
    }
}