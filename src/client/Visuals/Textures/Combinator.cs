// <copyright file="Combinator.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Client.Visuals.Textures;

/// <summary>
///     Defines how layers of a deck are combined.
/// </summary>
/// <param name="type">The type of this combinator. Used as a key to find the correct combinator.</param>
public abstract class Combinator(String type) : IIssueSource
{
    /// <summary>
    ///     Get the type of this combinator. Used as a key to find the correct combinator.
    /// </summary>
    public String Type { get; } = type;

    /// <inheritdoc />
    public String InstanceName => Type;

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
        void ReportWarning(String message);
    }
}
