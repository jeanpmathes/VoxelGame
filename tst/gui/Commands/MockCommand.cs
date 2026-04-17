// <copyright file="MockCommand.cs" company="VoxelGame">
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
using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Commands;

namespace VoxelGame.GUI.Tests.Commands;

public sealed class MockCommand : ICommand<Object>
{
    private readonly Slot<Boolean> canExecute;

    private Boolean willHoldExecution;
    private List<MockExecution> heldExecutions = [];

    public MockCommand()
    {
        canExecute = new Slot<Boolean>(value: true, this);

        CanExecute = Binding.To(canExecute).Parametrize<Object, Boolean>((_, value) => value);
    }

    /// <summary>
    ///     The number of times <see cref="Execute" /> has been called.
    /// </summary>
    public Int32 ExecutionCount { get; private set; }

    /// <inheritdoc />
    public IValueSource<Object, Boolean> CanExecute { get; }

    public ICommandExecution Execute(Object argument)
    {
        ExecutionCount++;

        if (!willHoldExecution) return Command.Succeeded;

        MockExecution execution = new();
        heldExecutions.Add(execution);
        return execution;
    }

    public void SetCanExecute(Boolean value)
    {
        canExecute.SetValue(value);
    }

    public void HoldExecutionCompletion()
    {
        willHoldExecution = true;
    }

    public void CompleteExecution()
    {
        willHoldExecution = false;

        foreach (MockExecution execution in heldExecutions)
            execution.Complete(Status.Succeeded);

        heldExecutions = [];
    }
}

public sealed class MockExecution : ICommandExecution
{
    private readonly Slot<Status> status;

    public MockExecution()
    {
        status = new Slot<Status>(GUI.Commands.Status.Running, this);
    }

    public IValueSource<Status> Status => status;

    public IValueSource<Single>? Progress => null;

    public void Dispose() {}

    public void Complete(Status completionStatus)
    {
        status.SetValue(completionStatus);
    }
}
