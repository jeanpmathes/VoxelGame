// <copyright file="Liquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using VoxelGame.Visuals;

namespace VoxelGame.Logic
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
        /// Gets whether this liquid is rendered.
        /// </summary>
        public bool IsRendered { get; }

        protected Liquid(string name, string namedId, float density, bool isRendered)
        {
            Name = name;
            NamedId = namedId;

            Density = density;
            Direction = Math.Sign(density);

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
            (Block? block, Liquid? target) = Game.World.GetPosition(x, y, z, out _, out LiquidLevel current, out _);

            if (block != Block.Air)
            {
                remaining = (int)level;
                return false;
            }

            if (target == this)
            {
                remaining = (int)current + (int)level + 1;
                remaining = remaining > 7 ? 7 : remaining;

                Game.World.SetLiquid(this, (LiquidLevel)remaining, false, x, y, z);
                ScheduleTick(x, y, z, 1);

                remaining = (int)level - remaining - (int)current;
                return true;
            }
            else if (target == Liquid.None)
            {
                Game.World.SetLiquid(this, level, false, x, y, z);
                ScheduleTick(x, y, z, 1);

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
            if (Game.World.GetLiquid(x, y, z, out LiquidLevel available, out _) == this)
            {
                if (level >= available)
                {
                    Game.World.SetLiquid(Liquid.None, LiquidLevel.Eight, true, x, y, z);
                }
                else
                {
                    Game.World.SetLiquid(this, (LiquidLevel)((int)available - (int)level - 1), false, x, y, z);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        protected abstract void ScheduledTick(int x, int y, int z, LiquidLevel level, bool isStatic);

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