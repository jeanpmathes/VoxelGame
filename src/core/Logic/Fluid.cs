// <copyright file="Fluid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic;

/// <summary>
///     The base class of all fluids.
/// </summary>
public abstract partial class Fluid : IIdentifiable<uint>, IIdentifiable<string>
{
    /// <summary>
    ///     The density of air.
    /// </summary>
    protected const float AirDensity = 1.2f;

    private const float GasFluidThreshold = 10f;

    private static readonly BoundingVolume[] volumes = CreateVolumes();

    /// <summary>
    ///     Create a new fluid.
    /// </summary>
    /// <param name="name">The name of the fluid. Can be localized.</param>
    /// <param name="namedId">The named ID of the fluid. This is a unique and unlocalized identifier.</param>
    /// <param name="density">The density of the fluid. This determines whether this is a gas or a fluid.</param>
    /// <param name="viscosity">The viscosity of the fluid. This determines the flow speed.</param>
    /// <param name="checkContact">Whether entity contact must be checked.</param>
    /// <param name="receiveContact">Whether entity contact should be passed to the fluid.</param>
    /// <param name="renderType">The render type of the fluid.</param>
    protected Fluid(string name, string namedId, float density, int viscosity,
        bool checkContact, bool receiveContact, RenderType renderType)
    {
        Debug.Assert(density > 0);

        Name = name;
        NamedId = namedId;

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

        if (fluidList.Count < FluidLimit)
        {
            fluidList.Add(this);
            namedFluidDictionary.Add(namedId, this);

            Id = (uint) (fluidList.Count - 1);
        }
        else
        {
            Debug.Fail($"Not more than {FluidLimit} fluids are allowed.");
        }
    }

    /// <summary>
    ///     Gets the fluid id which can be any value from 0 to 31.
    /// </summary>
    public uint Id { get; }

    /// <summary>
    ///     Gets the localized name of the fluid.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     An unlocalized string that identifies this fluid.
    /// </summary>
    public string NamedId { get; }

    /// <summary>
    ///     Gets the density of this fluid.
    /// </summary>
    public float Density { get; }

    /// <summary>
    ///     Gets the flowing direction of this fluid.
    /// </summary>
    public VerticalFlow Direction { get; }

    /// <summary>
    ///     Gets the viscosity of this fluid, meaning the tick offset between two updates.
    /// </summary>
    public int Viscosity { get; }

    /// <summary>
    ///     Gets whether entity contacts have to be checked.
    /// </summary>
    public bool CheckContact { get; }

    /// <summary>
    ///     Gets whether this fluid receives entity contacts.
    /// </summary>
    public bool ReceiveContact { get; }

    /// <summary>
    ///     Gets the <see cref="Visuals.RenderType" /> of this fluid.
    /// </summary>
    public RenderType RenderType { get; }

    /// <summary>
    ///     Get whether this fluids is a fluid.
    /// </summary>
    public bool IsFluid { get; }

    /// <summary>
    ///     Get whether this fluid is a gas.
    /// </summary>
    public bool IsGas { get; }

    /// <summary>
    ///     The flow direction of this fluid.
    /// </summary>
    public Vector3i FlowDirection => Direction.Direction();

    string IIdentifiable<string>.Id => NamedId;

    uint IIdentifiable<uint>.Id => Id;

    private static BoundingVolume[] CreateVolumes()
    {
        BoundingVolume CreateVolume(FluidLevel level)
        {
            float halfHeight = ((int) level + 1) * 0.0625f;

            return new BoundingVolume(
                new Vector3(x: 0f, halfHeight, z: 0f),
                new Vector3(x: 0.5f, halfHeight, z: 0.5f));
        }

        var fluidVolumes = new BoundingVolume[8];

        for (var i = 0; i < 8; i++) fluidVolumes[i] = CreateVolume((FluidLevel) i);

        return fluidVolumes;
    }

    /// <summary>
    ///     Called when loading fluids, meant to setup vertex data, indices etc.
    /// </summary>
    /// <param name="indexProvider"></param>
    protected virtual void Setup(ITextureIndexProvider indexProvider) {}

    /// <summary>
    ///     Get the mesh for this fluid.
    /// </summary>
    /// <param name="info">Information about the fluid instance.</param>
    /// <returns>The mesh data.</returns>
    public abstract FluidMeshData GetMesh(FluidMeshInfo info);

    /// <summary>
    ///     Get the collider for fluids.
    /// </summary>
    /// <param name="position">The position of the fluid.</param>
    /// <param name="level">The level of the fluid.</param>
    /// <returns>The collider.</returns>
    public static BoxCollider GetCollider(Vector3i position, FluidLevel level)
    {
        return volumes[(int) level].GetColliderAt(position);
    }

    /// <summary>
    ///     Notify this fluid that an entity has come in contact with it.
    /// </summary>
    /// <param name="entity">The entity that contacts the fluid.</param>
    /// <param name="position">The position of the fluid.</param>
    public void EntityContact(PhysicsEntity entity, Vector3i position)
    {
        FluidInstance? potentialFluid = entity.World.GetFluid(position);

        if (potentialFluid is {} fluid && fluid.Fluid == this)
            EntityContact(entity, position, fluid.Level, fluid.IsStatic);
    }

    /// <summary>
    ///     Override to provide custom contact handling.
    /// </summary>
    protected virtual void EntityContact(PhysicsEntity entity, Vector3i position, FluidLevel level,
        bool isStatic) {}

    /// <summary>
    ///     Tries to fill a position with the specified amount of fluid. The remaining fluid is specified, it can be
    ///     converted to <see cref="FluidLevel" /> if it is not <c>-1</c>.
    /// </summary>
    public bool Fill(World world, Vector3i position, FluidLevel level, BlockSide entrySide, out int remaining)
    {
        (BlockInstance, FluidInstance)? content = world.GetContent(position);

        if (content is ({ Block: IFillable fillable }, var target)
            && fillable.AllowInflow(world, position, entrySide, this))
        {
            if (target.Fluid == this && target.Level != FluidLevel.Eight)
            {
                int filled = (int) target.Level + (int) level + 1;
                filled = filled > 7 ? 7 : filled;

                SetFluid(world, this, (FluidLevel) filled, isStatic: false, fillable, position);
                if (target.IsStatic) ScheduleTick(world, position);

                remaining = (int) level - (filled - (int) target.Level);

                return true;
            }

            if (target.Fluid == None)
            {
                SetFluid(world, this, level, isStatic: false, fillable, position);
                ScheduleTick(world, position);

                remaining = -1;

                return true;
            }
        }

        remaining = (int) level;

        return false;
    }

    /// <summary>
    ///     Tries to take a certain amount of fluid from a position. The actually taken amount is given when finished.
    /// </summary>
    public bool Take(World world, Vector3i position, ref FluidLevel level)
    {
        (BlockInstance, FluidInstance)? content = world.GetContent(position);

        if (content is not var (block, fluid) || fluid.Fluid != this || this == None) return false;

        if (level >= fluid.Level)
        {
            SetFluid(world, None, FluidLevel.Eight, isStatic: true, block.Block as IFillable, position);
        }
        else
        {
            SetFluid(
                world,
                this,
                (FluidLevel) ((int) fluid.Level - (int) level - 1),
                isStatic: false,
                block.Block as IFillable,
                position);

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
    public bool TryTakeExact(World world, Vector3i position, FluidLevel level)
    {
        (BlockInstance, FluidInstance)? content = world.GetContent(position);

        if (content is not var (block, fluid) || fluid.Fluid != this || this == None ||
            level > fluid.Level) return false;

        if (level == fluid.Level)
        {
            SetFluid(world, None, FluidLevel.Eight, isStatic: true, block.Block as IFillable, position);
        }
        else
        {
            SetFluid(
                world,
                this,
                (FluidLevel) ((int) fluid.Level - (int) level - 1),
                isStatic: false,
                block.Block as IFillable,
                position);

            if (fluid.IsStatic) ScheduleTick(world, position);
        }

        return true;

    }

    /// <summary>
    ///     Override for scheduled update handling.
    /// </summary>
    protected abstract void ScheduledUpdate(World world, Vector3i position, FluidLevel level, bool isStatic);

    /// <summary>
    ///     Sets the fluid at the position and calls the necessary methods on the <see cref="IFillable" />.
    /// </summary>
    protected static void SetFluid(World world, Fluid fluid, FluidLevel level, bool isStatic,
        IFillable? fillable, Vector3i position)
    {
        world.SetFluid(fluid.AsInstance(level, isStatic), position);
        fillable?.FluidChange(world, position, fluid, level);
    }

    /// <summary>
    ///     Check if a fluid has a neighbor of the same fluid and this neighbor has a specified level. If the specified level
    ///     is <c>-1</c>, false is directly returned.
    /// </summary>
    protected bool HasNeighborWithLevel(World world, FluidLevel level, Vector3i position)
    {
        if ((int) level == -1) return false;

        if (world.GetBlock(position)?.Block is not IFillable currentFillable) return false;

        foreach (Orientation orientation in Orientations.All)
        {
            Vector3i neighborPosition = orientation.Offset(position);

            (BlockInstance, FluidInstance)? neighborContent = world.GetContent(neighborPosition);

            if (neighborContent is not var (neighborBlock, neighborFluid)) continue;

            bool isNeighborThisFluid = neighborFluid.Fluid == this && neighborFluid.Level == level;

            if (!isNeighborThisFluid) continue;

            if (neighborBlock.Block is IFillable neighborFillable
                && neighborFillable.AllowInflow(world, neighborPosition, orientation.Opposite().ToBlockSide(), this)
                && currentFillable.AllowOutflow(world, position, orientation.ToBlockSide())) return true;
        }

        return false;
    }

    /// <summary>
    ///     Check if a position has a neighboring position with no fluid.
    /// </summary>
    /// <param name="world">The world.</param>
    /// <param name="position">The position to check.</param>
    /// <returns>True if there is a neighboring position.</returns>
    protected bool HasNeighborWithEmpty(World world, Vector3i position)
    {
        if (world.GetBlock(position)?.Block is not IFillable currentFillable) return false;

        foreach (Orientation orientation in Orientations.All)
        {
            Vector3i neighborPosition = orientation.Offset(position);

            (BlockInstance, FluidInstance)? content = world.GetContent(neighborPosition);

            if (content is not var (neighborBlock, neighborFluid)) continue;

            if (neighborFluid.Fluid == None && neighborBlock.Block is IFillable neighborFillable
                                            && neighborFillable.AllowInflow(
                                                world,
                                                neighborPosition,
                                                orientation.Opposite().ToBlockSide(),
                                                this)
                                            && currentFillable.AllowOutflow(
                                                world,
                                                position,
                                                orientation.ToBlockSide()))
                return true;
        }

        return false;
    }

    /// <summary>
    ///     Search a flow target for a fluid.
    /// </summary>
    /// <param name="world">The world.</param>
    /// <param name="position">The current fluid position.</param>
    /// <param name="maximumLevel">The maximum level of a potential position.</param>
    /// <param name="range">The search range.</param>
    /// <returns>A potential target, if there is any.</returns>
    protected (Vector3i position, FluidInstance fluid, IFillable fillable)? SearchFlowTarget(
        World world, Vector3i position, FluidLevel maximumLevel, int range)
    {
        int extendedRange = range + 1;
        int extents = extendedRange * 2 + 1;
        Vector3i center = (extendedRange, 0, extendedRange);

            #pragma warning disable CA1814
        var mark = new bool[extents, extents];
            #pragma warning restore CA1814

        Queue<(Vector3i position, IFillable fillable)> queue = new();
        Queue<(Vector3i position, IFillable fillable)> nextQueue = new();

        (BlockInstance, FluidInstance)? startContent = world.GetContent(position);

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

                (BlockInstance, FluidInstance)? nextContent = world.GetContent(nextPosition);

                if (nextContent is not var (nextBlock, nextFluid)) continue;
                if (nextBlock.Block is not IFillable nextFillable || nextFluid.Fluid != this) continue;

                bool canFlow = e.fillable.AllowOutflow(world, e.position, orientation.ToBlockSide()) &&
                               nextFillable.AllowInflow(
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
            (int x, _, int z) = center + centerOffset;

            mark[x, z] = true;
        }

        bool IsMarked(Vector3i positionToCheck)
        {
            Vector3i centerOffset = positionToCheck - position;
            (int x, _, int z) = center + centerOffset;

            return mark[x, z];
        }
    }

    /// <summary>
    ///     Check if a fluid at a given position is at the surface.
    /// </summary>
    /// <param name="world">The world.</param>
    /// <param name="position">The position of the fluid.</param>
    /// <returns>True if the fluid is at the surface.</returns>
    protected bool IsAtSurface(World world, Vector3i position)
    {
        return world.GetFluid(position - FlowDirection)?.Fluid != this;
    }

    internal virtual void RandomUpdate(World world, Vector3i position, FluidLevel level, bool isStatic) {}

    /// <inheritdoc />
    public sealed override string ToString()
    {
        return NamedId;
    }
}
