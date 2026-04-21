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

using System.Drawing;
using JetBrains.Annotations;
using NSubstitute;
using VoxelGame.GUI.Input;
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
        source.SendPointerButtonEvent(new PointF(x: 12, y: 13), PointerButton.Left, isDown: true, ModifierKeys.None);
        receiver.Received().ReceivePointerButtonEvent(new PointF(x: 12, y: 13), PointerButton.Left, isDown: true, ModifierKeys.None);
    }

    [Fact]
    public void InputSource_ShouldForwardSentPointerMoveEvent()
    {
        source.SendPointerMoveEvent(new PointF(x: 14, y: 15), deltaX: 1, deltaY: 2);
        receiver.Received().ReceivePointerMoveEvent(new PointF(x: 14, y: 15), deltaX: 1, deltaY: 2);
    }

    [Fact]
    public void InputSource_ShouldForwardSentScrollEvent()
    {
        source.SendScrollEvent(new PointF(x: 16, y: 17), deltaX: 3, deltaY: 4);
        receiver.Received().ReceiveScrollEvent(new PointF(x: 16, y: 17), deltaX: 3, deltaY: 4);
    }
}
