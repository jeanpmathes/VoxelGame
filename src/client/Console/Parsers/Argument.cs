// <copyright file="Argument.cs" company="VoxelGame">
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

namespace VoxelGame.Client.Console.Parsers;

/// <summary>
///     Represents a parsed console command argument that can be evaluated in an execution context.
///     Resolution evaluates context-dependent expressions against the execution <see cref="Context" />
/// to produce the final value passed to the command.
/// </summary>
public abstract class Argument
{
    /// <summary>
    ///     Evaluate this argument within the specified execution context to produce the runtime value.
    /// </summary>
    /// <param name="context">The execution context in which the command is being invoked.</param>
    /// <returns>The evaluated argument value, or <see langword="null" /> if resolution failed.</returns>
    public abstract Object? Resolve(Context context);
}
