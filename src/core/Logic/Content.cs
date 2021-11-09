// <copyright file="Content.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core.Logic
{
    public record BlockInstance(Block Block, uint Data)
    {
        public static BlockInstance Default => new(Block.Air, Data: 0);
    }

    public record LiquidInstance(Liquid Liquid, LiquidLevel Level, bool IsStatic)
    {
        public static LiquidInstance Default => new(Liquid.None, LiquidLevel.Eight, IsStatic: true);
    }

    public static class ContentExtensions
    {
        public static BlockInstance AsInstance(this Block? block, uint data = 0)
        {
            return block is null ? BlockInstance.Default : new BlockInstance(block, data);
        }

        public static LiquidInstance AsInstance(this Liquid? liquid, LiquidLevel level = LiquidLevel.Eight,
            bool isStatic = true)
        {
            return liquid is null ? LiquidInstance.Default : new LiquidInstance(liquid, level, isStatic);
        }
    }
}