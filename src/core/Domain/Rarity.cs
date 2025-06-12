// <copyright file="Rarity.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Domain;

/// <summary>
///     Rarity levels for items, loot, etc.
/// </summary>
public enum Rarity
{
    /// <summary>
    ///     The lowest rarity level, not because of the probability of getting it, but because of its value.
    ///     It should be indicated by a grey color.
    /// </summary>
    Junk,

    /// <summary>
    ///     The most common rarity level.
    ///     It should be indicated by a white color.
    /// </summary>
    Common,

    /// <summary>
    ///     Uncommon rarity level.
    ///     It should be indicated by a green color.
    /// </summary>
    Uncommon,

    /// <summary>
    ///     Rare rarity level.
    ///     It should be indicated by a blue color.
    /// </summary>
    Rare,

    /// <summary>
    ///     Exceptional rarity level.
    ///     It should be indicated by a purple color.
    /// </summary>
    Exceptional,

    /// <summary>
    ///     The highest normal rarity level.
    ///     It should be indicated by a yellow color.
    /// </summary>
    Miraculous,

    /// <summary>
    ///     Essentially impossible to get.
    ///     Used for very special items that are tied to specific events or achievements.
    ///     Developer items should also be of this rarity.
    ///     It should be indicated by a dark color, showing the item in grayscale.
    /// </summary>
    Unreal
}
