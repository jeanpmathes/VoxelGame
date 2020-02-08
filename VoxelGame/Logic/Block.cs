// <copyright file="Block.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
namespace VoxelGame.Logic
{
    /// <summary>
    /// The basic block class. Blocks are used to construct the world.
    /// </summary>
    public abstract class Block
    {
        public string Name { get; protected set; }
        public bool IsFull { get; protected set; }
        public bool IsOpaque { get; protected set; }

        /// <summary>
        /// This property is only relevant for non-opaque full blocks. It decides if their faces should be rendered next to another non-opaque block.
        /// </summary>
        public bool RenderFaceAtNonOpaques { get; protected set; } = true;

        public Block(string name, bool isFull, bool isOpaque)
        {
            Name = name;
            IsFull = isFull;
            IsOpaque = isOpaque;
        }

        public abstract uint GetMesh(BlockSide side, out float[] vertecies, out uint[] indicies);
    }
}