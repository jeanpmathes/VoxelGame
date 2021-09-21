// <copyright file="BasicLiquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

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
                checkContact: true,
                receiveContact: false,
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

        protected override void ScheduledUpdate(World world, Vector3i position, LiquidLevel level, bool isStatic)
        {
            if (CheckVerticalWorldBounds(world, position)) return;

            Block block = world.GetBlock(position, out _) ?? Block.Air;

            if (block is IFillable fillable) ValidLocationFlow(world, position, level, fillable);
            else InvalidLocationFlow(world, position, level);
        }

        private void InvalidLocationFlow(World world, Vector3i position, LiquidLevel level)
        {
            if (FlowVertical(
                    world,
                    position,
                    currentFillable: null,
                    level,
                    -Direction,
                    handleContact: false,
                    out int remaining) && remaining == -1 ||
                FlowVertical(
                    world,
                    position,
                    currentFillable: null,
                    (LiquidLevel) remaining,
                    Direction,
                    handleContact: false,
                    out remaining) &&
                remaining == -1) return;

            SpreadOrDestroyLiquid(world, position, (LiquidLevel) remaining);
        }

        private void ValidLocationFlow(World world, Vector3i position, LiquidLevel level, IFillable current)
        {
            if (FlowVertical(world, position, current, level, Direction, handleContact: true, out _)) return;

            if (level != LiquidLevel.One
                ? FlowHorizontal(world, position, level, current) || FarFlowHorizontal(world, position, level, current)
                : TryPuddleFlow(world, position, current)) return;

            world.ModifyLiquid(isStatic: true, position);
        }

        private bool CheckVerticalWorldBounds(World world, Vector3i position)
        {
            if (position.Y == 0 && Direction > 0 ||
                position.Y == Section.SectionSize * Chunk.VerticalSectionCount - 1 && Direction < 0)
            {
                world.SetDefaultLiquid(position);

                return true;
            }

            return false;
        }

        private bool FlowVertical(World world, Vector3i position, IFillable? currentFillable, LiquidLevel level,
            int direction, bool handleContact, out int remaining)
        {
            (Block? blockVertical, Liquid? liquidVertical) = world.GetPosition(
                position + FlowDirection,
                out _,
                out LiquidLevel levelVertical,
                out bool isStatic);

            if (blockVertical is IFillable verticalFillable
                && verticalFillable.AllowInflow(
                    world,
                    position + FlowDirection,
                    direction > 0 ? BlockSide.Top : BlockSide.Bottom,
                    this)
                && (currentFillable?.AllowOutflow(world, position, direction > 0 ? BlockSide.Bottom : BlockSide.Top) ??
                    true))
            {
                if (liquidVertical == None)
                {
                    SetLiquid(world, this, level, isStatic: false, verticalFillable, position + FlowDirection);
                    SetLiquid(world, None, LiquidLevel.Eight, isStatic: true, currentFillable, position);

                    ScheduleTick(world, position + FlowDirection);

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
                            isStatic: false,
                            verticalFillable,
                            position + FlowDirection);

                        SetLiquid(world, None, LiquidLevel.Eight, isStatic: true, currentFillable, position);

                        remaining = -1;
                    }
                    else
                    {
                        SetLiquid(
                            world,
                            this,
                            LiquidLevel.Eight,
                            isStatic: false,
                            verticalFillable,
                            position + FlowDirection);

                        SetLiquid(world, this, level - volume - 1, isStatic: false, currentFillable, position);

                        remaining = (int) (level - volume - 1);

                        ScheduleTick(world, position);
                    }

                    if (isStatic) ScheduleTick(world, position + FlowDirection);

                    return true;
                }

                if (handleContact && liquidVertical != null)
                {
                    remaining = (int) level;

                    return ContactManager.HandleContact(
                        world,
                        this,
                        position,
                        level,
                        liquidVertical,
                        position + FlowDirection,
                        levelVertical,
                        isStatic);
                }
            }

            remaining = (int) level;

            return false;
        }

        private bool TryPuddleFlow(World world, Vector3i position, IFillable currentFillable)
        {
            bool liquidBelowIsNone = world.GetLiquid(position + FlowDirection, out _, out _) == None;

            for (var orientation = Orientation.North; orientation <= Orientation.West; orientation++)
            {
                if (!currentFillable.AllowOutflow(world, position, orientation.ToBlockSide())) continue;

                Vector3i neighborPosition = orientation.Offset(position);

                (Block? neighborBlock, Liquid? neighborLiquid) =
                    world.GetPosition(neighborPosition, out _, out _, out _);

                if (neighborBlock is IFillable neighborFillable
                    && neighborFillable.AllowInflow(world, neighborPosition, orientation.Opposite().ToBlockSide(), this)
                    && neighborFillable.AllowOutflow(
                        world,
                        neighborPosition,
                        Direction > 0 ? BlockSide.Bottom : BlockSide.Top)
                    && neighborLiquid == None && CheckLowerPosition(neighborPosition + FlowDirection))
                {
                    SetLiquid(world, this, LiquidLevel.One, isStatic: false, neighborFillable, neighborPosition);
                    SetLiquid(world, None, LiquidLevel.Eight, isStatic: true, currentFillable, position);

                    ScheduleTick(world, neighborPosition);

                    return true;
                }
            }

            return false;

            bool CheckLowerPosition(Vector3i lowerPosition)
            {
                (Block? lowerBlock, Liquid? lowerLiquid) = world.GetPosition(
                    lowerPosition,
                    out _,
                    out LiquidLevel level,
                    out _);

                return lowerBlock is IFillable fillable
                       && fillable.AllowInflow(
                           world,
                           lowerPosition,
                           Direction > 0 ? BlockSide.Top : BlockSide.Bottom,
                           this)
                       && (lowerLiquid == this && level != LiquidLevel.Eight ||
                           liquidBelowIsNone && lowerLiquid != this);
            }
        }

        private bool FlowHorizontal(World world, Vector3i position, LiquidLevel level, IFillable currentFillable)
        {
            Vector3i horizontalPosition = position;
            var isHorStatic = false;
            var levelHorizontal = LiquidLevel.Eight;
            IFillable? horizontalFillable = null;

            int start = BlockUtilities.GetPositionDependentNumber(position.X, position.Z, mod: 4);

            for (int i = start; i < start + 4; i++)
            {
                var orientation = (Orientation) (i % 4);

                if (CheckNeighbor(
                    currentFillable.AllowOutflow(world, position, orientation.ToBlockSide()),
                    orientation.Offset(position),
                    orientation.Opposite().ToBlockSide())) return true;
            }

            if (horizontalPosition == position) return false;

            SetLiquid(world, this, levelHorizontal + 1, isStatic: false, horizontalFillable, horizontalPosition);

            if (isHorStatic) ScheduleTick(world, horizontalPosition);

            bool hasRemaining = level != LiquidLevel.One;

            SetLiquid(
                world,
                hasRemaining ? this : None,
                hasRemaining ? level - 1 : LiquidLevel.Eight,
                !hasRemaining,
                currentFillable,
                position);

            if (hasRemaining) ScheduleTick(world, position);

            return true;

            bool CheckNeighbor(bool outflowAllowed, Vector3i neighborPosition, BlockSide side)
            {
                (Block? blockNeighbor, Liquid? liquidNeighbor) = world.GetPosition(
                    neighborPosition,
                    out _,
                    out LiquidLevel levelNeighbor,
                    out bool isStatic);

                if (!outflowAllowed || blockNeighbor is not IFillable neighborFillable ||
                    !neighborFillable.AllowInflow(world, neighborPosition, side, this)) return false;

                if (liquidNeighbor == None)
                {
                    isStatic = true;

                    Vector3i belowNeighborPosition = neighborPosition + FlowDirection;

                    (Block? belowNeighborBlock, Liquid? belowNeighborLiquid) = world.GetPosition(
                        belowNeighborPosition,
                        out _,
                        out _,
                        out _);

                    if (belowNeighborLiquid == None
                        && belowNeighborBlock is IFillable belowFillable
                        && belowFillable.AllowInflow(
                            world,
                            belowNeighborPosition,
                            Direction > 0 ? BlockSide.Top : BlockSide.Bottom,
                            this)
                        && neighborFillable.AllowOutflow(
                            world,
                            neighborPosition,
                            Direction > 0 ? BlockSide.Bottom : BlockSide.Top))
                    {
                        SetLiquid(world, this, level, isStatic: false, belowFillable, belowNeighborPosition);

                        ScheduleTick(world, belowNeighborPosition);

                        SetLiquid(world, None, LiquidLevel.Eight, isStatic: true, currentFillable, neighborPosition);
                    }
                    else
                    {
                        SetLiquid(world, this, LiquidLevel.One, isStatic: false, neighborFillable, neighborPosition);

                        if (isStatic) ScheduleTick(world, neighborPosition);

                        bool remaining = level != LiquidLevel.One;

                        SetLiquid(
                            world,
                            remaining ? this : None,
                            remaining ? level - 1 : LiquidLevel.Eight,
                            !remaining,
                            currentFillable,
                            position);

                        if (remaining) ScheduleTick(world, position);
                    }

                    return true;
                }

                if (liquidNeighbor != null && liquidNeighbor != this)
                {
                    if (ContactManager.HandleContact(
                        world,
                        this,
                        position,
                        level,
                        liquidNeighbor,
                        neighborPosition,
                        levelNeighbor,
                        isStatic)) return true;
                }
                else if (liquidNeighbor == this && level > levelNeighbor && levelNeighbor < levelHorizontal)
                {
                    bool allowsFlow = levelNeighbor != level - 1
                                      || level == LiquidLevel.Eight && !IsAtSurface(world, position) &&
                                      IsAtSurface(world, neighborPosition)
                                      || HasNeighborWithLevel(world, level - 2, neighborPosition)
                                      || HasNeighborWithEmpty(world, neighborPosition);

                    if (!allowsFlow) return false;

                    levelHorizontal = levelNeighbor;
                    horizontalPosition = neighborPosition;
                    isHorStatic = isStatic;

                    horizontalFillable = neighborFillable;
                }

                return false;
            }
        }

        private bool FarFlowHorizontal(World world, Vector3i position, LiquidLevel level, IFillable currentFillable)
        {
            if (level < LiquidLevel.Three) return false;

            int start = BlockUtilities.GetPositionDependentNumber(position.X, position.Z, mod: 4);

            for (int i = start; i < start + 4; i++)
            {
                var orientation = (Orientation) (i % 4);

                if (currentFillable.AllowOutflow(world, position, orientation.ToBlockSide())
                    && CheckDirection(orientation.ToVector3i(), orientation.Opposite().ToBlockSide())) return true;
            }

            return false;

            bool CheckDirection(Vector3i direction, BlockSide side)
            {
                if (!SearchLevel(
                    world,
                    position,
                    direction,
                    range: 4,
                    level - 2,
                    out Vector3i targetPosition)) return false;

                (Block? block, Liquid? liquid) = world.GetPosition(
                    targetPosition,
                    out _,
                    out LiquidLevel target,
                    out bool isStatic);

                if (block is not IFillable targetFillable ||
                    !targetFillable.AllowInflow(world, targetPosition, side, this) || liquid != this) return false;

                SetLiquid(world, this, target + 1, isStatic: false, targetFillable, targetPosition);
                if (isStatic) ScheduleTick(world, targetPosition);

                SetLiquid(world, this, level - 1, isStatic: false, currentFillable, position);
                ScheduleTick(world, position);

                return true;
            }
        }

        private void SpreadOrDestroyLiquid(World world, Vector3i position, LiquidLevel level)
        {
            var remaining = (int) level;

            for (var orientation = Orientation.North; orientation <= Orientation.West; orientation++)
            {
                FillNeighbor(orientation.Offset(position), orientation.ToBlockSide());

                if (remaining == -1) break;
            }

            world.SetDefaultLiquid(position);

            void FillNeighbor(Vector3i neighborPosition, BlockSide side)
            {
                (Block? blockNeighbor, Liquid? liquidNeighbor) = world.GetPosition(
                    neighborPosition,
                    out _,
                    out LiquidLevel levelNeighbor,
                    out bool isStatic);

                if (blockNeighbor is not IFillable neighborFillable ||
                    !neighborFillable.AllowInflow(world, neighborPosition, side.Opposite(), this)) return;

                if (liquidNeighbor == None)
                {
                    isStatic = true;

                    SetLiquid(
                        world,
                        this,
                        (LiquidLevel) remaining,
                        isStatic: false,
                        neighborFillable,
                        neighborPosition);

                    remaining = -1;

                    if (isStatic) ScheduleTick(world, neighborPosition);
                }
                else if (liquidNeighbor == this)
                {
                    int volume = LiquidLevel.Eight - levelNeighbor - 1;

                    if (volume >= remaining)
                    {
                        SetLiquid(
                            world,
                            this,
                            levelNeighbor + remaining + 1,
                            isStatic: false,
                            neighborFillable,
                            neighborPosition);

                        remaining = -1;
                    }
                    else
                    {
                        SetLiquid(world, this, LiquidLevel.Eight, isStatic: false, neighborFillable, neighborPosition);

                        remaining = remaining - volume - 1;
                    }

                    if (isStatic) ScheduleTick(world, neighborPosition);
                }
            }
        }
    }
}
