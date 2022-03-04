// <copyright file="Content.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

// XML-Documentation for Records seems to not really work...

#pragma warning disable CS1591
#pragma warning disable CS1572
#pragma warning disable CS1573

namespace VoxelGame.Core.Logic
{
    /// <summary>
    ///     A specific instance of a block.
    /// </summary>
    /// <param name="Block">The block.</param>
    /// <param name="Data">The data of the block.</param>
    public record BlockInstance(Block Block, uint Data)
    {
        /// <summary>
        ///     Get the default block instance.
        /// </summary>
        public static BlockInstance Default => new(Block.Air, Data: 0);
    }

    /// <summary>
    ///     A specific instance of a liquid.
    /// </summary>
    /// <param name="Liquid">The liquid.</param>
    /// <param name="Level">The level of the liquid.</param>
    /// <param name="IsStatic">Whether the liquid is static.</param>
    public record LiquidInstance(Liquid Liquid, LiquidLevel Level, bool IsStatic)
    {
        /// <summary>
        ///     Get the default liquid instance.
        /// </summary>
        public static LiquidInstance Default => new(Liquid.None, LiquidLevel.Eight, IsStatic: true);
    }

    /// <summary>
    ///     Extends the <see cref="BlockInstance" /> and <see cref="LiquidInstance" /> classes.
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
        ///     Get a liquid as instance.
        /// </summary>
        public static LiquidInstance AsInstance(this Liquid? liquid, LiquidLevel level = LiquidLevel.Eight,
            bool isStatic = true)
        {
            return liquid is null ? LiquidInstance.Default : new LiquidInstance(liquid, level, isStatic);
        }
    }
}
