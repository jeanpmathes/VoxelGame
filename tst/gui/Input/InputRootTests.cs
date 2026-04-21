// <copyright file="InputRootTests.cs" company="VoxelGame">
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
using System.Drawing;
using JetBrains.Annotations;
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Input;
using VoxelGame.GUI.Tests.Controls;
using VoxelGame.GUI.Tests.Rendering;
using VoxelGame.GUI.Tests.Utilities;
using VoxelGame.GUI.Tests.Visuals;
using VoxelGame.GUI.Themes;
using Xunit;

namespace VoxelGame.GUI.Tests.Input;

[TestSubject(typeof(InputRoot))]
public sealed class InputRootTests : IDisposable
{
    private readonly Canvas canvas;
    private readonly MockVisual visual;

    private readonly EventObserver observer = new();

    public InputRootTests()
    {
        canvas = Canvas.Create(new MockRenderer(), new Theme());

        MockControl control = new();
        canvas.Child = control;

        canvas.SetRenderingSize(new Size(width: 500, height: 500));

        visual = (control.Visualization.GetValue() as MockVisual)!;
    }

    public void Dispose()
    {
        canvas.Dispose();
    }

    [Fact]
    public void InputRoot_ShouldDeliverPointerButtonDownEventToVisualWhoseBoundsContainEventPosition()
    {
        visual.OnInputHandler = observer.OnAction;

        canvas.Press(new PointF(x: 100f, y: 100f));

        Assert.IsType<PointerButtonEvent>(observer.LastArgs);
    }

    [Fact]
    public void InputRoot_ShouldNotDeliverPointerButtonDownEventToHiddenVisual()
    {
        visual.OnInputHandler = observer.OnAction;
        visual.Visibility.Value = Visibility.Hidden;

        canvas.Press(new PointF(x: 100f, y: 100f));

        Assert.Equal(expected: 0, observer.InvocationCount);
    }

    [Fact]
    public void InputRoot_ShouldNotDeliverPointerButtonDownEventToDisabledVisual()
    {
        visual.OnInputHandler = observer.OnAction;
        visual.Enablement.Value = Enablement.Disabled;

        canvas.Press(new PointF(x: 100f, y: 100f));

        Assert.Equal(expected: 0, observer.InvocationCount);
    }

    [Fact]
    public void InputRoot_ShouldNotDeliverPointerButtonDownEventThatFallsOutOfBounds()
    {
        visual.OnInputHandler = observer.OnAction;

        canvas.Press(new PointF(x: -10f, y: -10f));

        Assert.Equal(expected: 0, observer.InvocationCount);
    }

    [Fact]
    public void InputRoot_ShouldFirstTunnelThenBubbleInputEvents()
    {
        List<String> order = [];

        visual.OnInputPreviewHandler = _ => order.Add("tunnel");
        visual.OnInputHandler = _ => order.Add("bubble");

        canvas.Context.KeyboardFocus.Set(visual);
        canvas.EnterText("Hello World.");

        Assert.Equal(["tunnel", "bubble"], order);
    }

    [Fact]
    public void InputRoot_ShouldFirstTunnelThenBubbleInputEventsInDeepHierarchy()
    {
        List<String> order = [];

        visual.OnInputPreviewHandler = _ => order.Add("tunnel0");
        visual.OnInputHandler = _ => order.Add("bubble0");

        visual.CreateDeepChildHierarchy(depth: 3,
            (child, depth) =>
            {
                child.OnInputPreviewHandler = _ => order.Add($"tunnel{depth + 1}");
                child.OnInputHandler = _ => order.Add($"bubble{depth + 1}");
            });

        canvas.Press(new PointF(x: 100f, y: 100f));

        Assert.Equal(["tunnel0", "tunnel1", "tunnel2", "tunnel3", "bubble3", "bubble2", "bubble1", "bubble0"], order);
    }

    [Fact]
    public void InputRoot_ShouldNotBubbleInputEventsIfTheyAreHandled()
    {
        visual.OnInputPreviewHandler = e => e.Handled = true;
        visual.OnInputHandler = observer.OnAction;

        canvas.Press(new PointF(x: 100f, y: 100f));

        Assert.Equal(expected: 0, observer.InvocationCount);
    }

    [Fact]
    public void InputRoot_ShouldDeliverPointerMoveEventToVisualWhoseBoundsContainEventPosition()
    {
        visual.OnInputHandler = observer.OnAction;

        canvas.MovePointerTo(new PointF(x: 100f, y: 100f));

        Assert.IsType<PointerMoveEvent>(observer.LastArgs);
    }

    [Fact]
    public void InputRoot_ShouldDeliverScrollEventToVisualWhoseBoundsContainEventPosition()
    {
        visual.OnInputHandler = observer.OnAction;

        canvas.Scroll(new PointF(x: 100f, y: 100f), deltaX: 0f, deltaY: 120f);

        Assert.IsType<ScrollEvent>(observer.LastArgs);
    }

    [Fact]
    public void InputRoot_ShouldNotDeliverKeyEventsIfNoElementHasKeyboardFocus()
    {
        visual.OnInputHandler = observer.OnAction;

        canvas.PressKey(Key.A);

        Assert.Equal(expected: 0, observer.InvocationCount);
    }

    [Fact]
    public void InputRoot_ShouldNotDeliverTextEventsIfNoElementHasKeyboardFocus()
    {
        visual.OnInputHandler = observer.OnAction;

        canvas.EnterText("hello");

        Assert.Equal(expected: 0, observer.InvocationCount);
    }

    [Fact]
    public void InputRoot_ShouldNotThrowOnTabIfNoNavigableVisualsExist()
    {
        canvas.PressKey(Key.Tab);

        Assert.NotNull(canvas.Input.GetValue());
        Assert.Null(canvas.Input.GetValue()?.KeyboardFocus.GetFocused());
    }

    [Fact]
    public void InputRoot_ShouldNotThrowOnShiftTabIfNoNavigableVisualsExist()
    {
        canvas.PressKey(Key.Tab, ModifierKeys.Shift);

        Assert.NotNull(canvas.Input.GetValue());
        Assert.Null(canvas.Input.GetValue()?.KeyboardFocus.GetFocused());
    }

    [Fact]
    public void InputRoot_ShouldFocusSingleNavigableVisualOnTab()
    {
        visual.IsNavigable.Value = true;

        canvas.PressKey(Key.Tab);

        Assert.Same(visual, canvas.Input.GetValue()?.KeyboardFocus.GetFocused());
    }

    [Fact]
    public void InputRoot_ShouldMoveForwardOnTabAndBackWardOnShiftTab()
    {
        // ┌──────┐               
        // │0     │               
        // └┬────┬┘               
        // ┌▽──┐┌▽─────────────┐  
        // │1.1││1.2           │  
        // └┬──┘└────────┬────┬┘  
        // ┌▽──────────┐┌▽──┐┌▽──┐
        // │2          ││4.1││4.2│
        // └┬────┬────┬┘└───┘└───┘
        // ┌▽──┐┌▽──┐┌▽──┐        
        // │3.1││3.2││3.3│        
        // └───┘└───┘└───┘        

        MockVisual visual0 = CreateVisual(nameof(visual0));
        visual.AddChildVisual(visual0);

        MockVisual visual1P1 = CreateVisual(nameof(visual1P1));
        visual0.AddChildVisual(visual1P1);

        MockVisual visual1P2 = CreateVisual(nameof(visual1P2));
        visual0.AddChildVisual(visual1P2);

        MockVisual visual2 = CreateVisual(nameof(visual2));
        visual.AddChildVisual(visual2);

        MockVisual visual3P1 = CreateVisual(nameof(visual3P1));
        visual2.AddChildVisual(visual3P1);

        MockVisual visual3P2 = CreateVisual(nameof(visual3P2));
        visual2.AddChildVisual(visual3P2);

        MockVisual visual3P3 = CreateVisual(nameof(visual3P3));
        visual2.AddChildVisual(visual3P3);

        MockVisual visual4P1 = CreateVisual(nameof(visual4P1));
        visual.AddChildVisual(visual4P1);

        MockVisual visual4P2 = CreateVisual(nameof(visual4P2));
        visual.AddChildVisual(visual4P2);

        List<MockVisual> expectedOrder = [visual0, visual1P1, visual1P2, visual2, visual3P1, visual3P2, visual3P3, visual4P1, visual4P2, visual0];

        foreach (MockVisual expectedFocused in expectedOrder)
        {
            canvas.PressKey(Key.Tab);

            Assert.Same(expectedFocused, canvas.Input.GetValue()?.KeyboardFocus.GetFocused());
            Assert.True(expectedFocused.IsKeyboardFocused.GetValue());

            AssertNotKeyboardFocused(except: expectedFocused);
        }

        expectedOrder.Reverse();
        expectedOrder.RemoveAt(0);
        expectedOrder.Add(visual4P2);

        canvas.Input.GetValue()?.KeyboardFocus.Clear();

        foreach (MockVisual expectedFocused in expectedOrder)
        {
            canvas.PressKey(Key.Tab, ModifierKeys.Shift);

            Assert.Same(expectedFocused, canvas.Input.GetValue()?.KeyboardFocus.GetFocused());
            Assert.True(expectedFocused.IsKeyboardFocused.GetValue());

            AssertNotKeyboardFocused(except: expectedFocused);
        }

        MockVisual CreateVisual(String tag)
        {
            return new MockVisual(tag) {IsNavigable = {Value = true}};
        }

        void AssertNotKeyboardFocused(MockVisual except)
        {
            foreach (MockVisual navigableVisual in expectedOrder)
            {
                if (navigableVisual != except)
                    Assert.False(navigableVisual.IsKeyboardFocused.GetValue());
            }
        }
    }

    [Fact]
    public void InputRoot_ShouldSkipCollapsedVisualsOnTabAndShiftTab()
    {
        List<MockVisual> children = [];

        visual.CreateWideChildHierarchy(width: 3,
            (child, index) =>
            {
                child.IsNavigable.Value = true;

                if (index == 1)
                    child.Visibility.Value = Visibility.Collapsed;

                children.Add(child);
            });

        canvas.PressKey(Key.Tab);
        canvas.PressKey(Key.Tab);

        Assert.Same(children[2], canvas.Input.GetValue()?.KeyboardFocus.GetFocused());

        canvas.PressKey(Key.Tab, ModifierKeys.Shift);

        Assert.Same(children[0], canvas.Input.GetValue()?.KeyboardFocus.GetFocused());
    }

    [Fact]
    public void InputRoot_ShouldSkipDisabledVisualsOnTabAndShiftTab()
    {
        List<MockVisual> children = [];

        visual.CreateWideChildHierarchy(width: 3,
            (child, index) =>
            {
                child.IsNavigable.Value = true;

                if (index == 1)
                    child.Enablement.Value = Enablement.Disabled;

                children.Add(child);
            });

        canvas.PressKey(Key.Tab);
        canvas.PressKey(Key.Tab);

        Assert.Same(children[2], canvas.Input.GetValue()?.KeyboardFocus.GetFocused());

        canvas.PressKey(Key.Tab, ModifierKeys.Shift);

        Assert.Same(children[0], canvas.Input.GetValue()?.KeyboardFocus.GetFocused());
    }
}
