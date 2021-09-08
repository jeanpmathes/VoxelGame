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
    ///     A block that changes into dirt when something is placed on top of it. This block can use a neutral tint if
    ///     specified in the constructor.
    ///     Data bit usage: <c>------</c>
    /// </summary>
    public class CoveredDirtBlock : BasicBlock, IFillable, IPlantable
    {
        private readonly bool hasNeutralTint;
        private readonly TextureLayout wet;

        private int[] wetTextureIndices = null!;

        protected CoveredDirtBlock(string name, string namedId, TextureLayout normal, TextureLayout wet,
            bool hasNeutralTint, bool supportsFullGrowth) :
            base(
                name,
                namedId,
                normal)
        {
            this.hasNeutralTint = hasNeutralTint;
            SupportsFullGrowth = supportsFullGrowth;

            this.wet = wet;
        }

        public virtual bool AllowInflow(World world, int x, int y, int z, BlockSide side, Liquid liquid)
        {
            return liquid.Viscosity < 100;
        }

        public bool SupportsFullGrowth { get; }

        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            base.Setup(indexProvider);

            wetTextureIndices = wet.GetTexIndexArray();
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            BlockMeshData mesh = base.GetMesh(info);

            mesh = mesh.Modified(hasNeutralTint ? TintColor.Neutral : TintColor.None);

            if (info.Liquid.Direction > 0) mesh = mesh.SwapTextureIndex(wetTextureIndices[(int) info.Side]);

            return mesh;
        }

        internal override bool CanPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            return !world.HasSolidTop(x, y, z) || Dirt.CanPlace(world, x, y, z, entity);
        }

        protected override void DoPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            if (world.HasSolidTop(x, y, z)) Dirt.Place(world, x, y, z, entity);
            else world.SetBlock(this, 0, x, y, z);
        }

        internal override void BlockUpdate(World world, int x, int y, int z, uint data, BlockSide side)
        {
            if (side == BlockSide.Top && world.HasOpaqueTop(x, y, z)) world.SetBlock(Dirt, 0, x, y, z);
        }
    }
}