// <copyright file="InsetDirtBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
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
            dryLayout = dry;
            wetLayout = wet;

            SupportsFullGrowth = supportsFullGrowth;
        }

        public void CoverWithAsh(World world, Vector3i position)
        {
            world.SetBlock(GrassBurned, data: 0, position);
        }

        public int GetHeight(uint data)
        {
            return Height;
        }

        public bool SupportsFullGrowth { get; }

        public void BecomeSolid(World world, Vector3i position)
        {
            world.SetBlock(Dirt, data: 0, position);
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

        internal override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            return !world.HasOpaqueTop(position) || Dirt.CanPlace(world, position, entity);
        }

        protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            if (world.HasOpaqueTop(position)) Dirt.Place(world, position, entity);
            else world.SetBlock(this, data: 0, position);
        }

        internal override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
        {
            if (side == BlockSide.Top && world.HasOpaqueTop(position)) BecomeSolid(world, position);
        }
    }
}