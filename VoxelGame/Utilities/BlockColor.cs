// <copyright file="BlockColor.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Visuals;

namespace VoxelGame.Utilities
{
    /// <summary>
    /// A set of colors that can be stored in three bits.
    /// </summary>
    public enum BlockColor
    {
        Default,
        Red,
        Green,
        Blue,
        Yellow,
        Cyan,
        Magenta
    }

    public static class BlockColorExtensions
    {
        /// <summary>
        /// Converts a <see cref="BlockColor"/> to a <see cref="TintColor"/>.
        /// </summary>
        /// <param name="color">The <see cref="BlockColor"/> to convert.</param>
        /// <returns>The resulting TintColor.</returns>
        public static TintColor ToTintColor(this BlockColor color)
        {
            return color switch
            {
                BlockColor.Red => TintColor.Red,
                BlockColor.Green => TintColor.Green,
                BlockColor.Blue => TintColor.Blue,
                BlockColor.Yellow => TintColor.Yellow,
                BlockColor.Cyan => TintColor.Cyan,
                BlockColor.Magenta => TintColor.Magenta,
                _ => new TintColor(1f, 1f, 1f),
            };
        }
    }
}