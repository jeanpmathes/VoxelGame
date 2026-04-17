// <copyright file="PointerMoveEvent.cs" company="VoxelGame">
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
using VoxelGame.GUI.Visuals;

namespace VoxelGame.GUI.Input;

/// <summary>
///     Represents a pointer move event, which occurs when the pointer moves across the canvas.
/// </summary>
public sealed class PointerMoveEvent : PointerEvent
{
    /// <summary>
    ///     Creates a new <seealso cref="PointerMoveEvent" /> with the specified source visual, pointer position, and change in
    ///     pointer coordinates since the last pointer move event.
    /// </summary>
    public PointerMoveEvent(Visual source, PointF position, Single deltaX, Single deltaY) : base(source, position)
    {
        DeltaX = deltaX;
        DeltaY = deltaY;
    }

    /// <summary>
    ///     The change in the pointer's X coordinate since the last pointer move event, in root canvas coordinates.
    /// </summary>
    public Single DeltaX { get; }

    /// <summary>
    ///     The change in the pointer's Y coordinate since the last pointer move event, in root canvas coordinates.
    /// </summary>
    public Single DeltaY { get; }
}
