// <copyright file="BlockColor.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     A set of colors that can be stored in up to four bits, less bits are supported too.
/// </summary>
public enum BlockColor
{
    /// <summary>
    ///     The default color.
    /// </summary>
    Default = 0b0,

    /// <summary>
    ///     Red color.
    /// </summary>
    Red = 0b01,

    /// <summary>
    ///     Green color.
    /// </summary>
    Green = 0b10,

    /// <summary>
    ///     Blue color.
    /// </summary>
    Blue = 0b11,

    /// <summary>
    ///     Yellow color.
    /// </summary>
    Yellow = 0b100,

    /// <summary>
    ///     Cyan color.
    /// </summary>
    Cyan = 0b101,

    /// <summary>
    ///     Magenta color.
    /// </summary>
    Magenta = 0b110,

    /// <summary>
    ///     Orange color.
    /// </summary>
    Orange = 0b111,

    /// <summary>
    ///     Dark green color.
    /// </summary>
    DarkGreen = 0b1000,

    /// <summary>
    ///     Lime color.
    /// </summary>
    Lime = 0b1001,

    /// <summary>
    ///     Gray color.
    /// </summary>
    Gray = 0b1010,

    /// <summary>
    ///     Indigo color.
    /// </summary>
    Indigo = 0b1011,

    /// <summary>
    ///     Maroon color.
    /// </summary>
    Maroon = 0b1100,

    /// <summary>
    ///     Olive color.
    /// </summary>
    Olive = 0b1101,

    /// <summary>
    ///     Brown color.
    /// </summary>
    Brown = 0b1110,

    /// <summary>
    ///     Navy color.
    /// </summary>
    Navy = 0b1111,

    /// <summary>
    ///     Amaranth color.
    /// </summary>
    Amaranth = 0b10000,

    /// <summary>
    ///     Amber color.
    /// </summary>
    Amber = 0b10001,

    /// <summary>
    ///     Apricot color.
    /// </summary>
    Apricot = 0b10010,

    /// <summary>
    ///     Aquamarine color.
    /// </summary>
    Aquamarine = 0b10011,

    /// <summary>
    ///     Beige color.
    /// </summary>
    Beige = 0b10100,

    /// <summary>
    ///     Coffee color.
    /// </summary>
    Coffee = 0b10101,

    /// <summary>
    ///     Coral color.
    /// </summary>
    Coral = 0b10110,

    /// <summary>
    ///     Crimson color.
    /// </summary>
    Crimson = 0b10111,

    /// <summary>
    ///     Emerald color.
    /// </summary>
    Emerald = 0b11000,

    /// <summary>
    ///     Lilac color.
    /// </summary>
    Lilac = 0b11001,

    /// <summary>
    ///     Mauve color.
    /// </summary>
    Mauve = 0b11010,

    /// <summary>
    ///     Periwinkle color.
    /// </summary>
    Periwinkle = 0b11011,

    /// <summary>
    ///     Prussian blue color.
    /// </summary>
    PrussianBlue = 0b11100,

    /// <summary>
    ///     Slate gray color.
    /// </summary>
    SlateGray = 0b11101,

    /// <summary>
    ///     Taupe color.
    /// </summary>
    Taupe = 0b11110,

    /// <summary>
    ///     Viridian color.
    /// </summary>
    Viridian = 0b11111
}

/// <summary>
///     Extension methods for <see cref="BlockColor" />.
/// </summary>
public static class BlockColorExtensions
{
    /// <summary>
    ///     Converts a <see cref="BlockColor" /> to a <see cref="TintColor" />.
    /// </summary>
    /// <param name="color">The <see cref="BlockColor" /> to convert.</param>
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

            _ => new TintColor(r: 1f, g: 1f, b: 1f)
        };
    }
}
