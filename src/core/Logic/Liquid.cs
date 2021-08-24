// <copyright file="Liquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using System;
using System.Diagnostics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic
{
    public abstract partial class Liquid : IIdentifiable<uint>, IIdentifiable<string>
    {
        /// <summary>
        /// Gets the liquid id which can be any value from 0 to 31.
        /// </summary>
        public uint Id { get; }

        /// <summary>
        /// Gets the localized name of the liquid.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// An unlocalized string that identifies this liquid.
        /// </summary>
        public string NamedId { get; }

        /// <summary>
        /// Gets the density of this liquid.
        /// </summary>
        public float Density { get; }

        /// <summary>
        /// Gets the flowing direction of this liquid. Positive means down, negative means up.
        /// </summary>
        public int Direction { get; }

        /// <summary>
        /// Gets the viscosity of this liquid, meaning the tick offset between two updates.
        /// </summary>
        public int Viscosity { get; }

        /// <summary>
        /// Gets whether entity contacts have to be checked.
        /// </summary>
        public bool CheckContact { get; }

        /// <summary>
        /// Gets whether this liquid receives entity contacts.
        /// </summary>
        public bool ReceiveContact { get; }

        /// <summary>
        /// Gets the <see cref="Visuals.RenderType"/> of this liquid.
        /// </summary>
        public RenderType RenderType { get; }

        /// <summary>
        /// Get whether this liquid is a fluid that flows down.
        /// </summary>
        public bool IsFluid => Direction > 0;

        /// <summary>
        /// Get whether this liquid is a gas that flows up.
        /// </summary>
        public bool IsGas => Direction < 0;

        protected Liquid(string name, string namedId, float density, int viscosity, bool checkContact, bool receiveContact, RenderType renderType)
        {
            Name = name;
            NamedId = namedId;

            Density = Math.Abs(density);
            Direction = Math.Sign(density);

            Viscosity = viscosity;

            CheckContact = checkContact;
            ReceiveContact = receiveContact;

            RenderType = renderType;

            if (LiquidList.Count < LiquidLimit)
            {
                LiquidList.Add(this);
                NamedLiquidDictionary.Add(namedId, this);

                Id = (uint) (LiquidList.Count - 1);
            }
            else
            {
                Debug.Fail($"Not more than {LiquidLimit} liquids are allowed.");
            }
        }

        uint IIdentifiable<uint>.Id => Id;
        string IIdentifiable<string>.Id => NamedId;

        /// <summary>
        /// Called when loading liquids, meant to setup vertex data, indices etc.
        /// </summary>
        /// <param name="indexProvider"></param>
        protected virtual void Setup(ITextureIndexProvider indexProvider)
        {
        }

        public abstract LiquidMeshData GetMesh(LiquidMeshInfo info);

        public static BoundingBox GetBoundingBox(World world, int x, int y, int z, LiquidLevel level)
        {
            float halfHeight = ((int) level + 1) * 0.0625f;
            return new BoundingBox(new Vector3(x, y + halfHeight, z), new Vector3(0.5f, halfHeight, 0.5f));
        }

        public void EntityContact(PhysicsEntity entity, int x, int y, int z)
        {
            if (entity.World.GetLiquid(x, y, z, out LiquidLevel level, out bool isStatic) == this)
            {
                EntityContact(entity, x, y, z, level, isStatic);
            }
        }

        protected virtual void EntityContact(PhysicsEntity entity, int x, int y, int z, LiquidLevel level, bool isStatic)
        {
        }

        /// <summary>
        /// Tries to fill a position with the specified amount of liquid. The remaining liquid is specified, it can be converted to <see cref="LiquidLevel"/> if it is not <c>-1</c>.
        /// </summary>
        public bool Fill(World world, int x, int y, int z, LiquidLevel level, out int remaining)
        {
            (Block? block, Liquid? target) = world.GetPosition(x, y, z, out _, out LiquidLevel current, out bool isStatic);

            if (block is IFillable fillable && fillable.AllowInflow(world, x, y, z, BlockSide.Top, this))
            {
                if (target == this && current != LiquidLevel.Eight)
                {
                    int filled = (int) current + (int) level + 1;
                    filled = filled > 7 ? 7 : filled;

                    SetLiquid(world, this, (LiquidLevel) filled, false, fillable, x, y, z);
                    if (isStatic) ScheduleTick(world, x, y, z);

                    remaining = (int) level - (filled - (int) current);
                    return true;
                }

                if (target == Liquid.None)
                {
                    SetLiquid(world, this, level, false, fillable, x, y, z);
                    ScheduleTick(world, x, y, z);

                    remaining = -1;
                    return true;
                }
            }

            remaining = (int) level;
            return false;
        }

        /// <summary>
        /// Tries to take a certain amount of liquid from a position. The actually taken amount is given when finished.
        /// </summary>
        public bool Take(World world, int x, int y, int z, ref LiquidLevel level)
        {
            (Block? block, Liquid? liquid) = world.GetPosition(x, y, z, out _, out LiquidLevel available, out bool isStatic);

            if (liquid != this || this == Liquid.None) return false;

            if (level >= available)
            {
                SetLiquid(world, Liquid.None, LiquidLevel.Eight, true, block as IFillable, x, y, z);
            }
            else
            {
                SetLiquid(world, this, (LiquidLevel) ((int) available - (int) level - 1), false, block as IFillable, x, y, z);
                if (isStatic) ScheduleTick(world, x, y, z);
            }

            return true;
        }

        public bool TryTakeExact(World world, int x, int y, int z, LiquidLevel level)
        {
            (Block? block, Liquid? liquid) = world.GetPosition(x, y, z, out _, out LiquidLevel available, out bool isStatic);

            if (liquid == this && this != Liquid.None && level <= available)
            {
                if (level == available)
                {
                    SetLiquid(world, Liquid.None, LiquidLevel.Eight, true, block as IFillable, x, y, z);
                }
                else
                {
                    SetLiquid(world, this, (LiquidLevel) ((int) available - (int) level - 1), false, block as IFillable, x, y, z);
                    if (isStatic) ScheduleTick(world, x, y, z);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        protected abstract void ScheduledUpdate(World world, int x, int y, int z, LiquidLevel level, bool isStatic);

        /// <summary>
        /// Sets the liquid at the position and calls the necessary methods on the <see cref="IFillable"/>.
        /// </summary>
        protected static void SetLiquid(World world, Liquid liquid, LiquidLevel level, bool isStatic, IFillable? fillable, int x, int y, int z)
        {
            world.SetLiquid(liquid, level, isStatic, x, y, z);
            fillable?.LiquidChange(world, x, y, z, liquid, level);
        }

        /// <summary>
        /// Check if a liquid has a neighbor of the same liquid and this neighbor has a specified level. If the specified level is <c>-1</c>, false is directly returned.
        /// </summary>
        protected bool HasNeighborWithLevel(World world, LiquidLevel level, int x, int y, int z)
        {
            return (int) level != -1
                   && world.GetBlock(x, y, z, out _) is IFillable currentFillable
                   && (CheckNeighborForLevel(x, z - 1, BlockSide.Back)
                       || CheckNeighborForLevel(x + 1, z, BlockSide.Right)
                       || CheckNeighborForLevel(x, z + 1, BlockSide.Front)
                       || CheckNeighborForLevel(x - 1, z, BlockSide.Left));

            bool CheckNeighborForLevel(int nx, int nz, BlockSide side)
            {
                (Block? block, Liquid? liquid) = world.GetPosition(nx, y, nz, out _, out LiquidLevel neighborLevel, out _);

                return liquid == this && level == neighborLevel
                       && block is IFillable neighborFillable
                       && neighborFillable.AllowInflow(world, nx, y, nz, side.Opposite(), this)
                       && currentFillable.AllowOutflow(world, x, y, z, side);
            }
        }

        protected bool HasNeighborWithEmpty(World world, int x, int y, int z)
        {
            return world.GetBlock(x, y, z, out _) is IFillable currentFillable
                   && (CheckNeighborForEmpty(x, z - 1, BlockSide.Back)
                       || CheckNeighborForEmpty(x + 1, z, BlockSide.Right)
                       || CheckNeighborForEmpty(x, z + 1, BlockSide.Front)
                       || CheckNeighborForEmpty(x - 1, z, BlockSide.Left));

            bool CheckNeighborForEmpty(int nx, int nz, BlockSide side)
            {
                (Block? block, Liquid? liquid) = world.GetPosition(nx, y, nz, out _, out _, out _);

                return liquid == Liquid.None && block is IFillable neighborFillable
                                             && neighborFillable.AllowInflow(world, nx, y, nz, side.Opposite(), this)
                                             && currentFillable.AllowOutflow(world, x, y, z, side);
            }
        }

        protected bool SearchLevel(World world, int x, int y, int z, Vector2i direction, int range, LiquidLevel target, out Vector3i targetPosition)
        {
            targetPosition = (0, 0, 0);

            var pos = new Vector3i(x, y, z);
            var dir = new Vector3i(direction.X, 0, direction.Y);
            var perpendicularDir = new Vector3i(direction.Y, 0, -direction.X);

            bool[] ignoreRows = new bool[range * 2];

            for (var r = 0; r < range; r++)
            {
                Vector3i line = (-r * perpendicularDir) + ((1 + r) * dir) + pos;

                for (var s = 0; s < 2 * (r + 1); s++)
                {
                    int row = s + (range - r);

                    if (ignoreRows[row - 1]) continue;

                    Vector3i current = (s * perpendicularDir) + line;

                    (Block? block, Liquid? liquid) = world.GetPosition(current.X, current.Y, current.Z, out _, out LiquidLevel level, out _);

                    if (liquid != this || !(block is IFillable fillable) || !fillable.AllowInflow(world, current.X, current.Y, current.Z, BlockSide.Top, this))
                    {
                        ignoreRows[row - 1] = true;

                        if (s == 0)
                        {
                            for (var i = 0; i < row - 1; i++) ignoreRows[i] = true;
                        }
                        else if (s == 2 * (r + 1) - 1)
                        {
                            for (int i = row; i < range * 2; i++) ignoreRows[i] = true;
                        }
                    }
                    else if (level <= target)
                    {
                        targetPosition = current;

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Check if there is a liquid of the same type above this position or a gas of the same type below.
        /// </summary>
        protected bool IsAtSurface(World world, int x, int y, int z)
        {
            return world.GetLiquid(x, y + Direction, z, out _, out _) != this;
        }

        internal virtual void RandomUpdate(World world, int x, int y, int z, LiquidLevel level, bool isStatic)
        {
        }

        public sealed override string ToString()
        {
            return NamedId;
        }
    }
}