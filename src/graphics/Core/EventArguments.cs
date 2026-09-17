// <copyright file="EventArguments.cs" company="VoxelGame">
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
using OpenTK.Mathematics;

namespace VoxelGame.Graphics.Core;

/// <summary>
///     Event arguments for window size change.
/// </summary>
/// <param name="oldSize">The old size.</param>
/// <param name="newSize">The new size.</param>
public class SizeChangeEventArgs(Vector2i oldSize, Vector2i newSize) : EventArgs
{
    /// <summary>The old size.</summary>
    public Vector2i OldSize { get; init; } = oldSize;

    /// <summary>The new size.</summary>
    public Vector2i NewSize { get; init; } = newSize;
}

/// <summary>
///     Event arguments for focus change.
/// </summary>
/// <param name="oldFocus">Whether the window was focused before.</param>
/// <param name="newFocus">Whether the window is focused now.</param>
public class FocusChangeEventArgs(Boolean oldFocus, Boolean newFocus) : EventArgs
{
    /// <summary>Whether the window was focused before.</summary>
    public Boolean OldFocus { get; init; } = oldFocus;

    /// <summary>Whether the window is focused now.</summary>
    public Boolean NewFocus { get; init; } = newFocus;
}
