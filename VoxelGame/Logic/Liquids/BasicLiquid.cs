// <copyright file="BasicLiquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using VoxelGame.Visuals;

namespace VoxelGame.Logic.Liquids
{
    public class BasicLiquid : Liquid
    {
        protected readonly static float[][] vertices = BlockModel.CubeVertices();

        private protected TextureLayout movingLayout;
        private protected TextureLayout staticLayout;

        private protected int[][] movingTex = null!;
        private protected int[][] staticTex = null!;

        private protected uint[] indices = null!;

        public BasicLiquid(string name, string namedId, float density, TextureLayout movingLayout, TextureLayout staticLayout) :
            base(
                name,
                namedId,
                density,
                isRendered: true)
        {
            this.movingLayout = movingLayout;
            this.staticLayout = staticLayout;
        }

        protected override void Setup()
        {
            indices = new uint[]
            {
                0, 2, 1,
                0, 3, 2,
                0, 1, 2,
                0, 2, 3
            };

            movingTex = movingLayout.GetTexIndexArrays();
            staticTex = staticLayout.GetTexIndexArrays();
        }

        public override uint GetMesh(LiquidLevel level, BlockSide side, int sideHeight, bool isStatic, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            float start = (sideHeight + 1) * 0.125f;
            float height = ((int)level + 1) * 0.125f;

            switch (side)
            {
                case BlockSide.Front:
                case BlockSide.Back:
                case BlockSide.Left:
                case BlockSide.Right:

                    vertices = new float[32];
                    Array.Copy(BasicLiquid.vertices[(int)side], vertices, 32);
                    vertices[1] = vertices[25] = vertices[4] = vertices[28] = start;
                    vertices[9] = vertices[12] = vertices[17] = vertices[20] = height;

                    break;

                case BlockSide.Bottom:

                    vertices = BasicLiquid.vertices[4];

                    break;

                case BlockSide.Top:

                    vertices = new float[32];
                    Array.Copy(BasicLiquid.vertices[5], vertices, 32);
                    vertices[1] = vertices[9] = vertices[17] = vertices[25] = height;

                    break;

                default:
                    throw new ArgumentException("Only the six sides are valid arguments.", nameof(side));
            }

            textureIndices = isStatic ? staticTex[(int)side] : movingTex[(int)side];

            indices = this.indices;
            tint = TintColor.None;

            return 4;
        }

        internal override void LiquidUpdate(int x, int y, int z, LiquidLevel level, bool isStatic)
        {
            (Block? blockVertical, Liquid? liquidVertical) = Game.World.GetPosition(x, y - Direction, z, out _, out LiquidLevel levelVertical, out _);

            if (blockVertical != Block.Air)
            {
                Game.World.SetLiquid(this, level, true, x, y, z);
                return;
            }

            if (liquidVertical == Liquid.None)
            {
                Game.World.SetLiquid(this, LiquidLevel.One, false, x, y - Direction, z);

                bool remaining = level != LiquidLevel.One;
                Game.World.SetLiquid(remaining ? this : Liquid.None, remaining ? level - Direction : LiquidLevel.Eight, !remaining, x, y, z);
            }
            else if (liquidVertical == this && levelVertical != LiquidLevel.Eight)
            {
                Game.World.SetLiquid(this, levelVertical + 1, false, x, y - Direction, z);

                bool remaining = level != LiquidLevel.One;
                Game.World.SetLiquid(remaining ? this : Liquid.None, remaining ? level - Direction : LiquidLevel.Eight, !remaining, x, y, z);
            }
            else
            {
                Game.World.SetLiquid(this, level, true, x, y, z);
            }
        }
    }
}