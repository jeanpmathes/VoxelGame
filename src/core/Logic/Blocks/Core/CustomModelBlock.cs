﻿// <copyright file="CustomModelBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A block that loads its complete model from a file. The block can only be placed on top of solid and full blocks.
    /// Data bit usage: <c>------</c>
    /// </summary>
    public class CustomModelBlock : Block, IFillable
    {
        private protected float[] vertices = null!;
        private protected int[] texIndices = null!;
        private protected uint[] indices = null!;

        private protected uint vertCount;

        private protected string model;

        public CustomModelBlock(string name, string namedId, string modelName, Physics.BoundingBox boundingBox, bool isSolid = true, bool isInteractable = false) :
            base(
                name,
                namedId,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: true,
                isSolid,
                recieveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                isInteractable,
                boundingBox,
                TargetBuffer.Complex)
        {
            this.model = modelName;
        }

        protected override void Setup()
        {
            BlockModel blockModel = BlockModel.Load(this.model);

            blockModel.ToData(out vertices, out texIndices, out indices);
            vertCount = (uint)(blockModel.VertexCount);
        }

        public override uint GetMesh(BlockSide side, uint data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated)
        {
            vertices = this.vertices;
            textureIndices = texIndices;
            indices = this.indices;

            tint = TintColor.None;
            isAnimated = false;

            return vertCount;
        }

        protected override bool Place(PhysicsEntity? entity, int x, int y, int z)
        {
            if ((Game.World.GetBlock(x, y - 1, z, out _) ?? Block.Air).IsSolidAndFull)
            {
                Game.World.SetBlock(this, 0, x, y, z);

                return true;
            }
            else
            {
                return false;
            }
        }

        internal override void BlockUpdate(int x, int y, int z, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom && !(Game.World.GetBlock(x, y - 1, z, out _) ?? Block.Air).IsSolidAndFull)
            {
                Destroy(x, y, z);
            }
        }
    }
}