// <copyright file="CustomModelBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Entities;
using VoxelGame.Visuals;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// A block that loads its complete model from a file. The block can only be placed on top of solid and full blocks.
    /// </summary>
    public class CustomModelBlock : Block
    {
        private protected float[] vertices = null!;
        private protected int[] texIndices = null!;
        private protected uint[] indices = null!;

        private protected uint vertCount;

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
                isInteractable: false,
                boundingBox,
                TargetBuffer.Complex)
        {
#pragma warning disable CA2214 // Do not call overridable methods in constructors
            Setup(modelName);
#pragma warning restore CA2214 // Do not call overridable methods in constructors
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            vertices = this.vertices;
            textureIndices = texIndices;
            indices = this.indices;

            tint = TintColor.None;

            return vertCount;
        }

        protected virtual void Setup(string modelName)
        {
            BlockModel model = BlockModel.Load(modelName);

            model.ToData(out vertices, out texIndices, out indices);
            vertCount = (uint)(model.VertexCount);
        }

        protected override bool Place(int x, int y, int z, bool? replaceable, PhysicsEntity? entity)
        {
            if (replaceable == true && (Game.World.GetBlock(x, y - 1, z, out _) ?? Block.AIR).IsSolidAndFull)
            {
                Game.World.SetBlock(this, 0, x, y, z);

                return true;
            }
            else
            {
                return false;
            }
        }

        internal override void BlockUpdate(int x, int y, int z, byte data)
        {
            if (!(Game.World.GetBlock(x, y - 1, z, out _) ?? Block.AIR).IsSolidAndFull)
            {
                Destroy(x, y, z, null);
            }
        }
    }
}