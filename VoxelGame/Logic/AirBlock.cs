// <copyright file="AirBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Physics;

namespace VoxelGame.Logic
{
    /// <summary>
    /// AirBlocks are blocks that have no collision and are not rendered. They are used for the air block that stands for the absence of other blocks.
    /// </summary>
    public class AirBlock : Block
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AirBlock"/> class.
        /// </summary>
        /// <param name="name">The unique name of this block</param>
        public AirBlock(string name) :
            base(
                name: name,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: false,
                isSolid: false,
                recieveCollisions: false,
                isTrigger: false,
                isReplaceable: true,
                BoundingBox.Block)
        {
        }

        public override bool Place(int x, int y, int z, Entities.PhysicsEntity entity)
        {
            return false;
        }

        public override bool Destroy(int x, int y, int z, Entities.PhysicsEntity entity)
        {
            return false;
        }

        /// <summary>
        /// This method is used to get the mesh from a block.
        /// </summary>
        /// <param name="side">The side of the block the mesh is required from.</param>
        /// <param name="data">The parameter is not used.</param>
        /// <param name="vertecies">The parameter is not used.</param>
        /// <param name="indicies">The parameter is not used.</param>
        /// <returns>Returns null.</returns>
        public override uint GetMesh(BlockSide side, ushort data, out float[] vertecies, out uint[] indicies)
        {
            vertecies = null;
            indicies = null;

            return 0;
        }

        public override void OnCollision(Entities.PhysicsEntity entity, int x, int y, int z)
        {
            throw new System.NotImplementedException();
        }

        public override void BlockUpdate(int x, int y, int z)
        {
        }
    }
}