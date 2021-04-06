// <copyright file="LiquidContactManager.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Logic.Interfaces;

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
                case (nameof(Liquid.Lava), nameof(Liquid.Concrete)) or (nameof(Liquid.Concrete), nameof(Liquid.Lava)):
                    return LavaCooling(a, posA, levelA, posB, levelB);

                case (nameof(Liquid.Lava), nameof(Liquid.CrudeOil)) or (nameof(Liquid.CrudeOil), nameof(Liquid.Lava)):
                case (nameof(Liquid.Lava), nameof(Liquid.NaturalGas)) or (nameof(Liquid.NaturalGas), nameof(Liquid.Lava)):
                    return LavaBurn(a, posA, b, posB, isStaticB);

                case (nameof(Liquid.Concrete), nameof(Liquid.Water)) or (nameof(Liquid.Water), nameof(Liquid.Concrete)):
                case (nameof(Liquid.Milk), nameof(Liquid.Concrete)) or (nameof(Liquid.Concrete), nameof(Liquid.Milk)):
                    return ConcreteDissolve(a, posA, levelA, b, posB, levelB, isStaticB);

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
            if (posA.Y == posB.Y) return DensityLift(a, posA, levelA, b, posB, levelB);

            if ((posA.Y <= posB.Y || a.Density <= b.Density) &&
                (posA.Y >= posB.Y || a.Density >= b.Density)) return false;

            Game.World.SetLiquid(a, levelA, false, posB.X, posB.Y, posB.Z);

            a.TickSoon(posB.X, posB.Y, posB.Z, true);

            Game.World.SetLiquid(b, levelB, false, posA.X, posA.Y, posA.Z);

            b.TickSoon(posA.X, posA.Y, posA.Z, true);

            return true;
        }

        private static bool DensityLift(Liquid a, Vector3i posA, LiquidLevel levelA, Liquid b, Vector3i posB, LiquidLevel levelB)
        {
            Liquid dense;
            Vector3i densePos;
            LiquidLevel denseLevel;
            Liquid light;
            Vector3i lightPos;
            LiquidLevel lightLevel;

            if (a.Density > b.Density)
            {
                dense = a;
                light = b;

                densePos = posA;
                lightPos = posB;

                denseLevel = levelA;
                lightLevel = levelB;
            }
            else
            {
                dense = b;
                light = a;

                densePos = posB;
                lightPos = posA;

                denseLevel = levelB;
                lightLevel = levelA;
            }

            if (denseLevel == LiquidLevel.One) return false;

            (Block? aboveLightBlock, Liquid? aboveLightLiquid) = Game.World.GetPosition(lightPos.X, lightPos.Y + light.Direction, lightPos.Z, out _, out _, out _);

            if (aboveLightBlock is IFillable fillable && fillable.AllowInflow(lightPos.X, lightPos.Y + light.Direction, lightPos.Z, light.Direction > 0 ? BlockSide.Bottom : BlockSide.Top, light)
                                                      && aboveLightLiquid == Liquid.None)
            {
                Game.World.SetLiquid(light, lightLevel, true, lightPos.X, lightPos.Y + light.Direction, lightPos.Z);
                light.TickSoon(lightPos.X, lightPos.Y + light.Direction, lightPos.Z, true);

                Game.World.SetLiquid(dense, LiquidLevel.One, true, lightPos.X, lightPos.Y, lightPos.Z);
                dense.TickSoon(lightPos.X, lightPos.Y, lightPos.Z, true);

                Game.World.SetLiquid(dense, denseLevel - 1, true, densePos.X, densePos.Y, densePos.Z);
                dense.TickSoon(densePos.X, densePos.Y, densePos.Z, true);

                return true;
            }

            return false;
        }

        private static bool ConcreteDissolve(Liquid a, Vector3i posA, LiquidLevel levelA, Liquid b, Vector3i posB, LiquidLevel levelB, bool isStaticB)
        {
            LiquidLevel concreteLevel;
            Liquid other;
            Vector3i concretePos;
            Vector3i otherPos;
            bool tickOther;

            if (a == Liquid.Concrete)
            {
                concreteLevel = levelA;
                other = b;
                concretePos = posA;
                otherPos = posB;
                tickOther = isStaticB;
            }
            else
            {
                concreteLevel = levelB;
                other = a;
                concretePos = posB;
                otherPos = posA;
                tickOther = true;
            }

            other.TickSoon(otherPos.X, otherPos.Y, otherPos.Z, tickOther);

            Game.World.SetLiquid(Liquid.Water, concreteLevel, true, concretePos.X, concretePos.Y, concretePos.Z);
            Liquid.Water.TickSoon(concretePos.X, concretePos.Y, concretePos.Z, true);

            return true;
        }
    }
}