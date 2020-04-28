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
    public class SpiderWeb : CrossBlock
    {
        private readonly float maxVelocity;

        /// <summary>
        /// Creates a SpiderWeb block, a block that slows down entities that collide with it.
        /// </summary>
        /// <param name="name">The name of the block.</param>
        /// <param name="maxVelocity">The maximum velocity of entities colliding with this block.</param>
        public SpiderWeb(string name, string texture, float maxVelocity) :
        base(
            name,
            texture,
            isReplaceable: false,
            recieveCollisions: true,
            isTrigger: true,
            BoundingBox.Block)
        {
            this.maxVelocity = maxVelocity;
        }

        public override void OnCollision(PhysicsEntity entity, int x, int y, int z)
        {
            entity.Velocity = VMath.Clamp(entity.Velocity, -1f, maxVelocity);
        }
    }
}