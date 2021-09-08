// <copyright file="BasicLiquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Liquids
{
    public class BasicLiquid : Liquid, IOverlayTextureProvider
    {
        private readonly TextureLayout movingLayout;
        private readonly bool neutralTint;
        private readonly TextureLayout staticLayout;

        private int[] movingTex = null!;
        private int[] staticTex = null!;

        public BasicLiquid(string name, string namedId, float density, int viscosity, bool neutralTint,
            TextureLayout movingLayout, TextureLayout staticLayout, RenderType renderType = RenderType.Opaque) :
            base(
                name,
                namedId,
                density,
                viscosity,
                true,
                false,
                renderType)
        {
            this.neutralTint = neutralTint;

            this.movingLayout = movingLayout;
            this.staticLayout = staticLayout;
        }

        public int TextureIdentifier => staticLayout.Front;

        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            movingTex = movingLayout.GetTexIndexArray();
            staticTex = staticLayout.GetTexIndexArray();
        }

        public override LiquidMeshData GetMesh(LiquidMeshInfo info)
        {
            return LiquidMeshData.Basic(
                info.IsStatic ? staticTex[(int) info.Side] : movingTex[(int) info.Side],
                neutralTint ? TintColor.Neutral : TintColor.None);
        }

        protected override void ScheduledUpdate(World world, int x, int y, int z, LiquidLevel level, bool isStatic)
        {
            if (CheckVerticalWorldBounds(world, x, y, z)) return;

            Block block = world.GetBlock(x, y, z, out _) ?? Block.Air;

            if (block is IFillable fillable) ValidLocationFlow(world, x, y, z, level, fillable);
            else InvalidLocationFlow(world, x, y, z, level);
        }

        private void InvalidLocationFlow(World world, int x, int y, int z, LiquidLevel level)
        {
            if (FlowVertical(world, x, y, z, null, level, -Direction, false, out int remaining) && remaining == -1 ||
                FlowVertical(world, x, y, z, null, (LiquidLevel) remaining, Direction, false, out remaining) &&
                remaining == -1) return;

            SpreadOrDestroyLiquid(world, x, y, z, (LiquidLevel) remaining);
        }

        private void ValidLocationFlow(World world, int x, int y, int z, LiquidLevel level, IFillable current)
        {
            if (FlowVertical(world, x, y, z, current, level, Direction, true, out _)) return;

            if (level != LiquidLevel.One
                ? FlowHorizontal(world, x, y, z, level, current) || FarFlowHorizontal(world, x, y, z, level, current)
                : TryPuddleFlow(world, x, y, z, current)) return;

            world.ModifyLiquid(true, x, y, z);
        }

        private bool CheckVerticalWorldBounds(World world, int x, int y, int z)
        {
            if (y == 0 && Direction > 0 ||
                y == Section.SectionSize * Chunk.VerticalSectionCount - 1 && Direction < 0)
            {
                world.SetDefaultLiquid(x, y, z);

                return true;
            }

            return false;
        }

        private bool FlowVertical(World world, int x, int y, int z, IFillable? currentFillable, LiquidLevel level,
            int direction, bool handleContact, out int remaining)
        {
            (Block? blockVertical, Liquid? liquidVertical) = world.GetPosition(
                x,
                y - direction,
                z,
                out _,
                out LiquidLevel levelVertical,
                out bool isStatic);

            if (blockVertical is IFillable verticalFillable
                && verticalFillable.AllowInflow(
                    world,
                    x,
                    y - direction,
                    z,
                    direction > 0 ? BlockSide.Top : BlockSide.Bottom,
                    this)
                && (currentFillable?.AllowOutflow(world, x, y, z, direction > 0 ? BlockSide.Bottom : BlockSide.Top) ??
                    true))
            {
                if (liquidVertical == None)
                {
                    SetLiquid(world, this, level, false, verticalFillable, x, y - direction, z);
                    SetLiquid(world, None, LiquidLevel.Eight, true, currentFillable, x, y, z);

                    ScheduleTick(world, x, y - direction, z);

                    remaining = -1;

                    return true;
                }

                if (liquidVertical == this)
                {
                    if (levelVertical == LiquidLevel.Eight)
                    {
                        remaining = (int) level;

                        return false;
                    }

                    int volume = LiquidLevel.Eight - levelVertical - 1;

                    if (volume >= (int) level)
                    {
                        SetLiquid(
                            world,
                            this,
                            levelVertical + (int) level + 1,
                            false,
                            verticalFillable,
                            x,
                            y - direction,
                            z);

                        SetLiquid(world, None, LiquidLevel.Eight, true, currentFillable, x, y, z);

                        remaining = -1;
                    }
                    else
                    {
                        SetLiquid(world, this, LiquidLevel.Eight, false, verticalFillable, x, y - direction, z);
                        SetLiquid(world, this, level - volume - 1, false, currentFillable, x, y, z);

                        remaining = (int) (level - volume - 1);

                        ScheduleTick(world, x, y, z);
                    }

                    if (isStatic) ScheduleTick(world, x, y - direction, z);

                    return true;
                }

                if (handleContact && liquidVertical != null)
                {
                    remaining = (int) level;

                    return ContactManager.HandleContact(
                        world,
                        this,
                        (x, y, z),
                        level,
                        liquidVertical,
                        (x, y - Direction, z),
                        levelVertical,
                        isStatic);
                }
            }

            remaining = (int) level;

            return false;
        }

        private bool TryPuddleFlow(World world, int x, int y, int z, IFillable currentFillable)
        {
            bool liquidBelowIsNone = world.GetLiquid(x, y - Direction, z, out _, out _) == None;

            if (currentFillable.AllowOutflow(world, x, y, z, BlockSide.Back) &&
                TryFlow(x, z - 1, BlockSide.Front)) return true;

            if (currentFillable.AllowOutflow(world, x, y, z, BlockSide.Right) &&
                TryFlow(x + 1, z, BlockSide.Left)) return true;

            if (currentFillable.AllowOutflow(world, x, y, z, BlockSide.Front) &&
                TryFlow(x, z + 1, BlockSide.Back)) return true;

            if (currentFillable.AllowOutflow(world, x, y, z, BlockSide.Left) &&
                TryFlow(x - 1, z, BlockSide.Right)) return true;

            return false;

            bool TryFlow(int px, int pz, BlockSide side)
            {
                (Block? block, Liquid? liquid) = world.GetPosition(px, y, pz, out _, out _, out _);

                if (block is IFillable puddleFillable
                    && puddleFillable.AllowInflow(world, px, y, pz, side, this)
                    && puddleFillable.AllowOutflow(world, px, y, pz, Direction > 0 ? BlockSide.Bottom : BlockSide.Top)
                    && liquid == None && CheckLowerPosition(px, pz))
                {
                    SetLiquid(world, this, LiquidLevel.One, false, puddleFillable, px, y, pz);
                    SetLiquid(world, None, LiquidLevel.Eight, true, currentFillable, x, y, z);

                    ScheduleTick(world, px, y, pz);

                    return true;
                }

                return false;
            }

            bool CheckLowerPosition(int px, int pz)
            {
                (Block? lowerBlock, Liquid? lowerLiquid) = world.GetPosition(
                    px,
                    y - Direction,
                    pz,
                    out _,
                    out LiquidLevel level,
                    out _);

                return lowerBlock is IFillable fillable
                       && fillable.AllowInflow(
                           world,
                           px,
                           y - Direction,
                           pz,
                           Direction > 0 ? BlockSide.Top : BlockSide.Bottom,
                           this)
                       && (lowerLiquid == this && level != LiquidLevel.Eight ||
                           liquidBelowIsNone && lowerLiquid != this);
            }
        }

        private bool FlowHorizontal(World world, int x, int y, int z, LiquidLevel level, IFillable currentFillable)
        {
            int horX = x, horZ = z;
            var isHorStatic = false;
            var levelHorizontal = LiquidLevel.Eight;
            IFillable? horizontalFillable = null;

            int start = BlockUtilities.GetPositionDependentNumber(x, z, 4);

            for (int i = start; i < start + 4; i++)
                switch ((Orientation) (i % 4))
                {
                    case Orientation.North:
                        if (CheckNeighbor(
                            currentFillable.AllowOutflow(world, x, y, z, Orientation.North.ToBlockSide()),
                            x,
                            y,
                            z - 1,
                            Orientation.North.Invert().ToBlockSide())) return true;

                        break;

                    case Orientation.East:
                        if (CheckNeighbor(
                            currentFillable.AllowOutflow(world, x, y, z, Orientation.East.ToBlockSide()),
                            x + 1,
                            y,
                            z,
                            Orientation.East.Invert().ToBlockSide())) return true;

                        break;

                    case Orientation.South:
                        if (CheckNeighbor(
                            currentFillable.AllowOutflow(world, x, y, z, Orientation.South.ToBlockSide()),
                            x,
                            y,
                            z + 1,
                            Orientation.South.Invert().ToBlockSide())) return true;

                        break;

                    case Orientation.West:
                        if (CheckNeighbor(
                            currentFillable.AllowOutflow(world, x, y, z, Orientation.West.ToBlockSide()),
                            x - 1,
                            y,
                            z,
                            Orientation.West.Invert().ToBlockSide())) return true;

                        break;

                    default:
                        throw new NotSupportedException();
                }

            if (horX == x && horZ == z) return false;

            SetLiquid(world, this, levelHorizontal + 1, false, horizontalFillable, horX, y, horZ);

            if (isHorStatic) ScheduleTick(world, horX, y, horZ);

            bool hasRemaining = level != LiquidLevel.One;

            SetLiquid(
                world,
                hasRemaining ? this : None,
                hasRemaining ? level - 1 : LiquidLevel.Eight,
                !hasRemaining,
                currentFillable,
                x,
                y,
                z);

            if (hasRemaining) ScheduleTick(world, x, y, z);

            return true;

            bool CheckNeighbor(bool outflowAllowed, int nx, int ny, int nz, BlockSide side)
            {
                (Block? blockNeighbor, Liquid? liquidNeighbor) = world.GetPosition(
                    nx,
                    ny,
                    nz,
                    out _,
                    out LiquidLevel levelNeighbor,
                    out bool isStatic);

                if (!(blockNeighbor is IFillable neighborFillable) ||
                    !neighborFillable.AllowInflow(world, nx, ny, nz, side, this) || !outflowAllowed) return false;

                if (liquidNeighbor == None)
                {
                    isStatic = true;

                    (Block? belowNeighborBlock, Liquid? belowNeighborLiquid) = world.GetPosition(
                        nx,
                        ny - Direction,
                        nz,
                        out _,
                        out _,
                        out _);

                    if (belowNeighborLiquid == None
                        && belowNeighborBlock is IFillable belowFillable
                        && belowFillable.AllowInflow(
                            world,
                            nx,
                            ny - Direction,
                            nz,
                            Direction > 0 ? BlockSide.Top : BlockSide.Bottom,
                            this)
                        && neighborFillable.AllowOutflow(
                            world,
                            nx,
                            ny,
                            nz,
                            Direction > 0 ? BlockSide.Bottom : BlockSide.Top))
                    {
                        SetLiquid(world, this, level, false, belowFillable, nx, ny - Direction, nz);

                        ScheduleTick(world, nx, ny - Direction, nz);

                        SetLiquid(world, None, LiquidLevel.Eight, true, currentFillable, x, y, z);
                    }
                    else
                    {
                        SetLiquid(world, this, LiquidLevel.One, false, neighborFillable, nx, ny, nz);

                        if (isStatic) ScheduleTick(world, nx, ny, nz);

                        bool remaining = level != LiquidLevel.One;

                        SetLiquid(
                            world,
                            remaining ? this : None,
                            remaining ? level - 1 : LiquidLevel.Eight,
                            !remaining,
                            currentFillable,
                            x,
                            y,
                            z);

                        if (remaining) ScheduleTick(world, x, y, z);
                    }

                    return true;
                }

                if (liquidNeighbor != null && liquidNeighbor != this)
                {
                    if (ContactManager.HandleContact(
                        world,
                        this,
                        (x, y, z),
                        level,
                        liquidNeighbor,
                        (nx, ny, nz),
                        levelNeighbor,
                        isStatic)) return true;
                }
                else if (liquidNeighbor == this && level > levelNeighbor && levelNeighbor < levelHorizontal)
                {
                    bool allowsFlow = levelNeighbor != level - 1
                                      || level == LiquidLevel.Eight && !IsAtSurface(world, x, y, z) &&
                                      IsAtSurface(world, nx, ny, nz)
                                      || HasNeighborWithLevel(world, level - 2, nx, ny, nz)
                                      || HasNeighborWithEmpty(world, nx, ny, nz);

                    if (!allowsFlow) return false;

                    levelHorizontal = levelNeighbor;
                    horX = nx;
                    horZ = nz;
                    isHorStatic = isStatic;

                    horizontalFillable = neighborFillable;
                }

                return false;
            }
        }

        private bool FarFlowHorizontal(World world, int x, int y, int z, LiquidLevel level, IFillable currentFillable)
        {
            if (level < LiquidLevel.Three) return false;

            int start = BlockUtilities.GetPositionDependentNumber(x, z, 4);

            for (int i = start; i < start + 4; i++)
                switch ((Orientation) (i % 4))
                {
                    case Orientation.North:
                        if (currentFillable.AllowOutflow(world, x, y, z, Orientation.North.ToBlockSide())
                            && CheckDirection((0, -1), Orientation.North.Invert().ToBlockSide())) return true;

                        break;

                    case Orientation.East:
                        if (currentFillable.AllowOutflow(world, x, y, z, Orientation.East.ToBlockSide())
                            && CheckDirection((1, 0), Orientation.East.Invert().ToBlockSide())) return true;

                        break;

                    case Orientation.South:
                        if (currentFillable.AllowOutflow(world, x, y, z, Orientation.South.ToBlockSide())
                            && CheckDirection((0, 1), Orientation.South.Invert().ToBlockSide())) return true;

                        break;

                    case Orientation.West:
                        if (currentFillable.AllowOutflow(world, x, y, z, Orientation.West.ToBlockSide())
                            && CheckDirection((-1, 0), Orientation.West.Invert().ToBlockSide())) return true;

                        break;
                }

            return false;

            bool CheckDirection(Vector2i dir, BlockSide side)
            {
                if (!SearchLevel(world, x, y, z, dir, 4, level - 2, out Vector3i pos)) return false;

                (Block? block, Liquid? liquid) = world.GetPosition(
                    pos.X,
                    pos.Y,
                    pos.Z,
                    out _,
                    out LiquidLevel target,
                    out bool isStatic);

                if (!(block is IFillable targetFillable) ||
                    !targetFillable.AllowInflow(world, pos.X, pos.Y, pos.Z, side, this) || liquid != this) return false;

                SetLiquid(world, this, target + 1, false, targetFillable, pos.X, pos.Y, pos.Z);
                if (isStatic) ScheduleTick(world, pos.X, pos.Y, pos.Z);

                SetLiquid(world, this, level - 1, false, currentFillable, x, y, z);
                ScheduleTick(world, x, y, z);

                return true;
            }
        }

        private void SpreadOrDestroyLiquid(World world, int x, int y, int z, LiquidLevel level)
        {
            var remaining = (int) level;

            SpreadLiquid();

            world.SetDefaultLiquid(x, y, z);

            void SpreadLiquid()
            {
                if (FillNeighbor(x, y, z - 1, BlockSide.Front) == -1) return; // North.
                if (FillNeighbor(x + 1, y, z, BlockSide.Left) == -1) return; // East.
                if (FillNeighbor(x, y, z + 1, BlockSide.Back) == -1) return; // South.
                FillNeighbor(x - 1, y, z, BlockSide.Right); // West.
            }

            int FillNeighbor(int nx, int ny, int nz, BlockSide side)
            {
                (Block? blockNeighbor, Liquid? liquidNeighbor) = world.GetPosition(
                    nx,
                    ny,
                    nz,
                    out _,
                    out LiquidLevel levelNeighbor,
                    out bool isStatic);

                if (!(blockNeighbor is IFillable neighborFillable) ||
                    !neighborFillable.AllowInflow(world, nx, ny, nz, side, this)) return remaining;

                if (liquidNeighbor == None)
                {
                    isStatic = true;

                    SetLiquid(world, this, (LiquidLevel) remaining, false, neighborFillable, nx, ny, nz);

                    remaining = -1;

                    if (isStatic) ScheduleTick(world, nx, ny, nz);
                }
                else if (liquidNeighbor == this)
                {
                    int volume = LiquidLevel.Eight - levelNeighbor - 1;

                    if (volume >= remaining)
                    {
                        SetLiquid(world, this, levelNeighbor + remaining + 1, false, neighborFillable, nx, ny, nz);

                        remaining = -1;
                    }
                    else
                    {
                        SetLiquid(world, this, LiquidLevel.Eight, false, neighborFillable, nx, ny, nz);

                        remaining = remaining - volume - 1;
                    }

                    if (isStatic) ScheduleTick(world, nx, ny, nz);
                }

                return remaining;
            }
        }
    }
}