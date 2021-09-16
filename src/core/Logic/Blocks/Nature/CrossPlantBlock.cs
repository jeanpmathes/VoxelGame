// <copyright file="CrossPlantBlock.cs" company="VoxelGame">
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
    ///     A plant made out of two intersecting planes. It uses neutral tint.
    ///     Data bit usage: <c>-----l</c>
    /// </summary>
    // l = lowered
    public class CrossPlantBlock : Block, IFlammable, IFillable
    {
        private readonly string texture;

        private int textureIndex;

        /// <summary>
        ///     Initializes a new instance of a cross plant.
        /// </summary>
        /// <param name="name">The name of this block.</param>
        /// <param name="namedId">The unique and unlocalized name of this block.</param>
        /// <param name="texture">The name of the texture of this block.</param>
        /// <param name="isReplaceable">Indicates whether this block will be replaceable.</param>
        /// <param name="boundingBox">The bounding box of this block.</param>
        internal CrossPlantBlock(string name, string namedId, string texture, bool isReplaceable,
            BoundingBox boundingBox) :
            base(
                name,
                namedId,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: false,
                isSolid: false,
                receiveCollisions: false,
                isTrigger: false,
                isReplaceable,
                isInteractable: false,
                boundingBox,
                TargetBuffer.CrossPlant)
        {
            this.texture = texture;
        }

        public void LiquidChange(World world, Vector3i position, Liquid liquid, LiquidLevel level)
        {
            if (liquid.Direction > 0 && level > LiquidLevel.Four) Destroy(world, position);
        }

        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            textureIndex = indexProvider.GetTextureIndex(texture);
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return BlockMeshData.CrossPlant(
                textureIndex,
                TintColor.Neutral,
                hasUpper: false,
                (info.Data & 0b1) == 1,
                isUpper: false);
        }

        internal override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            Block ground = world.GetBlock(position - Vector3i.UnitY, out _) ?? Air;

            return ground is IPlantable;
        }

        protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            bool isLowered = world.IsLowered(position);
            world.SetBlock(this, isLowered ? 1u : 0u, position);
        }

        internal override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom && (world.GetBlock(position - Vector3i.UnitY, out _) ?? Air) is not IPlantable)
                Destroy(world, position);
        }
    }
}