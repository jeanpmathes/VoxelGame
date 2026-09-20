// <copyright file="SingleWordParser.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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
using System.Collections.Generic;

namespace VoxelGame.Client.Console.Parsers;

/// <summary>
///     Base class for parsers that parse a value from a single word.
/// </summary>
/// <typeparam name="T">The type of the parsed value.</typeparam>
public abstract class SingleWordParser<T> : Parser
{
    /// <inheritdoc />
    public sealed override Type ParsedType => typeof(T);

    /// <inheritdoc />
    public sealed override Argument? Parse(IReadOnlyList<String> args, Int32 offset, out Int32 consumed)
    {
        consumed = 0;

        if (offset >= args.Count) return null;

        if (!TryParse(args[offset], out T? value)) return null;

        consumed = 1;

        return new SimpleArgument(value);
    }

    /// <summary>
    ///     Try to parse a value from a single word.
    /// </summary>
    /// <param name="word">The word to parse.</param>
    /// <param name="value">The parsed value if successful.</param>
    /// <returns>Whether parsing was successful.</returns>
    protected abstract Boolean TryParse(String word, out T? value);
}
