// <copyright file="Fluid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Elements;

/// <summary>
///     The base class of all fluids.
/// </summary>
public abstract partial class Fluid : IIdentifiable<UInt32>, IIdentifiable<String>
{
    /// <summary>
    ///     The density of air.
    /// </summary>
    protected const Double AirDensity = 1.2f;

    private const Double GasFluidThreshold = 10f;

    private const UInt32 InvalidID = UInt32.MaxValue;

    private static readonly BoundingVolume[] volumes = CreateVolumes();

    /// <summary>
    ///     Create a new fluid.
    /// </summary>
    /// <param name="name">The name of the fluid. Can be localized.</param>
    /// <param name="namedID">The named ID of the fluid. This is a unique and unlocalized identifier.</param>
    /// <param name="density">The density of the fluid. This determines whether this is a gas or a fluid.</param>
    /// <param name="viscosity">The viscosity of the fluid. This determines the flow speed.</param>
    /// <param name="checkContact">Whether actor contact must be checked.</param>
    /// <param name="receiveContact">Whether actor contact should be passed to the fluid.</param>
    /// <param name="renderType">The render type of the fluid.</param>
    protected Fluid(String name, String namedID, Double density, Int32 viscosity,
        Boolean checkContact, Boolean receiveContact, RenderType renderType)
    {
        Debug.Assert(density > 0);

        Name = name;
        NamedID = namedID;

        Density = density;

        Direction = (density - AirDensity) switch
        {
            > +0.001f => VerticalFlow.Downwards,
            < -0.001f => VerticalFlow.Upwards,
            _ => VerticalFlow.Static
        };

        if (Direction == VerticalFlow.Static)
        {
            IsFluid = false;
            IsGas = false;
        }
        else
        {
            IsFluid = density > GasFluidThreshold;
            IsGas = density <= GasFluidThreshold;
        }

        Viscosity = viscosity;

        CheckContact = checkContact;
        ReceiveContact = receiveContact;

        RenderType = renderType;
    }

    /// <summary>
    ///     Gets the fluid id which can be any value from 0 to 31.
    ///     This value will be initialized after all fluids have been registered, and is therefore not set in the constructor.
    /// </summary>
    public UInt32 ID { get; private set; } = InvalidID;

    /// <summary>
    ///     Gets the localized name of the fluid.
    /// </summary>
    public String Name { get; }

    /// <summary>
    ///     An unlocalized string that identifies this fluid.
    /// </summary>
    public String NamedID { get; }

    /// <summary>
    ///     Gets the density of this fluid.
    /// </summary>
    public Double Density { get; }

    /// <summary>
    ///     Gets the flowing direction of this fluid.
    /// </summary>
    public VerticalFlow Direction { get; }

    /// <summary>
    ///     Gets the viscosity of this fluid, meaning the tick offset between two updates.
    /// </summary>
    public Int32 Viscosity { get; }

    /// <summary>
    ///     Gets whether actor contacts have to be checked.
    /// </summary>
    public Boolean CheckContact { get; }

    /// <summary>
    ///     Gets whether this fluid receives actor contacts.
    /// </summary>
    public Boolean ReceiveContact { get; }

    /// <summary>
    ///     Gets the <see cref="Visuals.RenderType" /> of this fluid.
    /// </summary>
    private RenderType RenderType { get; }

    /// <summary>
    ///     Get whether this fluids is a fluid.
    /// </summary>
    public Boolean IsFluid { get; }

    /// <summary>
    ///     Get whether this fluid is a gas.
    /// </summary>
    public Boolean IsGas { get; }

    /// <summary>
    ///     The flow direction of this fluid.
    /// </summary>
    public Vector3i FlowDirection => Direction.Direction();

    String IIdentifiable<String>.ID => NamedID;

    UInt32 IIdentifiable<UInt32>.ID => ID;

    private static BoundingVolume[] CreateVolumes()
    {
        BoundingVolume CreateVolume(FluidLevel level)
        {
            Single halfHeight = ((Int32) level + 1) * 0.0625f;

            return new BoundingVolume(
                new Vector3d(x: 0.5f, halfHeight, z: 0.5f),
                new Vector3d(x: 0.5f, halfHeight, z: 0.5f));
        }

        var fluidVolumes = new BoundingVolume[8];

        for (var i = 0; i < 8; i++) fluidVolumes[i] = CreateVolume((FluidLevel) i);

        return fluidVolumes;
    }

    /// <summary>
    ///     Called when loading fluids, meant to setup vertex data, indices etc.
    /// </summary>
    /// <param name="id">The id of the fluid.</param>
    /// <param name="indexProvider">A provider for texture indices.</param>
    /// <param name="dominantColorProvider">A provider for dominant colors.</param>
    public void SetUp(UInt32 id, ITextureIndexProvider indexProvider, IDominantColorProvider dominantColorProvider)
    {
        Debug.Assert(ID == InvalidID);
        ID = id;

        OnSetUp(indexProvider, dominantColorProvider);
    }

    /// <summary>
    ///     Called on fluid setup, after the ID has been set.
    /// </summary>
    /// <param name="indexProvider">A provider for texture indices.</param>
    /// <param name="dominantColorProvider">A provider for dominant colors.</param>
    protected virtual void OnSetUp(ITextureIndexProvider indexProvider, IDominantColorProvider dominantColorProvider) {}

    /// <summary>
    ///     Get the main color of this fluid, e.g. for fog.
    ///     Can be null to disable all usage of this color.
    /// </summary>
    /// <param name="tint">The current fluid tint.</param>
    /// <returns>The color.</returns>
    public virtual Color4? GetColor(TintColor tint)
    {
        return null;
    }

    /// <summary>
    ///     Create the mesh for this fluid.
    /// </summary>
    /// <param name="position">The position of the fluid.</param>
    /// <param name="info">Info about the fluid.</param>
    /// <param name="context">The context of the meshing operation.</param>
    public void CreateMesh(Vector3i position, FluidMeshInfo info, MeshingContext context)
    {
        if (RenderType == RenderType.NotRendered || (info.Block.Block is not IFillable {IsFluidRendered: true} &&
                                                     (info.Block.Block is IFillable || info.Block.IsSolidAndFull))) return;

        Sides<MeshFaceHolder> fluidMeshFaceHolders = context.GetFluidMeshFaceHolders();

        MeshFluidSide(BlockSide.Front);
        MeshFluidSide(BlockSide.Back);
        MeshFluidSide(BlockSide.Left);
        MeshFluidSide(BlockSide.Right);
        MeshFluidSide(BlockSide.Bottom);
        MeshFluidSide(BlockSide.Top);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void MeshFluidSide(BlockSide side)
        {
            (BlockInstance, FluidInstance)? content = context.GetBlockAndFluid(side.Offset(position), side);

            if (content == null) return;

            (BlockInstance blockToCheck, FluidInstance fluidToCheck) = content.Value;

            Boolean atVerticalEnd = side is BlockSide.Top or BlockSide.Bottom;

            Boolean isNeighborFluidMeshed =
                blockToCheck.Block is IFillable {IsFluidRendered: true};

            var sideHeight = (Int32) fluidToCheck.Level;

            if (fluidToCheck.Fluid != this || !isNeighborFluidMeshed) sideHeight = FluidLevels.None;

            Boolean flowsTowardsFace = side == BlockSide.Top
                ? Direction == VerticalFlow.Upwards
                : Direction == VerticalFlow.Downwards;

            Boolean meshAtSide = (Int32) info.Level > sideHeight && !blockToCheck.IsOpaqueAndFull;

            Boolean meshAtDrainEnd = sideHeight != 7 && !blockToCheck.IsOpaqueAndFull;
            Boolean meshAtSourceEnd = info.Level != FluidLevel.Eight || (fluidToCheck.Fluid != this && !blockToCheck.IsOpaqueAndFull);

            Boolean meshAtEnd = flowsTowardsFace ? meshAtDrainEnd : meshAtSourceEnd;

            if (atVerticalEnd ? !meshAtEnd : !meshAtSide) return;

            FluidMeshData mesh = GetMeshData(info with {Side = side});

            Boolean singleSided = blockToCheck is {IsSolidAndFull: true, IsOpaqueAndFull: true};

            (UInt32 a, UInt32 b, UInt32 c, UInt32 d) data = (0, 0, 0, 0);

            Meshing.SetTextureIndex(ref data, mesh.TextureIndex);
            Meshing.SetTint(ref data, mesh.Tint.Select(context.GetFluidTint(position)));

            if (side is not (BlockSide.Top or BlockSide.Bottom))
            {
                (Vector2 min, Vector2 max) uvs = info.Level.GetUVs(sideHeight, Direction);
                Meshing.SetUVs(ref data, uvs.min, (uvs.min.X, uvs.max.Y), uvs.max, (uvs.max.X, uvs.min.Y));
            }
            else
            {
                Meshing.SetFullUVs(ref data);
            }

            Meshing.SetFlag(ref data, Meshing.QuadFlag.IsAnimated, value: true);
            Meshing.SetFlag(ref data, Meshing.QuadFlag.IsUnshaded, value: false);

            fluidMeshFaceHolders[side].AddFace(
                position,
                info.Level.GetBlockHeight(),
                IHeightVariable.GetBlockHeightFromFluidHeight(sideHeight),
                Direction != VerticalFlow.Upwards,
                data,
                singleSided,
                info.Level == FluidLevel.Eight);
        }
    }

    /// <summary>
    ///     Get the mesh data for this fluid.
    /// </summary>
    protected abstract FluidMeshData GetMeshData(FluidMeshInfo info);

    /// <summary>
    ///     Get the collider for fluids.
    /// </summary>
    /// <param name="position">The position of the fluid.</param>
    /// <param name="level">The level of the fluid.</param>
    /// <returns>The collider.</returns>
    public static BoxCollider GetCollider(Vector3i position, FluidLevel level)
    {
        return volumes[(Int32) level].GetColliderAt(position);
    }

    /// <summary>
    ///     Notify this fluid that an actor has come in contact with it.
    /// </summary>
    /// <param name="actor">The actor that contacts the fluid.</param>
    /// <param name="position">The position of the fluid.</param>
    public void EntityContact(PhysicsActor actor, Vector3i position)
    {
        FluidInstance? potentialFluid = actor.World.GetFluid(position);

        if (potentialFluid is {} fluid && fluid.Fluid == this)
            EntityContact(actor, position, fluid.Level, fluid.IsStatic);
    }

    /// <summary>
    ///     Override to provide custom contact handling.
    /// </summary>
    protected virtual void EntityContact(PhysicsActor actor, Vector3i position, FluidLevel level,
        Boolean isStatic) {}

    /// <summary>
    ///     Tries to fill a position with the specified amount of fluid. The remaining fluid is specified, it can be
    ///     converted to <see cref="FluidLevel" /> if it is not <c>-1</c>.
    /// </summary>
    public Boolean Fill(World world, Vector3i position, FluidLevel level, BlockSide entrySide, out Int32 remaining)
    {
        Content? content = world.GetContent(position);

        if (content is ({Block: IFillable fillable}, var target)
            && fillable.IsInflowAllowed(world, position, entrySide, this))
        {
            if (target.Fluid == this && target.Level != FluidLevel.Eight)
            {
                Int32 filled = (Int32) target.Level + (Int32) level + 1;
                filled = filled > 7 ? 7 : filled;

                world.SetFluid(this.AsInstance((FluidLevel) filled, isStatic: false), position);
                if (target.IsStatic) ScheduleTick(world, position);

                remaining = (Int32) level - (filled - (Int32) target.Level);

                return true;
            }

            if (target.Fluid == Fluids.Instance.None)
            {
                world.SetFluid(this.AsInstance(level, isStatic: false), position);
                ScheduleTick(world, position);

                remaining = -1;

                return true;
            }
        }

        remaining = (Int32) level;

        return false;
    }

    /// <summary>
    ///     Tries to take a certain amount of fluid from a position. The actually taken amount is given when finished.
    /// </summary>
    public Boolean Take(World world, Vector3i position, ref FluidLevel level)
    {
        Content? content = world.GetContent(position);

        if (content is not var (_, fluid) || fluid.Fluid != this || this == Fluids.Instance.None) return false;

        if (level >= fluid.Level)
        {
            world.SetFluid(Fluids.Instance.None.AsInstance(), position);
        }
        else
        {
            world.SetFluid(this.AsInstance((FluidLevel) ((Int32) fluid.Level - (Int32) level - 1), isStatic: false), position);

            if (fluid.IsStatic) ScheduleTick(world, position);
        }

        return true;
    }

    /// <summary>
    ///     Try to take an exact amount of fluid.
    /// </summary>
    /// <param name="world">The world.</param>
    /// <param name="position">The fluid position.</param>
    /// <param name="level">The amount of fluid to take.</param>
    /// <returns>True if taking the fluid was successful.</returns>
    public Boolean TryTakeExact(World world, Vector3i position, FluidLevel level)
    {
        Content? content = world.GetContent(position);

        if (content is not var (_, fluid) || fluid.Fluid != this || this == Fluids.Instance.None ||
            level > fluid.Level) return false;

        if (level == fluid.Level)
        {
            world.SetFluid(Fluids.Instance.None.AsInstance(), position);
        }
        else
        {
            world.SetFluid(this.AsInstance((FluidLevel) ((Int32) fluid.Level - (Int32) level - 1), isStatic: false), position);

            if (fluid.IsStatic) ScheduleTick(world, position);
        }

        return true;

    }

    /// <summary>
    ///     Override for scheduled update handling.
    /// </summary>
    protected abstract void ScheduledUpdate(World world, Vector3i position, FluidInstance instance);

    /// <summary>
    ///     Check if a fluid has a neighbor of the same fluid and this neighbor has a specified level. If the specified level
    ///     is <c>-1</c>, false is directly returned.
    /// </summary>
    protected Boolean HasNeighborWithLevel(World world, FluidLevel level, Vector3i position)
    {
        if ((Int32) level == -1) return false;

        if (world.GetBlock(position)?.Block is not IFillable currentFillable) return false;

        foreach (Orientation orientation in Orientations.All)
        {
            Vector3i neighborPosition = orientation.Offset(position);

            Content? neighborContent = world.GetContent(neighborPosition);

            if (neighborContent is not var (neighborBlock, neighborFluid)) continue;

            Boolean isNeighborThisFluid = neighborFluid.Fluid == this && neighborFluid.Level == level;

            if (!isNeighborThisFluid) continue;

            if (neighborBlock.Block is IFillable neighborFillable
                && neighborFillable.IsInflowAllowed(world, neighborPosition, orientation.Opposite().ToBlockSide(), this)
                && currentFillable.IsOutflowAllowed(world, position, orientation.ToBlockSide())) return true;
        }

        return false;
    }

    /// <summary>
    ///     Check if a position has a neighboring position with no fluid.
    /// </summary>
    /// <param name="world">The world.</param>
    /// <param name="position">The position to check.</param>
    /// <returns>True if there is a neighboring position.</returns>
    protected Boolean HasNeighborWithEmpty(World world, Vector3i position)
    {
        if (world.GetBlock(position)?.Block is not IFillable currentFillable) return false;

        foreach (Orientation orientation in Orientations.All)
        {
            Vector3i neighborPosition = orientation.Offset(position);

            Content? content = world.GetContent(neighborPosition);

            if (content is not var (neighborBlock, neighborFluid)) continue;

            if (neighborFluid.IsEmpty && neighborBlock.Block is IFillable neighborFillable
                                      && neighborFillable.IsInflowAllowed(
                                          world,
                                          neighborPosition,
                                          orientation.Opposite().ToBlockSide(),
                                          this)
                                      && currentFillable.IsOutflowAllowed(
                                          world,
                                          position,
                                          orientation.ToBlockSide()))
                return true;
        }

        return false;
    }

    /// <summary>
    ///     Search a flow target for a fluid.
    ///     Performs a breadth-first search to find a potential target for the fluid.
    /// </summary>
    /// <param name="world">The world.</param>
    /// <param name="position">The current fluid position.</param>
    /// <param name="maximumLevel">The maximum level of a potential position.</param>
    /// <param name="range">The search range.</param>
    /// <returns>A potential target, if there is any.</returns>
    protected (Vector3i position, FluidInstance fluid, IFillable fillable)? SearchFlowTarget(
        World world, Vector3i position, FluidLevel maximumLevel, Int32 range)
    {
        Int32 extendedRange = range + 1;
        Int32 extents = extendedRange * 2 + 1;
        Vector3i center = (extendedRange, 0, extendedRange);

#pragma warning disable CA1814
        var mark = new Boolean[extents, extents];
#pragma warning restore CA1814

        Queue<(Vector3i position, IFillable fillable)> queue = new();
        Queue<(Vector3i position, IFillable fillable)> nextQueue = new();

        Content? startContent = world.GetContent(position);

        if (startContent is not var (startBlock, startFluid)) return null;
        if (startBlock.Block is not IFillable startFillable || startFluid.Fluid != this) return null;

        queue.Enqueue((position, startFillable));
        Mark(position);

        for (var depth = 0; queue.Count > 0 && depth <= range; depth++)
        {
            foreach ((Vector3i position, IFillable fillable) e in queue)
            foreach (Orientation orientation in Orientations.All)
            {
                Vector3i nextPosition = orientation.Offset(e.position);

                if (IsMarked(nextPosition)) continue;

                Content? nextContent = world.GetContent(nextPosition);

                if (nextContent is not var (nextBlock, nextFluid)) continue;
                if (nextBlock.Block is not IFillable nextFillable || nextFluid.Fluid != this) continue;

                Boolean canFlow = e.fillable.IsOutflowAllowed(world, e.position, orientation.ToBlockSide()) &&
                                  nextFillable.IsInflowAllowed(
                                      world,
                                      nextPosition,
                                      orientation.Opposite().ToBlockSide(),
                                      this);

                if (!canFlow) continue;

                if (nextFluid.Level <= maximumLevel) return (nextPosition, nextFluid, nextFillable);

                Mark(nextPosition);
                nextQueue.Enqueue((nextPosition, nextFillable));
            }

            (queue, nextQueue) = (nextQueue, new Queue<(Vector3i position, IFillable fillable)>());
        }

        return null;

        void Mark(Vector3i positionToMark)
        {
            Vector3i centerOffset = positionToMark - position;
            (Int32 x, _, Int32 z) = center + centerOffset;

            mark[x, z] = true;
        }

        Boolean IsMarked(Vector3i positionToCheck)
        {
            Vector3i centerOffset = positionToCheck - position;
            (Int32 x, _, Int32 z) = center + centerOffset;

            return mark[x, z];
        }
    }

    /// <summary>
    ///     Elevate a fluid. This tries to move the fluid up, to the next suitable position.
    /// </summary>
    /// <param name="world">The world.</param>
    /// <param name="position">The position of the fluid.</param>
    /// <param name="pumpDistance">The maximum amount of elevation.</param>
    public static void Elevate(World world, Vector3i position, Int32 pumpDistance)
    {
        Content? content = world.GetContent(position);

        if (content is not var (start, toElevate)) return;
        if (toElevate.Fluid == Fluids.Instance.None || toElevate.Fluid.IsGas) return;

        var currentLevel = (Int32) toElevate.Level;

        if (start.Block is not IFillable startFillable ||
            !startFillable.IsOutflowAllowed(world, position, BlockSide.Top)) return;

        for (var offset = 1; offset <= pumpDistance && currentLevel > -1; offset++)
        {
            Vector3i elevatedPosition = position + (0, offset, 0);

            var currentBlock = world.GetBlock(elevatedPosition)?.Block as IFillable;

            if (currentBlock == null) break;

            toElevate.Fluid.Fill(
                world,
                elevatedPosition,
                (FluidLevel) currentLevel,
                BlockSide.Bottom,
                out currentLevel);

            if (!currentBlock.IsOutflowAllowed(world, elevatedPosition, BlockSide.Top)) break;
        }

        FluidLevel elevated = toElevate.Level - (currentLevel + 1);
        toElevate.Fluid.Take(world, position, ref elevated);
    }

    /// <summary>
    ///     Check if a fluid at a given position is at the surface.
    /// </summary>
    /// <param name="world">The world.</param>
    /// <param name="position">The position of the fluid.</param>
    /// <returns>True if the fluid is at the surface.</returns>
    protected Boolean IsAtSurface(World world, Vector3i position)
    {
        return world.GetFluid(position - FlowDirection)?.Fluid != this;
    }

    internal virtual void RandomUpdate(World world, Vector3i position, FluidLevel level, Boolean isStatic) {}

    /// <inheritdoc />
    public sealed override String ToString()
    {
        return NamedID;
    }
}
