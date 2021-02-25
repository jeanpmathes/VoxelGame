// <copyright file="Liquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic
{
    public abstract partial class Liquid
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

            if (liquidDictionary.Count < LiquidLimit)
            {
                liquidDictionary.Add((uint)liquidDictionary.Count, this);
                namedLiquidDictionary.Add(namedId, this);

                Id = (uint)(liquidDictionary.Count - 1);
            }
            else
            {
                throw new InvalidOperationException($"Not more than {LiquidLimit} liquids are allowed.");
            }
        }

        /// <summary>
        /// Called when loading liquids, meant to setup vertex data, indices etc.
        /// </summary>
        protected virtual void Setup()
        {
        }

        public abstract void GetMesh(LiquidLevel level, BlockSide side, bool isStatic, out int textureIndex, out TintColor tint);

        public static BoundingBox GetBoundingBox(int x, int y, int z, LiquidLevel level)
        {
            float halfHeight = ((int)level + 1) * 0.0625f;
            return new BoundingBox(new Vector3(x, y + halfHeight, z), new Vector3(0.5f, halfHeight, 0.5f));
        }

        public void EntityContact(PhysicsEntity entity, int x, int y, int z)
        {
            if (Game.World.GetLiquid(x, y, z, out LiquidLevel level, out bool isStatic) == this)
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
        public bool Fill(int x, int y, int z, LiquidLevel level, out int remaining)
        {
            (Block? block, Liquid? target) = Game.World.GetPosition(x, y, z, out _, out LiquidLevel current, out bool isStatic);

            if (block is IFillable fillable && fillable.AllowInflow(x, y, z, BlockSide.Top, this))
            {
                if (target == this && current != LiquidLevel.Eight)
                {
                    remaining = (int)current + (int)level + 1;
                    remaining = remaining > 7 ? 7 : remaining;

                    SetLiquid(this, (LiquidLevel)remaining, false, fillable, x, y, z);
                    if (isStatic) ScheduleTick(x, y, z);

                    remaining = (int)level - remaining - (int)current;
                    return true;
                }

                if (target == Liquid.None)
                {
                    SetLiquid(this, level, false, fillable, x, y, z);
                    ScheduleTick(x, y, z);

                    remaining = 0;
                    return true;
                }
            }

            remaining = (int)level;
            return false;
        }

        /// <summary>
        /// Tries to take a certain amount of liquid from a position. The actually taken amount is given when finished.
        /// </summary>
        public bool Take(int x, int y, int z, ref LiquidLevel level)
        {
            (Block? block, Liquid? liquid) = Game.World.GetPosition(x, y, z, out _, out LiquidLevel available, out bool isStatic);

            if (liquid == this && this != Liquid.None)
            {
                if (level >= available)
                {
                    SetLiquid(Liquid.None, LiquidLevel.Eight, true, block as IFillable, x, y, z);
                }
                else
                {
                    SetLiquid(this, (LiquidLevel)((int)available - (int)level - 1), false, block as IFillable, x, y, z);
                    if (isStatic) ScheduleTick(x, y, z);
                }

                return true;
            }

            return false;
        }

        public bool TryTakeExact(int x, int y, int z, LiquidLevel level)
        {
            (Block? block, Liquid? liquid) = Game.World.GetPosition(x, y, z, out _, out LiquidLevel available, out bool isStatic);

            if (liquid == this && this != Liquid.None && level <= available)
            {
                if (level == available)
                {
                    SetLiquid(Liquid.None, LiquidLevel.Eight, true, block as IFillable, x, y, z);
                }
                else
                {
                    SetLiquid(this, (LiquidLevel)((int)available - (int)level - 1), false, block as IFillable, x, y, z);
                    if (isStatic) ScheduleTick(x, y, z);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        protected abstract void ScheduledUpdate(int x, int y, int z, LiquidLevel level, bool isStatic);

        /// <summary>
        /// Sets the liquid at the position and calls the necessary methods on the <see cref="IFillable"/>.
        /// </summary>
        protected static void SetLiquid(Liquid liquid, LiquidLevel level, bool isStatic, IFillable? fillable, int x, int y, int z)
        {
            Game.World.SetLiquid(liquid, level, isStatic, x, y, z);
            fillable?.LiquidChange(x, y, z, liquid, level);
        }

        /// <summary>
        /// Check if a liquid has a neighbor of the same liquid and this neighbor has a specified level. If the specified level is <c>-1</c>, false is directly returned.
        /// </summary>
        protected bool HasNeighborWithLevel(LiquidLevel level, int x, int y, int z)
        {
            return (int)level != -1
                   && Game.World.GetBlock(x, y, z, out _) is IFillable currentFillable
                   && (CheckNeighborForLevel(x, z - 1, BlockSide.Back)
                       || CheckNeighborForLevel(x + 1, z, BlockSide.Right)
                       || CheckNeighborForLevel(x, z + 1, BlockSide.Front)
                       || CheckNeighborForLevel(x - 1, z, BlockSide.Left));

            bool CheckNeighborForLevel(int nx, int nz, BlockSide side)
            {
                (Block? block, Liquid? liquid) = Game.World.GetPosition(nx, y, nz, out _, out LiquidLevel neighborLevel, out _);

                return liquid == this && level == neighborLevel
                       && block is IFillable neighborFillable
                       && neighborFillable.AllowInflow(nx, y, nz, side.Opposite(), this)
                       && currentFillable.AllowOutflow(x, y, z, side);
            }
        }

        protected bool HasNeighborWithEmpty(int x, int y, int z)
        {
            return Game.World.GetBlock(x, y, z, out _) is IFillable currentFillable
                   && (CheckNeighborForEmpty(x, z - 1, BlockSide.Back)
                       || CheckNeighborForEmpty(x + 1, z, BlockSide.Right)
                       || CheckNeighborForEmpty(x, z + 1, BlockSide.Front)
                       || CheckNeighborForEmpty(x - 1, z, BlockSide.Left));

            bool CheckNeighborForEmpty(int nx, int nz, BlockSide side)
            {
                (Block? block, Liquid? liquid) = Game.World.GetPosition(nx, y, nz, out _, out _, out _);

                return liquid == Liquid.None && block is IFillable neighborFillable
                                             && neighborFillable.AllowInflow(nx, y, nz, side.Opposite(), this)
                                             && currentFillable.AllowOutflow(x, y, z, side);
            }
        }

        protected bool SearchLevel(int x, int y, int z, Vector2i direction, int range, LiquidLevel target, out Vector3i targetPosition)
        {
            targetPosition = (0, 0, 0);

            Vector3i pos = new Vector3i(x, y, z);
            Vector3i dir = new Vector3i(direction.X, 0, direction.Y);
            Vector3i perpDir = new Vector3i(direction.Y, 0, -direction.X);

            bool[] ignoreRows = new bool[range * 2];

            for (int r = 0; r < range; r++)
            {
                Vector3i line = (-r * perpDir) + ((1 + r) * dir) + pos;

                for (int s = 0; s < 2 * (r + 1); s++)
                {
                    int row = s + (range - r);

                    if (ignoreRows[row - 1]) continue;

                    Vector3i current = (s * perpDir) + line;

                    (Block? block, Liquid? liquid) = Game.World.GetPosition(current.X, current.Y, current.Z, out _, out LiquidLevel level, out _);

                    if (liquid != this || !(block is IFillable fillable) || !fillable.AllowInflow(current.X, current.Y, current.Z, BlockSide.Top, this))
                    {
                        ignoreRows[row - 1] = true;

                        if (s == 0)
                        {
                            for (int i = 0; i < row - 1; i++) ignoreRows[i] = true;
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
        protected bool IsAtSurface(int x, int y, int z)
        {
            return Game.World.GetLiquid(x, y + Direction, z, out _, out _) != this;
        }

        internal virtual void RandomUpdate(int x, int y, int z, LiquidLevel level, bool isStatic)
        {
        }

        public sealed override string ToString()
        {
            return NamedId;
        }
    }
}