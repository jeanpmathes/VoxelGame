// <copyright file="Parser.cs" company="VoxelGame">
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
using VoxelGame.Client.Console.Parsers;

namespace VoxelGame.Client.Console;

/// <summary>
///     Base class for argument parsers, which parse words of a command to produce a command argument.
/// </summary>
public abstract class Parser
{
    /// <summary>
    ///     Get the type parsed by this parser.
    /// </summary>
    public abstract Type ParsedType { get; }

    /// <summary>
    ///     Get a user-facing representation of the parsed parameter type.
    /// </summary>
    public virtual String TypeRepresentation => ParsedType.Name;

    /// <summary>
    ///     Parse arguments starting at a given offset.
    /// </summary>
    /// <param name="args">The list of arguments.</param>
    /// <param name="offset">The starting index in the argument list.</param>
    /// <param name="consumed">The number of words consumed by this parser if successful, otherwise 0.</param>
    /// <returns>The parsed argument, or <c>null</c> if parsing was not successful.</returns>
    public abstract Argument? Parse(IReadOnlyList<String> args, Int32 offset, out Int32 consumed);
}
