// <copyright file="LiquidContactManager.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics;
using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Logic
{
    public static class LiquidContactManager
    {
        public static bool HandleContact(Liquid a, Vector3i posA, LiquidLevel levelA, Liquid b, Vector3i posB, LiquidLevel levelB, bool isStaticB)
        {
            Debug.Assert(a != b);

            switch ((a.NamedId, b.NamedId))
            {
                case (nameof(Liquid.Lava), nameof(Liquid.Water)) or (nameof(Liquid.Water), nameof(Liquid.Lava)):
                case (nameof(Liquid.Milk), nameof(Liquid.Lava)) or (nameof(Liquid.Lava), nameof(Liquid.Milk)):
                    return LavaCooling(a, posA, levelA, b, posB, levelB, isStaticB);

                default: return DensitySwap(a, posA, levelA, b, posB, levelB, isStaticB);
            }
        }

        private static bool LavaCooling(Liquid a, Vector3i posA, LiquidLevel levelA, Liquid b, Vector3i posB, LiquidLevel levelB, bool isStaticB)
        {
            Vector3i lavaPos;
            Vector3i coolantPos;
            LiquidLevel coolantLevel;
            bool tickCoolant;

            if (a == Liquid.Lava)
            {
                lavaPos = posA;
                coolantPos = posB;
                coolantLevel = levelB;
                tickCoolant = isStaticB;
            }
            else
            {
                Debug.Assert(b == Liquid.Lava);

                lavaPos = posB;
                coolantPos = posA;
                coolantLevel = levelA;
                tickCoolant = true;
            }

            Game.World.SetPosition(Block.Stone, 0, Liquid.None, LiquidLevel.Eight, true, lavaPos.X, lavaPos.Y, lavaPos.Z);

            Game.World.SetLiquid(Liquid.Steam, coolantLevel, false, coolantPos.X, coolantPos.Y, coolantPos.Z);

            Liquid.Steam.TickSoon(coolantPos.X, coolantPos.Y, coolantPos.Z, tickCoolant);

            return true;
        }

        private static bool DensitySwap(Liquid a, Vector3i posA, LiquidLevel levelA, Liquid b, Vector3i posB, LiquidLevel levelB, bool isStaticB)
        {
            if ((posA.Y <= posB.Y || !(a.Density > b.Density)) &&
                (posA.Y >= posB.Y || !(a.Density < b.Density))) return false;

            Game.World.SetLiquid(a, levelA, false, posB.X, posB.Y, posB.Z);

            a.TickSoon(posB.X, posB.Y, posB.Z, isStaticB);

            Game.World.SetLiquid(b, levelB, false, posA.X, posA.Y, posA.Z);

            b.TickSoon(posA.X, posA.Y, posA.Z, true);

            return true;
        }
    }
}