// <copyright file="Command.cs" company="VoxelGame">
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
///     Utility class for commands.
/// </summary>
public static class Command
{
    /// <summary>
    ///     Get a successful command execution. This can be used for commands that execute synchronously and always succeed.
    /// </summary>
    public static ICommandExecution Succeeded { get; } = new SucceededExecution();

    /// <summary>
    ///     Create a command from an action. The command will always be executable and will return a successful execution when
    ///     executed.
    /// </summary>
    /// <param name="action">The action to execute when the command is executed.</param>
    /// <returns>A command that executes the given action.</returns>
    public static ICommand<Object> FromAction(Action action)
    {
        return new ActionCommand(action);
    }

    /// <summary>
    ///     Create a command from an action with an argument. The command will always be executable and will return a
    ///     successful execution when executed.
    /// </summary>
    /// <param name="action">
    ///     The action to execute when the command is executed. The argument passed to the command will be
    ///     passed to the action.
    /// </param>
    /// <typeparam name="TArgument">The type of the argument passed to the command and the action.</typeparam>
    /// <returns>A command that executes the given action with an argument.</returns>
    public static ICommand<TArgument> FromAction<TArgument>(Action<TArgument> action)
    {
        return new ActionCommand<TArgument>(action);
    }

    private class SucceededExecution : ICommandExecution
    {
        public IValueSource<Status> Status => field ??= new Slot<Status>(Commands.Status.Succeeded, this);

        public IValueSource<Single>? Progress => null;

        public void Dispose()
        {
            // Nothing to dispose.
        }
    }

    private class ActionCommand(Action action) : ICommand<Object>
    {
        public IValueSource<Object, Boolean> CanExecute { get; } = Binding.Constant<Object, Boolean>(true);

        public ICommandExecution Execute(Object argument)
        {
            action();

            return Succeeded;
        }
    }

    private class ActionCommand<TArgument>(Action<TArgument> action) : ICommand<TArgument>
    {
        public IValueSource<TArgument, Boolean> CanExecute { get; } = Binding.Constant<TArgument, Boolean>(true);

        public ICommandExecution Execute(TArgument argument)
        {
            action(argument);

            return Succeeded;
        }
    }
}
