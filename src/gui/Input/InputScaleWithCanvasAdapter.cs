// <copyright file="InputScaleWithCanvasAdapter.cs" company="VoxelGame">
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

namespace VoxelGame.GUI.Input;

/// <summary>
///     Scales all received input events to the scale of a given canvas and passes the input to another receiver.
/// </summary>
/// <param name="canvas">The canvas whose scale is used to scale all input events.</param>
/// <param name="receiver">The next receiver which will receive all input events.</param>
public class InputScaleWithCanvasAdapter(Canvas canvas, IInputReceiver receiver) : IInputReceiver
{
    /// <inheritdoc />
    public void ReceiveKeyEvent(Key key, Boolean isDown, Boolean isRepeat, ModifierKeys modifiers)
    {
        receiver.ReceiveKeyEvent(key, isDown, isRepeat, modifiers);
    }

    /// <inheritdoc />
    public void ReceiveTextEvent(String text)
    {
        receiver.ReceiveTextEvent(text);
    }

    /// <inheritdoc />
    public void ReceivePointerButtonEvent(PointF position, PointerButton button, Boolean isDown, ModifierKeys modifiers)
    {
        receiver.ReceivePointerButtonEvent(ScalePosition(position), button, isDown, modifiers);
    }

    /// <inheritdoc />
    public void ReceivePointerMoveEvent(PointF position, Single deltaX, Single deltaY)
    {
        receiver.ReceivePointerMoveEvent(ScalePosition(position), deltaX / canvas.Scale, deltaY / canvas.Scale);
    }

    /// <inheritdoc />
    public void ReceiveScrollEvent(PointF position, Single deltaX, Single deltaY)
    {
        receiver.ReceiveScrollEvent(ScalePosition(position), deltaX, deltaY);
    }

    private PointF ScalePosition(PointF position)
    {
        return new PointF(position.X / canvas.Scale, position.Y / canvas.Scale);
    }
}
