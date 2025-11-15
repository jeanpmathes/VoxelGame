// <copyright file="BasicFluid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Units;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Contents.Fluids;

/// <summary>
///     A normal fluid with simple flowing behavior.
/// </summary>
public class BasicFluid : Fluid, IOverlayTextureProvider
{
    private readonly Boolean hasNeutralTint;
    private readonly TextureLayout movingLayout;
    private readonly TextureLayout staticLayout;
    private ColorS dominantColor;

    private Int32 mainTexture;

    private SideArray<Int32> movingTextures = null!;
    private SideArray<Int32> staticTextures = null!;

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
    public BasicFluid(String name, String namedID, Density density, Viscosity viscosity, Boolean hasNeutralTint,
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
            Tint = hasNeutralTint ? ColorS.Neutral : ColorS.NoTint,
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
            hasNeutralTint ? ColorS.Neutral : ColorS.NoTint);
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
        if (FlowVertical(
                world,
                position,
                currentFillable: null,
                level,
                Direction,
                handleContact: false,
                out FluidLevel remaining) && remaining == FluidLevel.None) return;

        if (FlowVertical(
                world,
                position,
                currentFillable: null,
                remaining,
                Direction.Opposite(),
                handleContact: false,
                out remaining) &&
            remaining == FluidLevel.None) return;

        SpreadOrDestroyFluid(world, position, remaining);
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
        VerticalFlow flow, Boolean handleContact, out FluidLevel remaining)
    {
        Content? content = world.GetContent(
            position + flow.Direction());

        if (content?.Block.Block.Get<Fillable>() is not {} verticalFillable || content is not {Fluid: var fluidVertical}
                                                                            || !verticalFillable.CanInflow(world, position + flow.Direction(), flow.EntrySide(), this)
                                                                            || !(currentFillable?.CanOutflow(world, position, flow.ExitSide(), this) ?? true))
        {
            remaining = level;

            return false;
        }

        if (fluidVertical.Fluid == Voxels.Fluids.Instance.None)
        {
            Vector3i position1 = position + flow.Direction();
            world.SetFluid(this.AsInstance(level, isStatic: false), position1);
            world.SetFluid(Voxels.Fluids.Instance.None.AsInstance(), position);

            ScheduleUpdate(world, position + flow.Direction());

            remaining = FluidLevel.None;

            return true;
        }

        if (fluidVertical.Fluid == this)
        {
            if (fluidVertical.Level.IsFull)
            {
                remaining = level;

                return false;
            }

            FluidLevel volume = FluidLevel.Full - fluidVertical.Level;

            if (volume >= level)
            {
                Vector3i position1 = position + flow.Direction();

                world.SetFluid(this.AsInstance(fluidVertical.Level + level, isStatic: false), position1);
                world.SetFluid(Voxels.Fluids.Instance.None.AsInstance(), position);

                remaining = FluidLevel.None;
            }
            else
            {
                Vector3i position1 = position + flow.Direction();

                world.SetFluid(this.AsInstance(isStatic: false), position1);
                world.SetFluid(this.AsInstance(level - volume, isStatic: false), position);

                remaining = level - volume;

                ScheduleUpdate(world, position);
            }

            if (fluidVertical.IsStatic) ScheduleUpdate(world, position + flow.Direction());

            return true;
        }

        if (handleContact)
        {
            remaining = level;

            return Voxels.Fluids.ContactManager.HandleContact(
                world,
                this.AsInstance(level),
                position,
                fluidVertical,
                position + flow.Direction());
        }

        remaining = level;

        return false;
    }

    private Boolean TryPuddleFlow(World world, Vector3i position, Fillable currentFillable)
    {
        Boolean fluidBelowIsNone = world.GetFluid(position + FlowDirection)?.Fluid == Voxels.Fluids.Instance.None;

        foreach (Orientation orientation in Orientations.All)
        {
            if (!currentFillable.CanOutflow(world, position, orientation.ToSide(), this)) continue;

            Vector3i neighborPosition = position.Offset(orientation);

            Content? content = world.GetContent(neighborPosition);

            if (content?.Block.Block.Get<Fillable>() is not {} neighborFillable || content is not {Fluid: var neighborFluid}
                                                                                || !AllowsFlowTrough(neighborFillable, world, neighborPosition, orientation.Opposite().ToSide(), Direction.ExitSide())
                                                                                || neighborFluid.Fluid != Voxels.Fluids.Instance.None
                                                                                || !CheckLowerPosition(neighborPosition + FlowDirection)) continue;

            world.SetFluid(this.AsInstance(FluidLevel.One, isStatic: false), neighborPosition);
            world.SetFluid(Voxels.Fluids.Instance.None.AsInstance(), position);

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
        FluidLevel levelHorizontal = FluidLevel.Eight;

        if (Orientations.ShuffledStart(position)
            .Any(orientation => CheckNeighbor(
                currentFillable.CanOutflow(world, position, orientation.ToSide(), this),
                position.Offset(orientation),
                orientation.Opposite().ToSide()))) return true;

        if (horizontalPosition == position) return false;

        world.SetFluid(this.AsInstance(levelHorizontal + FluidLevel.One, isStatic: false), horizontalPosition);

        if (isHorStatic) ScheduleUpdate(world, horizontalPosition);

        Boolean hasRemaining = level != FluidLevel.One;

        Boolean isStatic1 = !hasRemaining;

        world.SetFluid((hasRemaining ? this : Voxels.Fluids.Instance.None).AsInstance(hasRemaining ? level - FluidLevel.One : FluidLevel.Eight, isStatic1), position);

        if (hasRemaining) ScheduleUpdate(world, position);

        return true;

        Boolean CheckNeighbor(Boolean outflowAllowed, Vector3i neighborPosition, Side side)
        {
            Content? neighborContent = world.GetContent(neighborPosition);

            if (!outflowAllowed ||
                neighborContent?.Block.Block.Get<Fillable>() is not {} neighborFillable || neighborContent is not {Fluid: var neighborFluid} ||
                !neighborFillable.CanInflow(world, neighborPosition, side, this)) return false;

            Boolean isStatic = neighborFluid.IsStatic;

            if (neighborFluid.Fluid == Voxels.Fluids.Instance.None)
            {
                Vector3i belowNeighborPosition = neighborPosition + FlowDirection;

                Content? belowNeighborContent = world.GetContent(belowNeighborPosition);

                if (belowNeighborContent?.Block.Block.Get<Fillable>() is {} belowNeighborFillable && belowNeighborContent is {Fluid: var belowNeighborFluid}
                                                                                                  && belowNeighborFluid.Fluid == Voxels.Fluids.Instance.None
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

                    world.SetFluid(Voxels.Fluids.Instance.None.AsInstance(), position);
                }
                else
                {
                    world.SetFluid(this.AsInstance(FluidLevel.One, isStatic: false), neighborPosition);

                    ScheduleUpdate(world, neighborPosition);

                    Boolean remaining = level != FluidLevel.One;

                    Boolean isStatic2 = !remaining;

                    world.SetFluid((remaining ? this : Voxels.Fluids.Instance.None).AsInstance(remaining ? level - FluidLevel.One : FluidLevel.Eight, isStatic2), position);

                    if (remaining) ScheduleUpdate(world, position);
                }

                return true;
            }

            if (neighborFluid.Fluid != this)
            {
                if (Voxels.Fluids.ContactManager.HandleContact(
                        world,
                        this.AsInstance(level),
                        position,
                        neighborFluid,
                        neighborPosition)) return true;
            }
            else if (neighborFluid.Fluid == this && level > neighborFluid.Level &&
                     neighborFluid.Level < levelHorizontal)
            {
                Boolean neighborHasSignificantlyLowerLevel = neighborFluid.Level != level - FluidLevel.One;

                Boolean neighborHasLessPressure = level.IsFull && !IsAtSurface(world, position) &&
                                                  IsAtSurface(world, neighborPosition);

                Boolean directNeighborAllowsFlow = neighborHasSignificantlyLowerLevel || neighborHasLessPressure;

                Boolean allowsFlow = directNeighborAllowsFlow
                                     || HasNeighborWithLevel(world, level - FluidLevel.Two, neighborPosition)
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
            SearchFlowTarget(world, position, level - FluidLevel.Two, range: 4);

        if (potentialTarget == null) return false;

        var target = ((Vector3i position, FluidInstance fluid, Fillable fillable)) potentialTarget;

        world.SetFluid(this.AsInstance(target.fluid.Level + FluidLevel.One, isStatic: false), target.position);
        if (target.fluid.IsStatic) ScheduleUpdate(world, target.position);

        world.SetFluid(this.AsInstance(level - FluidLevel.One, isStatic: false), position);
        ScheduleUpdate(world, position);

        return true;
    }

    private void SpreadOrDestroyFluid(World world, Vector3i position, FluidLevel level)
    {
        FluidLevel remaining = level;

        foreach (Orientation orientation in Orientations.All)
        {
            FillNeighbor(world, position.Offset(orientation), orientation.ToSide(), ref remaining);

            if (remaining == FluidLevel.None) break;
        }

        world.SetDefaultFluid(position);
    }

    private void FillNeighbor(World world, Vector3i neighborPosition, Side side, ref FluidLevel remaining)
    {
        Content? neighborContent = world.GetContent(neighborPosition);

        if (neighborContent?.Block.Block.Get<Fillable>() is not {} neighborFillable || neighborContent is not {Fluid: var neighborFluid} ||
            !neighborFillable.CanInflow(world, neighborPosition, side.Opposite(), this)) return;

        Boolean isStatic = neighborFluid.IsStatic;

        if (remaining == FluidLevel.None) return;

        if (neighborFluid.Fluid == Voxels.Fluids.Instance.None)
        {
            world.SetFluid(this.AsInstance(remaining, isStatic: false), neighborPosition);

            remaining = FluidLevel.None;

            ScheduleUpdate(world, neighborPosition);
        }
        else if (neighborFluid.Fluid == this)
        {
            FluidLevel volume = FluidLevel.Eight - neighborFluid.Level;

            if (volume >= remaining)
            {
                world.SetFluid(this.AsInstance(neighborFluid.Level + remaining, isStatic: false), neighborPosition);

                remaining = FluidLevel.None;
            }
            else
            {
                world.SetFluid(this.AsInstance(isStatic: false), neighborPosition);

                remaining = remaining - volume;
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
