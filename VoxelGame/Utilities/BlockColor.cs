// <copyright file="BlockColor.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Logic;
using VoxelGame.Visuals;

namespace VoxelGame.Utilities
{
    /// <summary>
    /// A set of colors that can be stored in up to four bits, less bits are supported too.
    /// </summary>
    public enum BlockColor
    {
        Default = 0b0,

        Red = 0b01,
        Green = 0b10,
        Blue = 0b11,

        Yellow = 0b100,
        Cyan = 0b101,
        Magenta = 0b110,
        Orange = 0b111,

        DarkGreen = 0b1000,
        Lime = 0b1001,
        Gray = 0b1010,
        Indigo = 0b1011,
        Maroon = 0b1100,
        Olive = 0b1101,
        Brown = 0b1110,
        Navy = 0b1111,
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
                BlockColor.Orange => TintColor.Orange,

                BlockColor.DarkGreen => TintColor.DarkGreen,
                BlockColor.Lime => TintColor.Lime,
                BlockColor.Gray => TintColor.Gray,
                BlockColor.Indigo => TintColor.Indigo,
                BlockColor.Maroon => TintColor.Maroon,
                BlockColor.Olive => TintColor.Olive,
                BlockColor.Brown => TintColor.Brown,
                BlockColor.Navy => TintColor.Navy,

                _ => new TintColor(1f, 1f, 1f),
            };
        }
    }
}