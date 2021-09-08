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
    ///     A dirt-like block that is a bit lower then normal dirt.
    ///     Data bit usage: <c>------</c>.
    /// </summary>
    public class InsetDirtBlock : Block, IHeightVariable, IFillable, IPlantable, IPotentiallySolid, IAshCoverable
    {
        private const int Height = IHeightVariable.MaximumHeight - 1;

        private readonly TextureLayout dryLayout;
        private readonly TextureLayout wetLayout;

        private int[] dryTextureIndices = null!;
        private int[] wetTextureIndices = null!;

        internal InsetDirtBlock(string name, string namedId, TextureLayout dry, TextureLayout wet,
            bool supportsFullGrowth) :
            base(
                name,
                namedId,
                false,
                false,
                false,
                true,
                false,
                false,
                false,
                false,
                BoundingBox.Block,
                TargetBuffer.VaryingHeight)
        {
            dryLayout = dry;
            wetLayout = wet;

            SupportsFullGrowth = supportsFullGrowth;
        }

        public void CoverWithAsh(World world, int x, int y, int z)
        {
            world.SetBlock(GrassBurned, 0, x, y, z);
        }

        public int GetHeight(uint data)
        {
            return Height;
        }

        public bool SupportsFullGrowth { get; }

        public void BecomeSolid(World world, int x, int y, int z)
        {
            world.SetBlock(Dirt, 0, x, y, z);
        }

        protected override void Setup(ITextureIndexProvider indexProvider)
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
                ? wetTextureIndices[(int) info.Side]
                : dryTextureIndices[(int) info.Side];

            return BlockMeshData.VaryingHeight(texture, TintColor.None);
        }

        internal override bool CanPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            return !world.HasOpaqueTop(x, y, z) || Dirt.CanPlace(world, x, y, z, entity);
        }

        protected override void DoPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            if (world.HasOpaqueTop(x, y, z)) Dirt.Place(world, x, y, z, entity);
            else world.SetBlock(this, 0, x, y, z);
        }

        internal override void BlockUpdate(World world, int x, int y, int z, uint data, BlockSide side)
        {
            if (side == BlockSide.Top && world.HasOpaqueTop(x, y, z)) BecomeSolid(world, x, y, z);
        }
    }
}