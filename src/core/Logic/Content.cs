// <copyright file="Content.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core.Logic;

/// <summary>
///     A specific instance of a block.
/// </summary>
/// <param name="Block">The block.</param>
/// <param name="Data">The data of the block.</param>
public readonly record struct BlockInstance(Block Block, uint Data)
{
    /// <summary>
    ///     Get the default block instance.
    /// </summary>
    public static BlockInstance Default => new(Block.Air, Data: 0);

    /// <inheritdoc cref="IBlockBase.IsSolidAndFull(uint)" />
    public bool IsSolidAndFull => Block.Base.IsSolidAndFull(Data);

    /// <inheritdoc cref="IBlockBase.IsOpaqueAndFull(uint)" />
    public bool IsOpaqueAndFull => Block.Base.IsOpaqueAndFull(Data);

    /// <inheritdoc cref="IBlockBase.IsSideFull" />
    public bool IsSideFull(BlockSide side)
    {
        return Block.Base.IsSideFull(side, Data);
    }
}

/// <summary>
///     A specific instance of a fluid.
/// </summary>
/// <param name="Fluid">The fluid.</param>
/// <param name="Level">The level of the fluid.</param>
/// <param name="IsStatic">Whether the fluid is static.</param>
public record struct FluidInstance(Fluid Fluid, FluidLevel Level, bool IsStatic)
{
    /// <summary>
    ///     Get the default fluid instance.
    /// </summary>
    public static FluidInstance Default => new(Fluid.None, FluidLevel.Eight, IsStatic: true);
}

/// <summary>
///     The content of a position in the world.
/// </summary>
/// <param name="Block">The block instance.</param>
/// <param name="Fluid">The fluid instance.</param>
public record struct Content(BlockInstance Block, FluidInstance Fluid)
{
    /// <summary>
    ///     Create a new content instance.
    /// </summary>
    /// <param name="block">The block instance. The data is assumed to be 0.</param>
    /// <param name="fluid">The fluid instance. The level is assumed to be maximal and the fluid is assumed to be static.</param>
    public Content(Block? block = null, Fluid? fluid = null) : this(block.AsInstance(), fluid.AsInstance()) {}

    /// <summary>
    ///     Get the default content.
    /// </summary>
    public static Content Default => new(BlockInstance.Default, FluidInstance.Default);
}

/// <summary>
///     Extends the <see cref="BlockInstance" /> and <see cref="FluidInstance" /> classes.
/// </summary>
public static class ContentExtensions
{
    #pragma warning disable S4226 // Extensions can handle null references in their first argument
    /// <summary>
    ///     Get a block as instance.
    /// </summary>
    public static BlockInstance AsInstance(this Block? block, uint data = 0)
    {
        return block is null ? BlockInstance.Default : new BlockInstance(block, data);
    }

    /// <summary>
    ///     Get a fluid as instance.
    /// </summary>
    public static FluidInstance AsInstance(this Fluid? fluid, FluidLevel level = FluidLevel.Eight,
        bool isStatic = true)
    {
        return fluid is null ? FluidInstance.Default : new FluidInstance(fluid, level, isStatic);
    }
    #pragma warning restore S4226
}
