// <copyright file="CustomModelBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using VoxelGame.Entities;
using VoxelGame.Rendering;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// A block that loads its complete model from a file. The block can only be placed on top of solid and full blocks.
    /// </summary>
    public class CustomModelBlock : Block
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        protected float[] vertices = null!;
        protected int[] texIndices = null!;
        protected uint[] indices = null!;

        protected uint vertCount;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public CustomModelBlock(string name, string modelName, bool isSolid, Physics.BoundingBox boundingBox) :
            base(
                name,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: true,
                isSolid,
                recieveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                boundingBox,
                TargetBuffer.Complex)
        {
#pragma warning disable CA2214 // Do not call overridable methods in constructors
            Setup(modelName);
#pragma warning restore CA2214 // Do not call overridable methods in constructors
        }

        protected virtual void Setup(string modelName)
        {
            BlockModel model = BlockModel.Load(modelName);

            model.ToData(out vertices, out texIndices, out indices);
            vertCount = (uint)(model.VertexCount);
        }

        public override bool Place(int x, int y, int z, PhysicsEntity? entity)
        {
            Block ground = Game.World.GetBlock(x, y - 1, z, out _) ?? Block.AIR;

            if (ground.IsFull && ground.IsSolid)
            {
                return base.Place(x, y, z, entity);
            }
            else
            {
                return false;
            }
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            vertices = this.vertices;
            textureIndices = texIndices;
            indices = this.indices;

            tint = TintColor.None;

            return vertCount;
        }

        public override void BlockUpdate(int x, int y, int z, byte data)
        {
            Block ground = Game.World.GetBlock(x, y - 1, z, out _) ?? Block.AIR;

            if (!ground.IsFull || !ground.IsSolid)
            {
                Destroy(x, y, z, null);
            }
        }
    }
}
