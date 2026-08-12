// <copyright file="MockInputSource.cs" company="VoxelGame">
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
using VoxelGame.GUI.Input;

namespace VoxelGame.GUI.Tests.Input;

public class MockInputSource : InputSource
{
    public new Boolean SendKeyEvent(Key key, Boolean isDown, Boolean isRepeat, ModifierKeys modifiers, Boolean isSynthetic = false)
    {
        return base.SendKeyEvent(key, isDown, isRepeat, modifiers, isSynthetic);
    }

    public new Boolean SendTextEvent(String text)
    {
        return base.SendTextEvent(text);
    }

    public new Boolean SendPointerButtonEvent(PointF position, PointerButton button, Boolean isDown, ModifierKeys modifiers, Boolean isSynthetic = false)
    {
        return base.SendPointerButtonEvent(position, button, isDown, modifiers, isSynthetic);
    }

    public new Boolean SendPointerMoveEvent(PointF position, Single deltaX, Single deltaY)
    {
        return base.SendPointerMoveEvent(position, deltaX, deltaY);
    }

    public new Boolean SendScrollEvent(PointF position, Single deltaX, Single deltaY)
    {
        return base.SendScrollEvent(position, deltaX, deltaY);
    }
}
