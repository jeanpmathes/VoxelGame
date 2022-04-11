// <copyright file="Content.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

// XML-Documentation for Records seems to not really work...

#pragma warning disable CS1591
#pragma warning disable CS1572
#pragma warning disable CS1573

namespace VoxelGame.Core.Logic;

/// <summary>
///     A specific instance of a block.
/// </summary>
/// <param name="Block">The block.</param>
/// <param name="Data">The data of the block.</param>
public record struct BlockInstance(Block Block, uint Data)
{
    /// <summary>
    ///     Get the default block instance.
    /// </summary>
    public static BlockInstance Default => new(Block.Air, Data: 0);
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
///     Extends the <see cref="BlockInstance" /> and <see cref="FluidInstance" /> classes.
/// </summary>
public static class ContentExtensions
{
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
}
