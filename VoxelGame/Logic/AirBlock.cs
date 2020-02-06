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
        public AirBlock(string name) : base(name, false, false)
        {
        }

        public override uint GetMesh(BlockSide side, out float[] vertecies, out uint[] indicies)
        {
            vertecies = null;
            indicies = null;

            return 0;
        }
    }
}
