// <copyright file="GrassBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
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

        private protected int[] wetTextureIndices = null!;
        private protected TextureLayout wet;

        public bool SupportsFullGrowth => supportsFullGrowth;

        public CoveredDirtBlock(string name, string namedId, TextureLayout normal, TextureLayout wet, bool hasNeutralTint, bool supportsFullGrowth) :
            base(
                name,
                namedId,
                layout: normal,
                isOpaque: true,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                receiveCollisions: false,
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

            wetTextureIndices = wet.GetTexIndexArray();
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            BlockMeshData mesh = base.GetMesh(info);

            mesh = mesh.Modified((hasNeutralTint) ? TintColor.Neutral : TintColor.None);

            if (info.Liquid.Direction > 0) mesh = mesh.SwapTextureIndex(wetTextureIndices[(int)info.Side]);

            return mesh;
        }

        protected override bool Place(PhysicsEntity? entity, int x, int y, int z)
        {
            if (Game.World.HasSolidTop(x, y, z))
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
            if (side == BlockSide.Top && Game.World.HasOpaqueTop(x, y, z))
            {
                Game.World.SetBlock(Block.Dirt, 0, x, y, z);
            }
        }

        public virtual bool AllowInflow(int x, int y, int z, BlockSide side, Liquid liquid)
        {
            return liquid.Viscosity < 200;
        }
    }
}