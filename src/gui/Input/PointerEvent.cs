// <copyright file="PointerEvent.cs" company="VoxelGame">
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
using VoxelGame.GUI.Visuals;

namespace VoxelGame.GUI.Input;

/// <summary>
///     Event representing pointer input.
/// </summary>
public abstract class PointerEvent : InputEvent
{
    /// <summary>
    ///     Creates a new <seealso cref="PointerEvent" />.
    /// </summary>
    protected PointerEvent(Visual source, PointF position) : base(source)
    {
        RootPosition = position;
    }

    /// <summary>
    ///     The position in root canvas coordinates where the pointer event occurred.
    /// </summary>
    public PointF RootPosition { get; }

    /// <summary>
    ///     The position in the local coordinate space of the current target visual.
    /// </summary>
    public PointF LocalPosition => Target.RootPointToLocal(RootPosition);

    /// <summary>
    ///     Whether the pointer event hits the specified visual.
    /// </summary>
    /// <param name="visual">The visual to test for hit.</param>
    /// <returns>>True if the pointer event hits the visual; otherwise, false.</returns>
    public Boolean Hits(Visual visual)
    {
        return visual.Bounds.Contains(visual.RootPointToLocal(RootPosition));
    }

    /// <summary>
    ///     Whether the pointer event hits the specified control. This checks whether the pointer event hits the visualization
    ///     of the control.
    ///     If the control does not have a visualization, this will return false.
    /// </summary>
    /// <param name="control">The control to test for hit.</param>
    /// <returns>True if the pointer event hits the control's visualization; otherwise, false.</returns>
    public Boolean Hits(Control control)
    {
        return control.Visualization.GetValue() is {} visual && Hits(visual);
    }
}
