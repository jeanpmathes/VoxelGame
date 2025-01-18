// <copyright file="NamedColor.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Visuals;

/// <summary>
///     A set of colors that can be stored in up to four bits, fewer bits are supported too.
/// </summary>
public enum NamedColor
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
///     Extension methods for <see cref="NamedColor" />.
/// </summary>
public static class BlockColorExtensions
{
    /// <summary>
    ///     Converts a <see cref="NamedColor" /> to a <see cref="ColorS" />.
    /// </summary>
    /// <param name="color">The <see cref="NamedColor" /> to convert.</param>
    /// <returns>The resulting <see cref="ColorS" />.</returns>
    public static ColorS ToColorS(this NamedColor color)
    {
        return color switch
        {
            NamedColor.Red => ColorS.Red,
            NamedColor.Green => ColorS.Green,
            NamedColor.Blue => ColorS.Blue,

            NamedColor.Yellow => ColorS.Yellow,
            NamedColor.Cyan => ColorS.Cyan,
            NamedColor.Magenta => ColorS.Magenta,
            NamedColor.Orange => ColorS.Orange,

            NamedColor.DarkGreen => ColorS.DarkGreen,
            NamedColor.Lime => ColorS.Lime,
            NamedColor.Gray => ColorS.Gray,
            NamedColor.Indigo => ColorS.Indigo,
            NamedColor.Maroon => ColorS.Maroon,
            NamedColor.Olive => ColorS.Olive,
            NamedColor.Brown => ColorS.Brown,
            NamedColor.Navy => ColorS.Navy,

            NamedColor.Amaranth => ColorS.Amaranth,
            NamedColor.Amber => ColorS.Amber,
            NamedColor.Apricot => ColorS.Apricot,
            NamedColor.Aquamarine => ColorS.Aquamarine,
            NamedColor.Beige => ColorS.Beige,
            NamedColor.Coffee => ColorS.Coffee,
            NamedColor.Coral => ColorS.Coral,
            NamedColor.Crimson => ColorS.Crimson,
            NamedColor.Emerald => ColorS.Emerald,
            NamedColor.Lilac => ColorS.Lilac,
            NamedColor.Mauve => ColorS.Mauve,
            NamedColor.Periwinkle => ColorS.Periwinkle,
            NamedColor.PrussianBlue => ColorS.PrussianBlue,
            NamedColor.SlateGray => ColorS.SlateGray,
            NamedColor.Taupe => ColorS.Taupe,
            NamedColor.Viridian => ColorS.Viridian,

            _ => ColorS.None
        };
    }
}
