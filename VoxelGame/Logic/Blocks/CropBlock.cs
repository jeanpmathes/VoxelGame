// <copyright file="CropBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using VoxelGame.Entities;
using VoxelGame.Physics;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// A block which grows on farmland and has multiple growth stages.
    /// Data bit usage: <c>--sss</c>
    /// </summary>
    // s = stage
    public class CropBlock : Block
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        protected float[][] stageVertices;
        protected int[] stageTexIndices;

        protected uint[] indices =
        {
            0, 2, 1,
            0, 3, 2,
            0, 1, 2,
            0, 2, 3,

            4, 6, 5,
            4, 7, 6,
            4, 5, 6,
            4, 6, 7,

            8, 10, 9,
            8, 11, 10,
            8, 9, 10,
            8, 10, 11,

            12, 14, 13,
            12, 15, 14,
            12, 13, 14,
            12, 14, 15,

            16, 18, 17,
            16, 19, 18,
            16, 17, 18,
            16, 18, 19,

            20, 22, 21,
            20, 23, 22,
            20, 21, 22,
            20, 22, 23,
        };

#pragma warning restore CA1051 // Do not declare visible instance fields

        public CropBlock(string name, string texture, int second, int third, int fourth, int fifth, int sixth, int final, int dead) :
            base(
                name,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: true,
                isSolid: false,
                recieveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                BoundingBox.Block)
        {
#pragma warning disable CA2214 // Do not call overridable methods in constructors
            this.Setup(texture, second, third, fourth, fifth, sixth, final, dead);
#pragma warning restore CA2214 // Do not call overridable methods in constructors
        }

        protected virtual void Setup(string texture, int second, int third, int fourth, int fifth, int sixth, int final, int dead)
        {
            int baseIndex = Game.BlockTextureArray.GetTextureIndex(texture);

            stageTexIndices = new int[]
            {
                baseIndex,
                baseIndex + second,
                baseIndex + third,
                baseIndex + fourth,
                baseIndex + fifth,
                baseIndex + sixth,
                baseIndex + final,
                baseIndex + dead
            };

            stageVertices = new float[8][];

            for (int i = 0; i < 8; i++)
            {
                stageVertices[i] = new float[]
                {
                    0.25f, 0f, 0f, 0f, 0f,
                    0.25f, 1f, 0f, 0f, 1f,
                    0.25f, 1f, 1f, 1f, 1f,
                    0.25f, 0f, 1f, 1f, 0f,

                    0.5f, 0f, 0f, 0f, 0f,
                    0.5f, 1f, 0f, 0f, 1f,
                    0.5f, 1f, 1f, 1f, 1f,
                    0.5f, 0f, 1f, 1f, 0f,

                    0.75f, 0f, 0f, 0f, 0f,
                    0.75f, 1f, 0f, 0f, 1f,
                    0.75f, 1f, 1f, 1f, 1f,
                    0.75f, 0f, 1f, 1f, 0f,

                    0f, 0f, 0.25f, 0f, 0f,
                    0f, 1f, 0.25f, 0f, 1f,
                    1f, 1f, 0.25f, 1f, 1f,
                    1f, 0f, 0.25f, 1f, 0f,

                    0f, 0f, 0.5f, 0f, 0f,
                    0f, 1f, 0.5f, 0f, 1f,
                    1f, 1f, 0.5f, 1f, 1f,
                    1f, 0f, 0.5f, 1f, 0f,

                    0f, 0f, 0.75f, 0f, 0f,
                    0f, 1f, 0.75f, 0f, 1f,
                    1f, 1f, 0.75f, 1f, 1f,
                    1f, 0f, 0.75f, 1f, 0f
                };
            }
        }

        public override BoundingBox GetBoundingBox(int x, int y, int z)
        {
            Game.World.GetBlock(x, y, z, out byte data);

            switch ((GrowthStage)(data & 0b0_0111))
            {
                case GrowthStage.Initial:
                case GrowthStage.Dead:
                    return new BoundingBox(new Vector3(0.5f, 0.125f, 0.5f) + new Vector3(x, y, z), new Vector3 (0.5f, 0.125f, 0.5f));
                case GrowthStage.Second:
                    return new BoundingBox(new Vector3(0.5f, 0.1875f, 0.5f) + new Vector3(x, y, z), new Vector3(0.5f, 0.1875f, 0.5f));
                case GrowthStage.Third:
                    return new BoundingBox(new Vector3(0.5f, 0.25f, 0.5f) + new Vector3(x, y, z), new Vector3(0.5f, 0.25f, 0.5f));
                case GrowthStage.Forth:
                    return new BoundingBox(new Vector3(0.5f, 0.3125f, 0.5f) + new Vector3(x, y, z), new Vector3(0.5f, 0.3125f, 0.5f));
                case GrowthStage.Fifth:
                    return new BoundingBox(new Vector3(0.5f, 0.375f, 0.5f) + new Vector3(x, y, z), new Vector3(0.5f, 0.375f, 0.5f));
                case GrowthStage.Sixth:
                    return new BoundingBox(new Vector3(0.5f, 0.4375f, 0.5f) + new Vector3(x, y, z), new Vector3(0.5f, 0.4375f, 0.5f));
                case GrowthStage.Final:
                    return new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f) + new Vector3(x, y, z), new Vector3(0.5f, 0.5f, 0.5f));
            }

            return new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f) + new Vector3(x, y, z), new Vector3(0.5f, 0.5f, 0.5f));
        }

        public override bool Place(int x, int y, int z, PhysicsEntity entity)
        {
            if (Game.World.GetBlock(x, y, z, out _)?.IsReplaceable == false || Game.World.GetBlock(x, y - 1, z, out _) != Block.FARMLAND)
            {
                return false;
            }

            Game.World.SetBlock(this, (byte)(x % 8), x, y, z);

            return true;
        }

        public override void BlockUpdate(int x, int y, int z, byte data)
        {
            if (Game.World.GetBlock(x, y - 1, z, out _) != Block.FARMLAND)
            {
                Destroy(x, y, z, null);
            }
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices)
        {
            vertices = stageVertices[data & 0b0_0111];
            textureIndices = new int[24];

            for (int i = 0; i < 24; i++)
            {
                textureIndices[i] = stageTexIndices[data & 0b0_0111];
            }

            indices = this.indices;

            return 24;
        }

        public override void OnCollision(PhysicsEntity entity, int x, int y, int z)
        {
        }

        protected enum GrowthStage
        {
            Initial,
            Second,
            Third,
            Forth,
            Fifth,
            Sixth,
            Final,
            Dead
        }
    }
}
