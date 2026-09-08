// <copyright file="InputScaleWithCanvasAdapterTests.cs" company="VoxelGame">
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
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Input;
using VoxelGame.GUI.Tests.Rendering;
using VoxelGame.GUI.Themes;
using VoxelGame.GUI.Utilities;
using Xunit;

namespace VoxelGame.GUI.Tests.Input;

[TestSubject(typeof(InputScaleWithCanvasAdapter))]
public sealed class InputScaleWithCanvasAdapterTests : IDisposable
{
    private const Single Scale = 2.0f;

    private readonly Canvas canvas;
    private readonly IInputReceiver receiver;
    private readonly InputScaleWithCanvasAdapter adapter;

    public InputScaleWithCanvasAdapterTests()
    {
        canvas = Canvas.Create(new MockRenderer(), new Theme());
        receiver = Substitute.For<IInputReceiver>();
        adapter = new InputScaleWithCanvasAdapter(canvas, receiver);

        canvas.SetScale(Scale);
    }

    public void Dispose()
    {
        canvas.Dispose();
    }

    [Fact]
    public void InputScaleWithCanvasAdapter_ShouldForwardReceivedKeyEvents()
    {
        receiver.ReceiveKeyEvent(Key.A, isDown: true, isRepeat: false, ModifierKeys.None).Returns(true);

        Boolean handled = adapter.ReceiveKeyEvent(Key.A, isDown: true, isRepeat: false, ModifierKeys.None);

        Assert.True(handled);
        receiver.Received().ReceiveKeyEvent(Key.A, isDown: true, isRepeat: false, ModifierKeys.None);
    }

    [Fact]
    public void InputScaleWithCanvasAdapter_ShouldForwardReceivedTextEvent()
    {
        receiver.ReceiveTextEvent(text: "Hello").Returns(true);

        Boolean handled = adapter.ReceiveTextEvent(text: "Hello");

        Assert.True(handled);
        receiver.Received().ReceiveTextEvent(text: "Hello");
    }

    [Fact]
    public void InputScaleWithCanvasAdapter_ShouldForwardReceivedPointerButtonEvent()
    {
        receiver.ReceivePointerButtonEvent(new Point(12 / Scale, 13 / Scale), PointerButton.Left, isDown: true, ModifierKeys.None).Returns(true);

        Boolean handled = adapter.ReceivePointerButtonEvent(new Point(X: 12, Y: 13), PointerButton.Left, isDown: true, ModifierKeys.None);

        Assert.True(handled);
        receiver.Received().ReceivePointerButtonEvent(new Point(12 / Scale, 13 / Scale), PointerButton.Left, isDown: true, ModifierKeys.None);
    }

    [Fact]
    public void InputScaleWithCanvasAdapter_ShouldForwardReceivedPointerMoveEvent()
    {
        receiver.ReceivePointerMoveEvent(new Point(14 / Scale, 15 / Scale), 1 / Scale, 2 / Scale).Returns(true);

        Boolean handled = adapter.ReceivePointerMoveEvent(new Point(X: 14, Y: 15), deltaX: 1, deltaY: 2);

        Assert.True(handled);
        receiver.Received().ReceivePointerMoveEvent(new Point(14 / Scale, 15 / Scale), 1 / Scale, 2 / Scale);
    }

    [Fact]
    public void InputScaleWithCanvasAdapter_ShouldForwardReceivedScrollEvent()
    {
        receiver.ReceiveScrollEvent(new Point(16 / Scale, 17 / Scale), deltaX: 3, deltaY: 4).Returns(true);

        Boolean handled = adapter.ReceiveScrollEvent(new Point(X: 16, Y: 17), deltaX: 3, deltaY: 4);

        Assert.True(handled);
        receiver.Received().ReceiveScrollEvent(new Point(16 / Scale, 17 / Scale), deltaX: 3, deltaY: 4);
    }
}
