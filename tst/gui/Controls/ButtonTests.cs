// <copyright file="ButtonTests.cs" company="VoxelGame">
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
using System.Drawing;
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Input;
using VoxelGame.GUI.Tests.Commands;
using VoxelGame.GUI.Tests.Input;
using VoxelGame.GUI.Tests.Rendering;
using VoxelGame.GUI.Themes;
using Xunit;
using Canvas = VoxelGame.GUI.Controls.Canvas;

namespace VoxelGame.GUI.Tests.Controls;

public sealed class ButtonTests : ControlTestBase<Button<String>>, IDisposable
{
    private readonly Canvas canvas;
    private readonly Button<String> button;
    private readonly MockCommand command;

    public ButtonTests() : base(() => new Button<String>())
    {
        canvas = Canvas.Create(new MockRenderer(), new Theme());

        command = new MockCommand();

        button = new Button<String>
        {
            Command = {Value = command},
            Content = {Value = "Click me"}
        };

        canvas.Child = button;

        canvas.SetRenderingSize(new Size(width: 500, height: 500));
    }

    public void Dispose()
    {
        canvas.Dispose();
    }

    [Fact]
    public void Button_ShouldExecuteCommandOnClick()
    {
        canvas.Click(button);

        Assert.Equal(expected: 1, command.ExecutionCount);
    }

    [Fact]
    public void Button_ShouldExecuteCommandOncePerClick()
    {
        canvas.Click(button);
        canvas.Click(button);
        canvas.Click(button);

        Assert.Equal(expected: 3, command.ExecutionCount);
    }

    [Fact]
    public void Button_ShouldNotExecuteCommandIfReleasingOutsideOfButton()
    {
        canvas.Press(button);
        canvas.Release(new PointF(x: -10f, y: -10f));

        Assert.Equal(expected: 0, command.ExecutionCount);
    }

    [Fact]
    public void Button_ShouldSetIsPressedWhilePressed()
    {
        canvas.Press(button);

        Assert.True(button.IsPressed.GetValue());
    }

    [Fact]
    public void Button_ShouldClearIsPressedWhenReleased()
    {
        canvas.Press(button);
        canvas.Release(button);

        Assert.False(button.IsPressed.GetValue());
    }

    [Fact]
    public void Button_ShouldAcquirePointerFocusOnPress()
    {
        canvas.Press(button);

        Assert.Equal(button.Visualization.GetValue(), canvas.GetPointerFocused());
    }

    [Fact]
    public void Button_ShouldReleasePointerFocusOnRelease()
    {
        canvas.Press(button);
        canvas.Release(button);

        Assert.Null(canvas.GetKeyboardFocused());
    }

    [Fact]
    public void Button_ShouldExecuteCommandOnEnterKey()
    {
        canvas.Focus(button);
        canvas.PressKey(Key.Enter);

        Assert.Equal(expected: 1, command.ExecutionCount);
    }

    [Fact]
    public void Button_ShouldNotThrowIfCommandIsNull()
    {
        button.Command.Value = null;

        Exception? ex = Record.Exception(() => canvas.Click(button));

        Assert.Null(ex);
    }

    [Fact]
    public void Button_ShouldBeDisabledIfCommandCannotExecute()
    {
        command.SetCanExecute(false);

        Assert.Equal(Enablement.Disabled, button.Enablement.GetValue());
        Assert.Equal(expected: 0, command.ExecutionCount);
    }

    [Fact]
    public void Button_ShouldNotExecuteIfDisabled()
    {
        button.Enablement.Value = Enablement.Disabled;

        canvas.Click(button);

        Assert.Equal(expected: 0, command.ExecutionCount);
    }

    [Fact]
    public void Button_ShouldClearIsPressedIfContentIsRemoved()
    {
        canvas.Press(button);
        button.Content.Value = null;

        Assert.False(button.IsPressed.GetValue());
        Assert.Equal(expected: 0, command.ExecutionCount);
    }

    [Fact]
    public void Button_ShouldKeepIsPressedUntilCommandCompletes()
    {
        command.HoldExecutionCompletion();

        canvas.Click(button);

        Assert.True(button.IsPressed.GetValue());

        command.CompleteExecution();

        Assert.False(button.IsPressed.GetValue());
    }

    [Fact]
    public void Button_ShouldIgnoreClicksUntilCommandCompletes()
    {
        command.HoldExecutionCompletion();

        canvas.Click(button);
        canvas.Click(button);

        command.CompleteExecution();

        Assert.Equal(expected: 1, command.ExecutionCount);
    }
}
