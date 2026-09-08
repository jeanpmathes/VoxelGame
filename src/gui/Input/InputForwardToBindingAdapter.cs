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
using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Utilities;

namespace VoxelGame.GUI.Input;

/// <summary>
///     Forward all received input events to the current value of the value source.
///     If the value source provides <c>null</c>, the input is ignored.
/// </summary>
/// <param name="receiver">The value source.</param>
public class InputForwardToBindingAdapter(IValueSource<IInputReceiver?> receiver) : IInputReceiver
{
    /// <inheritdoc />
    public Boolean ReceiveKeyEvent(Key key, Boolean isDown, Boolean isRepeat, ModifierKeys modifiers, Boolean isSynthetic = false)
    {
        return receiver.GetValue()?.ReceiveKeyEvent(key, isDown, isRepeat, modifiers, isSynthetic) ?? false;
    }

    /// <inheritdoc />
    public Boolean ReceiveTextEvent(String text)
    {
        return receiver.GetValue()?.ReceiveTextEvent(text) ?? false;
    }

    /// <inheritdoc />
    public Boolean ReceivePointerButtonEvent(Point position, PointerButton button, Boolean isDown, ModifierKeys modifiers, Boolean isSynthetic = false)
    {
        return receiver.GetValue()?.ReceivePointerButtonEvent(position, button, isDown, modifiers, isSynthetic) ?? false;
    }

    /// <inheritdoc />
    public Boolean ReceivePointerMoveEvent(Point position, Single deltaX, Single deltaY)
    {
        return receiver.GetValue()?.ReceivePointerMoveEvent(position, deltaX, deltaY) ?? false;
    }

    /// <inheritdoc />
    public Boolean ReceiveScrollEvent(Point position, Single deltaX, Single deltaY)
    {
        return receiver.GetValue()?.ReceiveScrollEvent(position, deltaX, deltaY) ?? false;
    }
}
