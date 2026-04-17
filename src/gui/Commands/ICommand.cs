// <copyright file="ICommand.cs" company="VoxelGame">
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
using VoxelGame.GUI.Bindings;

namespace VoxelGame.GUI.Commands;

/// <summary>
///     Interface for commands. Commands are used to bind actions to controls.
/// </summary>
/// <typeparam name="TArgument">The type of argument passed to the command when executed.</typeparam>
public interface ICommand<in TArgument>
{
    /// <summary>
    ///     Whether the command can be executed.
    /// </summary>
    public IValueSource<TArgument, Boolean> CanExecute { get; }

    /// <summary>
    ///     Execute the command.
    /// </summary>
    public ICommandExecution Execute(TArgument argument);
}
