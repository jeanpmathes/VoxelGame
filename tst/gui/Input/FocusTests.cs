// <copyright file="FocusTests.cs" company="VoxelGame">
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
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Controls.Templates;
using VoxelGame.GUI.Input;
using VoxelGame.GUI.Tests.Controls;
using VoxelGame.GUI.Tests.Rendering;
using VoxelGame.GUI.Tests.Utilities;
using VoxelGame.GUI.Tests.Visuals;
using VoxelGame.GUI.Themes;
using Xunit;

namespace VoxelGame.GUI.Tests.Input;

public sealed class FocusTests : IDisposable
{
    private readonly Focus focus = new((_, _) => {});
    private readonly EventObserver observer = new();

    private readonly Canvas canvas = Canvas.Create(new MockRenderer(), new Theme());

    public void Dispose()
    {
        canvas.Dispose();
    }

    [Fact]
    public void Focus_GetFocused_ShouldReturnFocusedVisual()
    {
        MockVisual visual = new();

        focus.Set(visual);

        Assert.Same(visual, focus.GetFocused());
    }

    [Fact]
    public void Focus_Set_ShouldReplaceFocusedVisual()
    {
        MockVisual first = new();
        MockVisual second = new();

        focus.Set(first);
        focus.Set(second);

        Assert.Same(second, focus.GetFocused());
    }

    [Fact]
    public void Focus_Unset_ShouldClearFocusedVisual()
    {
        MockVisual visual = new();

        focus.Set(visual);
        focus.Unset(visual);

        Assert.Null(focus.GetFocused());
    }

    [Fact]
    public void Focus_Unset_ShouldNotClearWhenPassedVisualIsNotFocused()
    {
        MockVisual focused = new();
        MockVisual other = new();

        focus.Set(focused);
        focus.Unset(other);

        Assert.Same(focused, focus.GetFocused());
    }

    [Fact]
    public void Focus_Clear_ShouldClearFocusedVisual()
    {
        MockVisual visual = new();

        focus.Set(visual);
        focus.Clear();

        Assert.Null(focus.GetFocused());
    }

    [Fact]
    public void Focus_Set_ShouldFocusVisualizationOfFocusedControl()
    {
        MockControl control = new();
        canvas.Child = control;

        focus.Set(control);

        Assert.Same(control.Visualization.GetValue(), focus.GetFocused());
    }

    [Fact]
    public void Focus_Set_ShouldNotFocusHiddenControl()
    {
        MockControl control = new();
        canvas.Child = control;

        control.Visibility.Value = Visibility.Hidden;

        Exception? exception = Record.Exception(() => focus.Set(control));

        Assert.Null(exception);
        Assert.Null(focus.GetFocused());
    }

    [Fact]
    public void Focus_Set_ShouldNotFocusDisabledControl()
    {
        MockControl control = new();
        canvas.Child = control;

        control.Enablement.Value = Enablement.Disabled;

        focus.Set(control);

        Assert.Null(focus.GetFocused());
    }

    [Fact]
    public void Focus_Set_ShouldNotFocusHiddenVisual()
    {
        MockVisual visual = new()
        {
            Visibility = {Value = Visibility.Hidden}
        };

        focus.Set(visual);

        Assert.Null(focus.GetFocused());
    }

    [Fact]
    public void Focus_Set_ShouldNotFocusDisabledVisual()
    {
        MockVisual visual = new()
        {
            Enablement = {Value = Enablement.Disabled}
        };

        focus.Set(visual);

        Assert.Null(focus.GetFocused());
    }

    [Fact]
    public void Focus_Unset_ShouldClearFocusedControl()
    {
        MockControl control = new();
        canvas.Child = control;

        focus.Set(control);
        focus.Unset(control);

        Assert.Null(focus.GetFocused());
    }

    [Fact]
    public void Focus_Unset_ShouldNotClearWhenControlIsNotFocused()
    {
        MockControl focused = new();
        MockControl other = new();

        canvas.Child = focused;

        focus.Set(focused);
        focus.Unset(other);

        Assert.NotNull(focus.GetFocused());
        Assert.Same(focused.Visualization.GetValue(), focus.GetFocused());
    }

    [Fact]
    public void Focus_Set_ShouldCallPassedCallback()
    {
        Focus localFocus = new((_, _) => observer.OnAction());
        MockVisual visual = new();

        localFocus.Set(visual);

        Assert.Equal(expected: 1, observer.InvocationCount);
    }

    [Fact]
    public void Focus_Clear_ShouldNotInvokeCallbackIfNothingIsFocused()
    {
        Focus localFocus = new((_, _) => observer.OnAction());

        localFocus.Clear();

        Assert.Equal(expected: 0, observer.InvocationCount);
    }

    [Fact]
    public void Focus_Clear_ShouldInvokeCallbackIfSomethingIsFocused()
    {
        Focus localFocus = new((_, _) => observer.OnAction());
        MockVisual visual = new();

        localFocus.Set(visual);
        localFocus.Clear();

        Assert.Equal(expected: 2, observer.InvocationCount);
    }

    [Fact]
    public void Focus_Unset_ShouldInvokeCallbackIfVisualIsFocused()
    {
        Focus localFocus = new((_, _) => observer.OnAction());
        MockVisual visual = new();

        localFocus.Set(visual);
        observer.Reset();

        localFocus.Unset(visual);

        Assert.Equal(expected: 1, observer.InvocationCount);
    }

    [Fact]
    public void Focus_Unset_ShouldNotInvokeCallbackIfVisualIsNotFocused()
    {
        Focus localFocus = new((_, _) => observer.OnAction());
        MockVisual focused = new();
        MockVisual other = new();

        localFocus.Set(focused);
        observer.Reset();

        localFocus.Unset(other);

        Assert.Equal(expected: 0, observer.InvocationCount);
    }

    [Fact]
    public void Focus_ShouldInvokeCallbackWhenVisualizationOfFocusedControlChanges()
    {
        Focus localFocus = new((_, _) => observer.OnAction());

        MockControl control = new();
        canvas.Child = control;

        localFocus.Set(control);
        observer.Reset();

        control.Template.Value = ControlTemplate.Create<MockControl>(_ => new MockVisual());

        Assert.True(observer.InvocationCount >= 1);
    }

    [Fact]
    public void Focus_ShouldBeClearedWhenControlIsHidden()
    {
        MockControl control = new();
        canvas.Child = control;

        focus.Set(control);

        Assert.NotNull(focus.GetFocused());

        control.Visibility.Value = Visibility.Hidden;

        Assert.Null(focus.GetFocused());
    }

    [Fact]
    public void Focus_ShouldBeClearedWhenVisualIsHidden()
    {
        MockControl control = new();
        canvas.Child = control;

        focus.Set(control);

        Assert.NotNull(focus.GetFocused());

        control.Visualization.GetValue()?.Visibility.Value = Visibility.Hidden;

        Assert.Null(focus.GetFocused());
    }

    [Fact]
    public void Focus_ShouldBeClearedWhenControlIsDisabled()
    {
        MockControl control = new();
        canvas.Child = control;

        focus.Set(control);

        Assert.NotNull(focus.GetFocused());

        control.Enablement.Value = Enablement.Disabled;

        Assert.Null(focus.GetFocused());
    }

    [Fact]
    public void Focus_ShouldBeClearedWhenVisualIsDisabled()
    {
        MockControl control = new();
        canvas.Child = control;

        focus.Set(control);

        Assert.NotNull(focus.GetFocused());

        control.Visualization.GetValue()?.Enablement.Value = Enablement.Disabled;

        Assert.Null(focus.GetFocused());
    }
}
