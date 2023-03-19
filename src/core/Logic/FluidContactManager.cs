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
using VoxelGame.Core.Utilities;

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
            ContactAction.CoolLava,
            fluids.FreshWater,
            fluids.SeaWater,
            fluids.Milk,
            fluids.Concrete,
            fluids.Beer,
            fluids.Wine,
            fluids.Honey);

        map.AddCombination(fluids.Lava, ContactAction.BurnWithLava, fluids.CrudeOil, fluids.NaturalGas, fluids.Petrol);

        map.AddCombination(
            fluids.Concrete,
            ContactAction.DissolveConcrete,
            fluids.FreshWater,
            fluids.SeaWater,
            fluids.Milk,
            fluids.Beer,
            fluids.Wine);

        map.AddCombination(fluids.SeaWater, ContactAction.MixWater, fluids.FreshWater);
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
            ContactAction.Default => SwapByDensity(world, a, b),
            ContactAction.CoolLava => CoolLava(world, a, b),
            ContactAction.BurnWithLava => BurnWithLava(world, a, b),
            ContactAction.DissolveConcrete => DissolveConcrete(world, a, b),
            ContactAction.MixWater => MixWater(world, a, b),
            _ => throw new NotSupportedException()
        };
    }

    /// <summary>
    ///     Cool lava, turning it into pumice and the coolant into steam.
    /// </summary>
    private static bool CoolLava(World world, ContactInformation a, ContactInformation b)
    {
        Select(a, b, Fluids.Instance.Lava, out ContactInformation lava, out ContactInformation coolant);

        Block lavaBlock = world.GetBlock(lava.position)?.Block ?? Blocks.Instance.Air;

        if (lavaBlock.IsReplaceable || lavaBlock.Destroy(world, lava.position))
            world.SetContent(new Content(Blocks.Instance.Pumice), lava.position);

        SetFluid(world, coolant.position, Fluids.Instance.Steam, coolant.level);

        return true;
    }

    /// <summary>
    ///     Let lava burn the other fluid.
    /// </summary>
    private static bool BurnWithLava(World world, ContactInformation a, ContactInformation b)
    {
        Select(a, b, Fluids.Instance.Lava, out ContactInformation lava, out ContactInformation burned);

        lava.fluid.TickSoon(world, lava.position, lava.isStatic);

        world.SetDefaultFluid(burned.position);
        Blocks.Instance.Fire.Place(world, burned.position);

        return true;
    }

    /// <summary>
    ///     Swap the fluids if they are of different densities.
    /// </summary>
    private static bool SwapByDensity(World world, ContactInformation a, ContactInformation b)
    {
        if (VMath.NearlyEqual(a.fluid.Density, b.fluid.Density)) return false;

        if (a.position.Y == b.position.Y) return DensityLift(world, a, b);

        if ((a.position.Y <= b.position.Y || a.fluid.Density <= b.fluid.Density) &&
            (a.position.Y >= b.position.Y || a.fluid.Density >= b.fluid.Density)) return false;

        SetFluid(world, b.position, a.fluid, a.level);
        SetFluid(world, a.position, b.fluid, b.level);

        return true;
    }

    /// <summary>
    ///     Lift the fluid with the lower density, and move the heavier fluid to the old position of the lighter fluid.
    /// </summary>
    private static bool DensityLift(World world, ContactInformation a, ContactInformation b)
    {
        (ContactInformation light, ContactInformation dense) = VMath.ArgMinMax((a.fluid.Density, a), (b.fluid.Density, b));

        if (dense.level == FluidLevel.One) return false;

        Vector3i aboveLightPosition = light.position - light.fluid.FlowDirection;

        Content? content = world.GetContent(
            light.position - light.fluid.FlowDirection);

        if (content is not ({Block: IFillable fillable}, var aboveLightFluid)) return false;

        if (!fillable.IsInflowAllowed(
                world,
                aboveLightPosition,
                light.fluid.Direction.EntrySide().Opposite(),
                light.fluid) || aboveLightFluid.Fluid != Fluids.Instance.None) return false;

        SetFluid(world, aboveLightPosition, light.fluid, light.level);
        SetFluid(world, light.position, dense.fluid, FluidLevel.One);
        SetFluid(world, dense.position, dense.fluid, dense.level - 1);

        return true;

    }

    /// <summary>
    ///     Dissolve concrete into fresh water.
    /// </summary>
    private static bool DissolveConcrete(World world, ContactInformation a, ContactInformation b)
    {
        Select(a, b, Fluids.Instance.Concrete, out ContactInformation concrete, out ContactInformation other);

        other.fluid.TickSoon(world, other.position, other.isStatic);

        SetFluid(world, concrete.position, Fluids.Instance.FreshWater, concrete.level);

        return true;
    }

    /// <summary>
    ///     Mixes fresh water with sea water, turning the fresh water into sea water.
    /// </summary>
    private static bool MixWater(World world, ContactInformation a, ContactInformation b)
    {
        Select(a, b, Fluids.Instance.FreshWater, out ContactInformation fresh, out _);
        SetFluid(world, fresh.position, Fluids.Instance.SeaWater, fresh.level);

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

    private static void SetFluid(World world, Vector3i position, Fluid fluid, FluidLevel level)
    {
        world.SetFluid(
            fluid.AsInstance(level),
            position);

        fluid.TickSoon(world, position, isStatic: true);
    }

    private enum ContactAction
    {
        Default,
        CoolLava,
        BurnWithLava,
        DissolveConcrete,
        MixWater
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
