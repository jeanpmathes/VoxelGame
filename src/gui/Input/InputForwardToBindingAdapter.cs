// <copyright file="InputForwardToBindingAdapter.cs" company="VoxelGame">
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
using VoxelGame.GUI.Bindings;

namespace VoxelGame.GUI.Input;

/// <summary>
///     Forward all received input events to the current value of the value source.
///     If the value source provides <c>null</c>, the input is ignored.
/// </summary>
/// <param name="receiver">The value source.</param>
public class InputForwardToBindingAdapter(IValueSource<IInputReceiver?> receiver) : IInputReceiver
{
    /// <inheritdoc />
    public void ReceiveKeyEvent(Key key, Boolean isDown, Boolean isRepeat, ModifierKeys modifiers)
    {
        receiver.GetValue()?.ReceiveKeyEvent(key, isDown, isRepeat, modifiers);
    }

    /// <inheritdoc />
    public void ReceiveTextEvent(String text)
    {
        receiver.GetValue()?.ReceiveTextEvent(text);
    }

    /// <inheritdoc />
    public void ReceivePointerButtonEvent(PointF position, PointerButton button, Boolean isDown, ModifierKeys modifiers)
    {
        receiver.GetValue()?.ReceivePointerButtonEvent(position, button, isDown, modifiers);
    }

    /// <inheritdoc />
    public void ReceivePointerMoveEvent(PointF position, Single deltaX, Single deltaY)
    {
        receiver.GetValue()?.ReceivePointerMoveEvent(position, deltaX, deltaY);
    }

    /// <inheritdoc />
    public void ReceiveScrollEvent(PointF position, Single deltaX, Single deltaY)
    {
        receiver.GetValue()?.ReceiveScrollEvent(position, deltaX, deltaY);
    }
}
