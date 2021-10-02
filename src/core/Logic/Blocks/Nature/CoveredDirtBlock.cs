// <copyright file="GrassBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
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
                BlockFlags.Basic,
                normal)
        {
            this.hasNeutralTint = hasNeutralTint;
            SupportsFullGrowth = supportsFullGrowth;

            this.wet = wet;
        }

        public virtual bool AllowInflow(World world, Vector3i position, BlockSide side, Liquid liquid)
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

            if (info.Liquid.IsLiquid) mesh = mesh.SwapTextureIndex(wetTextureIndices[(int) info.Side]);

            return mesh;
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
            if (side == BlockSide.Top && world.HasOpaqueTop(position)) world.SetBlock(Dirt, data: 0, position);
        }
    }
}