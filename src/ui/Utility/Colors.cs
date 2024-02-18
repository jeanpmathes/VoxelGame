// <copyright file="Colors.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using Gwen.Net;

namespace VoxelGame.UI.Utility;

/// <summary>
///     Defines commonly used colors and gives them meaning.
/// </summary>
public static class Colors
{
    /// <summary>
    ///     The color for all primary elements, such as text and titles.
    /// </summary>
    public static readonly Color Primary = Color.White;

    /// <summary>
    ///     A color for less important elements.
    ///     This can be used for information which is useful, but not essential.
    /// </summary>
    public static readonly Color Secondary = Color.Gray;

    /// <summary>
    ///     The color for information which is good or positive.
    ///     Should be used in tandem with <see cref="Bad" />.
    /// </summary>
    public static readonly Color Good = Color.Green;

    /// <summary>
    ///     The color for information which is bad or negative.
    ///     Should be used in tandem with <see cref="Good" />.
    /// </summary>
    public static readonly Color Bad = Color.Red;

    /// <summary>
    ///     Marks text that might be interesting but not critical.
    /// </summary>
    public static readonly Color Interesting = Color.GreenYellow;

    /// <summary>
    ///     Marks text that is critical and should be read.
    /// </summary>
    public static readonly Color Error = Color.Red;

    /// <summary>
    ///     Invisible color.
    /// </summary>
    public static readonly Color Invisible = Color.Transparent;

    /// <summary>
    ///     Signifies that an operation is dangerous.
    /// </summary>
    public static readonly Color Danger = Color.Red;
}
