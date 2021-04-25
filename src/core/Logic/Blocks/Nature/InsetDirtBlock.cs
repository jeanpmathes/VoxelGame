// <copyright file="InsetDirtBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A dirt-like block that is a bit lower then normal dirt.
    /// Data bit usage: <c>------</c>.
    /// </summary>
    public class InsetDirtBlock : Block, IHeightVariable, IFillable, IPlantable
    {
        private const int Height = IHeightVariable.MaximumHeight - 1;

        private readonly TextureLayout dryLayout;
        private readonly TextureLayout wetLayout;

        private int[] dryTextureIndices = null!;
        private int[] wetTextureIndices = null!;

        public bool SupportsFullGrowth { get; }

        internal InsetDirtBlock(string name, string namedId, TextureLayout dry, TextureLayout wet, bool supportsFullGrowth) :
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

            SupportsFullGrowth = supportsFullGrowth;
        }

        protected override void Setup()
        {
            dryTextureIndices = dryLayout.GetTexIndexArray();
            wetTextureIndices = wetLayout.GetTexIndexArray();
        }

        protected override BoundingBox GetBoundingBox(uint data)
        {
            return BoundingBox.BlockWithHeight(Height);
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            int texture = info.Liquid.Direction > 0
                ? wetTextureIndices[(int)info.Side]
                : dryTextureIndices[(int)info.Side];

            return BlockMeshData.VaryingHeight(texture, TintColor.None);
        }

        internal override bool CanPlace(int x, int y, int z, PhysicsEntity? entity)
        {
            return !Game.World.HasOpaqueTop(x, y, z) || Block.Dirt.CanPlace(x, y, z, entity);
        }

        protected override void DoPlace(int x, int y, int z, PhysicsEntity? entity)
        {
            if (Game.World.HasOpaqueTop(x, y, z))
            {
                Block.Dirt.Place(x, y, z, entity);
            }
            else
            {
                Game.World.SetBlock(this, 0, x, y, z);
            }
        }

        internal override void BlockUpdate(int x, int y, int z, uint data, BlockSide side)
        {
            if (side == BlockSide.Top && Game.World.HasOpaqueTop(x, y, z))
            {
                Game.World.SetBlock(Block.Dirt, 0, x, y, z);
            }
        }

        public int GetHeight(uint data)
        {
            return Height;
        }
    }
}