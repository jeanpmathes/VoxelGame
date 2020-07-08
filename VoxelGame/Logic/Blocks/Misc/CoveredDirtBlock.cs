// <copyright file="GrassBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Entities;
using VoxelGame.Logic.Interfaces;
using VoxelGame.Visuals;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// A block that changes into dirt when something is placed on top of it. This block can use a neutral tint if specified in the constructor.
    /// </summary>
    public class CoveredDirtBlock : BasicBlock, IPlantable
    {
        private protected readonly bool hasNeutralTint;

        public CoveredDirtBlock(string name, TextureLayout layout, bool hasNeutralTint) :
            base(
                name,
                layout,
                isOpaque: true,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                isInteractable: false)
        {
            this.hasNeutralTint = hasNeutralTint;
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            tint = (hasNeutralTint) ? TintColor.Neutral : TintColor.None;

            return base.GetMesh(side, data, out vertices, out textureIndices, out indices, out _);
        }

        protected override bool Place(PhysicsEntity? entity, int x, int y, int z)
        {
            if ((Game.World.GetBlock(x, y + 1, z, out _) ?? Block.AIR).IsSolidAndFull)
            {
                return Block.DIRT.Place(x, y, z, entity);
            }
            else
            {
                Game.World.SetBlock(this, 0, x, y, z);

                return true;
            }
        }

        internal override void BlockUpdate(int x, int y, int z, byte data, BlockSide side)
        {
            if (side == BlockSide.Top && (Game.World.GetBlock(x, y + 1, z, out _) ?? Block.AIR).IsSolidAndFull)
            {
                Game.World.SetBlock(Block.DIRT, 0, x, y, z);
            }
        }
    }
}