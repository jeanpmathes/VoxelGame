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
            float upperBound;
            float lowerBound;

            if (Direction > 0)
            {
                upperBound = ((int)level + 1) * 0.125f;
                lowerBound = (sideHeight + 1) * 0.125f;
            }
            else
            {
                upperBound = (7 - sideHeight) * 0.125f;
                lowerBound = (7 - (int)level) * 0.125f;
            }

            switch (side)
            {
                case BlockSide.Front:
                case BlockSide.Back:
                case BlockSide.Left:
                case BlockSide.Right:

                    vertices = new float[32];
                    Array.Copy(BasicLiquid.vertices[(int)side], vertices, 32);
                    vertices[9] = vertices[12] = vertices[17] = vertices[20] = upperBound;
                    vertices[1] = vertices[25] = vertices[4] = vertices[28] = lowerBound;

                    break;

                case BlockSide.Bottom:

                    vertices = new float[32];
                    Array.Copy(BasicLiquid.vertices[4], vertices, 32);
                    if (Direction < 0) vertices[1] = vertices[9] = vertices[17] = vertices[25] = lowerBound;

                    break;

                case BlockSide.Top:

                    vertices = new float[32];
                    Array.Copy(BasicLiquid.vertices[5], vertices, 32);
                    if (Direction > 0) vertices[1] = vertices[9] = vertices[17] = vertices[25] = upperBound;

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
            if (FlowHorizontal(x, y, z, level)) return;

            if (level != LiquidLevel.One && FlowVertical(x, y, z, level)) return;

            Game.World.SetLiquid(this, level, true, x, y, z);
        }

        protected bool FlowHorizontal(int x, int y, int z, LiquidLevel level)
        {
            (Block? blockVertical, Liquid? liquidVertical) = Game.World.GetPosition(x, y - Direction, z, out _, out LiquidLevel levelVertical, out _);

            if (blockVertical != Block.Air) return false;

            if (liquidVertical == Liquid.None)
            {
                Game.World.SetLiquid(this, level, false, x, y - Direction, z);
                Game.World.SetLiquid(Liquid.None, LiquidLevel.Eight, true, x, y, z);

                return true;
            }
            else if (liquidVertical == this && levelVertical != LiquidLevel.Eight)
            {
                int volume = LiquidLevel.Eight - levelVertical - 1;

                if (volume >= (int)level)
                {
                    Game.World.SetLiquid(this, levelVertical + (int)level + 1, false, x, y - Direction, z);
                    Game.World.SetLiquid(Liquid.None, LiquidLevel.Eight, false, x, y, z);
                }
                else
                {
                    Game.World.SetLiquid(this, LiquidLevel.Eight, false, x, y - Direction, z);
                    Game.World.SetLiquid(this, level - volume - 1, false, x, y, z);
                }

                return true;
            }

            return false;
        }

        protected bool FlowVertical(int x, int y, int z, LiquidLevel level)
        {
            int horX = x, horZ = z;
            LiquidLevel levelHorizontal = LiquidLevel.Eight;

            if (CheckNeighbor(x, y, z - 1)) return true; // North.
            if (CheckNeighbor(x + 1, y, z)) return true; // East.
            if (CheckNeighbor(x, y, z + 1)) return true; // South.
            if (CheckNeighbor(x - 1, y, z)) return true; // West.

            if (horX != x || horZ != z)
            {
                Game.World.SetLiquid(this, levelHorizontal + 1, false, horX, y, horZ);

                bool remaining = level != LiquidLevel.One;
                Game.World.SetLiquid(remaining ? this : Liquid.None, remaining ? level - 1 : LiquidLevel.Eight, !remaining, x, y, z);

                return true;
            }

            return false;

            bool CheckNeighbor(int nx, int ny, int nz)
            {
                (Block? blockNeighbor, Liquid? liquidNeighbor) = Game.World.GetPosition(nx, ny, nz, out _, out LiquidLevel levelNeighbor, out _);

                if (blockNeighbor != Block.Air) return false;

                if (liquidNeighbor == Liquid.None)
                {
                    Game.World.SetLiquid(this, LiquidLevel.One, false, nx, ny, nz);

                    bool remaining = level != LiquidLevel.One;
                    Game.World.SetLiquid(remaining ? this : Liquid.None, remaining ? level - 1 : LiquidLevel.Eight, !remaining, x, y, z);

                    return true;
                }
                else if (liquidNeighbor == this && level > levelNeighbor && levelNeighbor < levelHorizontal)
                {
                    levelHorizontal = levelNeighbor;
                    horX = nx;
                    horZ = nz;
                }

                return false;
            }
        }
    }
}