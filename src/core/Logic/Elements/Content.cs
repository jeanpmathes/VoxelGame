// <copyright file="Content.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Elements;

/// <summary>
///     A specific instance of a fluid.
/// </summary>
/// <param name="Fluid">The fluid.</param>
/// <param name="Level">The level of the fluid.</param>
/// <param name="IsStatic">Whether the fluid is static.</param>
public readonly record struct FluidInstance(Fluid Fluid, FluidLevel Level, Boolean IsStatic)
{
    /// <summary>
    ///     Get the default fluid instance.
    /// </summary>
    public static FluidInstance Default => new(Fluids.Instance.None, FluidLevel.Eight, IsStatic: true);

    /// <summary>
    ///     Get whether the fluid is either fresh water or sea water.
    /// </summary>
    public Boolean IsAnyWater => Fluid == Fluids.Instance.FreshWater || Fluid == Fluids.Instance.SeaWater;

    /// <summary>
    ///     Whether the fluid is empty.
    /// </summary>
    public Boolean IsEmpty => Fluid == Fluids.Instance.None;
}

/// <summary>
///     The content of a position in the world.
/// </summary>
/// <param name="Block">The block instance.</param>
/// <param name="Fluid">The fluid instance.</param>
public record struct Content(State Block, FluidInstance Fluid)
{
    /// <summary>
    ///     Create a new content instance.
    /// </summary>
    /// <param name="block">The block instance. The data is assumed to be 0.</param>
    /// <param name="fluid">The fluid instance. The level is assumed to be maximal and the fluid is assumed to be static.</param>
    public Content(Block? block = null, Fluid? fluid = null) : this(block?.States.Default ?? DefaultState, fluid.AsInstance()) {}

    /// <summary>
    ///     Get the default state, which is always air.
    /// </summary>
    public static State DefaultState => Blocks.Instance.Core.Air.States.Default;

    /// <summary>
    ///     Get the default content.
    /// </summary>
    public static Content Default => new(DefaultState, FluidInstance.Default);

    /// <summary>
    ///     Whether the content is empty.
    /// </summary>
    public Boolean IsEmpty => this == Default;

    /// <summary>
    ///     Whether the block is replaceable and the fluid is empty,
    ///     allowing to set the block to a new value without any problems.
    /// </summary>
    public Boolean IsSettable => Block.IsReplaceable && Fluid.IsEmpty;

    /// <inheritdoc />
    public override String ToString()
    {
        return $"Content(Block: {Block}, Fluid: {Fluid})";
    }
}

/// <summary>
///     Extends the <see cref="FluidInstance" /> type.
/// </summary>
public static class ContentExtensions
{
    #pragma warning disable S4226 // Extensions can handle null references in their first argument
    /// <summary>
    ///     Get a fluid as instance.
    /// </summary>
    public static FluidInstance AsInstance(this Fluid? fluid, FluidLevel level = FluidLevel.Eight,
        Boolean isStatic = true)
    {
        return fluid is null ? FluidInstance.Default : new FluidInstance(fluid, level, isStatic);
    }
    #pragma warning restore S4226
}
