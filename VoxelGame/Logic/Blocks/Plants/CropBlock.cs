// <copyright file="CropBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using VoxelGame.Entities;
using VoxelGame.Logic.Interfaces;
using VoxelGame.Physics;
using VoxelGame.Visuals;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// A block which grows on farmland and has multiple growth stages.
    /// Data bit usage: <c>--sss</c>
    /// </summary>
    // s = stage
    public class CropBlock : Block
    {
        private protected int[] stageTexIndices = null!;

        private protected float[] vertices = new float[]
        {
            //X----Y---Z---U---V---N---O---P
            0.25f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
            0.25f, 1f, 0f, 0f, 1f, 0f, 0f, 0f,
            0.25f, 1f, 1f, 1f, 1f, 0f, 0f, 0f,
            0.25f, 0f, 1f, 1f, 0f, 0f, 0f, 0f,

            0.5f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
            0.5f, 1f, 0f, 0f, 1f, 0f, 0f, 0f,
            0.5f, 1f, 1f, 1f, 1f, 0f, 0f, 0f,
            0.5f, 0f, 1f, 1f, 0f, 0f, 0f, 0f,

            0.75f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
            0.75f, 1f, 0f, 0f, 1f, 0f, 0f, 0f,
            0.75f, 1f, 1f, 1f, 1f, 0f, 0f, 0f,
            0.75f, 0f, 1f, 1f, 0f, 0f, 0f, 0f,

            0f, 0f, 0.25f, 0f, 0f, 0f, 0f, 0f,
            0f, 1f, 0.25f, 0f, 1f, 0f, 0f, 0f,
            1f, 1f, 0.25f, 1f, 1f, 0f, 0f, 0f,
            1f, 0f, 0.25f, 1f, 0f, 0f, 0f, 0f,

            0f, 0f, 0.5f, 0f, 0f, 0f, 0f, 0f,
            0f, 1f, 0.5f, 0f, 1f, 0f, 0f, 0f,
            1f, 1f, 0.5f, 1f, 1f, 0f, 0f, 0f,
            1f, 0f, 0.5f, 1f, 0f, 0f, 0f, 0f,

            0f, 0f, 0.75f, 0f, 0f, 0f, 0f, 0f,
            0f, 1f, 0.75f, 0f, 1f, 0f, 0f, 0f,
            1f, 1f, 0.75f, 1f, 1f, 0f, 0f, 0f,
            1f, 0f, 0.75f, 1f, 0f, 0f, 0f, 0f
        };

        private protected uint[] indices =
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

        private protected string texture;
        private protected int second, third, fourth, fifth, sixth, final, dead;

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
                isInteractable: false,
                BoundingBox.Block,
                TargetBuffer.Complex)
        {
            this.texture = texture;
            this.second = second;
            this.third = third;
            this.fourth = fourth;
            this.fifth = fifth;
            this.sixth = sixth;
            this.final = final;
            this.dead = dead;
        }

        protected override void Setup()
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
        }

        protected override BoundingBox GetBoundingBox(int x, int y, int z, byte data)
        {
            switch ((GrowthStage)(data & 0b0_0111))
            {
                case GrowthStage.Initial:
                case GrowthStage.Dead:
                    return new BoundingBox(new Vector3(0.5f, 0.125f, 0.5f) + new Vector3(x, y, z), new Vector3(0.5f, 0.125f, 0.5f));

                case GrowthStage.Second:
                    return new BoundingBox(new Vector3(0.5f, 0.1875f, 0.5f) + new Vector3(x, y, z), new Vector3(0.5f, 0.1875f, 0.5f));

                case GrowthStage.Third:
                    return new BoundingBox(new Vector3(0.5f, 0.25f, 0.5f) + new Vector3(x, y, z), new Vector3(0.5f, 0.25f, 0.5f));

                case GrowthStage.Fourth:
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

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            vertices = this.vertices;
            textureIndices = new int[24];

            for (int i = 0; i < 24; i++)
            {
                textureIndices[i] = stageTexIndices[data & 0b0_0111];
            }

            indices = this.indices;
            tint = TintColor.None;

            return 24;
        }

        protected override bool Place(int x, int y, int z, bool? replaceable, PhysicsEntity? entity)
        {
            if (replaceable != true || !(Game.World.GetBlock(x, y - 1, z, out _) is IPlantable))
            {
                return false;
            }

            Game.World.SetBlock(this, (byte)GrowthStage.Initial, x, y, z);

            return true;
        }

        internal override void BlockUpdate(int x, int y, int z, byte data, BlockSide side)
        {
            if (side == BlockSide.Bottom && !(Game.World.GetBlock(x, y - 1, z, out _) is IPlantable))
            {
                Destroy(x, y, z, null);
            }
        }

        internal override void RandomUpdate(int x, int y, int z, byte data)
        {
            GrowthStage stage = (GrowthStage)(data & 0b0_0111);

            if ((int)stage > 2 && Game.World.GetBlock(x, y - 1, z, out _) != Block.FARMLAND)
            {
                return;
            }

            if (stage != GrowthStage.Final && stage != GrowthStage.Dead)
            {
                Game.World.SetBlock(this, (byte)(stage + 1), x, y, z);
            }
        }

        protected enum GrowthStage
        {
            Initial,
            Second,
            Third,
            Fourth,
            Fifth,
            Sixth,
            Final,
            Dead
        }
    }
}