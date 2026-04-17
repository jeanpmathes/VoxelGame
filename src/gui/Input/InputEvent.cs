// <copyright file="InputEvent.cs" company="VoxelGame">
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
using VoxelGame.GUI.Visuals;

namespace VoxelGame.GUI.Input;

/// <summary>
///     Base class for all input events.
/// </summary>
public abstract class InputEvent
{
    /// <summary>
    ///     Creates a new input event with the specified source visual. The current target is initially set to the source
    ///     visual.
    /// </summary>
    /// <param name="source">The visual that is the source of this event.</param>
    public InputEvent(Visual source)
    {
        Source = source;
        Target = source;
    }

    /// <summary>
    ///     The visual that is the source of this event.
    /// </summary>
    public Visual Source { get; private set; }

    /// <summary>
    ///     The visual that is currently targeted by this event.
    /// </summary>
    public Visual Target { get; private set; }

    /// <summary>
    ///     Whether this event has been handled. If true, the event will not be propagated to other visuals.
    /// </summary>
    public Boolean Handled { get; set; }

    internal void SetTarget(Visual target)
    {
        Target = target;
    }
}
