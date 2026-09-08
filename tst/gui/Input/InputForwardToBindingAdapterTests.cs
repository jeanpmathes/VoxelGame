// <copyright file="InputForwardToBindingAdapterTests.cs" company="VoxelGame">
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
using JetBrains.Annotations;
using NSubstitute;
using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Input;
using VoxelGame.GUI.Utilities;
using Xunit;

namespace VoxelGame.GUI.Tests.Input;

[TestSubject(typeof(InputForwardToBindingAdapter))]
public class InputForwardToBindingAdapterTests
{
    private readonly Slot<IInputReceiver?> receiver;
    private readonly InputForwardToBindingAdapter adapter;

    public InputForwardToBindingAdapterTests()
    {
        receiver = new Slot<IInputReceiver?>(Substitute.For<IInputReceiver>(), this);
        adapter = new InputForwardToBindingAdapter(Binding.To(receiver));
    }

    [Fact]
    public void InputForwardToBindingAdapter_ShouldForwardReceivedKeyEvents()
    {
        receiver.GetValue()!.ReceiveKeyEvent(Key.A, isDown: true, isRepeat: false, ModifierKeys.None).Returns(true);

        Boolean handled = adapter.ReceiveKeyEvent(Key.A, isDown: true, isRepeat: false, ModifierKeys.None);

        Assert.True(handled);
        receiver.GetValue()?.Received().ReceiveKeyEvent(Key.A, isDown: true, isRepeat: false, ModifierKeys.None);
    }

    [Fact]
    public void InputForwardToBindingAdapter_ShouldForwardReceivedTextEvent()
    {
        receiver.GetValue()!.ReceiveTextEvent(text: "Hello").Returns(true);

        Boolean handled = adapter.ReceiveTextEvent(text: "Hello");

        Assert.True(handled);
        receiver.GetValue()?.Received().ReceiveTextEvent(text: "Hello");
    }

    [Fact]
    public void InputForwardToBindingAdapter_ShouldForwardReceivedPointerButtonEvent()
    {
        receiver.GetValue()!.ReceivePointerButtonEvent(new Point(X: 12, Y: 13), PointerButton.Left, isDown: true, ModifierKeys.None).Returns(true);

        Boolean handled = adapter.ReceivePointerButtonEvent(new Point(X: 12, Y: 13), PointerButton.Left, isDown: true, ModifierKeys.None);

        Assert.True(handled);
        receiver.GetValue()?.Received().ReceivePointerButtonEvent(new Point(X: 12, Y: 13), PointerButton.Left, isDown: true, ModifierKeys.None);
    }

    [Fact]
    public void InputForwardToBindingAdapter_ShouldForwardReceivedPointerMoveEvent()
    {
        receiver.GetValue()!.ReceivePointerMoveEvent(new Point(X: 14, Y: 15), deltaX: 1, deltaY: 2).Returns(true);

        Boolean handled = adapter.ReceivePointerMoveEvent(new Point(X: 14, Y: 15), deltaX: 1, deltaY: 2);

        Assert.True(handled);
        receiver.GetValue()?.Received().ReceivePointerMoveEvent(new Point(X: 14, Y: 15), deltaX: 1, deltaY: 2);
    }

    [Fact]
    public void InputForwardToBindingAdapter_ShouldForwardReceivedScrollEvent()
    {
        receiver.GetValue()!.ReceiveScrollEvent(new Point(X: 16, Y: 17), deltaX: 3, deltaY: 4).Returns(true);

        Boolean handled = adapter.ReceiveScrollEvent(new Point(X: 16, Y: 17), deltaX: 3, deltaY: 4);

        Assert.True(handled);
        receiver.GetValue()?.Received().ReceiveScrollEvent(new Point(X: 16, Y: 17), deltaX: 3, deltaY: 4);
    }
}
