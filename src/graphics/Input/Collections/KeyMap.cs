﻿// <copyright file="KeyMap.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

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
        var unused = true;

        if (usageCount.ContainsKey(keyOrButton)) unused = false;
        else usageCount.Add(keyOrButton, value: 0);

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
