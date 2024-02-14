// <copyright file="BasicFluid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

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
    private readonly bool hasNeutralTint;
    private readonly TextureLayout movingLayout;
    private readonly TextureLayout staticLayout;

    private int[] movingTex = null!;
    private int[] staticTex = null!;

    /// <summary>
    ///     Create a new basic fluid.
    /// </summary>
    /// <param name="name">The name of the basic fluid.</param>
    /// <param name="namedID">The named ID of the fluid.</param>
    /// <param name="density">The density of the fluid.</param>
    /// <param name="viscosity">The viscosity of the fluid.</param>
    /// <param name="hasNeutralTint">Whether this fluid has a neutral tint.</param>
    /// <param name="movingLayout">The texture layout when this fluid is moving.</param>
    /// <param name="staticLayout">The texture layout when this fluid is static.</param>
    /// <param name="renderType">The render type of the fluid.</param>
    public BasicFluid(string name, string namedID, float density, int viscosity, bool hasNeutralTint,
        TextureLayout movingLayout, TextureLayout staticLayout, RenderType renderType = RenderType.Opaque) :
        base(
            name,
            namedID,
            density,
            viscosity,
            checkContact: true,
            receiveContact: false,
            renderType)
    {
        this.hasNeutralTint = hasNeutralTint;

        this.movingLayout = movingLayout;
        this.staticLayout = staticLayout;
    }

    /// <inheritdoc />
    public OverlayTexture GetOverlayTexture(Content content)
    {
        return new OverlayTexture
        {
            TextureIdentifier = staticLayout.Front,
            Tint = hasNeutralTint ? TintColor.Neutral : TintColor.None,
            IsAnimated = true
        };
    }

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
            hasNeutralTint ? TintColor.Neutral : TintColor.None);
    }

    /// <inheritdoc />
    protected override void ScheduledUpdate(World world, Vector3i position, FluidInstance instance)
    {
        Block block = world.GetBlock(position)?.Block ?? Logic.Blocks.Instance.Air;

        if (block is IFillable fillable) ValidLocationFlow(world, position, instance.Level, fillable);
        else InvalidLocationFlow(world, position, instance.Level);
    }

    private void InvalidLocationFlow(World world, Vector3i position, FluidLevel level)
    {
        if ((FlowVertical(
                world,
                position,
                currentFillable: null,
                level,
                Direction,
                handleContact: false,
                out int remaining) && remaining == -1) ||
            (FlowVertical(
                 world,
                 position,
                 currentFillable: null,
                 (FluidLevel) remaining,
                 Direction.Opposite(),
                 handleContact: false,
                 out remaining) &&
             remaining == -1)) return;

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
                  FarFlowHorizontal(world, position, level)
                : TryPuddleFlow(world, position, current)) return;

        world.ModifyFluid(isStatic: true, position);
    }

    private bool FlowVertical(World world, Vector3i position, IFillable? currentFillable, FluidLevel level,
        VerticalFlow flow, bool handleContact, out int remaining)
    {
        Content? content = world.GetContent(
            position + flow.Direction());

        if (content is not ({Block: IFillable verticalFillable}, var fluidVertical)
            || !verticalFillable.IsInflowAllowed(
                world,
                position + flow.Direction(),
                flow.EntrySide(),
                this)
            || !(currentFillable?.IsOutflowAllowed(world, position, flow.ExitSide()) ?? true))
        {
            remaining = (int) level;

            return false;
        }

        if (fluidVertical.Fluid == Logic.Fluids.Instance.None)
        {
            Vector3i position1 = position + flow.Direction();
            world.SetFluid(this.AsInstance(level, isStatic: false), position1);
            world.SetFluid(Logic.Fluids.Instance.None.AsInstance(), position);

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
                Vector3i position1 = position + flow.Direction();

                world.SetFluid(this.AsInstance(fluidVertical.Level + (int) level + 1, isStatic: false), position1);

                world.SetFluid(Logic.Fluids.Instance.None.AsInstance(), position);

                remaining = -1;
            }
            else
            {
                Vector3i position1 = position + flow.Direction();

                world.SetFluid(this.AsInstance(isStatic: false), position1);

                world.SetFluid(this.AsInstance(level - volume - 1, isStatic: false), position);

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
            if (!currentFillable.IsOutflowAllowed(world, position, orientation.ToBlockSide())) continue;

            Vector3i neighborPosition = orientation.Offset(position);

            Content? content = world.GetContent(neighborPosition);

            if (content is not ({Block: IFillable neighborFillable}, var neighborFluid)
                || !AllowsFlowTrough(
                    neighborFillable,
                    world,
                    neighborPosition,
                    orientation.Opposite().ToBlockSide(),
                    Direction.ExitSide())
                || neighborFluid.Fluid != Logic.Fluids.Instance.None
                || !CheckLowerPosition(neighborPosition + FlowDirection)) continue;

            world.SetFluid(this.AsInstance(FluidLevel.One, isStatic: false), neighborPosition);
            world.SetFluid(Logic.Fluids.Instance.None.AsInstance(), position);

            ScheduleTick(world, neighborPosition);

            return true;
        }

        return false;

        bool CheckLowerPosition(Vector3i lowerPosition)
        {
            Content? lowerContent = world.GetContent(lowerPosition);

            if (lowerContent is not ({Block: IFillable fillable}, var lowerFluid)) return false;

            bool canFlowWithoutCapacity = fluidBelowIsNone && lowerFluid.Fluid != this;

            return fillable.IsInflowAllowed(
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

        if (Orientations.ShuffledStart(position)
            .Any(
                orientation => CheckNeighbor(
                    currentFillable.IsOutflowAllowed(world, position, orientation.ToBlockSide()),
                    orientation.Offset(position),
                    orientation.Opposite().ToBlockSide()))) return true;

        if (horizontalPosition == position) return false;

        world.SetFluid(this.AsInstance(levelHorizontal + 1, isStatic: false), horizontalPosition);

        if (isHorStatic) ScheduleTick(world, horizontalPosition);

        bool hasRemaining = level != FluidLevel.One;

        bool isStatic1 = !hasRemaining;

        world.SetFluid((hasRemaining ? this : Logic.Fluids.Instance.None).AsInstance(hasRemaining ? level - 1 : FluidLevel.Eight, isStatic1), position);

        if (hasRemaining) ScheduleTick(world, position);

        return true;

        bool CheckNeighbor(bool outflowAllowed, Vector3i neighborPosition, BlockSide side)
        {
            Content? neighborContent = world.GetContent(neighborPosition);

            if (!outflowAllowed ||
                neighborContent is not ({Block: IFillable neighborFillable}, var fluidNeighbor) ||
                !neighborFillable.IsInflowAllowed(world, neighborPosition, side, this)) return false;

            bool isStatic = fluidNeighbor.IsStatic;

            if (fluidNeighbor.Fluid == Logic.Fluids.Instance.None)
            {
                Vector3i belowNeighborPosition = neighborPosition + FlowDirection;

                Content? belowNeighborContent = world.GetContent(belowNeighborPosition);

                if (belowNeighborContent is ({Block: IFillable belowNeighborFillable}, var belowNeighborFluid)
                    && belowNeighborFluid.Fluid == Logic.Fluids.Instance.None
                    && belowNeighborFillable.IsInflowAllowed(
                        world,
                        belowNeighborPosition,
                        Direction.EntrySide(),
                        this)
                    && neighborFillable.IsOutflowAllowed(
                        world,
                        neighborPosition,
                        Direction.ExitSide()))
                {
                    world.SetFluid(this.AsInstance(level, isStatic: false), belowNeighborPosition);

                    ScheduleTick(world, belowNeighborPosition);

                    world.SetFluid(Logic.Fluids.Instance.None.AsInstance(), position);
                }
                else
                {
                    world.SetFluid(this.AsInstance(FluidLevel.One, isStatic: false), neighborPosition);

                    ScheduleTick(world, neighborPosition);

                    bool remaining = level != FluidLevel.One;

                    bool isStatic2 = !remaining;

                    world.SetFluid((remaining ? this : Logic.Fluids.Instance.None).AsInstance(remaining ? level - 1 : FluidLevel.Eight, isStatic2), position);

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

            }

            return false;
        }
    }

    private bool FarFlowHorizontal(World world, Vector3i position, FluidLevel level)
    {
        if (level < FluidLevel.Three) return false;

        (Vector3i position, FluidInstance fluid, IFillable fillable)? potentialTarget =
            SearchFlowTarget(world, position, level - 2, range: 4);

        if (potentialTarget == null) return false;

        var target = ((Vector3i position, FluidInstance fluid, IFillable fillable)) potentialTarget;

        world.SetFluid(this.AsInstance(target.fluid.Level + 1, isStatic: false), target.position);
        if (target.fluid.IsStatic) ScheduleTick(world, target.position);

        world.SetFluid(this.AsInstance(level - 1, isStatic: false), position);
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

        if (neighborContent is not ({Block: IFillable neighborFillable}, var neighborFluid) ||
            !neighborFillable.IsInflowAllowed(world, neighborPosition, side.Opposite(), this)) return;

        bool isStatic = neighborFluid.IsStatic;

        if (neighborFluid.Fluid == Logic.Fluids.Instance.None)
        {
            world.SetFluid(this.AsInstance((FluidLevel) remaining, isStatic: false), neighborPosition);

            remaining = -1;

            ScheduleTick(world, neighborPosition);
        }
        else if (neighborFluid.Fluid == this)
        {
            int volume = FluidLevel.Eight - neighborFluid.Level - 1;

            if (volume >= remaining)
            {
                world.SetFluid(this.AsInstance(neighborFluid.Level + remaining + 1, isStatic: false), neighborPosition);

                remaining = -1;
            }
            else
            {
                world.SetFluid(this.AsInstance(isStatic: false), neighborPosition);

                remaining = remaining - volume - 1;
            }

            if (isStatic) ScheduleTick(world, neighborPosition);
        }
    }

    private bool AllowsFlowTrough(IFillable fillable, World world, Vector3i position, BlockSide incomingSide,
        BlockSide outgoingSide)
    {
        return fillable.IsInflowAllowed(
                   world,
                   position,
                   incomingSide,
                   this)
               && fillable.IsOutflowAllowed(
                   world,
                   position,
                   outgoingSide);
    }

    private bool HasCapacity(FluidInstance fluid)
    {
        return fluid.Fluid == this && fluid.Level != FluidLevel.Eight;
    }
}
