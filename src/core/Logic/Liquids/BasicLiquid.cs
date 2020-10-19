// <copyright file="BasicLiquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Utilities;
using OpenToolkit.Mathematics;
using System.Diagnostics;

namespace VoxelGame.Core.Logic.Liquids
{
    public class BasicLiquid : Liquid
    {
        private protected bool neutralTint;

        private protected TextureLayout movingLayout;
        private protected TextureLayout staticLayout;

        private protected int[] movingTex = null!;
        private protected int[] staticTex = null!;

        public BasicLiquid(string name, string namedId, float density, int viscosity, bool neutralTint, TextureLayout movingLayout, TextureLayout staticLayout, RenderType renderType = RenderType.Transparent) :
            base(
                name,
                namedId,
                density,
                viscosity,
                checkContact: true,
                receiveContact: false,
                renderType)
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
            if (CheckVerticalWorldBounds(x, y, z)) return;

            Block block = Game.World.GetBlock(x, y, z, out _) ?? Block.Air;

            if (block is IFillable fillable && fillable.IsFillable(x, y, z, this))
            {
                ValidLocationFlow(x, y, z, level, fillable);
            }
            else
            {
                InvalidLocationFlow(x, y, z, level);
            }
        }

        protected void InvalidLocationFlow(int x, int y, int z, LiquidLevel level)
        {
            if ((FlowVertical(x, y, z, null, level, -Direction, out int remaining) && remaining == -1) ||
                    (FlowVertical(x, y, z, null, (LiquidLevel)remaining, Direction, out remaining) && remaining == -1)) return;

            SpreadOrDestroyLiquid(x, y, z, (LiquidLevel)remaining);
        }

        protected void ValidLocationFlow(int x, int y, int z, LiquidLevel level, IFillable current)
        {
            if (FlowVertical(x, y, z, current, level, Direction, out _)) return;

            if (level != LiquidLevel.One ? (FlowHorizontal(x, y, z, level, current) || FarFlowHorizontal(x, y, z, level, current)) : TryPuddleFlow(x, y, z, current)) return;

            Game.World.ModifyLiquid(true, x, y, z);
        }

        protected bool CheckVerticalWorldBounds(int x, int y, int z)
        {
            if ((y == 0 && Direction > 0) || (y == Section.SectionSize * Chunk.ChunkHeight - 1 && Direction < 0))
            {
                Game.World.SetLiquid(Liquid.None, LiquidLevel.Eight, true, x, y, z);

                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool FlowVertical(int x, int y, int z, IFillable? currentFillable, LiquidLevel level, int direction, out int remaining)
        {
            (Block? blockVertical, Liquid? liquidVertical) = Game.World.GetPosition(x, y - direction, z, out _, out LiquidLevel levelVertical, out bool isStatic);

            if (blockVertical is IFillable verticalFillable && verticalFillable.IsFillable(x, y - direction, z, this))
            {
                if (liquidVertical == Liquid.None)
                {
                    SetLiquid(this, level, false, verticalFillable, x, y - direction, z);
                    SetLiquid(Liquid.None, LiquidLevel.Eight, true, currentFillable, x, y, z);

                    ScheduleTick(x, y - direction, z);

                    remaining = -1;

                    return true;
                }
                else if (liquidVertical == this && levelVertical != LiquidLevel.Eight)
                {
                    int volume = LiquidLevel.Eight - levelVertical - 1;

                    if (volume >= (int)level)
                    {
                        SetLiquid(this, levelVertical + (int)level + 1, false, verticalFillable, x, y - direction, z);
                        SetLiquid(Liquid.None, LiquidLevel.Eight, true, currentFillable, x, y, z);

                        remaining = -1;
                    }
                    else
                    {
                        SetLiquid(this, LiquidLevel.Eight, false, verticalFillable, x, y - direction, z);
                        SetLiquid(this, level - volume - 1, false, currentFillable, x, y, z);

                        remaining = (int)(level - volume - 1);

                        ScheduleTick(x, y, z);
                    }

                    if (isStatic) ScheduleTick(x, y - direction, z);

                    return true;
                }

                remaining = (int)level;

                return false;
            }
            else
            {
                remaining = (int)level;

                return false;
            }
        }

        protected bool TryPuddleFlow(int x, int y, int z, IFillable currentFillable)
        {
            if (TryFlow(x, z - 1)) return true;
            if (TryFlow(x + 1, z)) return true;
            if (TryFlow(x, z + 1)) return true;
            if (TryFlow(x - 1, z)) return true;

            return false;

            bool TryFlow(int px, int pz)
            {
                (Block? block, Liquid? liquid) = Game.World.GetPosition(px, y, pz, out _, out _, out _);

                if (block is IFillable puddleFillable && puddleFillable.IsFillable(px, y, pz, this) && liquid == Liquid.None && CheckLowerPosition(px, pz))
                {
                    SetLiquid(this, LiquidLevel.One, false, puddleFillable, px, y, pz);
                    SetLiquid(Liquid.None, LiquidLevel.Eight, true, currentFillable, x, y, z);

                    ScheduleTick(px, y, pz);

                    return true;
                }
                else
                {
                    return false;
                }
            }

            bool CheckLowerPosition(int px, int pz)
            {
                (Block? lowerBlock, Liquid? lowerLiquid) = Game.World.GetPosition(px, y - Direction, pz, out _, out LiquidLevel level, out _);

                return lowerBlock is IFillable fillable && fillable.IsFillable(px, y - Direction, pz, this) && ((lowerLiquid == this && level != LiquidLevel.Eight) || lowerLiquid == Liquid.None);
            }
        }

        protected bool FlowHorizontal(int x, int y, int z, LiquidLevel level, IFillable currentFillable)
        {
            int horX = x, horZ = z;
            bool isHorStatic = false;
            LiquidLevel levelHorizontal = LiquidLevel.Eight;
            IFillable? horizontalFillable = null;

            int start = BlockUtilities.GetPositionDependentNumber(x, z, 4);
            for (int i = start; i < start + 4; i++)
            {
                switch ((Orientation)(i % 4))
                {
                    case Orientation.North:
                        if (CheckNeighbor(x, y, z - 1)) return true;
                        break;

                    case Orientation.East:
                        if (CheckNeighbor(x + 1, y, z)) return true;
                        break;

                    case Orientation.South:
                        if (CheckNeighbor(x, y, z + 1)) return true;
                        break;

                    case Orientation.West:
                        if (CheckNeighbor(x - 1, y, z)) return true;
                        break;
                }
            }

            if (horX != x || horZ != z)
            {
                if (levelHorizontal == level - 1
                    && (IsAtSurface(x, y, z) || !IsAtSurface(horX, y, horZ)) // To fix "bubbles" when a liquid is next to a liquid under a block.
                    && !HasNeighborWithLevel(level - 2, horX, y, horZ)) return false;

                SetLiquid(this, levelHorizontal + 1, false, horizontalFillable, horX, y, horZ);

                if (isHorStatic) ScheduleTick(horX, y, horZ);

                bool remaining = level != LiquidLevel.One;
                SetLiquid(remaining ? this : Liquid.None, remaining ? level - 1 : LiquidLevel.Eight, !remaining, currentFillable, x, y, z);

                if (remaining) ScheduleTick(x, y, z);

                return true;
            }

            return false;

            bool CheckNeighbor(int nx, int ny, int nz)
            {
                (Block? blockNeighbor, Liquid? liquidNeighbor) = Game.World.GetPosition(nx, ny, nz, out _, out LiquidLevel levelNeighbor, out bool isStatic);

                if (blockNeighbor is IFillable neighborFillable && neighborFillable.IsFillable(nx, ny, nz, this))
                {
                    if (liquidNeighbor == Liquid.None)
                    {
                        isStatic = true;

                        SetLiquid(this, LiquidLevel.One, false, neighborFillable, nx, ny, nz);

                        if (isStatic) ScheduleTick(nx, ny, nz);

                        bool remaining = level != LiquidLevel.One;
                        SetLiquid(remaining ? this : Liquid.None, remaining ? level - 1 : LiquidLevel.Eight, !remaining, currentFillable, x, y, z);

                        if (remaining) ScheduleTick(x, y, z);

                        return true;
                    }
                    else if (liquidNeighbor == this && level > levelNeighbor && levelNeighbor < levelHorizontal)
                    {
                        levelHorizontal = levelNeighbor;
                        horX = nx;
                        horZ = nz;
                        isHorStatic = isStatic;

                        horizontalFillable = neighborFillable;
                    }
                }

                return false;
            }
        }

        protected bool FarFlowHorizontal(int x, int y, int z, LiquidLevel level, IFillable currentFillable)
        {
            if (level < LiquidLevel.Three) return false;

            int start = BlockUtilities.GetPositionDependentNumber(x, z, 4);
            for (int i = start; i < start + 4; i++)
            {
                switch ((Orientation)(i % 4))
                {
                    case Orientation.North:
                        if (CheckDirection((0, -1))) return true;
                        break;

                    case Orientation.East:
                        if (CheckDirection((1, 0))) return true;
                        break;

                    case Orientation.South:
                        if (CheckDirection((0, 1))) return true;
                        break;

                    case Orientation.West:
                        if (CheckDirection((-1, 0))) return true;
                        break;
                }
            }

            return false;

            bool CheckDirection(Vector2i dir)
            {
                if (SearchLevel(x, y, z, dir, 4, level - 2, out Vector3i pos))
                {
                    (Block? block, Liquid? liquid) = Game.World.GetPosition(pos.X, pos.Y, pos.Z, out _, out LiquidLevel target, out bool isStatic);

                    if (block is IFillable targetFillable && targetFillable.IsFillable(pos.X, pos.Y, pos.Z, this) && liquid == this)
                    {
                        SetLiquid(this, target + 1, false, targetFillable, pos.X, pos.Y, pos.Z);
                        if (isStatic) ScheduleTick(pos.X, pos.Y, pos.Z);

                        SetLiquid(this, level - 1, false, currentFillable, x, y, z);
                        ScheduleTick(x, y, z);

                        return true;
                    }
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

                if (blockNeighbor is IFillable neighborFillable && neighborFillable.IsFillable(nx, ny, nz, this))
                {
                    if (liquidNeighbor == Liquid.None)
                    {
                        isStatic = true;

                        SetLiquid(this, (LiquidLevel)remaining, false, neighborFillable, nx, ny, nz);

                        remaining = -1;

                        if (isStatic) ScheduleTick(nx, ny, nz);
                    }
                    else if (liquidNeighbor == this)
                    {
                        int volume = LiquidLevel.Eight - levelNeighbor - 1;

                        if (volume >= remaining)
                        {
                            SetLiquid(this, levelNeighbor + remaining + 1, false, neighborFillable, nx, ny, nz);

                            remaining = -1;
                        }
                        else
                        {
                            SetLiquid(this, LiquidLevel.Eight, false, neighborFillable, nx, ny, nz);

                            remaining = remaining - volume - 1;
                        }

                        if (isStatic) ScheduleTick(nx, ny, nz);
                    }
                }

                return remaining;
            }
        }
    }
}