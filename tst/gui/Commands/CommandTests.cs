// <copyright file="CommandTests.cs" company="VoxelGame">
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
using VoxelGame.GUI.Commands;
using VoxelGame.GUI.Tests.Utilities;
using Xunit;

namespace VoxelGame.GUI.Tests.Commands;

public class CommandTests
{
    private static readonly Object placeholder = new();

    private readonly EventObserver observer = new();

    [Fact]
    public void Command_Succeeded_ShouldHaveSucceededStatus()
    {
        ICommandExecution execution = Command.Succeeded;

        Assert.Equal(Status.Succeeded, execution.Status.GetValue());
    }

    [Fact]
    public void Command_FromAction_ShouldAlwaysBeExecutable()
    {
        ICommand<Object> command = Command.FromAction(() => {});

        Assert.True(command.CanExecute.GetValue(placeholder));
    }

    [Fact]
    public void Command_Execute_ShouldRunActionOnce()
    {
        ICommand<Object> command = Command.FromAction(observer.OnAction);

        command.Execute(placeholder);

        Assert.Equal(expected: 1, observer.InvocationCount);
    }

    [Fact]
    public void Command_Execute_ShouldRunActionMultipleTimesIfExecutedMultipleTimes()
    {
        ICommand<Object> command = Command.FromAction(observer.OnAction);

        command.Execute(placeholder);
        command.Execute(placeholder);
        command.Execute(placeholder);

        Assert.Equal(expected: 3, observer.InvocationCount);
    }

    [Fact]
    public void Command_Execute_ShouldPassArgumentToAction()
    {
        const String argument = "Argument";

        ICommand<String> command = Command.FromAction<String>(observer.OnAction);

        command.Execute(argument);

        Assert.Same(argument, observer.LastArgs);
    }
}
