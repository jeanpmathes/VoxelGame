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

            Block block = world.GetBlock(position)?.Block ?? Block.Air;

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
                    Direction,
                    handleContact: false,
                    out int remaining) && remaining == -1 ||
                FlowVertical(
                    world,
                    position,
                    currentFillable: null,
                    (LiquidLevel) remaining,
                    Direction.Opposite(),
                    handleContact: false,
                    out remaining) &&
                remaining == -1) return;

            SpreadOrDestroyLiquid(world, position, (LiquidLevel) remaining);
        }

        private void ValidLocationFlow(World world, Vector3i position, LiquidLevel level, IFillable current)
        {
            if (FlowVertical(
                world,
                position,
                current,
                level,
                Direction,
                handleContact: true,
                out _)) return;

            if (level != LiquidLevel.One
                ? FlowHorizontal(world, position, level, current) || FarFlowHorizontal(world, position, level, current)
                : TryPuddleFlow(world, position, current)) return;

            world.ModifyLiquid(isStatic: true, position);
        }

        private bool CheckVerticalWorldBounds(World world, Vector3i position)
        {
            if (position.Y == 0 && Direction == VerticalFlow.Downwards ||
                position.Y == Section.SectionSize * Chunk.VerticalSectionCount - 1 && Direction == VerticalFlow.Upwards)
            {
                world.SetDefaultLiquid(position);

                return true;
            }

            return false;
        }

        private bool FlowVertical(World world, Vector3i position, IFillable? currentFillable, LiquidLevel level,
            VerticalFlow flow, bool handleContact, out int remaining)
        {
            (BlockInstance? blockVertical, LiquidInstance? liquidVertical) = world.GetContent(
                position + flow.Direction());

            if (blockVertical?.Block is IFillable verticalFillable
                && verticalFillable.AllowInflow(
                    world,
                    position + flow.Direction(),
                    flow.EntrySide(),
                    this)
                && (currentFillable?.AllowOutflow(world, position, flow.ExitSide()) ??
                    true))
            {
                if (liquidVertical?.Liquid == None)
                {
                    SetLiquid(world, this, level, isStatic: false, verticalFillable, position + flow.Direction());
                    SetLiquid(world, None, LiquidLevel.Eight, isStatic: true, currentFillable, position);

                    ScheduleTick(world, position + flow.Direction());

                    remaining = -1;

                    return true;
                }

                if (liquidVertical?.Liquid == this)
                {
                    if (liquidVertical.Level == LiquidLevel.Eight)
                    {
                        remaining = (int) level;

                        return false;
                    }

                    int volume = LiquidLevel.Eight - liquidVertical.Level - 1;

                    if (volume >= (int) level)
                    {
                        SetLiquid(
                            world,
                            this,
                            liquidVertical.Level + (int) level + 1,
                            isStatic: false,
                            verticalFillable,
                            position + flow.Direction());

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
                            position + flow.Direction());

                        SetLiquid(world, this, level - volume - 1, isStatic: false, currentFillable, position);

                        remaining = (int) (level - volume - 1);

                        ScheduleTick(world, position);
                    }

                    if (liquidVertical.IsStatic) ScheduleTick(world, position + flow.Direction());

                    return true;
                }

                if (handleContact && liquidVertical != null)
                {
                    remaining = (int) level;

                    return ContactManager.HandleContact(
                        world,
                        this.AsInstance(level),
                        position,
                        liquidVertical,
                        position + flow.Direction());
                }
            }

            remaining = (int) level;

            return false;
        }

        private bool TryPuddleFlow(World world, Vector3i position, IFillable currentFillable)
        {
            bool liquidBelowIsNone = world.GetLiquid(position + FlowDirection)?.Liquid == None;

            foreach (Orientation orientation in Orientations.All)
            {
                if (!currentFillable.AllowOutflow(world, position, orientation.ToBlockSide())) continue;

                Vector3i neighborPosition = orientation.Offset(position);

                (BlockInstance? neighborBlock, LiquidInstance? neighborLiquid) = world.GetContent(neighborPosition);

                if (neighborBlock?.Block is IFillable neighborFillable
                    && neighborFillable.AllowInflow(world, neighborPosition, orientation.Opposite().ToBlockSide(), this)
                    && neighborFillable.AllowOutflow(
                        world,
                        neighborPosition,
                        Direction.ExitSide())
                    && neighborLiquid?.Liquid == None && CheckLowerPosition(neighborPosition + FlowDirection))
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
                (BlockInstance? lowerBlock, LiquidInstance? lowerLiquid) = world.GetContent(lowerPosition);

                return lowerBlock?.Block is IFillable fillable
                       && fillable.AllowInflow(
                           world,
                           lowerPosition,
                           Direction.EntrySide(),
                           this)
                       && (lowerLiquid?.Liquid == this && lowerLiquid.Level != LiquidLevel.Eight ||
                           liquidBelowIsNone && lowerLiquid?.Liquid != this);
            }
        }

        private bool FlowHorizontal(World world, Vector3i position, LiquidLevel level, IFillable currentFillable)
        {
            Vector3i horizontalPosition = position;
            var isHorStatic = false;
            var levelHorizontal = LiquidLevel.Eight;
            IFillable? horizontalFillable = null;

            foreach (Orientation orientation in Orientations.ShuffledStart(position))
                if (CheckNeighbor(
                    currentFillable.AllowOutflow(world, position, orientation.ToBlockSide()),
                    orientation.Offset(position),
                    orientation.Opposite().ToBlockSide()))
                    return true;

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
                (BlockInstance? blockNeighbor, LiquidInstance? liquidNeighbor) = world.GetContent(neighborPosition);

                if (!outflowAllowed || blockNeighbor?.Block is not IFillable neighborFillable ||
                    !neighborFillable.AllowInflow(world, neighborPosition, side, this)) return false;

                bool isStatic = liquidNeighbor?.IsStatic ?? false;

                if (liquidNeighbor?.Liquid == None)
                {
                    isStatic = true;

                    Vector3i belowNeighborPosition = neighborPosition + FlowDirection;

                    (BlockInstance? belowNeighborBlock, LiquidInstance? belowNeighborLiquid) = world.GetContent(
                        belowNeighborPosition);

                    if (belowNeighborLiquid?.Liquid == None
                        && belowNeighborBlock?.Block is IFillable belowFillable
                        && belowFillable.AllowInflow(
                            world,
                            belowNeighborPosition,
                            Direction.EntrySide(),
                            this)
                        && neighborFillable.AllowOutflow(
                            world,
                            neighborPosition,
                            Direction.ExitSide()))
                    {
                        SetLiquid(world, this, level, isStatic: false, belowFillable, belowNeighborPosition);

                        ScheduleTick(world, belowNeighborPosition);

                        SetLiquid(world, None, LiquidLevel.Eight, isStatic: true, currentFillable, position);
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

                if (liquidNeighbor != null && liquidNeighbor.Liquid != this)
                {
                    if (ContactManager.HandleContact(
                        world,
                        this.AsInstance(level),
                        position,
                        liquidNeighbor,
                        neighborPosition)) return true;
                }
                else if (liquidNeighbor?.Liquid == this && level > liquidNeighbor.Level &&
                         liquidNeighbor.Level < levelHorizontal)
                {
                    bool allowsFlow = liquidNeighbor.Level != level - 1
                                      || level == LiquidLevel.Eight && !IsAtSurface(world, position) &&
                                      IsAtSurface(world, neighborPosition)
                                      || HasNeighborWithLevel(world, level - 2, neighborPosition)
                                      || HasNeighborWithEmpty(world, neighborPosition);

                    if (!allowsFlow) return false;

                    levelHorizontal = liquidNeighbor.Level;
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

            (Vector3i position, LiquidInstance liquid, IFillable fillable)? potentialTarget =
                SearchFlowTarget(world, position, level - 2, range: 4);

            if (potentialTarget == null) return false;

            var target = ((Vector3i position, LiquidInstance liquid, IFillable fillable)) potentialTarget;

            SetLiquid(world, this, target.liquid.Level + 1, isStatic: false, target.fillable, target.position);
            if (target.liquid.IsStatic) ScheduleTick(world, target.position);

            SetLiquid(world, this, level - 1, isStatic: false, currentFillable, position);
            ScheduleTick(world, position);

            return true;
        }

        private void SpreadOrDestroyLiquid(World world, Vector3i position, LiquidLevel level)
        {
            var remaining = (int) level;

            foreach (Orientation orientation in Orientations.All)
            {
                FillNeighbor(orientation.Offset(position), orientation.ToBlockSide());

                if (remaining == -1) break;
            }

            world.SetDefaultLiquid(position);

            void FillNeighbor(Vector3i neighborPosition, BlockSide side)
            {
                (BlockInstance? blockNeighbor, LiquidInstance? liquidNeighbor) = world.GetContent(neighborPosition);

                if (blockNeighbor?.Block is not IFillable neighborFillable ||
                    !neighborFillable.AllowInflow(world, neighborPosition, side.Opposite(), this)) return;

                bool isStatic = liquidNeighbor?.IsStatic ?? false;

                if (liquidNeighbor?.Liquid == None)
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
                else if (liquidNeighbor?.Liquid == this)
                {
                    int volume = LiquidLevel.Eight - liquidNeighbor.Level - 1;

                    if (volume >= remaining)
                    {
                        SetLiquid(
                            world,
                            this,
                            liquidNeighbor.Level + remaining + 1,
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