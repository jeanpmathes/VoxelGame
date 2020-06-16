// <copyright file="GrassBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Entities;
using VoxelGame.Rendering;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// A block that changes into dirt when something is placed on top of it. This block can use a neutral tint if specified in the constructor.
    /// </summary>
    public class CoveredDirtBlock : BasicBlock
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        protected readonly bool hasNeutralTint;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public CoveredDirtBlock(string name, TextureLayout layout, bool hasNeutralTint) :
            base(
                name,
                layout,
                isOpaque: true,
                renderFaceAtNonOpaques: true,
                isSolid: true)
        {
            this.hasNeutralTint = hasNeutralTint;
        }

        public override bool Place(int x, int y, int z, PhysicsEntity entity)
        {
            Block above = Game.World.GetBlock(x, y + 1, z, out _);

            if (above.IsSolid && above.IsFull)
            {
                return Block.DIRT.Place(x, y, z, entity);
            }
            else
            {
                return base.Place(x, y, z, entity);
            }
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            tint = (hasNeutralTint) ? TintColor.Neutral : TintColor.None;

            return base.GetMesh(side, data, out vertices, out textureIndices, out indices, out _);
        }

        public override void BlockUpdate(int x, int y, int z, byte data)
        {
            // Check block on top of this block
            Block above = Game.World.GetBlock(x, y + 1, z, out _);

            if (above.IsSolid && above.IsFull)
            {
                Game.World.SetBlock(Block.DIRT, 0, x, y, z);
            }
        }
    }
}