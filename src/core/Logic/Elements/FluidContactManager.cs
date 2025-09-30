// <copyright file="FluidContactManager.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Fluids;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements;

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
    ///     Handle the contact between two fluids. Flow from position A to position B must be allowed.
    /// </summary>
    /// <param name="world">The world.</param>
    /// <param name="fluidA">The fluid that caused the contact.</param>
    /// <param name="posA">The position of fluid A.</param>
    /// <param name="fluidB">The other fluid.</param>
    /// <param name="posB">The position of fluid B.</param>
    /// <returns>If contact handling was successful and the flow step is complete.</returns>
    public Boolean HandleContact(World world, FluidInstance fluidA, Vector3i posA, FluidInstance fluidB,
        Vector3i posB)
    {
        Debug.Assert(fluidA != fluidB);

        var a = new ContactInformation(fluidA, posA);
        var b = new ContactInformation(fluidB, posB);

        ContactAction action = map.Resolve(a.fluid, b.fluid);

        return action switch
        {
            ContactAction.Default => SwapByDensity(world, a, b),
            ContactAction.CoolLava => CoolLava(world, a, b),
            ContactAction.BurnWithLava => BurnWithLava(world, a, b),
            ContactAction.DissolveConcrete => DissolveConcrete(world, a, b),
            ContactAction.MixWater => MixWater(world, a, b),
            _ => throw Exceptions.UnsupportedEnumValue(action)
        };
    }

    /// <summary>
    ///     Cool lava, turning it into pumice and the coolant into steam.
    /// </summary>
    private static Boolean CoolLava(World world, ContactInformation a, ContactInformation b)
    {
        Select(a, b, Fluids.Instance.Lava, out ContactInformation lava, out ContactInformation coolant);

        State lavaBlock = world.GetBlock(lava.position) ?? Content.DefaultState;

        if (lavaBlock.IsReplaceable || lavaBlock.Block.Destroy(world, lava.position))
            world.SetContent(new Content(Blocks.Instance.Stones.Pumice.Base), lava.position);

        SetFluid(world, coolant.position, Fluids.Instance.Steam, coolant.level);

        return true;
    }

    /// <summary>
    ///     Let lava burn the other fluid.
    /// </summary>
    private static Boolean BurnWithLava(World world, ContactInformation a, ContactInformation b)
    {
        Select(a, b, Fluids.Instance.Lava, out ContactInformation lava, out ContactInformation burned);

        lava.fluid.UpdateSoon(world, lava.position, lava.isStatic);

        world.SetDefaultFluid(burned.position);
        Blocks.Instance.Environment.Fire.Place(world, burned.position);

        return true;
    }

    /// <summary>
    ///     Swap the fluids if they are of different densities.
    /// </summary>
    private static Boolean SwapByDensity(World world, ContactInformation a, ContactInformation b)
    {
        if (MathTools.NearlyEqual(a.fluid.Density, b.fluid.Density)) return false;

        if (a.position.Y == b.position.Y) return DensityLift(world, a, b);

        if ((a.position.Y <= b.position.Y || a.fluid.Density <= b.fluid.Density) &&
            (a.position.Y >= b.position.Y || a.fluid.Density >= b.fluid.Density)) return false;

        if (!IsFlowAllowed(world, b.position, a.position)) return false;

        SetFluid(world, b.position, a.fluid, a.level);
        SetFluid(world, a.position, b.fluid, b.level);

        return true;
    }

    /// <summary>
    ///     Lift the fluid with the lower density, and move the heavier fluid to the old position of the lighter fluid.
    /// </summary>
    private static Boolean DensityLift(World world, ContactInformation a, ContactInformation b)
    {
        (ContactInformation light, ContactInformation dense) = MathTools.ArgMinMax((a.fluid.Density, a), (b.fluid.Density, b));

        if (dense.level == FluidLevel.One) return false;

        Vector3i aboveLightPosition = light.position - light.fluid.FlowDirection;

        Content? content = world.GetContent(
            light.position - light.fluid.FlowDirection);
        
        if (content?.Block.Block.Is<Fillable>() != true) return false;
        if (content is not {Fluid: var aboveLightFluid}) return false;
        if (!IsFlowAllowed(world, light.position, aboveLightPosition) || !aboveLightFluid.IsEmpty) return false;

        SetFluid(world, aboveLightPosition, light.fluid, light.level);
        SetFluid(world, light.position, dense.fluid, FluidLevel.One);
        SetFluid(world, dense.position, dense.fluid, dense.level - 1);

        return true;

    }

    /// <summary>
    ///     Dissolve concrete into fresh water.
    /// </summary>
    private static Boolean DissolveConcrete(World world, ContactInformation a, ContactInformation b)
    {
        Select(a, b, Fluids.Instance.Concrete, out ContactInformation concrete, out ContactInformation other);

        other.fluid.UpdateSoon(world, other.position, other.isStatic);

        SetFluid(world, concrete.position, Fluids.Instance.FreshWater, concrete.level);

        return true;
    }

    /// <summary>
    ///     Mixes fresh water with sea water, turning the fresh water into sea water.
    /// </summary>
    private static Boolean MixWater(World world, ContactInformation a, ContactInformation b)
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

        fluid.UpdateSoon(world, position, isStatic: true);
    }

    private static Boolean IsFlowAllowed(World world, Vector3i from, Vector3i to)
    {
        Content? fromContent = world.GetContent(from);
        Content? toContent = world.GetContent(to);
        
        if (fromContent?.Block.Block.Get<Fillable>() is not {} source) return false;
        if (toContent?.Block.Block.Get<Fillable>() is not {} target) return false;
        
        if (fromContent is not {Fluid.Fluid: {} fluid}) return false;

        var side = (to - from).ToSide();

        return source.CanOutflow(world, from, side, fluid) && target.CanInflow(world, to, side.Opposite(), fluid);
    }

    private enum ContactAction
    {
        Default,
        CoolLava,
        BurnWithLava,
        DissolveConcrete,
        MixWater
    }

    private readonly struct ContactInformation(FluidInstance fluid, Vector3i position) : IEquatable<ContactInformation>
    {
        public readonly Fluid fluid = fluid.Fluid;
        public readonly Vector3i position = position;
        public readonly FluidLevel level = fluid.Level;
        public readonly Boolean isStatic = fluid.IsStatic;

        public Boolean Equals(ContactInformation other)
        {
            return fluid.Equals(other.fluid) && position.Equals(other.position) && level == other.level &&
                   isStatic == other.isStatic;
        }

        public override Boolean Equals(Object? obj)
        {
            return obj is ContactInformation other && Equals(other);
        }

        public override Int32 GetHashCode()
        {
            return HashCode.Combine(fluid, position, (Int32) level, isStatic);
        }

        public static Boolean operator ==(ContactInformation left, ContactInformation right)
        {
            return left.Equals(right);
        }

        public static Boolean operator !=(ContactInformation left, ContactInformation right)
        {
            return !left.Equals(right);
        }
    }
}
