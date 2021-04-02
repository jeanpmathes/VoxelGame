using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A dirt-like block that is a bit lower then normal dirt.
    /// Data bit usage: <c>------</c>.
    /// </summary>
    public class InsetDirtBlock : Block, IHeightVariable, IFillable, IPlantable
    {
        private const int height = IHeightVariable.MaximumHeight - 1;

        private readonly TextureLayout dryLayout;
        private readonly TextureLayout wetLayout;

        private int[] dryTextureIndices = null!;
        private int[] wetTextureIndices = null!;

        public bool SupportsFullGrowth => true;

        public InsetDirtBlock(string name, string namedId, TextureLayout dry, TextureLayout wet) :
            base(
                name,
                namedId,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: false,
                isSolid: true,
                receiveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                isInteractable: false,
                BoundingBox.Block,
                TargetBuffer.VaryingHeight)
        {
            this.dryLayout = dry;
            this.wetLayout = wet;
        }

        protected override void Setup()
        {
            dryTextureIndices = dryLayout.GetTexIndexArray();
            wetTextureIndices = wetLayout.GetTexIndexArray();
        }

        protected override BoundingBox GetBoundingBox(int x, int y, int z, uint data)
        {
            return BoundingBox.BlockAt(height, x, y, z);
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            int texture = info.Liquid.Direction > 0
                ? wetTextureIndices[(int)info.Side]
                : dryTextureIndices[(int)info.Side];

            return BlockMeshData.VaryingHeight(texture, TintColor.None);
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

        public int GetHeight(uint data)
        {
            return height;
        }
    }
}