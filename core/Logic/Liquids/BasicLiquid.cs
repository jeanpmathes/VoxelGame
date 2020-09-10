﻿// <copyright file="BasicLiquid.cs" company="VoxelGame">
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
            if (FlowHorizontal(x, y, z, level)) return;

            if (level != LiquidLevel.One && FlowVertical(x, y, z, level)) return;

            Game.World.SetLiquid(this, level, true, x, y, z);
        }

        protected bool FlowHorizontal(int x, int y, int z, LiquidLevel level)
        {
            (Block? blockVertical, Liquid? liquidVertical) = Game.World.GetPosition(x, y - Direction, z, out _, out LiquidLevel levelVertical, out bool isStatic);

            if (blockVertical != Block.Air) return false;

            if (liquidVertical == Liquid.None)
            {
                Game.World.SetLiquid(this, level, false, x, y - Direction, z);
                Game.World.SetLiquid(Liquid.None, LiquidLevel.Eight, true, x, y, z);

                ScheduleTick(x, y - Direction, z);

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

                    ScheduleTick(x, y, z);
                }

                if (isStatic) ScheduleTick(x, y - Direction, z);

                return true;
            }

            return false;
        }

        protected bool FlowVertical(int x, int y, int z, LiquidLevel level)
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
    }
}