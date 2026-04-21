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
using System.Drawing;
using JetBrains.Annotations;
using NSubstitute;
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Input;
using VoxelGame.GUI.Tests.Rendering;
using VoxelGame.GUI.Themes;
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
        adapter.ReceiveKeyEvent(Key.A, isDown: true, isRepeat: false, ModifierKeys.None);
        receiver.Received().ReceiveKeyEvent(Key.A, isDown: true, isRepeat: false, ModifierKeys.None);
    }

    [Fact]
    public void InputScaleWithCanvasAdapter_ShouldForwardReceivedTextEvent()
    {
        adapter.ReceiveTextEvent(text: "Hello");
        receiver.Received().ReceiveTextEvent(text: "Hello");
    }

    [Fact]
    public void InputScaleWithCanvasAdapter_ShouldForwardReceivedPointerButtonEvent()
    {
        adapter.ReceivePointerButtonEvent(new PointF(x: 12, y: 13), PointerButton.Left, isDown: true, ModifierKeys.None);
        receiver.Received().ReceivePointerButtonEvent(new PointF(12 / Scale, 13 / Scale), PointerButton.Left, isDown: true, ModifierKeys.None);
    }

    [Fact]
    public void InputScaleWithCanvasAdapter_ShouldForwardReceivedPointerMoveEvent()
    {
        adapter.ReceivePointerMoveEvent(new PointF(x: 14, y: 15), deltaX: 1, deltaY: 2);
        receiver.Received().ReceivePointerMoveEvent(new PointF(14 / Scale, 15 / Scale), 1 / Scale, 2 / Scale);
    }

    [Fact]
    public void InputScaleWithCanvasAdapter_ShouldForwardReceivedScrollEvent()
    {
        adapter.ReceiveScrollEvent(new PointF(x: 16, y: 17), deltaX: 3, deltaY: 4);
        receiver.Received().ReceiveScrollEvent(new PointF(16 / Scale, 17 / Scale), deltaX: 3, deltaY: 4);
    }
}
