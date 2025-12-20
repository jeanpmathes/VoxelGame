// <copyright file="Context.cs" company="VoxelGame">
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
using VoxelGame.Client.Actors;

namespace VoxelGame.Client.Console;

/// <summary>
///     The command execution context.
/// </summary>
/// <param name="Output">The class used to receive output from the command.</param>
/// <param name="Invoker">The invoker used to invoke this command.</param>
/// <param name="Player">The player to execute the command for.</param>
/// <param name="IsScript">Whether this command originates from a script.</param>
public record Context(ConsoleOutput Output, CommandInvoker Invoker, Player Player, Boolean IsScript = false)
{
    /// <summary>
    ///     Get this context as a script context.
    /// </summary>
    /// <returns>The new script context.</returns>
    public Context ToScript()
    {
        return this with {IsScript = true};
    }
}
