// <copyright file="BasicFluid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Elements.Behaviors.Fluids;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Definitions.Fluids;

/// <summary>
///     A normal fluid with simple flowing behavior.
/// </summary>
public class BasicFluid : Fluid, IOverlayTextureProvider
{
    private readonly Boolean hasNeutralTint;
    private readonly TextureLayout staticLayout;
    private readonly TextureLayout movingLayout;

    private SideArray<Int32> movingTextures = null!;
    private SideArray<Int32> staticTextures = null!;

    private Int32 mainTexture;
    private ColorS dominantColor;

    /// <summary>
    ///     Create a new basic fluid.
    /// </summary>
    /// <param name="name">The name of the basic fluid.</param>
    /// <param name="namedID">The named ID of the fluid.</param>
    /// <param name="density">The density of the fluid.</param>
    /// <param name="viscosity">The viscosity of the fluid.</param>
    /// <param name="hasNeutralTint">Whether this fluid has a neutral tint.</param>
    /// <param name="texture">The texture of the fluid.</param>
    /// <param name="renderType">The render type of the fluid.</param>
    public BasicFluid(String name, String namedID, Double density, Int32 viscosity, Boolean hasNeutralTint,
        TID texture, RenderType renderType = RenderType.Opaque) :
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

        staticLayout = TextureLayout.Fluid(texture.Offset(y: 0), texture.Offset(y: 1));
        movingLayout = TextureLayout.Fluid(texture.Offset(y: 2), texture.Offset(y: 3));
    }

    /// <inheritdoc />
    public OverlayTexture GetOverlayTexture(Content content)
    {
        return new OverlayTexture
        {
            TextureIndex = mainTexture,
            Tint = hasNeutralTint ? ColorS.Neutral : ColorS.None,
            IsAnimated = true
        };
    }

    /// <inheritdoc />
    protected override void OnSetUp(ITextureIndexProvider indexProvider, IDominantColorProvider dominantColorProvider)
    {
        movingTextures = movingLayout.GetTextureIndices(indexProvider, isBlock: false);
        staticTextures = staticLayout.GetTextureIndices(indexProvider, isBlock: false);

        mainTexture = staticTextures[(Int32) Side.Front];
        dominantColor = dominantColorProvider.GetDominantColor(mainTexture, isBlock: false);
    }

    /// <inheritdoc />
    public override ColorS? GetColor(ColorS tint)
    {
        ColorS color = dominantColor;

        if (hasNeutralTint)
            color *= tint;

        return color;
    }

    /// <inheritdoc />
    protected override FluidMeshData GetMeshData(FluidMeshInfo info)
    {
        return FluidMeshData.Basic(
            info.IsStatic ? staticTextures[info.Side] : movingTextures[info.Side],
            hasNeutralTint ? ColorS.Neutral : ColorS.None);
    }

    /// <inheritdoc />
    protected override void ScheduledUpdate(World world, Vector3i position, FluidInstance instance)
    {
        Block block = world.GetBlock(position)?.Block ?? Blocks.Instance.Core.Air;

        if (block.Get<Fillable>() is {} fillable) ValidLocationFlow(world, position, instance.Level, fillable);
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
                out Int32 remaining) && remaining == -1) ||
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

    private void ValidLocationFlow(World world, Vector3i position, FluidLevel level, Fillable current)
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

    private Boolean FlowVertical(World world, Vector3i position, Fillable? currentFillable, FluidLevel level,
        VerticalFlow flow, Boolean handleContact, out Int32 remaining)
    {
        Content? content = world.GetContent(
            position + flow.Direction());
        
        if (content?.Block.Block.Get<Fillable>() is not {} verticalFillable || content is not { Fluid: var fluidVertical }
                                                                            || !verticalFillable.CanInflow(world, position + flow.Direction(), flow.EntrySide(), this)
                                                                            || !(currentFillable?.CanOutflow(world, position, flow.ExitSide(), this) ?? true))
        {
            remaining = (Int32) level;

            return false;
        }

        if (fluidVertical.Fluid == Elements.Fluids.Instance.None)
        {
            Vector3i position1 = position + flow.Direction();
            world.SetFluid(this.AsInstance(level, isStatic: false), position1);
            world.SetFluid(Elements.Fluids.Instance.None.AsInstance(), position);

            ScheduleUpdate(world, position + flow.Direction());

            remaining = -1;

            return true;
        }

        if (fluidVertical.Fluid == this)
        {
            if (fluidVertical.Level == FluidLevel.Eight)
            {
                remaining = (Int32) level;

                return false;
            }

            Int32 volume = FluidLevel.Eight - fluidVertical.Level - 1;

            if (volume >= (Int32) level)
            {
                Vector3i position1 = position + flow.Direction();

                world.SetFluid(this.AsInstance(fluidVertical.Level + (Int32) level + 1, isStatic: false), position1);
                world.SetFluid(Elements.Fluids.Instance.None.AsInstance(), position);

                remaining = -1;
            }
            else
            {
                Vector3i position1 = position + flow.Direction();

                world.SetFluid(this.AsInstance(isStatic: false), position1);
                world.SetFluid(this.AsInstance(level - volume - 1, isStatic: false), position);

                remaining = (Int32) (level - volume - 1);

                ScheduleUpdate(world, position);
            }

            if (fluidVertical.IsStatic) ScheduleUpdate(world, position + flow.Direction());

            return true;
        }

        if (handleContact)
        {
            remaining = (Int32) level;

            return Elements.Fluids.ContactManager.HandleContact(
                world,
                this.AsInstance(level),
                position,
                fluidVertical,
                position + flow.Direction());
        }

        remaining = (Int32) level;

        return false;
    }

    private Boolean TryPuddleFlow(World world, Vector3i position, Fillable currentFillable)
    {
        Boolean fluidBelowIsNone = world.GetFluid(position + FlowDirection)?.Fluid == Elements.Fluids.Instance.None;

        foreach (Orientation orientation in Orientations.All)
        {
            if (!currentFillable.CanOutflow(world, position, orientation.ToSide(), this)) continue;

            Vector3i neighborPosition = orientation.Offset(position);

            Content? content = world.GetContent(neighborPosition);

            if (content?.Block.Block.Get<Fillable>() is not {} neighborFillable || content is not {Fluid: var neighborFluid}
                                                                                || !AllowsFlowTrough(neighborFillable, world, neighborPosition, orientation.Opposite().ToSide(), Direction.ExitSide())
                                                                                || neighborFluid.Fluid != Elements.Fluids.Instance.None
                                                                                || !CheckLowerPosition(neighborPosition + FlowDirection)) continue;

            world.SetFluid(this.AsInstance(FluidLevel.One, isStatic: false), neighborPosition);
            world.SetFluid(Elements.Fluids.Instance.None.AsInstance(), position);

            ScheduleUpdate(world, neighborPosition);

            return true;
        }

        return false;

        Boolean CheckLowerPosition(Vector3i lowerPosition)
        {
            Content? lowerContent = world.GetContent(lowerPosition);

            if (lowerContent?.Block.Block.Get<Fillable>() is not {} fillable || lowerContent is not {Fluid: var lowerFluid}) return false;

            Boolean canFlowWithoutCapacity = fluidBelowIsNone && lowerFluid.Fluid != this;

            return fillable.CanInflow(world, lowerPosition, Direction.EntrySide(), this)
                   && (HasCapacity(lowerFluid) || canFlowWithoutCapacity);
        }
    }

    private Boolean FlowHorizontal(World world, Vector3i position, FluidLevel level, Fillable currentFillable)
    {
        Vector3i horizontalPosition = position;
        var isHorStatic = false;
        var levelHorizontal = FluidLevel.Eight;

        if (Orientations.ShuffledStart(position)
            .Any(orientation => CheckNeighbor(
                currentFillable.CanOutflow(world, position, orientation.ToSide(), this),
                orientation.Offset(position),
                orientation.Opposite().ToSide()))) return true;

        if (horizontalPosition == position) return false;

        world.SetFluid(this.AsInstance(levelHorizontal + 1, isStatic: false), horizontalPosition);

        if (isHorStatic) ScheduleUpdate(world, horizontalPosition);

        Boolean hasRemaining = level != FluidLevel.One;

        Boolean isStatic1 = !hasRemaining;

        world.SetFluid((hasRemaining ? this : Elements.Fluids.Instance.None).AsInstance(hasRemaining ? level - 1 : FluidLevel.Eight, isStatic1), position);

        if (hasRemaining) ScheduleUpdate(world, position);

        return true;

        Boolean CheckNeighbor(Boolean outflowAllowed, Vector3i neighborPosition, Side side)
        {
            Content? neighborContent = world.GetContent(neighborPosition);

            if (!outflowAllowed ||
                neighborContent?.Block.Block.Get<Fillable>() is not {} neighborFillable || neighborContent is not {Fluid: var neighborFluid} ||
                !neighborFillable.CanInflow(world, neighborPosition, side, this)) return false;

            Boolean isStatic = neighborFluid.IsStatic;

            if (neighborFluid.Fluid == Elements.Fluids.Instance.None)
            {
                Vector3i belowNeighborPosition = neighborPosition + FlowDirection;

                Content? belowNeighborContent = world.GetContent(belowNeighborPosition);

                if (belowNeighborContent?.Block.Block.Get<Fillable>() is {} belowNeighborFillable && belowNeighborContent is {Fluid: var belowNeighborFluid}
                    && belowNeighborFluid.Fluid == Elements.Fluids.Instance.None
                    && belowNeighborFillable.CanInflow(
                        world,
                        belowNeighborPosition,
                        Direction.EntrySide(),
                        this)
                    && neighborFillable.CanOutflow(
                        world,
                        neighborPosition,
                        Direction.ExitSide(),
                        this))
                {
                    world.SetFluid(this.AsInstance(level, isStatic: false), belowNeighborPosition);

                    ScheduleUpdate(world, belowNeighborPosition);

                    world.SetFluid(Elements.Fluids.Instance.None.AsInstance(), position);
                }
                else
                {
                    world.SetFluid(this.AsInstance(FluidLevel.One, isStatic: false), neighborPosition);

                    ScheduleUpdate(world, neighborPosition);

                    Boolean remaining = level != FluidLevel.One;

                    Boolean isStatic2 = !remaining;

                    world.SetFluid((remaining ? this : Elements.Fluids.Instance.None).AsInstance(remaining ? level - 1 : FluidLevel.Eight, isStatic2), position);

                    if (remaining) ScheduleUpdate(world, position);
                }

                return true;
            }

            if (neighborFluid.Fluid != this)
            {
                if (Elements.Fluids.ContactManager.HandleContact(
                        world,
                        this.AsInstance(level),
                        position,
                        neighborFluid,
                        neighborPosition)) return true;
            }
            else if (neighborFluid.Fluid == this && level > neighborFluid.Level &&
                     neighborFluid.Level < levelHorizontal)
            {
                Boolean neighborHasSignificantlyLowerLevel = neighborFluid.Level != level - 1;

                Boolean neighborHasLessPressure = level == FluidLevel.Eight && !IsAtSurface(world, position) &&
                                                  IsAtSurface(world, neighborPosition);

                Boolean directNeighborAllowsFlow = neighborHasSignificantlyLowerLevel || neighborHasLessPressure;

                Boolean allowsFlow = directNeighborAllowsFlow
                                     || HasNeighborWithLevel(world, level - 2, neighborPosition)
                                     || HasNeighborWithEmpty(world, neighborPosition);

                if (!allowsFlow) return false;

                levelHorizontal = neighborFluid.Level;
                horizontalPosition = neighborPosition;
                isHorStatic = isStatic;

            }

            return false;
        }
    }

    private Boolean FarFlowHorizontal(World world, Vector3i position, FluidLevel level)
    {
        if (level < FluidLevel.Three) return false;

        (Vector3i position, FluidInstance fluid, Fillable fillable)? potentialTarget =
            SearchFlowTarget(world, position, level - 2, range: 4);

        if (potentialTarget == null) return false;

        var target = ((Vector3i position, FluidInstance fluid, Fillable fillable)) potentialTarget;

        world.SetFluid(this.AsInstance(target.fluid.Level + 1, isStatic: false), target.position);
        if (target.fluid.IsStatic) ScheduleUpdate(world, target.position);

        world.SetFluid(this.AsInstance(level - 1, isStatic: false), position);
        ScheduleUpdate(world, position);

        return true;
    }

    private void SpreadOrDestroyFluid(World world, Vector3i position, FluidLevel level)
    {
        var remaining = (Int32) level;

        foreach (Orientation orientation in Orientations.All)
        {
            FillNeighbor(world, orientation.Offset(position), orientation.ToSide(), ref remaining);

            if (remaining == -1) break;
        }

        world.SetDefaultFluid(position);
    }

    private void FillNeighbor(World world, Vector3i neighborPosition, Side side, ref Int32 remaining)
    {
        Content? neighborContent = world.GetContent(neighborPosition);

        if (neighborContent?.Block.Block.Get<Fillable>() is not {} neighborFillable || neighborContent is not {Fluid: var neighborFluid} ||
            !neighborFillable.CanInflow(world, neighborPosition, side.Opposite(), this)) return;

        Boolean isStatic = neighborFluid.IsStatic;

        if (neighborFluid.Fluid == Elements.Fluids.Instance.None)
        {
            world.SetFluid(this.AsInstance((FluidLevel) remaining, isStatic: false), neighborPosition);

            remaining = -1;

            ScheduleUpdate(world, neighborPosition);
        }
        else if (neighborFluid.Fluid == this)
        {
            Int32 volume = FluidLevel.Eight - neighborFluid.Level - 1;

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

            if (isStatic) ScheduleUpdate(world, neighborPosition);
        }
    }

    private Boolean AllowsFlowTrough(Fillable fillable, World world, Vector3i position, Side incomingSide,
        Side outgoingSide)
    {
        return fillable.CanInflow(world, position, incomingSide, this)
               && fillable.CanOutflow(world, position, outgoingSide, this);
    }

    private Boolean HasCapacity(FluidInstance fluid)
    {
        return fluid.Fluid == this && fluid.Level != FluidLevel.Eight;
    }
}
