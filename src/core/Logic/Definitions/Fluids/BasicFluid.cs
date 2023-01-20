// <copyright file="BasicFluid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Fluids;

/// <summary>
///     A normal fluid with simple flowing behavior.
/// </summary>
public class BasicFluid : Fluid, IOverlayTextureProvider
{
    private readonly TextureLayout movingLayout;
    private readonly bool neutralTint;
    private readonly TextureLayout staticLayout;

    private int[] movingTex = null!;
    private int[] staticTex = null!;

    /// <summary>
    ///     Create a new basic fluid.
    /// </summary>
    /// <param name="name">The name of the basic fluid.</param>
    /// <param name="namedId">The named ID of the fluid.</param>
    /// <param name="density">The density of the fluid.</param>
    /// <param name="viscosity">The viscosity of the fluid.</param>
    /// <param name="neutralTint">Whether this fluid has a neutral tint.</param>
    /// <param name="movingLayout">The texture layout when this fluid is moving.</param>
    /// <param name="staticLayout">The texture layout when this fluid is static.</param>
    /// <param name="renderType">The render type of the fluid.</param>
    public BasicFluid(string name, string namedId, float density, int viscosity, bool neutralTint,
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

    /// <summary>
    ///     The texture to use for the fluid overlay.
    /// </summary>
    public int TextureIdentifier => staticLayout.Front;

    /// <inheritdoc />
    protected override void OnSetup(ITextureIndexProvider indexProvider)
    {
        movingTex = movingLayout.GetTexIndexArray();
        staticTex = staticLayout.GetTexIndexArray();
    }

    /// <inheritdoc />
    protected override FluidMeshData GetMeshData(FluidMeshInfo info)
    {
        return FluidMeshData.Basic(
            info.IsStatic ? staticTex[(int) info.Side] : movingTex[(int) info.Side],
            neutralTint ? TintColor.Neutral : TintColor.None);
    }

    /// <inheritdoc />
    protected override void ScheduledUpdate(World world, Vector3i position, FluidLevel level, bool isStatic)
    {
        Block block = world.GetBlock(position)?.Block ?? Logic.Blocks.Instance.Air;

        if (block is IFillable fillable) ValidLocationFlow(world, position, level, fillable);
        else InvalidLocationFlow(world, position, level);
    }

    private void InvalidLocationFlow(World world, Vector3i position, FluidLevel level)
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
                (FluidLevel) remaining,
                Direction.Opposite(),
                handleContact: false,
                out remaining) &&
            remaining == -1) return;

        SpreadOrDestroyFluid(world, position, (FluidLevel) remaining);
    }

    private void ValidLocationFlow(World world, Vector3i position, FluidLevel level, IFillable current)
    {
        if (FlowVertical(
                world,
                position,
                current,
                level,
                Direction,
                handleContact: true,
                out _)) return;

        if (level != FluidLevel.One
                ? FlowHorizontal(world, position, level, current) ||
                  FarFlowHorizontal(world, position, level, current)
                : TryPuddleFlow(world, position, current)) return;

        world.ModifyFluid(isStatic: true, position);
    }

    private bool FlowVertical(World world, Vector3i position, IFillable? currentFillable, FluidLevel level,
        VerticalFlow flow, bool handleContact, out int remaining)
    {
        Content? content = world.GetContent(
            position + flow.Direction());

        if (content is not ({Block: IFillable verticalFillable}, {} fluidVertical)
            || !verticalFillable.AllowInflow(
                world,
                position + flow.Direction(),
                flow.EntrySide(),
                this)
            || !(currentFillable?.AllowOutflow(world, position, flow.ExitSide()) ?? true))
        {
            remaining = (int) level;

            return false;
        }

        if (fluidVertical.Fluid == Logic.Fluids.Instance.None)
        {
            SetFluid(world, this, level, isStatic: false, verticalFillable, position + flow.Direction());
            SetFluid(world, Logic.Fluids.Instance.None, FluidLevel.Eight, isStatic: true, currentFillable, position);

            ScheduleTick(world, position + flow.Direction());

            remaining = -1;

            return true;
        }

        if (fluidVertical.Fluid == this)
        {
            if (fluidVertical.Level == FluidLevel.Eight)
            {
                remaining = (int) level;

                return false;
            }

            int volume = FluidLevel.Eight - fluidVertical.Level - 1;

            if (volume >= (int) level)
            {
                SetFluid(
                    world,
                    this,
                    fluidVertical.Level + (int) level + 1,
                    isStatic: false,
                    verticalFillable,
                    position + flow.Direction());

                SetFluid(world, Logic.Fluids.Instance.None, FluidLevel.Eight, isStatic: true, currentFillable, position);

                remaining = -1;
            }
            else
            {
                SetFluid(
                    world,
                    this,
                    FluidLevel.Eight,
                    isStatic: false,
                    verticalFillable,
                    position + flow.Direction());

                SetFluid(world, this, level - volume - 1, isStatic: false, currentFillable, position);

                remaining = (int) (level - volume - 1);

                ScheduleTick(world, position);
            }

            if (fluidVertical.IsStatic) ScheduleTick(world, position + flow.Direction());

            return true;
        }

        if (handleContact)
        {
            remaining = (int) level;

            return Logic.Fluids.Instance.ContactManager.HandleContact(
                world,
                this.AsInstance(level),
                position,
                fluidVertical,
                position + flow.Direction());
        }

        remaining = (int) level;

        return false;
    }

    private bool TryPuddleFlow(World world, Vector3i position, IFillable currentFillable)
    {
        bool fluidBelowIsNone = world.GetFluid(position + FlowDirection)?.Fluid == Logic.Fluids.Instance.None;

        foreach (Orientation orientation in Orientations.All)
        {
            if (!currentFillable.AllowOutflow(world, position, orientation.ToBlockSide())) continue;

            Vector3i neighborPosition = orientation.Offset(position);

            Content? content = world.GetContent(neighborPosition);

            if (content is not ({Block: IFillable neighborFillable}, {} neighborFluid)
                || !AllowsFlowTrough(
                    neighborFillable,
                    world,
                    neighborPosition,
                    orientation.Opposite().ToBlockSide(),
                    Direction.ExitSide())
                || neighborFluid.Fluid != Logic.Fluids.Instance.None
                || !CheckLowerPosition(neighborPosition + FlowDirection)) continue;

            SetFluid(world, this, FluidLevel.One, isStatic: false, neighborFillable, neighborPosition);
            SetFluid(world, Logic.Fluids.Instance.None, FluidLevel.Eight, isStatic: true, currentFillable, position);

            ScheduleTick(world, neighborPosition);

            return true;
        }

        return false;

        bool CheckLowerPosition(Vector3i lowerPosition)
        {
            Content? lowerContent = world.GetContent(lowerPosition);

            if (lowerContent is not ({Block: IFillable fillable}, var lowerFluid)) return false;

            bool canFlowWithoutCapacity = fluidBelowIsNone && lowerFluid.Fluid != this;

            return fillable.AllowInflow(
                       world,
                       lowerPosition,
                       Direction.EntrySide(),
                       this)
                   && (HasCapacity(lowerFluid) || canFlowWithoutCapacity);
        }
    }

    private bool FlowHorizontal(World world, Vector3i position, FluidLevel level, IFillable currentFillable)
    {
        Vector3i horizontalPosition = position;
        var isHorStatic = false;
        var levelHorizontal = FluidLevel.Eight;
        IFillable? horizontalFillable = null;

        if (Orientations.ShuffledStart(position)
            .Any(
                orientation => CheckNeighbor(
                    currentFillable.AllowOutflow(world, position, orientation.ToBlockSide()),
                    orientation.Offset(position),
                    orientation.Opposite().ToBlockSide()))) return true;

        if (horizontalPosition == position) return false;

        SetFluid(world, this, levelHorizontal + 1, isStatic: false, horizontalFillable, horizontalPosition);

        if (isHorStatic) ScheduleTick(world, horizontalPosition);

        bool hasRemaining = level != FluidLevel.One;

        SetFluid(
            world,
            hasRemaining ? this : Logic.Fluids.Instance.None,
            hasRemaining ? level - 1 : FluidLevel.Eight,
            !hasRemaining,
            currentFillable,
            position);

        if (hasRemaining) ScheduleTick(world, position);

        return true;

        bool CheckNeighbor(bool outflowAllowed, Vector3i neighborPosition, BlockSide side)
        {
            Content? neighborContent = world.GetContent(neighborPosition);

            if (!outflowAllowed ||
                neighborContent is not ({Block: IFillable neighborFillable}, {} fluidNeighbor) ||
                !neighborFillable.AllowInflow(world, neighborPosition, side, this)) return false;

            bool isStatic = fluidNeighbor.IsStatic;

            if (fluidNeighbor.Fluid == Logic.Fluids.Instance.None)
            {
                isStatic = true;

                Vector3i belowNeighborPosition = neighborPosition + FlowDirection;

                Content? belowNeighborContent = world.GetContent(belowNeighborPosition);



                if (belowNeighborContent is ({Block: IFillable belowNeighborFillable}, {} belowNeighborFluid)
                    && belowNeighborFluid.Fluid == Logic.Fluids.Instance.None
                    && belowNeighborFillable.AllowInflow(
                        world,
                        belowNeighborPosition,
                        Direction.EntrySide(),
                        this)
                    && neighborFillable.AllowOutflow(
                        world,
                        neighborPosition,
                        Direction.ExitSide()))
                {
                    SetFluid(world, this, level, isStatic: false, belowNeighborFillable, belowNeighborPosition);

                    ScheduleTick(world, belowNeighborPosition);

                    SetFluid(world, Logic.Fluids.Instance.None, FluidLevel.Eight, isStatic: true, currentFillable, position);
                }
                else
                {
                    SetFluid(world, this, FluidLevel.One, isStatic: false, neighborFillable, neighborPosition);

                    if (isStatic) ScheduleTick(world, neighborPosition);

                    bool remaining = level != FluidLevel.One;

                    SetFluid(
                        world,
                        remaining ? this : Logic.Fluids.Instance.None,
                        remaining ? level - 1 : FluidLevel.Eight,
                        !remaining,
                        currentFillable,
                        position);

                    if (remaining) ScheduleTick(world, position);
                }

                return true;
            }

            if (fluidNeighbor.Fluid != this)
            {
                if (Logic.Fluids.Instance.ContactManager.HandleContact(
                        world,
                        this.AsInstance(level),
                        position,
                        fluidNeighbor,
                        neighborPosition)) return true;
            }
            else if (fluidNeighbor.Fluid == this && level > fluidNeighbor.Level &&
                     fluidNeighbor.Level < levelHorizontal)
            {
                bool neighborHasSignificantlyLowerLevel = fluidNeighbor.Level != level - 1;

                bool neighborHasLessPressure = level == FluidLevel.Eight && !IsAtSurface(world, position) &&
                                               IsAtSurface(world, neighborPosition);

                bool directNeighborAllowsFlow = neighborHasSignificantlyLowerLevel || neighborHasLessPressure;

                bool allowsFlow = directNeighborAllowsFlow
                                  || HasNeighborWithLevel(world, level - 2, neighborPosition)
                                  || HasNeighborWithEmpty(world, neighborPosition);

                if (!allowsFlow) return false;

                levelHorizontal = fluidNeighbor.Level;
                horizontalPosition = neighborPosition;
                isHorStatic = isStatic;

                horizontalFillable = neighborFillable;
            }

            return false;
        }
    }

    private bool FarFlowHorizontal(World world, Vector3i position, FluidLevel level, IFillable currentFillable)
    {
        if (level < FluidLevel.Three) return false;

        (Vector3i position, FluidInstance fluid, IFillable fillable)? potentialTarget =
            SearchFlowTarget(world, position, level - 2, range: 4);

        if (potentialTarget == null) return false;

        var target = ((Vector3i position, FluidInstance fluid, IFillable fillable)) potentialTarget;

        SetFluid(world, this, target.fluid.Level + 1, isStatic: false, target.fillable, target.position);
        if (target.fluid.IsStatic) ScheduleTick(world, target.position);

        SetFluid(world, this, level - 1, isStatic: false, currentFillable, position);
        ScheduleTick(world, position);

        return true;
    }

    private void SpreadOrDestroyFluid(World world, Vector3i position, FluidLevel level)
    {
        var remaining = (int) level;

        foreach (Orientation orientation in Orientations.All)
        {
            FillNeighbor(world, orientation.Offset(position), orientation.ToBlockSide(), ref remaining);

            if (remaining == -1) break;
        }

        world.SetDefaultFluid(position);
    }

    private void FillNeighbor(World world, Vector3i neighborPosition, BlockSide side, ref int remaining)
    {
        Content? neighborContent = world.GetContent(neighborPosition);

        if (neighborContent is not ({Block: IFillable neighborFillable}, {} neighborFluid) ||
            !neighborFillable.AllowInflow(world, neighborPosition, side.Opposite(), this)) return;

        bool isStatic = neighborFluid.IsStatic;

        if (neighborFluid.Fluid == Logic.Fluids.Instance.None)
        {
            isStatic = true;

            SetFluid(
                world,
                this,
                (FluidLevel) remaining,
                isStatic: false,
                neighborFillable,
                neighborPosition);

            remaining = -1;

            if (isStatic) ScheduleTick(world, neighborPosition);
        }
        else if (neighborFluid.Fluid == this)
        {
            int volume = FluidLevel.Eight - neighborFluid.Level - 1;

            if (volume >= remaining)
            {
                SetFluid(
                    world,
                    this,
                    neighborFluid.Level + remaining + 1,
                    isStatic: false,
                    neighborFillable,
                    neighborPosition);

                remaining = -1;
            }
            else
            {
                SetFluid(world, this, FluidLevel.Eight, isStatic: false, neighborFillable, neighborPosition);

                remaining = remaining - volume - 1;
            }

            if (isStatic) ScheduleTick(world, neighborPosition);
        }
    }

    private bool AllowsFlowTrough(IFillable fillable, World world, Vector3i position, BlockSide incomingSide,
        BlockSide outgoingSide)
    {
        return fillable.AllowInflow(
                   world,
                   position,
                   incomingSide,
                   this)
               && fillable.AllowOutflow(
                   world,
                   position,
                   outgoingSide);
    }

    private bool HasCapacity(FluidInstance fluid)
    {
        return fluid.Fluid == this && fluid.Level != FluidLevel.Eight;
    }
}

