﻿// <copyright file="BasicLiquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Utilities;
using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Logic.Liquids
{
    public class BasicLiquid : Liquid, IOverlayTextureProvider
    {
        private protected bool neutralTint;

        private protected TextureLayout movingLayout;
        private protected TextureLayout staticLayout;

        private protected int[] movingTex = null!;
        private protected int[] staticTex = null!;

        public int TextureIdentifier => staticLayout.Front;

        public BasicLiquid(string name, string namedId, float density, int viscosity, bool neutralTint, TextureLayout movingLayout, TextureLayout staticLayout, RenderType renderType = RenderType.Opaque) :
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

            if (block is IFillable fillable)
            {
                ValidLocationFlow(x, y, z, level, fillable);
            }
            else
            {
                InvalidLocationFlow(x, y, z, level);
            }
        }

        private void InvalidLocationFlow(int x, int y, int z, LiquidLevel level)
        {
            if ((FlowVertical(x, y, z, null, level, -Direction, false, out int remaining) && remaining == -1) ||
                    (FlowVertical(x, y, z, null, (LiquidLevel)remaining, Direction, false, out remaining) && remaining == -1)) return;

            SpreadOrDestroyLiquid(x, y, z, (LiquidLevel)remaining);
        }

        private void ValidLocationFlow(int x, int y, int z, LiquidLevel level, IFillable current)
        {
            if (FlowVertical(x, y, z, current, level, Direction, true, out _)) return;

            if (level != LiquidLevel.One ? (FlowHorizontal(x, y, z, level, current) || FarFlowHorizontal(x, y, z, level, current)) : TryPuddleFlow(x, y, z, current)) return;

            Game.World.ModifyLiquid(true, x, y, z);
        }

        private bool CheckVerticalWorldBounds(int x, int y, int z)
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

        private bool FlowVertical(int x, int y, int z, IFillable? currentFillable, LiquidLevel level, int direction, bool handleContact, out int remaining)
        {
            (Block? blockVertical, Liquid? liquidVertical) = Game.World.GetPosition(x, y - direction, z, out _, out LiquidLevel levelVertical, out bool isStatic);

            if (blockVertical is IFillable verticalFillable
                && verticalFillable.AllowInflow(x, y - direction, z, direction > 0 ? BlockSide.Top : BlockSide.Bottom, this)
                && (currentFillable?.AllowOutflow(x, y, z, direction > 0 ? BlockSide.Bottom : BlockSide.Top) ?? true))
            {
                if (liquidVertical == Liquid.None)
                {
                    SetLiquid(this, level, false, verticalFillable, x, y - direction, z);
                    SetLiquid(Liquid.None, LiquidLevel.Eight, true, currentFillable, x, y, z);

                    ScheduleTick(x, y - direction, z);

                    remaining = -1;

                    return true;
                }

                if (liquidVertical == this)
                {
                    if (levelVertical == LiquidLevel.Eight)
                    {
                        remaining = (int)level;

                        return false;
                    }

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

                if (handleContact && liquidVertical != null)
                {
                    remaining = (int)level;

                    return LiquidContactManager.HandleContact(this, (x, y, z), level, liquidVertical, (x, y - Direction, z), levelVertical, isStatic);
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

        private bool TryPuddleFlow(int x, int y, int z, IFillable currentFillable)
        {
            bool liquidBelowIsNone = Game.World.GetLiquid(x, y - Direction, z, out _, out _) == Liquid.None;

            if (currentFillable.AllowOutflow(x, y, z, BlockSide.Back) && TryFlow(x, z - 1, BlockSide.Front)) return true;
            if (currentFillable.AllowOutflow(x, y, z, BlockSide.Right) && TryFlow(x + 1, z, BlockSide.Left)) return true;
            if (currentFillable.AllowOutflow(x, y, z, BlockSide.Front) && TryFlow(x, z + 1, BlockSide.Back)) return true;
            if (currentFillable.AllowOutflow(x, y, z, BlockSide.Left) && TryFlow(x - 1, z, BlockSide.Right)) return true;

            return false;

            bool TryFlow(int px, int pz, BlockSide side)
            {
                (Block? block, Liquid? liquid) = Game.World.GetPosition(px, y, pz, out _, out _, out _);

                if (block is IFillable puddleFillable
                    && puddleFillable.AllowInflow(px, y, pz, side, this)
                    && puddleFillable.AllowOutflow(px, y, pz, Direction > 0 ? BlockSide.Bottom : BlockSide.Top)
                    && liquid == Liquid.None && CheckLowerPosition(px, pz))
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

                return lowerBlock is IFillable fillable
                       && fillable.AllowInflow(px, y - Direction, pz, Direction > 0 ? BlockSide.Top : BlockSide.Bottom, this)
                       && ((lowerLiquid == this && level != LiquidLevel.Eight) || (liquidBelowIsNone && lowerLiquid != this));
            }
        }

        private bool FlowHorizontal(int x, int y, int z, LiquidLevel level, IFillable currentFillable)
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
                        if (CheckNeighbor(currentFillable.AllowOutflow(x, y, z, Orientation.North.ToBlockSide()),
                            x, y, z - 1, Orientation.North.Invert().ToBlockSide())) return true;
                        break;

                    case Orientation.East:
                        if (CheckNeighbor(currentFillable.AllowOutflow(x, y, z, Orientation.East.ToBlockSide()),
                            x + 1, y, z, Orientation.East.Invert().ToBlockSide())) return true;
                        break;

                    case Orientation.South:
                        if (CheckNeighbor(currentFillable.AllowOutflow(x, y, z, Orientation.South.ToBlockSide()),
                            x, y, z + 1, Orientation.South.Invert().ToBlockSide())) return true;
                        break;

                    case Orientation.West:
                        if (CheckNeighbor(currentFillable.AllowOutflow(x, y, z, Orientation.West.ToBlockSide()),
                            x - 1, y, z, Orientation.West.Invert().ToBlockSide())) return true;
                        break;
                }
            }

            if (horX != x || horZ != z)
            {
                SetLiquid(this, levelHorizontal + 1, false, horizontalFillable, horX, y, horZ);

                if (isHorStatic) ScheduleTick(horX, y, horZ);

                bool remaining = level != LiquidLevel.One;
                SetLiquid(remaining ? this : Liquid.None, remaining ? level - 1 : LiquidLevel.Eight, !remaining, currentFillable, x, y, z);

                if (remaining) ScheduleTick(x, y, z);

                return true;
            }

            return false;

            bool CheckNeighbor(bool outflowAllowed, int nx, int ny, int nz, BlockSide side)
            {
                (Block? blockNeighbor, Liquid? liquidNeighbor) = Game.World.GetPosition(nx, ny, nz, out _, out LiquidLevel levelNeighbor, out bool isStatic);

                if (blockNeighbor is IFillable neighborFillable && neighborFillable.AllowInflow(nx, ny, nz, side, this) && outflowAllowed)
                {
                    if (liquidNeighbor == Liquid.None)
                    {
                        isStatic = true;

                        (Block? belowNeighborBlock, Liquid? belowNeighborLiquid) = Game.World.GetPosition(nx, ny - Direction, nz, out _, out _, out _);

                        if (belowNeighborLiquid == Liquid.None
                            && belowNeighborBlock is IFillable belowFillable
                            && belowFillable.AllowInflow(nx, ny - Direction, nz, Direction > 0 ? BlockSide.Top : BlockSide.Bottom, this)
                            && neighborFillable.AllowOutflow(nx, ny, nz, Direction > 0 ? BlockSide.Bottom : BlockSide.Top))
                        {
                            SetLiquid(this, level, false, belowFillable, nx, ny - Direction, nz);

                            ScheduleTick(nx, ny - Direction, nz);

                            SetLiquid(Liquid.None, LiquidLevel.Eight, true, currentFillable, x, y, z);
                        }
                        else
                        {
                            SetLiquid(this, LiquidLevel.One, false, neighborFillable, nx, ny, nz);

                            if (isStatic) ScheduleTick(nx, ny, nz);

                            bool remaining = level != LiquidLevel.One;
                            SetLiquid(remaining ? this : Liquid.None, remaining ? level - 1 : LiquidLevel.Eight, !remaining, currentFillable, x, y, z);

                            if (remaining) ScheduleTick(x, y, z);
                        }

                        return true;
                    }
                    else if (liquidNeighbor != null && liquidNeighbor != this)
                    {
                        if (LiquidContactManager.HandleContact(this, (x, y, z), level, liquidNeighbor, (nx, ny, nz), levelNeighbor, isStatic)) return true;
                    }
                    else if (liquidNeighbor == this && level > levelNeighbor && levelNeighbor < levelHorizontal)
                    {
                        bool allowsFlow = levelNeighbor != level - 1 || (!IsAtSurface(x, y, z) && IsAtSurface(nx, ny, nz)) || HasNeighborWithLevel(level - 2, nx, ny, nz) || HasNeighborWithEmpty(nx, ny, nz);

                        if (allowsFlow)
                        {
                            levelHorizontal = levelNeighbor;
                            horX = nx;
                            horZ = nz;
                            isHorStatic = isStatic;

                            horizontalFillable = neighborFillable;
                        }
                    }
                }

                return false;
            }
        }

        private bool FarFlowHorizontal(int x, int y, int z, LiquidLevel level, IFillable currentFillable)
        {
            if (level < LiquidLevel.Three) return false;

            int start = BlockUtilities.GetPositionDependentNumber(x, z, 4);
            for (int i = start; i < start + 4; i++)
            {
                switch ((Orientation)(i % 4))
                {
                    case Orientation.North:
                        if (currentFillable.AllowOutflow(x, y, z, Orientation.North.ToBlockSide())
                            && CheckDirection((0, -1), Orientation.North.Invert().ToBlockSide())) return true;
                        break;

                    case Orientation.East:
                        if (currentFillable.AllowOutflow(x, y, z, Orientation.East.ToBlockSide())
                            && CheckDirection((1, 0), Orientation.East.Invert().ToBlockSide())) return true;
                        break;

                    case Orientation.South:
                        if (currentFillable.AllowOutflow(x, y, z, Orientation.South.ToBlockSide())
                            && CheckDirection((0, 1), Orientation.South.Invert().ToBlockSide())) return true;
                        break;

                    case Orientation.West:
                        if (currentFillable.AllowOutflow(x, y, z, Orientation.West.ToBlockSide())
                            && CheckDirection((-1, 0), Orientation.West.Invert().ToBlockSide())) return true;
                        break;
                }
            }

            return false;

            bool CheckDirection(Vector2i dir, BlockSide side)
            {
                if (SearchLevel(x, y, z, dir, 4, level - 2, out Vector3i pos))
                {
                    (Block? block, Liquid? liquid) = Game.World.GetPosition(pos.X, pos.Y, pos.Z, out _, out LiquidLevel target, out bool isStatic);

                    if (block is IFillable targetFillable && targetFillable.AllowInflow(pos.X, pos.Y, pos.Z, side, this) && liquid == this)
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

        private void SpreadOrDestroyLiquid(int x, int y, int z, LiquidLevel level)
        {
            int remaining = (int)level;

            SpreadLiquid();

            Game.World.SetLiquid(Liquid.None, LiquidLevel.Eight, true, x, y, z);

            void SpreadLiquid()
            {
                if (FillNeighbor(x, y, z - 1, BlockSide.Front) == -1) return; // North.
                if (FillNeighbor(x + 1, y, z, BlockSide.Left) == -1) return; // East.
                if (FillNeighbor(x, y, z + 1, BlockSide.Back) == -1) return; // South.
                FillNeighbor(x - 1, y, z, BlockSide.Right); // West.
            }

            int FillNeighbor(int nx, int ny, int nz, BlockSide side)
            {
                (Block? blockNeighbor, Liquid? liquidNeighbor) = Game.World.GetPosition(nx, ny, nz, out _, out LiquidLevel levelNeighbor, out bool isStatic);

                if (blockNeighbor is IFillable neighborFillable && neighborFillable.AllowInflow(nx, ny, nz, side, this))
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