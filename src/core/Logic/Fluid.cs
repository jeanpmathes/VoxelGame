// <copyright file="Fluid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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
    protected const double AirDensity = 1.2f;

    private const double GasFluidThreshold = 10f;

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
    protected Fluid(string name, string namedId, double density, int viscosity,
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
    public double Density { get; }

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
                new Vector3d(x: 0f, halfHeight, z: 0f),
                new Vector3d(x: 0.5f, halfHeight, z: 0.5f));
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
    /// Create the mesh for this fluid.
    /// </summary>
    /// <param name="position">The position of the fluid.</param>
    /// <param name="info">Info about the fluid.</param>
    /// <param name="context">The context of the meshing operation.</param>
    public void CreateMesh(Vector3i position, FluidMeshInfo info, MeshingContext context)
    {
        if (RenderType == RenderType.NotRendered || (info.Block.Block is not IFillable {RenderFluid: true} &&
                                                     (info.Block.Block is IFillable || info.Block.IsSolidAndFull))) return;

        VaryingHeightMeshFaceHolder[] fluidMeshFaceHolders =
            context.GetFluidMeshFaceHolders(RenderType == RenderType.Opaque);

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

            bool atVerticalEnd = side is BlockSide.Top or BlockSide.Bottom;

            bool isNeighborFluidMeshed =
                blockToCheck.Block is IFillable {RenderFluid: true};

            var sideHeight = (int) fluidToCheck.Level;

            if (fluidToCheck.Fluid != this || !isNeighborFluidMeshed) sideHeight = -1;

            bool flowsTowardsFace = side == BlockSide.Top
                ? Direction == VerticalFlow.Upwards
                : Direction == VerticalFlow.Downwards;

            bool meshAtSide = (int) info.Level > sideHeight && !blockToCheck.IsOpaqueAndFull;

            bool meshAtDrainEnd = sideHeight != 7 && !blockToCheck.IsOpaqueAndFull;
            bool meshAtSourceEnd = info.Level != FluidLevel.Eight || (fluidToCheck.Fluid != this && !blockToCheck.IsOpaqueAndFull);

            bool meshAtEnd = flowsTowardsFace ? meshAtDrainEnd : meshAtSourceEnd;

            if (atVerticalEnd ? !meshAtEnd : !meshAtSide) return;

            FluidMeshData mesh = GetMeshData(info with {Side = side});

            bool singleSided = !blockToCheck.IsOpaqueAndFull &&
                               blockToCheck.IsSolidAndFull;

            (int x, int y, int z) = position;
            side.Corners(out int[] a, out int[] b, out int[] c, out int[] d);

            // int: uv-- ---- ---- ---- -xxx xxey yyyz zzzz (uv: texture coords; xyz: position; e: lower/upper end)
            int upperDataA = (0 << 31) | (0 << 30) | ((x + a[0]) << 10) | (a[1] << 9) | (y << 5) |
                             (z + a[2]);

            int upperDataB = (0 << 31) | (1 << 30) | ((x + b[0]) << 10) | (b[1] << 9) | (y << 5) |
                             (z + b[2]);

            int upperDataC = (1 << 31) | (1 << 30) | ((x + c[0]) << 10) | (c[1] << 9) | (y << 5) |
                             (z + c[2]);

            int upperDataD = (1 << 31) | (0 << 30) | ((x + d[0]) << 10) | (d[1] << 9) | (y << 5) |
                             (z + d[2]);

            // int: tttt tttt t--- -nnn hhhh dlll siii iiii (t: tint; n: normal; h: side height; d: direction; l: level; s: isStatic; i: texture index)
            int lowerData = (mesh.Tint.GetBits(context.GetFluidTint(position)) << 23) | ((int) side << 16) |
                            ((sideHeight + 1) << 12) |
                            (Direction.GetBit() << 11) | ((int) info.Level << 8) |
                            (info.IsStatic ? 1 << 7 : 0 << 7) |
                            ((((mesh.TextureIndex - 1) >> 4) + 1) & 0b0111_1111);

            fluidMeshFaceHolders[(int) side].AddFace(
                position,
                lowerData,
                (upperDataA, upperDataB, upperDataC, upperDataD),
                singleSided,
                info.Level == FluidLevel.Eight);
        }
    }

    /// <summary>
    /// Get the mesh data for this fluid.
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
        Content? content = world.GetContent(position);

        if (content is ({Block: IFillable fillable}, var target)
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
        Content? content = world.GetContent(position);

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
        Content? content = world.GetContent(position);

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

            Content? neighborContent = world.GetContent(neighborPosition);

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

            Content? content = world.GetContent(neighborPosition);

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
