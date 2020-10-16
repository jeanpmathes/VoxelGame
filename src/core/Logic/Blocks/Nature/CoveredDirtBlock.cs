// <copyright file="GrassBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A block that changes into dirt when something is placed on top of it. This block can use a neutral tint if specified in the constructor.
    /// Data bit usage: <c>------</c>
    /// </summary>
    public class CoveredDirtBlock : BasicBlock, IFillable, IPlantable
    {
        private protected readonly bool hasNeutralTint;
        private protected readonly bool supportsFullGrowth;

        private protected int[][] wetTextureIndices = null!;
        private protected TextureLayout wet;

        public bool SupportsFullGrowth { get => supportsFullGrowth; }

        public CoveredDirtBlock(string name, string namedId, TextureLayout normal, TextureLayout wet, bool hasNeutralTint, bool supportsFullGrowth) :
            base(
                name,
                namedId,
                layout: normal,
                isOpaque: true,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                recieveCollisions: false,
                isTrigger: false,
                isInteractable: false)
        {
            this.hasNeutralTint = hasNeutralTint;
            this.supportsFullGrowth = supportsFullGrowth;

            this.wet = wet;
        }

        protected override void Setup()
        {
            base.Setup();

            wetTextureIndices = wet.GetTexIndexArrays();
        }

        public override uint GetMesh(BlockSide side, uint data, Liquid liquid, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated)
        {
            uint verts = base.GetMesh(side, data, liquid, out vertices, out textureIndices, out indices, out _, out isAnimated);

            tint = (hasNeutralTint) ? TintColor.Neutral : TintColor.None;

            if (liquid.Direction > 0) textureIndices = wetTextureIndices[(int)side];

            return verts;
        }

        protected override bool Place(PhysicsEntity? entity, int x, int y, int z)
        {
            if ((Game.World.GetBlock(x, y + 1, z, out _) ?? Block.Air).IsSolidAndFull)
            {
                return Block.Dirt.Place(x, y, z, entity);
            }
            else
            {
                Game.World.SetBlock(this, 0, x, y, z);

                return true;
            }
        }

        internal override void BlockUpdate(int x, int y, int z, uint data, BlockSide side)
        {
            Block above = Game.World.GetBlock(x, y + 1, z, out _) ?? Block.Air;

            if (side == BlockSide.Top && above.IsSolidAndFull && above.IsOpaque)
            {
                Game.World.SetBlock(Block.Dirt, 0, x, y, z);
            }
        }

        public virtual bool IsFillable(int x, int y, int z, Liquid liquid)
        {
            return liquid.Viscosity < 200;
        }
    }
}