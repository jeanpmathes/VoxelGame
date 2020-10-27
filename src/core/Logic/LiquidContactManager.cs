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
                    return LavaCooling(a, posA, levelA, posB, levelB);

                case (nameof(Liquid.Lava), nameof(Liquid.CrudeOil)) or (nameof(Liquid.CrudeOil), nameof(Liquid.Lava)):
                case (nameof(Liquid.Lava), nameof(Liquid.NaturalGas)) or (nameof(Liquid.NaturalGas), nameof(Liquid.Lava)):
                    return LavaBurn(a, posA, b, posB, isStaticB);

                default: return DensitySwap(a, posA, levelA, b, posB, levelB);
            }
        }

        private static bool LavaCooling(Liquid a, Vector3i posA, LiquidLevel levelA, Vector3i posB, LiquidLevel levelB)
        {
            Vector3i lavaPos;
            Vector3i coolantPos;
            LiquidLevel coolantLevel;

            if (a == Liquid.Lava)
            {
                lavaPos = posA;
                coolantPos = posB;
                coolantLevel = levelB;
            }
            else
            {
                lavaPos = posB;
                coolantPos = posA;
                coolantLevel = levelA;
            }

            Block lavaBlock = Game.World.GetBlock(lavaPos.X, lavaPos.Y, lavaPos.Z, out _) ?? Block.Air;

            if (lavaBlock.IsReplaceable || lavaBlock.Destroy(lavaPos.X, lavaPos.Y, lavaPos.Z))
            {
                Game.World.SetPosition(Block.Pumice, 0, Liquid.None, LiquidLevel.Eight, true, lavaPos.X, lavaPos.Y, lavaPos.Z);
            }

            Game.World.SetLiquid(Liquid.Steam, coolantLevel, false, coolantPos.X, coolantPos.Y, coolantPos.Z);

            Liquid.Steam.TickSoon(coolantPos.X, coolantPos.Y, coolantPos.Z, true);

            return true;
        }

        private static bool LavaBurn(Liquid a, Vector3i posA, Liquid b, Vector3i posB, bool isStaticB)
        {
            Liquid lava;
            Vector3i lavaPos;
            Vector3i burnedPos;
            bool tickLava;

            if (a == Liquid.Lava)
            {
                lava = a;
                lavaPos = posA;
                burnedPos = posB;
                tickLava = true;
            }
            else
            {
                lava = b;
                lavaPos = posB;
                burnedPos = posA;
                tickLava = isStaticB;
            }

            lava.TickSoon(lavaPos.X, lavaPos.Y, lavaPos.Z, tickLava);

            Game.World.SetLiquid(Liquid.None, LiquidLevel.Eight, true, burnedPos.X, burnedPos.Y, burnedPos.Z);
            Block.Fire.Place(burnedPos.X, burnedPos.Y, burnedPos.Z);

            return true;
        }

        private static bool DensitySwap(Liquid a, Vector3i posA, LiquidLevel levelA, Liquid b, Vector3i posB, LiquidLevel levelB)
        {
            if ((posA.Y <= posB.Y || !(a.Density > b.Density)) &&
                (posA.Y >= posB.Y || !(a.Density < b.Density))) return false;

            Game.World.SetLiquid(a, levelA, false, posB.X, posB.Y, posB.Z);

            a.TickSoon(posB.X, posB.Y, posB.Z, true);

            Game.World.SetLiquid(b, levelB, false, posA.X, posA.Y, posA.Z);

            b.TickSoon(posA.X, posA.Y, posA.Z, true);

            return true;
        }
    }
}