// <copyright file="Liquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Visuals;

namespace VoxelGame.Logic
{
    public abstract partial class Liquid
    {
        public const int maxLevel = 8;

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
        /// Gets whether this liquid is rendered.
        /// </summary>
        public bool IsRendered { get; }

        protected Liquid(string name, string namedId, bool isRendered)
        {
            Name = name;
            NamedId = namedId;

            IsRendered = isRendered;

            if (liquidDictionary.Count < LiquidLimit)
            {
                liquidDictionary.Add((uint)liquidDictionary.Count, this);
                namedLiquidDictionary.Add(namedId, this);

                Id = (uint)(liquidDictionary.Count - 1);
            }
            else
            {
                throw new System.InvalidOperationException($"Not more than {LiquidLimit} liquids are allowed.");
            }
        }

        /// <summary>
        /// Called when loading liquids, meant to setup vertex data, indices etc.
        /// </summary>
        protected virtual void Setup()
        {
        }

        public abstract uint GetMesh(BlockSide side, LiquidLevel level, bool isStatic, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint);

        /// <summary>
        /// Tries to fill a position with the specified amount of liquid. The remaining liquid is specified, it can be converted to <see cref="LiquidLevel"/> using <c>remaining - 1</c>.
        /// </summary>
        public bool Fill(int x, int y, int z, LiquidLevel level, out int remaining)
        {
            (Block? block, Liquid? target) = Game.World.GetPosition(x, y, z, out _, out LiquidLevel current, out _);

            if (block?.IsReplaceable != true)
            {
                remaining = (int)level + 1;
                return false;
            }

            if (target == this)
            {
                remaining = (int)current + (int)level + 1;
                remaining = remaining > 7 ? 7 : remaining;

                Game.World.SetLiquid(this, (LiquidLevel)remaining, false, x, y, z);

                remaining = (int)level - remaining - (int)current;
                remaining = remaining < 0 ? 0 : remaining;

                return true;
            }
            else if (target == Liquid.None)
            {
                Game.World.SetLiquid(this, level, false, x, y, z);

                remaining = 0;
                return true;
            }
            else
            {
                remaining = (int)level + 1;
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
                    Game.World.SetLiquid(this, (LiquidLevel)((int)available - (int)level - 1), true, x, y, z);
                }

                return true;
            }
            else
            {
                return false;
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