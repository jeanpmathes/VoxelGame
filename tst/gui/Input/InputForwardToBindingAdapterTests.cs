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

using System.Drawing;
using JetBrains.Annotations;
using NSubstitute;
using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Input;
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
        adapter.ReceiveKeyEvent(Key.A, isDown: true, isRepeat: false, ModifierKeys.None);
        receiver.GetValue()?.Received().ReceiveKeyEvent(Key.A, isDown: true, isRepeat: false, ModifierKeys.None);
    }

    [Fact]
    public void InputForwardToBindingAdapter_ShouldForwardReceivedTextEvent()
    {
        adapter.ReceiveTextEvent(text: "Hello");
        receiver.GetValue()?.Received().ReceiveTextEvent(text: "Hello");
    }

    [Fact]
    public void InputForwardToBindingAdapter_ShouldForwardReceivedPointerButtonEvent()
    {
        adapter.ReceivePointerButtonEvent(new PointF(x: 12, y: 13), PointerButton.Left, isDown: true, ModifierKeys.None);
        receiver.GetValue()?.Received().ReceivePointerButtonEvent(new PointF(x: 12, y: 13), PointerButton.Left, isDown: true, ModifierKeys.None);
    }

    [Fact]
    public void InputForwardToBindingAdapter_ShouldForwardReceivedPointerMoveEvent()
    {
        adapter.ReceivePointerMoveEvent(new PointF(x: 14, y: 15), deltaX: 1, deltaY: 2);
        receiver.GetValue()?.Received().ReceivePointerMoveEvent(new PointF(x: 14, y: 15), deltaX: 1, deltaY: 2);
    }

    [Fact]
    public void InputForwardToBindingAdapter_ShouldForwardReceivedScrollEvent()
    {
        adapter.ReceiveScrollEvent(new PointF(x: 16, y: 17), deltaX: 3, deltaY: 4);
        receiver.GetValue()?.Received().ReceiveScrollEvent(new PointF(x: 16, y: 17), deltaX: 3, deltaY: 4);
    }
}
