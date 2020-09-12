// <copyright file="BlockColor.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Utilities
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

        Amaranth = 0b10000,
        Amber = 0b10001,
        Apricot = 0b10010,
        Aquamarine = 0b10011,
        Beige = 0b10100,
        Coffee = 0b10101,
        Coral = 0b10110,
        Crimson = 0b10111,
        Emerald = 0b11000,
        Lilac = 0b11001,
        Mauve = 0b11010,
        Periwinkle = 0b11011,
        PrussianBlue = 0b11100,
        SlateGray = 0b11101,
        Taupe = 0b11110,
        Viridian = 0b11111,
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

                BlockColor.Amaranth => TintColor.Amaranth,
                BlockColor.Amber => TintColor.Amber,
                BlockColor.Apricot => TintColor.Apricot,
                BlockColor.Aquamarine => TintColor.Aquamarine,
                BlockColor.Beige => TintColor.Beige,
                BlockColor.Coffee => TintColor.Coffee,
                BlockColor.Coral => TintColor.Coral,
                BlockColor.Crimson => TintColor.Crimson,
                BlockColor.Emerald => TintColor.Emerald,
                BlockColor.Lilac => TintColor.Lilac,
                BlockColor.Mauve => TintColor.Mauve,
                BlockColor.Periwinkle => TintColor.Periwinkle,
                BlockColor.PrussianBlue => TintColor.PrussianBlue,
                BlockColor.SlateGray => TintColor.SlateGray,
                BlockColor.Taupe => TintColor.Taupe,
                BlockColor.Viridian => TintColor.Viridian,

                _ => new TintColor(1f, 1f, 1f),
            };
        }
    }
}