// <copyright file="InputSourceTests.cs" company="VoxelGame">
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
using JetBrains.Annotations;
using NSubstitute;
using VoxelGame.GUI.Input;
using VoxelGame.GUI.Utilities;
using Xunit;

namespace VoxelGame.GUI.Tests.Input;

[TestSubject(typeof(InputSource))]
public class InputSourceTests
{
    private readonly MockInputSource source;
    private readonly IInputReceiver receiver;

    public InputSourceTests()
    {
        source = new MockInputSource();

        receiver = Substitute.For<IInputReceiver>();
        source.AddReceiver(receiver);
    }

    [Fact]
    public void InputSource_ShouldForwardSentKeyEvents()
    {
        source.SendKeyEvent(Key.A, isDown: true, isRepeat: false, ModifierKeys.None);
        receiver.Received().ReceiveKeyEvent(Key.A, isDown: true, isRepeat: false, ModifierKeys.None);
    }

    [Fact]
    public void InputSource_ShouldForwardSentTextEvent()
    {
        source.SendTextEvent(text: "Hello");
        receiver.Received().ReceiveTextEvent(text: "Hello");
    }

    [Fact]
    public void InputSource_ShouldForwardSentPointerButtonEvent()
    {
        source.SendPointerButtonEvent(new Point(X: 12, Y: 13), PointerButton.Left, isDown: true, ModifierKeys.None);
        receiver.Received().ReceivePointerButtonEvent(new Point(X: 12, Y: 13), PointerButton.Left, isDown: true, ModifierKeys.None);
    }

    [Fact]
    public void InputSource_ShouldForwardSentPointerMoveEvent()
    {
        source.SendPointerMoveEvent(new Point(X: 14, Y: 15), deltaX: 1, deltaY: 2);
        receiver.Received().ReceivePointerMoveEvent(new Point(X: 14, Y: 15), deltaX: 1, deltaY: 2);
    }

    [Fact]
    public void InputSource_ShouldForwardSentScrollEvent()
    {
        source.SendScrollEvent(new Point(X: 16, Y: 17), deltaX: 3, deltaY: 4);
        receiver.Received().ReceiveScrollEvent(new Point(X: 16, Y: 17), deltaX: 3, deltaY: 4);
    }

    [Fact]
    public void InputSource_ShouldStopAtFirstHandlingReceiverInRegistrationOrder()
    {
        MockInputSource orderedSource = new();
        List<String> order = [];

        IInputReceiver first = Substitute.For<IInputReceiver>();
        IInputReceiver second = Substitute.For<IInputReceiver>();
        IInputReceiver third = Substitute.For<IInputReceiver>();

        first.ReceiveKeyEvent(Key.A, isDown: true, isRepeat: false, ModifierKeys.None).Returns(_ =>
        {
            order.Add("first");
            return false;
        });

        second.ReceiveKeyEvent(Key.A, isDown: true, isRepeat: false, ModifierKeys.None).Returns(_ =>
        {
            order.Add("second");
            return true;
        });

        third.ReceiveKeyEvent(Key.A, isDown: true, isRepeat: false, ModifierKeys.None).Returns(_ =>
        {
            order.Add("third");
            return false;
        });

        orderedSource.AddReceiver(first);
        orderedSource.AddReceiver(second);
        orderedSource.AddReceiver(third);

        Boolean handled = orderedSource.SendKeyEvent(Key.A, isDown: true, isRepeat: false, ModifierKeys.None);

        Assert.True(handled);
        Assert.Equal(["first", "second"], order);
    }
}
