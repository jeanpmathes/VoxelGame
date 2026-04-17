// <copyright file="ScrollEvent.cs" company="VoxelGame">
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
///     A scroll input event.
/// </summary>
public sealed class ScrollEvent : PointerEvent
{
    /// <summary>
    ///     Creates a new <seealso cref="ScrollEvent" />.
    /// </summary>
    public ScrollEvent(Visual source, PointF position, Single deltaX, Single deltaY) : base(source, position)
    {
        DeltaX = deltaX;
        DeltaY = deltaY;
    }

    /// <summary>
    ///     The scroll delta along the horizontal axis.
    /// </summary>
    public Single DeltaX { get; }

    /// <summary>
    ///     The scroll delta along the vertical axis.
    /// </summary>
    public Single DeltaY { get; }
}
