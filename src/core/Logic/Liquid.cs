// <copyright file="Liquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic
{
    public abstract partial class Liquid : IIdentifiable<uint>, IIdentifiable<string>
    {
        protected Liquid(string name, string namedId, float density, int viscosity, bool checkContact,
            bool receiveContact, RenderType renderType)
        {
            Name = name;
            NamedId = namedId;

            Density = Math.Abs(density);

            Direction = Math.Sign(density) switch
            {
                > 0 => VerticalFlow.Downwards,
                0 => VerticalFlow.Static,
                < 0 => VerticalFlow.Upwards
            };

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
        ///     Gets the flowing direction of this liquid. Positive means down, negative means up.
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
        public bool IsLiquid => Direction == VerticalFlow.Downwards;

        /// <summary>
        ///     Get whether this fluid is a gas.
        /// </summary>
        public bool IsGas => Direction == VerticalFlow.Upwards;

        public Vector3i FlowDirection => Direction.FlowDirection();

        string IIdentifiable<string>.Id => NamedId;

        uint IIdentifiable<uint>.Id => Id;

        /// <summary>
        ///     Called when loading liquids, meant to setup vertex data, indices etc.
        /// </summary>
        /// <param name="indexProvider"></param>
        protected virtual void Setup(ITextureIndexProvider indexProvider) {}

        public abstract LiquidMeshData GetMesh(LiquidMeshInfo info);

        public static BoundingBox GetBoundingBox(Vector3i position, LiquidLevel level)
        {
            float halfHeight = ((int) level + 1) * 0.0625f;

            return new BoundingBox(
                position.ToVector3() + (0f, halfHeight, 0f),
                new Vector3(x: 0.5f, halfHeight, z: 0.5f));
        }

        public void EntityContact(PhysicsEntity entity, Vector3i position)
        {
            if (entity.World.GetLiquid(position, out LiquidLevel level, out bool isStatic) == this)
                EntityContact(entity, position, level, isStatic);
        }

        protected virtual void EntityContact(PhysicsEntity entity, Vector3i position, LiquidLevel level,
            bool isStatic) {}

        /// <summary>
        ///     Tries to fill a position with the specified amount of liquid. The remaining liquid is specified, it can be
        ///     converted to <see cref="LiquidLevel" /> if it is not <c>-1</c>.
        /// </summary>
        public bool Fill(World world, Vector3i position, LiquidLevel level, BlockSide entrySide, out int remaining)
        {
            (Block? block, Liquid? target) = world.GetPosition(
                position,
                out _,
                out LiquidLevel current,
                out bool isStatic);

            if (block is IFillable fillable && fillable.AllowInflow(world, position, entrySide, this))
            {
                if (target == this && current != LiquidLevel.Eight)
                {
                    int filled = (int) current + (int) level + 1;
                    filled = filled > 7 ? 7 : filled;

                    SetLiquid(world, this, (LiquidLevel) filled, isStatic: false, fillable, position);
                    if (isStatic) ScheduleTick(world, position);

                    remaining = (int) level - (filled - (int) current);

                    return true;
                }

                if (target == None)
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
            (Block? block, Liquid? liquid) = world.GetPosition(
                position,
                out _,
                out LiquidLevel available,
                out bool isStatic);

            if (liquid != this || this == None) return false;

            if (level >= available)
            {
                SetLiquid(world, None, LiquidLevel.Eight, isStatic: true, block as IFillable, position);
            }
            else
            {
                SetLiquid(
                    world,
                    this,
                    (LiquidLevel) ((int) available - (int) level - 1),
                    isStatic: false,
                    block as IFillable,
                    position);

                if (isStatic) ScheduleTick(world, position);
            }

            return true;
        }

        public bool TryTakeExact(World world, Vector3i position, LiquidLevel level)
        {
            (Block? block, Liquid? liquid) = world.GetPosition(
                position,
                out _,
                out LiquidLevel available,
                out bool isStatic);

            if (liquid == this && this != None && level <= available)
            {
                if (level == available)
                {
                    SetLiquid(world, None, LiquidLevel.Eight, isStatic: true, block as IFillable, position);
                }
                else
                {
                    SetLiquid(
                        world,
                        this,
                        (LiquidLevel) ((int) available - (int) level - 1),
                        isStatic: false,
                        block as IFillable,
                        position);

                    if (isStatic) ScheduleTick(world, position);
                }

                return true;
            }

            return false;
        }

        protected abstract void ScheduledUpdate(World world, Vector3i position, LiquidLevel level, bool isStatic);

        /// <summary>
        ///     Sets the liquid at the position and calls the necessary methods on the <see cref="IFillable" />.
        /// </summary>
        protected static void SetLiquid(World world, Liquid liquid, LiquidLevel level, bool isStatic,
            IFillable? fillable, Vector3i position)
        {
            world.SetLiquid(liquid, level, isStatic, position);
            fillable?.LiquidChange(world, position, liquid, level);
        }

        /// <summary>
        ///     Check if a liquid has a neighbor of the same liquid and this neighbor has a specified level. If the specified level
        ///     is <c>-1</c>, false is directly returned.
        /// </summary>
        protected bool HasNeighborWithLevel(World world, LiquidLevel level, Vector3i position)
        {
            if ((int) level == -1) return false;

            if (world.GetBlock(position, out _) is not IFillable currentFillable) return false;

            foreach (Orientation orientation in Orientations.All)
            {
                Vector3i neighborPosition = orientation.Offset(position);

                (Block? neighborBlock, Liquid? neighborLiquid) = world.GetPosition(
                    neighborPosition,
                    out _,
                    out LiquidLevel neighborLevel,
                    out _);

                if (neighborLiquid == this && neighborLevel == level && neighborBlock is IFillable neighborFillable
                    && neighborFillable.AllowInflow(world, neighborPosition, orientation.Opposite().ToBlockSide(), this)
                    && currentFillable.AllowOutflow(world, position, orientation.ToBlockSide())) return true;
            }

            return false;
        }

        protected bool HasNeighborWithEmpty(World world, Vector3i position)
        {
            if (world.GetBlock(position, out _) is not IFillable currentFillable) return false;

            foreach (Orientation orientation in Orientations.All)
            {
                Vector3i neighborPosition = orientation.Offset(position);

                (Block? neighborBlock, Liquid? neighborLiquid) = world.GetPosition(
                    neighborPosition,
                    out _,
                    out LiquidLevel _,
                    out _);

                if (neighborLiquid == None && neighborBlock is IFillable neighborFillable
                                           && neighborFillable.AllowInflow(
                                               world,
                                               neighborPosition,
                                               orientation.Opposite().ToBlockSide(),
                                               this)
                                           && currentFillable.AllowOutflow(world, position, orientation.ToBlockSide()))
                    return true;
            }

            return false;
        }

        protected (Vector3i position, LiquidLevel level, bool isStatic, IFillable fillable)? SearchFlowTarget(
            World world, Vector3i position, LiquidLevel maximumLevel, int range)
        {
            int extendedRange = range + 1;
            int extents = extendedRange * 2 + 1;
            Vector3i center = (extendedRange, 0, extendedRange);

            #pragma warning disable CA1814
            bool[,] mark = new bool[extents, extents];
            #pragma warning restore CA1814

            Queue<(Vector3i position, IFillable fillable)> queue = new();
            Queue<(Vector3i position, IFillable fillable)> nextQueue = new();

            (Block? startBlock, Liquid? startLiquid) = world.GetPosition(position, out _, out _, out _);

            if (startBlock is not IFillable startFillable || startLiquid != this) return null;

            queue.Enqueue((position, startFillable));
            Mark(position);

            for (var depth = 0; queue.Count > 0 && depth <= range; depth++)
            {
                foreach ((Vector3i position, IFillable fillable) e in queue)
                foreach (Orientation orientation in Orientations.All)
                {
                    Vector3i nextPosition = orientation.Offset(e.position);

                    if (IsMarked(nextPosition)) continue;

                    (Block? nextBlock, Liquid? nextLiquid) = world.GetPosition(
                        nextPosition,
                        out _,
                        out LiquidLevel nextLevel,
                        out bool isNextStatic);

                    if (nextBlock is not IFillable nextFillable || nextLiquid != this) continue;

                    bool canFlow = e.fillable.AllowOutflow(world, e.position, orientation.ToBlockSide()) &&
                                   nextFillable.AllowInflow(
                                       world,
                                       nextPosition,
                                       orientation.Opposite().ToBlockSide(),
                                       this);

                    if (!canFlow) continue;

                    if (nextLevel <= maximumLevel) return (nextPosition, nextLevel, isNextStatic, nextFillable);

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

        protected bool IsAtSurface(World world, Vector3i position)
        {
            return world.GetLiquid(position - FlowDirection, out _, out _) != this;
        }

        internal virtual void RandomUpdate(World world, Vector3i position, LiquidLevel level, bool isStatic) {}

        public sealed override string ToString()
        {
            return NamedId;
        }
    }
}
