// <copyright file="CustomModelBlock.cs" company="VoxelGame">
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
    ///     A block that loads its complete model from a file. The block can only be placed on top of solid and full blocks.
    ///     Data bit usage: <c>------</c>
    /// </summary>
    public class CustomModelBlock : Block, IFillable
    {
        private readonly string model;
        private uint[] indices = null!;
        private int[] texIndices = null!;

        private uint vertexCount;
        private float[] vertices = null!;

        internal CustomModelBlock(string name, string namedId, string modelName, BoundingBox boundingBox,
            bool isSolid = true, bool isInteractable = false) :
            base(
                name,
                namedId,
                false,
                false,
                true,
                isSolid,
                false,
                false,
                false,
                isInteractable,
                boundingBox,
                TargetBuffer.Complex)
        {
            model = modelName;
        }

        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            BlockModel blockModel = BlockModel.Load(model);

            blockModel.ToData(out vertices, out texIndices, out indices);
            vertexCount = (uint) blockModel.VertexCount;
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return BlockMeshData.Complex(vertexCount, vertices, texIndices, indices);
        }

        internal override bool CanPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            return world.HasSolidGround(x, y, z, true);
        }

        internal override void BlockUpdate(World world, int x, int y, int z, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom && !world.HasSolidGround(x, y, z)) Destroy(world, x, y, z);
        }
    }
}