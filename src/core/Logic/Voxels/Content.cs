// <copyright file="Content.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Voxels;

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
    ///     Get whether the fluid is either fresh water or seawater.
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
    /// Create a new content value. Do not use this within world generator code.
    /// </summary>
    /// <param name="block">The block, or the default block if not provided.</param>
    /// <param name="fluid">The fluid, or an empty fluid if not provided. Will have maximal level and be static.</param>
    /// <returns>>The created content.</returns>
    public static Content Create(Block? block = null, Fluid? fluid = null)
    {
        return new Content(block?.States.Default ?? DefaultState, fluid.AsInstance());
    }
    
    /// <summary>
    /// Create a new content value for world generation. Do not use this outside of world generator code.
    /// </summary>
    /// <param name="block">The block, or the generation default block if not provided.</param>
    /// <param name="fluid">The fluid, or an empty fluid if not provided. Will have maximal level and be static.</param>
    /// <returns>>The created content.</returns>
    public static Content CreateGenerated(Block? block = null, Fluid? fluid = null)
    {
        return new Content(block?.States.GenerationDefault ?? DefaultState, fluid.AsInstance());
    }
    
    private static Block DefaultBlock => Blocks.Instance.Core.Air;
    
    /// <summary>
    ///     Get the default state, which is always air.
    /// </summary>
    public static State DefaultState => DefaultBlock.States.Default;

    /// <summary>
    ///     Get the default content.
    /// </summary>
    public static Content Default => new(DefaultBlock.States.Default, FluidInstance.Default);

    /// <summary>
    ///     Get the generation default content.
    /// </summary>
    public static Content GenerationDefault => new(DefaultBlock.States.GenerationDefault, FluidInstance.Default);
    
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
    public static FluidInstance AsInstance(this Fluid? fluid, FluidLevel? level = null, Boolean isStatic = true)
    {
        return fluid is null ? FluidInstance.Default : new FluidInstance(fluid, level ?? FluidLevel.Full, isStatic);
    }
    #pragma warning restore S4226
}
