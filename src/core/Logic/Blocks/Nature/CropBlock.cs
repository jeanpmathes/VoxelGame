// <copyright file="CropBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A block which grows on farmland and has multiple growth stages.
    /// Data bit usage: <c>---sss</c>
    /// </summary>
    // s = stage
    public class CropBlock : Block, IFlammable, IFillable
    {
        private float[] vertices = null!;
        private int[] stageTexIndices = null!;
        private uint[] indices = null!;

        private readonly string texture;
        private int second, third, fourth, fifth, sixth, final, dead;

        public CropBlock(string name, string namedId, string texture, int second, int third, int fourth, int fifth, int sixth, int final, int dead) :
            base(
                name,
                namedId,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: true,
                isSolid: false,
                receiveCollisions: false,
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
            vertices = new float[]
            {
                //   X   Y    Z   U    V   N    O   P
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

            int baseIndex = Game.BlockTextures.GetTextureIndex(texture);

            if (baseIndex == 0)
            {
                second = third = fourth = fifth = sixth = final = dead = 0;
            }

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

            indices = new uint[]
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
        }

        protected override BoundingBox GetBoundingBox(int x, int y, int z, uint data)
        {
            switch ((GrowthStage)(data & 0b00_0111))
            {
                case GrowthStage.Initial:
                case GrowthStage.Dead:
                    return BoundingBox.BlockAt(3, x, y, z);

                case GrowthStage.Second:
                    return BoundingBox.BlockAt(5, x, y, z);

                case GrowthStage.Third:
                    return BoundingBox.BlockAt(7, x, y, z);

                case GrowthStage.Fourth:
                    return BoundingBox.BlockAt(9, x, y, z);

                case GrowthStage.Fifth:
                    return BoundingBox.BlockAt(11, x, y, z);

                case GrowthStage.Sixth:
                    return BoundingBox.BlockAt(13, x, y, z);

                case GrowthStage.Final:
                    return BoundingBox.BlockAt(15, x, y, z);
            }

            return BoundingBox.BlockAt(x, y, z);
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            int[] textureIndices = new int[24];

            for (var i = 0; i < 24; i++)
            {
                textureIndices[i] = stageTexIndices[info.Data & 0b00_0111];
            }

            return new BlockMeshData(24, vertices, textureIndices, indices);
        }

        protected override bool Place(PhysicsEntity? entity, int x, int y, int z)
        {
            if (!(Game.World.GetBlock(x, y - 1, z, out _) is IPlantable))
            {
                return false;
            }

            Game.World.SetBlock(this, (uint)GrowthStage.Initial, x, y, z);

            return true;
        }

        internal override void BlockUpdate(int x, int y, int z, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom && !(Game.World.GetBlock(x, y - 1, z, out _) is IPlantable))
            {
                Destroy(x, y, z);
            }
        }

        internal override void RandomUpdate(int x, int y, int z, uint data)
        {
            var stage = (GrowthStage)(data & 0b00_0111);

            if (stage != GrowthStage.Final && stage != GrowthStage.Dead && Game.World.GetBlock(x, y - 1, z, out _) is IPlantable plantable)
            {
                if ((int)stage > 2)
                {
                    if (!plantable.SupportsFullGrowth) return;

                    if (!plantable.TryGrow(x, y - 1, z, Liquid.Water, LiquidLevel.One))
                    {
                        Game.World.SetBlock(this, (uint)GrowthStage.Dead, x, y, z);

                        return;
                    }
                }

                Game.World.SetBlock(this, (uint)(stage + 1), x, y, z);
            }
        }

        public void LiquidChange(int x, int y, int z, Liquid liquid, LiquidLevel level)
        {
            if (liquid.Direction > 0 && level > LiquidLevel.Three) Destroy(x, y, z);
        }

        private enum GrowthStage
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