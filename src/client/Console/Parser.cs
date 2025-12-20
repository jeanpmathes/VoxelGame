// <copyright file="Parser.cs" company="VoxelGame">
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

namespace VoxelGame.Client.Console;

/// <summary>
///     Base class for argument parsers, which parse a given string to provide a value of specific type.
/// </summary>
public abstract class Parser
{
    /// <summary>
    ///     Get the type parsed by this parser.
    /// </summary>
    public abstract Type ParsedType { get; }

    /// <summary>
    ///     Check if the given string can be parsed by this parser.
    /// </summary>
    /// <param name="input">The input string to check.</param>
    /// <returns>True if this parser can parse the provided input.</returns>
    public abstract Boolean CanParse(String input);

    /// <summary>
    ///     Parse the given string to the type this parser is for.
    /// </summary>
    /// <param name="input">The input to parse. Must be checked with <see cref="CanParse" /></param>
    /// before.
    /// <returns>A value of the type this parsers targets.</returns>
    public abstract Object Parse(String input);

    /// <summary>
    ///     Create a parser for a specific type.
    /// </summary>
    /// <param name="check">A function checking if a string can be parsed.</param>
    /// <param name="parse">A function parsing a string.</param>
    /// <typeparam name="T">The type the new parsers should parse.</typeparam>
    /// <returns>A parser for the specific type.</returns>
    public static Parser BuildParser<T>(Func<String, Boolean> check, Func<String, T> parse)
    {
        return new SimpleParser<T>(check, parse);
    }

    private sealed class SimpleParser<T>(Func<String, Boolean> check, Func<String, T> parse) : Parser
    {
        public override Type ParsedType => typeof(T);

        public override Boolean CanParse(String input)
        {
            return check(input);
        }

        public override Object Parse(String input)
        {
            return parse(input)!;
        }
    }
}
