// <copyright file="Liquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
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
        /// Gets whether this liquid is rendered.
        /// </summary>
        public bool IsRendered { get; }

        protected Liquid(string name, string namedId, float density, int viscosity, bool isRendered)
        {
            Name = name;
            NamedId = namedId;

            Density = density;
            Direction = Math.Sign(density);

            Viscosity = viscosity;

            IsRendered = isRendered;

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

        /// <summary>
        /// Tries to fill a position with the specified amount of liquid. The remaining liquid is specified, it can be converted to <see cref="LiquidLevel"/> if it is not <c>-1</c>.
        /// </summary>
        public bool Fill(int x, int y, int z, LiquidLevel level, out int remaining)
        {
            (Block? block, Liquid? target) = Game.World.GetPosition(x, y, z, out _, out LiquidLevel current, out bool isStatic);

            // ! Liquid flow check, has to be replaced with interface check when available.
            if (block != Block.Air)
            {
                remaining = (int)level;
                return false;
            }

            if (target == this && current != LiquidLevel.Eight)
            {
                remaining = (int)current + (int)level + 1;
                remaining = remaining > 7 ? 7 : remaining;

                Game.World.SetLiquid(this, (LiquidLevel)remaining, false, x, y, z);
                if (isStatic) ScheduleTick(x, y, z);

                remaining = (int)level - remaining - (int)current;
                return true;
            }
            else if (target == Liquid.None)
            {
                Game.World.SetLiquid(this, level, false, x, y, z);
                ScheduleTick(x, y, z);

                remaining = 0;
                return true;
            }
            else
            {
                remaining = (int)level;
                return false;
            }
        }

        /// <summary>
        /// Tries to take a certain amount of liquid from a position. The actually taken amount is given when finished.
        /// </summary>
        public bool Take(int x, int y, int z, ref LiquidLevel level)
        {
            if (Game.World.GetLiquid(x, y, z, out LiquidLevel available, out bool isStatic) == this && this != Liquid.None)
            {
                if (level >= available)
                {
                    Game.World.SetLiquid(Liquid.None, LiquidLevel.Eight, true, x, y, z);
                }
                else
                {
                    Game.World.SetLiquid(this, (LiquidLevel)((int)available - (int)level - 1), false, x, y, z);
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
        /// Check if a liquid has a neighbor of the same liquid and this neighbor has a specified level. If the specified level is <c>-1</c>, the method searches for empty space instead.
        /// </summary>
        protected bool HasNeighborWithLevel(LiquidLevel level, int x, int y, int z)
        {
            return ((int)level != -1)
                ? CheckNeighborForLevel(x, z - 1) || CheckNeighborForLevel(x + 1, z) || CheckNeighborForLevel(x, z + 1) || CheckNeighborForLevel(x - 1, z)
                : CheckNeighborForEmpty(x, z - 1) || CheckNeighborForEmpty(x + 1, z) || CheckNeighborForEmpty(x, z + 1) || CheckNeighborForEmpty(x - 1, z)
                ;

            bool CheckNeighborForLevel(int nx, int nz)
            {
                if (Game.World.GetLiquid(nx, y, nz, out LiquidLevel neighborLevel, out _) == this)
                {
                    return neighborLevel == level;
                }
                else
                {
                    return false;
                }
            }

            bool CheckNeighborForEmpty(int nx, int nz)
            {
                return Game.World.GetLiquid(nx, y, nz, out LiquidLevel _, out _) == Liquid.None;
            }
        }

        public sealed override string ToString()
        {
            return NamedId;
        }

        public sealed override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj);
        }

        public sealed override int GetHashCode()
        {
            return (int)Id;
        }
    }
}