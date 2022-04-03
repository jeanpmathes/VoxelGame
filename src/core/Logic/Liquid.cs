// <copyright file="Liquid.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Logic
{
    /// <summary>
    ///     The base class of all liquids.
    /// </summary>
    public abstract partial class Liquid : IIdentifiable<uint>, IIdentifiable<string>
    {
        /// <summary>
        ///     The density of air.
        /// </summary>
        protected const float AirDensity = 1.2f;

        private const float GasLiquidThreshold = 10f;

        private static readonly BoundingVolume[] volumes;

        static Liquid()
        {
            BoundingVolume CreateVolume(LiquidLevel level)
            {
                float halfHeight = ((int) level + 1) * 0.0625f;

                return new BoundingVolume(
                    new Vector3(x: 0f, halfHeight, z: 0f),
                    new Vector3(x: 0.5f, halfHeight, z: 0.5f));
            }

            volumes = new BoundingVolume[8];

            for (var i = 0; i < 8; i++) volumes[i] = CreateVolume((LiquidLevel) i);
        }

        /// <summary>
        ///     Create a new liquid.
        /// </summary>
        /// <param name="name">The name of the liquid. Can be localized.</param>
        /// <param name="namedId">The named ID of the liquid. This is a unique and unlocalized identifier.</param>
        /// <param name="density">The density of the fluid. This determines whether this is a gas or a liquid.</param>
        /// <param name="viscosity">The viscosity of the fluid. This determines the flow speed.</param>
        /// <param name="checkContact">Whether entity contact must be checked.</param>
        /// <param name="receiveContact">Whether entity contact should be passed to the liquid.</param>
        /// <param name="renderType">The render type of the liquid.</param>
        protected Liquid(string name, string namedId, float density, int viscosity,
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
                IsLiquid = false;
                IsGas = false;
            }
            else
            {
                IsLiquid = density > GasLiquidThreshold;
                IsGas = density <= GasLiquidThreshold;
            }

            Viscosity = viscosity;

            CheckContact = checkContact;
            ReceiveContact = receiveContact;

            RenderType = renderType;

            if (liquidList.Count < LiquidLimit)
            {
                liquidList.Add(this);
                namedLiquidDictionary.Add(namedId, this);

                Id = (uint) (liquidList.Count - 1);
            }
            else
            {
                Debug.Fail($"Not more than {LiquidLimit} liquids are allowed.");
            }
        }

        /// <summary>
        ///     Gets the liquid id which can be any value from 0 to 31.
        /// </summary>
        public uint Id { get; }

        /// <summary>
        ///     Gets the localized name of the liquid.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     An unlocalized string that identifies this liquid.
        /// </summary>
        public string NamedId { get; }

        /// <summary>
        ///     Gets the density of this liquid.
        /// </summary>
        public float Density { get; }

        /// <summary>
        ///     Gets the flowing direction of this liquid.
        /// </summary>
        public VerticalFlow Direction { get; }

        /// <summary>
        ///     Gets the viscosity of this liquid, meaning the tick offset between two updates.
        /// </summary>
        public int Viscosity { get; }

        /// <summary>
        ///     Gets whether entity contacts have to be checked.
        /// </summary>
        public bool CheckContact { get; }

        /// <summary>
        ///     Gets whether this liquid receives entity contacts.
        /// </summary>
        public bool ReceiveContact { get; }

        /// <summary>
        ///     Gets the <see cref="Visuals.RenderType" /> of this liquid.
        /// </summary>
        public RenderType RenderType { get; }

        /// <summary>
        ///     Get whether this fluids is a liquid.
        /// </summary>
        public bool IsLiquid { get; }

        /// <summary>
        ///     Get whether this fluid is a gas.
        /// </summary>
        public bool IsGas { get; }

        /// <summary>
        ///     The flow direction of this liquid.
        /// </summary>
        public Vector3i FlowDirection => Direction.Direction();

        string IIdentifiable<string>.Id => NamedId;

        uint IIdentifiable<uint>.Id => Id;

        /// <summary>
        ///     Called when loading liquids, meant to setup vertex data, indices etc.
        /// </summary>
        /// <param name="indexProvider"></param>
        protected virtual void Setup(ITextureIndexProvider indexProvider) {}

        /// <summary>
        ///     Get the mesh for this liquid.
        /// </summary>
        /// <param name="info">Information about the liquid instance.</param>
        /// <returns>The mesh data.</returns>
        public abstract LiquidMeshData GetMesh(LiquidMeshInfo info);

        /// <summary>
        ///     Get the collider for liquids.
        /// </summary>
        /// <param name="position">The position of the liquid.</param>
        /// <param name="level">The level of the liquid.</param>
        /// <returns>The collider.</returns>
        public static BoxCollider GetCollider(Vector3i position, LiquidLevel level)
        {
            return volumes[(int) level].GetColliderAt(position);
        }

        /// <summary>
        ///     Notify this liquid that an entity has come in contact with it.
        /// </summary>
        /// <param name="entity">The entity that contacts the liquid.</param>
        /// <param name="position">The position of the liquid.</param>
        public void EntityContact(PhysicsEntity entity, Vector3i position)
        {
            LiquidInstance? liquid = entity.World.GetLiquid(position);

            if (liquid?.Liquid == this)
                EntityContact(entity, position, liquid.Level, liquid.IsStatic);
        }

        /// <summary>
        ///     Override to provide custom contact handling.
        /// </summary>
        protected virtual void EntityContact(PhysicsEntity entity, Vector3i position, LiquidLevel level,
            bool isStatic) {}

        /// <summary>
        ///     Tries to fill a position with the specified amount of liquid. The remaining liquid is specified, it can be
        ///     converted to <see cref="LiquidLevel" /> if it is not <c>-1</c>.
        /// </summary>
        public bool Fill(World world, Vector3i position, LiquidLevel level, BlockSide entrySide, out int remaining)
        {
            (BlockInstance? block, LiquidInstance? target) = world.GetContent(position);

            if (block?.Block is IFillable fillable && fillable.AllowInflow(world, position, entrySide, this))
            {
                if (target?.Liquid == this && target.Level != LiquidLevel.Eight)
                {
                    int filled = (int) target.Level + (int) level + 1;
                    filled = filled > 7 ? 7 : filled;

                    SetLiquid(world, this, (LiquidLevel) filled, isStatic: false, fillable, position);
                    if (target.IsStatic) ScheduleTick(world, position);

                    remaining = (int) level - (filled - (int) target.Level);

                    return true;
                }

                if (target?.Liquid == None)
                {
                    SetLiquid(world, this, level, isStatic: false, fillable, position);
                    ScheduleTick(world, position);

                    remaining = -1;

                    return true;
                }
            }

            remaining = (int) level;

            return false;
        }

        /// <summary>
        ///     Tries to take a certain amount of liquid from a position. The actually taken amount is given when finished.
        /// </summary>
        public bool Take(World world, Vector3i position, ref LiquidLevel level)
        {
            (BlockInstance? block, LiquidInstance? liquid) = world.GetContent(position);

            if (liquid?.Liquid != this || this == None) return false;

            if (level >= liquid.Level)
            {
                SetLiquid(world, None, LiquidLevel.Eight, isStatic: true, block?.Block as IFillable, position);
            }
            else
            {
                SetLiquid(
                    world,
                    this,
                    (LiquidLevel) ((int) liquid.Level - (int) level - 1),
                    isStatic: false,
                    block?.Block as IFillable,
                    position);

                if (liquid.IsStatic) ScheduleTick(world, position);
            }

            return true;
        }

        /// <summary>
        ///     Try to take an exact amount of liquid.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="position">The liquid position.</param>
        /// <param name="level">The amount of liquid to take.</param>
        /// <returns>True if taking the liquid was successful.</returns>
        public bool TryTakeExact(World world, Vector3i position, LiquidLevel level)
        {
            (BlockInstance? block, LiquidInstance? liquid) = world.GetContent(position);

            if (liquid?.Liquid == this && this != None && level <= liquid.Level)
            {
                if (level == liquid.Level)
                {
                    SetLiquid(world, None, LiquidLevel.Eight, isStatic: true, block?.Block as IFillable, position);
                }
                else
                {
                    SetLiquid(
                        world,
                        this,
                        (LiquidLevel) ((int) liquid.Level - (int) level - 1),
                        isStatic: false,
                        block?.Block as IFillable,
                        position);

                    if (liquid.IsStatic) ScheduleTick(world, position);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Override for scheduled update handling.
        /// </summary>
        protected abstract void ScheduledUpdate(World world, Vector3i position, LiquidLevel level, bool isStatic);

        /// <summary>
        ///     Sets the liquid at the position and calls the necessary methods on the <see cref="IFillable" />.
        /// </summary>
        protected static void SetLiquid(World world, Liquid liquid, LiquidLevel level, bool isStatic,
            IFillable? fillable, Vector3i position)
        {
            world.SetLiquid(liquid.AsInstance(level, isStatic), position);
            fillable?.LiquidChange(world, position, liquid, level);
        }

        /// <summary>
        ///     Check if a liquid has a neighbor of the same liquid and this neighbor has a specified level. If the specified level
        ///     is <c>-1</c>, false is directly returned.
        /// </summary>
        protected bool HasNeighborWithLevel(World world, LiquidLevel level, Vector3i position)
        {
            if ((int) level == -1) return false;

            if (world.GetBlock(position)?.Block is not IFillable currentFillable) return false;

            foreach (Orientation orientation in Orientations.All)
            {
                Vector3i neighborPosition = orientation.Offset(position);

                (BlockInstance? neighborBlock, LiquidInstance? neighborLiquid) = world.GetContent(neighborPosition);

                bool isNeighborThisLiquid = neighborLiquid?.Liquid == this && neighborLiquid.Level == level;

                if (!isNeighborThisLiquid) continue;

                if (neighborBlock?.Block is IFillable neighborFillable
                    && neighborFillable.AllowInflow(world, neighborPosition, orientation.Opposite().ToBlockSide(), this)
                    && currentFillable.AllowOutflow(world, position, orientation.ToBlockSide())) return true;
            }

            return false;
        }

        /// <summary>
        ///     Check if a position has a neighboring position with no liquid.
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

                (BlockInstance? neighborBlock, LiquidInstance? neighborLiquid) = world.GetContent(neighborPosition);

                if (neighborLiquid?.Liquid == None && neighborBlock?.Block is IFillable neighborFillable
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
        ///     Search a flow target for a liquid.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="position">The current liquid position.</param>
        /// <param name="maximumLevel">The maximum level of a potential position.</param>
        /// <param name="range">The search range.</param>
        /// <returns>A potential target, if there is any.</returns>
        protected (Vector3i position, LiquidInstance liquid, IFillable fillable)? SearchFlowTarget(
            World world, Vector3i position, LiquidLevel maximumLevel, int range)
        {
            int extendedRange = range + 1;
            int extents = extendedRange * 2 + 1;
            Vector3i center = (extendedRange, 0, extendedRange);

            #pragma warning disable CA1814
            var mark = new bool[extents, extents];
            #pragma warning restore CA1814

            Queue<(Vector3i position, IFillable fillable)> queue = new();
            Queue<(Vector3i position, IFillable fillable)> nextQueue = new();

            (BlockInstance? startBlock, LiquidInstance? startLiquid) = world.GetContent(position);

            if (startBlock?.Block is not IFillable startFillable || startLiquid?.Liquid != this) return null;

            queue.Enqueue((position, startFillable));
            Mark(position);

            for (var depth = 0; queue.Count > 0 && depth <= range; depth++)
            {
                foreach ((Vector3i position, IFillable fillable) e in queue)
                foreach (Orientation orientation in Orientations.All)
                {
                    Vector3i nextPosition = orientation.Offset(e.position);

                    if (IsMarked(nextPosition)) continue;

                    (BlockInstance? nextBlock, LiquidInstance? nextLiquid) = world.GetContent(nextPosition);

                    if (nextBlock?.Block is not IFillable nextFillable || nextLiquid?.Liquid != this) continue;

                    bool canFlow = e.fillable.AllowOutflow(world, e.position, orientation.ToBlockSide()) &&
                                   nextFillable.AllowInflow(
                                       world,
                                       nextPosition,
                                       orientation.Opposite().ToBlockSide(),
                                       this);

                    if (!canFlow) continue;

                    if (nextLiquid.Level <= maximumLevel) return (nextPosition, nextLiquid, nextFillable);

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
        ///     Check if a liquid at a given position is at the surface.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="position">The position of the liquid.</param>
        /// <returns>True if the liquid is at the surface.</returns>
        protected bool IsAtSurface(World world, Vector3i position)
        {
            return world.GetLiquid(position - FlowDirection)?.Liquid != this;
        }

        internal virtual void RandomUpdate(World world, Vector3i position, LiquidLevel level, bool isStatic) {}

        /// <inheritdoc />
        public sealed override string ToString()
        {
            return NamedId;
        }
    }
}
