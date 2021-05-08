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
    public class LiquidContactManager
    {
        public bool HandleContact(Liquid liquidA, Vector3i posA, LiquidLevel levelA, Liquid liquidB, Vector3i posB, LiquidLevel levelB, bool isStaticB)
        {
            Debug.Assert(liquidA != liquidB);

            var a = new ContactInformation(liquidA, posA, levelA);
            var b = new ContactInformation(liquidB, posB, levelB, isStaticB);

            switch ((liquidA.NamedId, liquidB.NamedId))
            {
                case (nameof(Liquid.Lava), nameof(Liquid.Water)) or (nameof(Liquid.Water), nameof(Liquid.Lava)):
                case (nameof(Liquid.Milk), nameof(Liquid.Lava)) or (nameof(Liquid.Lava), nameof(Liquid.Milk)):
                case (nameof(Liquid.Lava), nameof(Liquid.Concrete)) or (nameof(Liquid.Concrete), nameof(Liquid.Lava)):
                case (nameof(Liquid.Lava), nameof(Liquid.Beer)) or (nameof(Liquid.Beer), nameof(Liquid.Lava)):
                case (nameof(Liquid.Lava), nameof(Liquid.Wine)) or (nameof(Liquid.Wine), nameof(Liquid.Lava)):
                case (nameof(Liquid.Lava), nameof(Liquid.Honey)) or (nameof(Liquid.Honey), nameof(Liquid.Lava)):
                    return LavaCooling(a, b);

                case (nameof(Liquid.Lava), nameof(Liquid.CrudeOil)) or (nameof(Liquid.CrudeOil), nameof(Liquid.Lava)):
                case (nameof(Liquid.Lava), nameof(Liquid.NaturalGas)) or (nameof(Liquid.NaturalGas), nameof(Liquid.Lava)):
                    return LavaBurn(a, b);

                case (nameof(Liquid.Concrete), nameof(Liquid.Water)) or (nameof(Liquid.Water), nameof(Liquid.Concrete)):
                case (nameof(Liquid.Milk), nameof(Liquid.Concrete)) or (nameof(Liquid.Concrete), nameof(Liquid.Milk)):
                case (nameof(Liquid.Beer), nameof(Liquid.Concrete)) or (nameof(Liquid.Concrete), nameof(Liquid.Beer)):
                case (nameof(Liquid.Wine), nameof(Liquid.Concrete)) or (nameof(Liquid.Concrete), nameof(Liquid.Wine)):
                    return ConcreteDissolve(a, b);

                default: return DensitySwap(a, b);
            }
        }

        private bool LavaCooling(ContactInformation a, ContactInformation b)
        {
            Select(a, b, Liquid.Lava, out ContactInformation lava, out ContactInformation coolant);

            Block lavaBlock = Game.World.GetBlock(lava.position.X, lava.position.Y, lava.position.Z, out _) ?? Block.Air;

            if (lavaBlock.IsReplaceable || lavaBlock.Destroy(lava.position.X, lava.position.Y, lava.position.Z))
            {
                Game.World.SetPosition(Block.Pumice, 0, Liquid.None, LiquidLevel.Eight, true, lava.position.X, lava.position.Y, lava.position.Z);
            }

            Game.World.SetLiquid(Liquid.Steam, coolant.level, false, coolant.position.X, coolant.position.Y, coolant.position.Z);

            Liquid.Steam.TickSoon(coolant.position.X, coolant.position.Y, coolant.position.Z, true);

            return true;
        }

        private bool LavaBurn(ContactInformation a, ContactInformation b)
        {
            Select(a, b, Liquid.Lava, out ContactInformation lava, out ContactInformation burned);

            lava.liquid.TickSoon(lava.position.X, lava.position.Y, lava.position.Z, lava.isStatic);

            Game.World.SetDefaultLiquid(burned.position.X, burned.position.Y, burned.position.Z);
            Block.Fire.Place(burned.position.X, burned.position.Y, burned.position.Z);

            return true;
        }

        private bool DensitySwap(ContactInformation a, ContactInformation b)
        {
            if (a.position.Y == b.position.Y) return DensityLift(a, b);

            if ((a.position.Y <= b.position.Y || a.liquid.Density <= b.liquid.Density) &&
                (a.position.Y >= b.position.Y || a.liquid.Density >= b.liquid.Density)) return false;

            Game.World.SetLiquid(a.liquid, a.level, false, b.position.X, b.position.Y, b.position.Z);

            a.liquid.TickSoon(b.position.X, b.position.Y, b.position.Z, true);

            Game.World.SetLiquid(b.liquid, b.level, false, a.position.X, a.position.Y, a.position.Z);

            b.liquid.TickSoon(a.position.X, a.position.Y, a.position.Z, true);

            return true;
        }

        private bool DensityLift(ContactInformation a, ContactInformation b)
        {
            ContactInformation dense;
            ContactInformation light;

            if (a.liquid.Density > b.liquid.Density)
            {
                dense = a;
                light = b;
            }
            else
            {
                dense = b;
                light = a;
            }

            if (dense.level == LiquidLevel.One) return false;

            (Block? aboveLightBlock, Liquid? aboveLightLiquid) = Game.World.GetPosition(light.position.X, light.position.Y + light.liquid.Direction, light.position.Z, out _, out _, out _);

            if (aboveLightBlock is IFillable fillable && fillable.AllowInflow(light.position.X, light.position.Y + light.liquid.Direction, light.position.Z, light.liquid.Direction > 0 ? BlockSide.Bottom : BlockSide.Top, light.liquid)
                                                      && aboveLightLiquid == Liquid.None)
            {
                Game.World.SetLiquid(light.liquid, light.level, true, light.position.X, light.position.Y + light.liquid.Direction, light.position.Z);
                light.liquid.TickSoon(light.position.X, light.position.Y + light.liquid.Direction, light.position.Z, true);

                Game.World.SetLiquid(dense.liquid, LiquidLevel.One, true, light.position.X, light.position.Y, light.position.Z);
                dense.liquid.TickSoon(light.position.X, light.position.Y, light.position.Z, true);

                Game.World.SetLiquid(dense.liquid, dense.level - 1, true, dense.position.X, dense.position.Y, dense.position.Z);
                dense.liquid.TickSoon(dense.position.X, dense.position.Y, dense.position.Z, true);

                return true;
            }

            return false;
        }

        private bool ConcreteDissolve(ContactInformation a, ContactInformation b)
        {
            Select(a, b, Liquid.Concrete, out ContactInformation concrete, out ContactInformation other);

            other.liquid.TickSoon(other.position.X, other.position.Y, other.position.Z, other.isStatic);

            Game.World.SetLiquid(Liquid.Water, concrete.level, true, concrete.position.X, concrete.position.Y, concrete.position.Z);
            Liquid.Water.TickSoon(concrete.position.X, concrete.position.Y, concrete.position.Z, true);

            return true;
        }

        private static void Select(ContactInformation a, ContactInformation b, Liquid liquid, out ContactInformation selected, out ContactInformation other)
        {
            if (a.liquid == liquid)
            {
                selected = a;
                other = b;
            }
            else
            {
                selected = b;
                other = a;
            }
        }

        private readonly struct ContactInformation
        {
            public readonly Liquid liquid;
            public readonly Vector3i position;
            public readonly LiquidLevel level;
            public readonly bool isStatic;

            public ContactInformation(Liquid liquid, Vector3i position, LiquidLevel level, bool isStatic = true)
            {
                this.liquid = liquid;
                this.position = position;
                this.level = level;
                this.isStatic = isStatic;
            }
        }
    }
}