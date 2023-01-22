// <copyright file="FluidContactManager.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Interfaces;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Handles contacts between fluids.
/// </summary>
public class FluidContactManager
{
    private readonly CombinationMap<Fluid, ContactAction> map;

    /// <summary>
    ///     Create a new fluid contact manager.
    /// </summary>
    public FluidContactManager(Fluids fluids)
    {
        map = new CombinationMap<Fluid, ContactAction>(fluids.Count);

        map.AddCombination(
            fluids.Lava,
            ContactAction.LavaCooling,
            fluids.Water,
            fluids.Milk,
            fluids.Concrete,
            fluids.Beer,
            fluids.Wine,
            fluids.Honey);

        map.AddCombination(fluids.Lava, ContactAction.LavaBurn, fluids.CrudeOil, fluids.NaturalGas, fluids.Petrol);

        map.AddCombination(
            fluids.Concrete,
            ContactAction.ConcreteDissolve,
            fluids.Water,
            fluids.Milk,
            fluids.Beer,
            fluids.Wine);
    }

    /// <summary>
    ///     Handle the contact between two fluids.
    /// </summary>
    /// <param name="world">The world.</param>
    /// <param name="fluidA">The fluid that caused the contact.</param>
    /// <param name="posA">The position of fluid A.</param>
    /// <param name="fluidB">The other fluid.</param>
    /// <param name="posB">The position of fluid B.</param>
    /// <returns>If contact handling was successful and the flow step is complete.</returns>
    public bool HandleContact(World world, FluidInstance fluidA, Vector3i posA, FluidInstance fluidB,
        Vector3i posB)
    {
        Debug.Assert(fluidA != fluidB);

        var a = new ContactInformation(fluidA, posA);
        var b = new ContactInformation(fluidB, posB);

        return map.Resolve(a.fluid, b.fluid) switch
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
        Select(a, b, Fluids.Instance.Lava, out ContactInformation lava, out ContactInformation coolant);

        Block lavaBlock = world.GetBlock(lava.position)?.Block ?? Blocks.Instance.Air;

        if (lavaBlock.IsReplaceable || lavaBlock.Destroy(world, lava.position))
            world.SetContent(new Content(Blocks.Instance.Pumice), lava.position);

        world.SetFluid(
            Fluids.Instance.Steam.AsInstance(coolant.level, isStatic: false),
            coolant.position);

        Fluids.Instance.Steam.TickSoon(world, coolant.position, isStatic: true);

        return true;
    }

    private static bool LavaBurn(World world, ContactInformation a, ContactInformation b)
    {
        Select(a, b, Fluids.Instance.Lava, out ContactInformation lava, out ContactInformation burned);

        lava.fluid.TickSoon(world, lava.position, lava.isStatic);

        world.SetDefaultFluid(burned.position);
        Blocks.Instance.Fire.Place(world, burned.position);

        return true;
    }

    private static bool DensitySwap(World world, ContactInformation a, ContactInformation b)
    {
        if (a.position.Y == b.position.Y) return DensityLift(world, a, b);

        if ((a.position.Y <= b.position.Y || a.fluid.Density <= b.fluid.Density) &&
            (a.position.Y >= b.position.Y || a.fluid.Density >= b.fluid.Density)) return false;

        world.SetFluid(a.fluid.AsInstance(a.level, isStatic: false), b.position);
        a.fluid.TickSoon(world, b.position, isStatic: true);

        world.SetFluid(b.fluid.AsInstance(b.level, isStatic: false), a.position);
        b.fluid.TickSoon(world, a.position, isStatic: true);

        return true;
    }

    private static bool DensityLift(World world, ContactInformation a, ContactInformation b)
    {
        ContactInformation dense;
        ContactInformation light;

        if (a.fluid.Density > b.fluid.Density)
        {
            dense = a;
            light = b;
        }
        else
        {
            dense = b;
            light = a;
        }

        if (dense.level == FluidLevel.One) return false;

        Vector3i aboveLightPosition = light.position - light.fluid.FlowDirection;

        Content? content = world.GetContent(
            light.position - light.fluid.FlowDirection);

        if (content is not ({Block: IFillable fillable}, {} aboveLightFluid)) return false;

        if (!fillable.AllowInflow(
                world,
                aboveLightPosition,
                light.fluid.Direction.EntrySide().Opposite(),
                light.fluid) || aboveLightFluid.Fluid != Fluids.Instance.None) return false;

        world.SetFluid(
            light.fluid.AsInstance(light.level),
            aboveLightPosition);

        light.fluid.TickSoon(
            world,
            aboveLightPosition,
            isStatic: true);

        world.SetFluid(
            dense.fluid.AsInstance(FluidLevel.One),
            light.position);

        dense.fluid.TickSoon(world, light.position, isStatic: true);

        world.SetFluid(
            dense.fluid.AsInstance(dense.level - 1),
            dense.position);

        dense.fluid.TickSoon(world, dense.position, isStatic: true);

        return true;

    }

    private static bool ConcreteDissolve(World world, ContactInformation a, ContactInformation b)
    {
        Select(a, b, Fluids.Instance.Concrete, out ContactInformation concrete, out ContactInformation other);

        other.fluid.TickSoon(world, other.position, other.isStatic);

        world.SetFluid(
            Fluids.Instance.Water.AsInstance(concrete.level),
            concrete.position);

        Fluids.Instance.Water.TickSoon(world, concrete.position, isStatic: true);

        return true;
    }

    private static void Select(ContactInformation a, ContactInformation b, Fluid fluid,
        out ContactInformation selected, out ContactInformation other)
    {
        if (a.fluid == fluid)
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

    private readonly struct ContactInformation : IEquatable<ContactInformation>
    {
        public readonly Fluid fluid;
        public readonly Vector3i position;
        public readonly FluidLevel level;
        public readonly bool isStatic;

        public ContactInformation(FluidInstance fluid, Vector3i position)
        {
            this.fluid = fluid.Fluid;
            this.position = position;

            level = fluid.Level;
            isStatic = fluid.IsStatic;
        }

        public bool Equals(ContactInformation other)
        {
            return fluid.Equals(other.fluid) && position.Equals(other.position) && level == other.level &&
                   isStatic == other.isStatic;
        }

        public override bool Equals(object? obj)
        {
            return obj is ContactInformation other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(fluid, position, (int) level, isStatic);
        }

        public static bool operator ==(ContactInformation left, ContactInformation right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ContactInformation left, ContactInformation right)
        {
            return !left.Equals(right);
        }
    }
}

