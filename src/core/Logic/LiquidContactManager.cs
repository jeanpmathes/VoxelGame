// <copyright file="LiquidContactManager.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Logic
{
    /// <summary>
    ///     Handles contacts between liquids.
    /// </summary>
    public class LiquidContactManager
    {
        private readonly CombinationMap<Liquid, ContactAction> map =
            new(Liquid.Count);

        /// <summary>
        ///     Create a new liquid contact manager.
        /// </summary>
        public LiquidContactManager()
        {
            map.AddCombination(
                Liquid.Lava,
                ContactAction.LavaCooling,
                Liquid.Water,
                Liquid.Milk,
                Liquid.Concrete,
                Liquid.Beer,
                Liquid.Wine,
                Liquid.Honey);

            map.AddCombination(Liquid.Lava, ContactAction.LavaBurn, Liquid.CrudeOil, Liquid.NaturalGas, Liquid.Petrol);

            map.AddCombination(
                Liquid.Concrete,
                ContactAction.ConcreteDissolve,
                Liquid.Water,
                Liquid.Milk,
                Liquid.Beer,
                Liquid.Wine);
        }

        /// <summary>
        ///     Handle the contact between two liquids.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="liquidA">The liquid that caused the contact.</param>
        /// <param name="posA">The position of liquid A.</param>
        /// <param name="liquidB">The other liquid.</param>
        /// <param name="posB">The position of liquid B.</param>
        /// <returns>If contact handling was successful and the flow step is complete.</returns>
        public bool HandleContact(World world, LiquidInstance liquidA, Vector3i posA, LiquidInstance liquidB,
            Vector3i posB)
        {
            Debug.Assert(liquidA != liquidB);

            var a = new ContactInformation(liquidA, posA);
            var b = new ContactInformation(liquidB, posB);

            return map.Resolve(a.liquid, b.liquid) switch
            {
                ContactAction.Default => DensitySwap(world, a, b),
                ContactAction.LavaCooling => LavaCooling(world, a, b),
                ContactAction.LavaBurn => LavaBurn(world, a, b),
                ContactAction.ConcreteDissolve => ConcreteDissolve(world, a, b),
                _ => throw new NotSupportedException()
            };
        }

        private static bool LavaCooling(World world, ContactInformation a, ContactInformation b)
        {
            Select(a, b, Liquid.Lava, out ContactInformation lava, out ContactInformation coolant);

            Block lavaBlock = world.GetBlock(lava.position)?.Block ?? Block.Air;

            if (lavaBlock.IsReplaceable || lavaBlock.Destroy(world, lava.position))
                world.SetPosition(
                    Block.Pumice,
                    data: 0,
                    Liquid.None,
                    LiquidLevel.Eight,
                    isStatic: true,
                    lava.position);

            world.SetLiquid(
                Liquid.Steam.AsInstance(coolant.level, isStatic: false),
                coolant.position);

            Liquid.Steam.TickSoon(world, coolant.position, isStatic: true);

            return true;
        }

        private static bool LavaBurn(World world, ContactInformation a, ContactInformation b)
        {
            Select(a, b, Liquid.Lava, out ContactInformation lava, out ContactInformation burned);

            lava.liquid.TickSoon(world, lava.position, lava.isStatic);

            world.SetDefaultLiquid(burned.position);
            Block.Fire.Place(world, burned.position);

            return true;
        }

        private static bool DensitySwap(World world, ContactInformation a, ContactInformation b)
        {
            if (a.position.Y == b.position.Y) return DensityLift(world, a, b);

            if ((a.position.Y <= b.position.Y || a.liquid.Density <= b.liquid.Density) &&
                (a.position.Y >= b.position.Y || a.liquid.Density >= b.liquid.Density)) return false;

            world.SetLiquid(a.liquid.AsInstance(a.level, isStatic: false), b.position);
            a.liquid.TickSoon(world, b.position, isStatic: true);

            world.SetLiquid(b.liquid.AsInstance(b.level, isStatic: false), a.position);
            b.liquid.TickSoon(world, a.position, isStatic: true);

            return true;
        }

        private static bool DensityLift(World world, ContactInformation a, ContactInformation b)
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

            Vector3i aboveLightPosition = light.position - light.liquid.FlowDirection;

            (BlockInstance? aboveLightBlock, LiquidInstance? aboveLightLiquid) = world.GetContent(
                light.position - light.liquid.FlowDirection);

            if (aboveLightBlock?.Block is IFillable fillable && fillable.AllowInflow(
                                                                 world,
                                                                 aboveLightPosition,
                                                                 light.liquid.Direction.EntrySide().Opposite(),
                                                                 light.liquid)
                                                             && aboveLightLiquid?.Liquid == Liquid.None)
            {
                world.SetLiquid(
                    light.liquid.AsInstance(light.level, isStatic: true),
                    aboveLightPosition);

                light.liquid.TickSoon(
                    world,
                    aboveLightPosition,
                    isStatic: true);

                world.SetLiquid(
                    dense.liquid.AsInstance(LiquidLevel.One, isStatic: true),
                    light.position);

                dense.liquid.TickSoon(world, light.position, isStatic: true);

                world.SetLiquid(
                    dense.liquid.AsInstance(dense.level - 1, isStatic: true),
                    dense.position);

                dense.liquid.TickSoon(world, dense.position, isStatic: true);

                return true;
            }

            return false;
        }

        private static bool ConcreteDissolve(World world, ContactInformation a, ContactInformation b)
        {
            Select(a, b, Liquid.Concrete, out ContactInformation concrete, out ContactInformation other);

            other.liquid.TickSoon(world, other.position, other.isStatic);

            world.SetLiquid(
                Liquid.Water.AsInstance(concrete.level, isStatic: true),
                concrete.position);

            Liquid.Water.TickSoon(world, concrete.position, isStatic: true);

            return true;
        }

        private static void Select(ContactInformation a, ContactInformation b, Liquid liquid,
            out ContactInformation selected, out ContactInformation other)
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

        private enum ContactAction
        {
            Default,
            LavaCooling,
            LavaBurn,
            ConcreteDissolve
        }

        private readonly struct ContactInformation
        {
            public readonly Liquid liquid;
            public readonly Vector3i position;
            public readonly LiquidLevel level;
            public readonly bool isStatic;

            public ContactInformation(LiquidInstance liquid, Vector3i position)
            {
                this.liquid = liquid.Liquid;
                this.position = position;

                level = liquid.Level;
                isStatic = liquid.IsStatic;
            }
        }
    }
}
