// <copyright file="CustomModelBlock.cs" company="VoxelGame">
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
    ///     A block that loads its complete model from a file. The block can only be placed on top of solid and full blocks.
    ///     Data bit usage: <c>------</c>
    /// </summary>
    public class CustomModelBlock : Block, IFillable
    {
        private readonly BlockMesh mesh;

        /// <summary>
        ///     Create a new <see cref="CustomModelBlock" />.
        /// </summary>
        /// <param name="name">The name of the block.</param>
        /// <param name="namedId">The named ID of the block.</param>
        /// <param name="flags">The block flags.</param>
        /// <param name="modelName">The name of the model to use for this block.</param>
        /// <param name="boundingBox">The bounding box of the block.</param>
        internal CustomModelBlock(string name, string namedId, BlockFlags flags, string modelName,
            BoundingBox boundingBox) :
            base(
                name,
                namedId,
                flags with { IsFull = false, IsOpaque = false },
                boundingBox,
                TargetBuffer.Complex)
        {
            mesh = BlockModel.Load(modelName).GetMesh();
        }

        /// <inheritdoc />
        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return mesh.GetComplexMeshData();
        }

        internal override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            return world.HasSolidGround(position, solidify: true);
        }

        internal override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom && !world.HasSolidGround(position)) Destroy(world, position);
        }
    }
}