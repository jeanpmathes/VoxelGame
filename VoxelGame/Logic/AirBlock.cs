// <copyright file="AirBlock.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
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
        public AirBlock(string name) : base(name, false, false)
        {
        }

        /// <summary>
        /// This method is used to get the mesh from a block.
        /// </summary>
        /// <param name="side">The side of the block the mesh is required from.</param>
        /// <param name="vertecies">The parameter is not used.</param>
        /// <param name="indicies">The parameter is not used.</param>
        /// <returns>Returns null.</returns>
        public override uint GetMesh(BlockSide side, ushort data, out float[] vertecies, out uint[] indicies)
        {
            vertecies = null;
            indicies = null;

            return 0;
        }
    }
}