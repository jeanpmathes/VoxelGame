// <copyright file="KeyMap.cs" company="VoxelGame">
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
using System.Collections.Generic;
using VoxelGame.Graphics.Definition;

namespace VoxelGame.Graphics.Input.Collections;

/// <summary>
///     Maps keys to their usage count.
/// </summary>
public class KeyMap
{
    private readonly Dictionary<VirtualKeys, Int32> usageCount = new();

    /// <summary>
    ///     Add a binding to the map.
    /// </summary>
    /// <param name="keyOrButton">The key or button targeted by the binding.</param>
    /// <returns>True if the binding does not cause conflicts.</returns>
    public Boolean AddBinding(VirtualKeys keyOrButton)
    {
        Boolean unused = usageCount.TryAdd(keyOrButton, value: 0);

        usageCount[keyOrButton]++;

        return unused;
    }

    /// <summary>
    ///     Remove a binding from the map.
    /// </summary>
    /// <param name="keyOrButton">The key or button that is targeted by one action less.</param>
    public void RemoveBinding(VirtualKeys keyOrButton)
    {
        usageCount[keyOrButton]--;

        if (usageCount[keyOrButton] == 0) usageCount.Remove(keyOrButton);
    }

    /// <summary>
    ///     Get the usage count of a key or button.
    /// </summary>
    /// <param name="keyOrButton">The key or button.</param>
    /// <returns>The usage of the key or button.</returns>
    public Int32 GetUsageCount(VirtualKeys keyOrButton)
    {
        if (!usageCount.TryGetValue(keyOrButton, out Int32 count)) count = 0;

        return count;
    }
}
