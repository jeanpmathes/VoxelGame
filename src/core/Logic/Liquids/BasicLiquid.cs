// <copyright file="BasicLiquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Liquids
{
    public class BasicLiquid : Liquid
    {
        private protected bool neutralTint;

        private protected TextureLayout movingLayout;
        private protected TextureLayout staticLayout;

        private protected int[] movingTex = null!;
        private protected int[] staticTex = null!;

        public BasicLiquid(string name, string namedId, float density, int viscosity, bool neutralTint, TextureLayout movingLayout, TextureLayout staticLayout) :
            base(
                name,
                namedId,
                density,
                viscosity,
                isRendered: true)
        {
            this.neutralTint = neutralTint;

            this.movingLayout = movingLayout;
            this.staticLayout = staticLayout;
        }

        protected override void Setup()
        {
            movingTex = movingLayout.GetTexIndexArray();
            staticTex = staticLayout.GetTexIndexArray();
        }

        public override void GetMesh(LiquidLevel level, BlockSide side, bool isStatic, out int textureIndex, out TintColor tint)
        {
            textureIndex = isStatic ? staticTex[(int)side] : movingTex[(int)side];
            tint = neutralTint ? TintColor.Neutral : TintColor.None;
        }

        protected override void ScheduledUpdate(int x, int y, int z, LiquidLevel level, bool isStatic)
        {
            Block block = Game.World.GetBlock(x, y, z, out _) ?? Block.Air;
            bool invalidLocation = (block != Block.Air);

            if (invalidLocation)
            {
                if (FlowVertical(x, y, z, level, -Direction, out int remaining) && remaining == -1) return;

                if (FlowVertical(x, y, z, (LiquidLevel)remaining, Direction, out remaining) && remaining == -1) return;

                SpreadOrDestroyLiquid(x, y, z, (LiquidLevel)remaining);
            }
            else
            {
                if (FlowVertical(x, y, z, level, Direction, out _)) return;

                if (level != LiquidLevel.One && FlowHorizontal(x, y, z, level)) return;

                Game.World.ModifyLiquid(true, x, y, z);
            }
        }

        protected bool FlowVertical(int x, int y, int z, LiquidLevel level, int direction, out int remaining)
        {
            (Block? blockVertical, Liquid? liquidVertical) = Game.World.GetPosition(x, y - direction, z, out _, out LiquidLevel levelVertical, out bool isStatic);

            if (blockVertical != Block.Air)
            {
                remaining = (int)level;

                return false;
            }

            if (liquidVertical == Liquid.None)
            {
                Game.World.SetLiquid(this, level, false, x, y - direction, z);
                Game.World.SetLiquid(Liquid.None, LiquidLevel.Eight, true, x, y, z);

                ScheduleTick(x, y - direction, z);

                remaining = -1;

                return true;
            }
            else if (liquidVertical == this && levelVertical != LiquidLevel.Eight)
            {
                int volume = LiquidLevel.Eight - levelVertical - 1;

                if (volume >= (int)level)
                {
                    Game.World.SetLiquid(this, levelVertical + (int)level + 1, false, x, y - direction, z);
                    Game.World.SetLiquid(Liquid.None, LiquidLevel.Eight, true, x, y, z);

                    remaining = -1;
                }
                else
                {
                    Game.World.SetLiquid(this, LiquidLevel.Eight, false, x, y - direction, z);
                    Game.World.SetLiquid(this, level - volume - 1, false, x, y, z);

                    remaining = (int)(level - volume - 1);

                    ScheduleTick(x, y, z);
                }

                if (isStatic) ScheduleTick(x, y - direction, z);

                return true;
            }

            remaining = (int)level;

            return false;
        }

        protected bool FlowHorizontal(int x, int y, int z, LiquidLevel level)
        {
            int horX = x, horZ = z;
            bool isHorStatic = false;
            LiquidLevel levelHorizontal = LiquidLevel.Eight;

            if (CheckNeighbor(x, y, z - 1)) return true; // North.
            if (CheckNeighbor(x + 1, y, z)) return true; // East.
            if (CheckNeighbor(x, y, z + 1)) return true; // South.
            if (CheckNeighbor(x - 1, y, z)) return true; // West.

            if (horX != x || horZ != z)
            {
                if (levelHorizontal == level - 1) return false;

                Game.World.SetLiquid(this, levelHorizontal + 1, false, horX, y, horZ);

                if (isHorStatic) ScheduleTick(horX, y, horZ);

                bool remaining = level != LiquidLevel.One;
                Game.World.SetLiquid(remaining ? this : Liquid.None, remaining ? level - 1 : LiquidLevel.Eight, !remaining, x, y, z);

                if (remaining) ScheduleTick(x, y, z);

                return true;
            }

            return false;

            bool CheckNeighbor(int nx, int ny, int nz)
            {
                (Block? blockNeighbor, Liquid? liquidNeighbor) = Game.World.GetPosition(nx, ny, nz, out _, out LiquidLevel levelNeighbor, out bool isStatic);

                if (blockNeighbor != Block.Air) return false;

                if (liquidNeighbor == Liquid.None)
                {
                    isStatic = true;

                    Game.World.SetLiquid(this, LiquidLevel.One, false, nx, ny, nz);

                    if (isStatic) ScheduleTick(nx, ny, nz);

                    bool remaining = level != LiquidLevel.One;
                    Game.World.SetLiquid(remaining ? this : Liquid.None, remaining ? level - 1 : LiquidLevel.Eight, !remaining, x, y, z);

                    if (remaining) ScheduleTick(x, y, z);

                    return true;
                }
                else if (liquidNeighbor == this && level > levelNeighbor && levelNeighbor < levelHorizontal)
                {
                    levelHorizontal = levelNeighbor;
                    horX = nx;
                    horZ = nz;
                    isHorStatic = isStatic;
                }

                return false;
            }
        }

        protected void SpreadOrDestroyLiquid(int x, int y, int z, LiquidLevel level)
        {
            int remaining = (int)level;

            SpreadLiquid();

            Game.World.SetLiquid(Liquid.None, LiquidLevel.Eight, true, x, y, z);

            void SpreadLiquid()
            {
                if (FillNeighbor(x, y, z - 1) == -1) return; // North.
                if (FillNeighbor(x + 1, y, z) == -1) return; // East.
                if (FillNeighbor(x, y, z + 1) == -1) return; // South.
                FillNeighbor(x - 1, y, z); // West.
            }

            int FillNeighbor(int nx, int ny, int nz)
            {
                (Block? blockNeighbor, Liquid? liquidNeighbor) = Game.World.GetPosition(nx, ny, nz, out _, out LiquidLevel levelNeighbor, out bool isStatic);

                if (blockNeighbor != Block.Air) return remaining;

                if (liquidNeighbor == Liquid.None)
                {
                    isStatic = true;

                    Game.World.SetLiquid(this, (LiquidLevel)remaining, false, nx, ny, nz);

                    remaining = -1;

                    if (isStatic) ScheduleTick(nx, ny, nz);
                }
                else if (liquidNeighbor == this)
                {
                    int volume = LiquidLevel.Eight - levelNeighbor - 1;

                    if (volume >= remaining)
                    {
                        Game.World.SetLiquid(this, levelNeighbor + remaining + 1, false, nx, ny, nz);

                        remaining = -1;
                    }
                    else
                    {
                        Game.World.SetLiquid(this, LiquidLevel.Eight, false, nx, ny, nz);

                        remaining = remaining - volume - 1;
                    }

                    if (isStatic) ScheduleTick(nx, ny, nz);
                }

                return remaining;
            }
        }
    }
}