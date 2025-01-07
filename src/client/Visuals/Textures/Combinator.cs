// <copyright file="Combinator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Client.Visuals.Textures;

/// <summary>
///     Defines how layers of a deck are combined.
/// </summary>
/// <param name="type">The type of this combinator. Used as a key to find the correct combinator.</param>
public abstract class Combinator(String type)
{
    /// <summary>
    ///     Get the type of this combinator. Used as a key to find the correct combinator.
    /// </summary>
    public String Type { get; } = type;

    /// <summary>
    ///     Combine the current sheet with the next sheet.
    ///     The combinator may re-use any of the passed sheets instead of creating a new one as a result.
    /// </summary>
    /// <param name="current">The current sheet.</param>
    /// <param name="next">The next sheet, which goes on top of the current sheet.</param>
    /// <param name="context">The context in which the combination occurs.</param>
    /// <returns>The combined sheet, or <c>null</c> if the combination failed.</returns>
    public abstract Sheet? Combine(Sheet current, Sheet next, IContext context);

    /// <summary>
    ///     The context in which combination occurs.
    /// </summary>
    public interface IContext
    {
        /// <summary>
        ///     Report a warning message for an issue that occurred during combination.
        ///     Use this when a combinator will return <c>null</c>.
        /// </summary>
        /// <param name="message">The warning message.</param>
        public void ReportWarning(String message);
    }
}
