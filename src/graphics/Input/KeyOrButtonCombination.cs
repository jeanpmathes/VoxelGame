// <copyright file="KeyOrButtonCombination.cs" company="VoxelGame">
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
using VoxelGame.GUI.Input;

namespace VoxelGame.Graphics.Input;

/// <summary>
///     A key or pointer button together with modifier keys.
/// </summary>
public readonly record struct KeyOrButtonCombination
{
    /// <summary>
    ///     Create a key or pointer button combination.
    /// </summary>
    /// <param name="keyOrButton">The primary key or pointer button.</param>
    /// <param name="modifiers">The modifier keys required in addition to the primary input.</param>
    public KeyOrButtonCombination(VirtualKeys keyOrButton, ModifierKeys modifiers = ModifierKeys.None)
    {
        KeyOrButton = keyOrButton;
        Modifiers = NormalizeModifiers(keyOrButton, modifiers);
    }

    /// <summary>
    ///     Get the primary key or pointer button.
    /// </summary>
    public VirtualKeys KeyOrButton { get; }

    /// <summary>
    ///     Get the required modifier keys.
    /// </summary>
    public ModifierKeys Modifiers { get; }

    /// <summary>
    ///     Create a combination without modifier keys from a key or pointer button.
    /// </summary>
    public static implicit operator KeyOrButtonCombination(VirtualKeys keyOrButton)
    {
        return new KeyOrButtonCombination(keyOrButton);
    }

    /// <inheritdoc />
    public override String ToString()
    {
        List<String> parts = [];

        if (Modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Control");
        if (Modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
        if (Modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");

        parts.Add(KeyOrButton.ToStringFast());

        return String.Join("+", parts);
    }

    private static ModifierKeys NormalizeModifiers(VirtualKeys keyOrButton, ModifierKeys modifiers)
    {
        ModifierKeys ownModifier = keyOrButton switch
        {
            VirtualKeys.LeftControl or VirtualKeys.RightControl or VirtualKeys.Control => ModifierKeys.Control,
            VirtualKeys.LeftMenu or VirtualKeys.RightMenu or VirtualKeys.Menu => ModifierKeys.Alt,
            VirtualKeys.LeftShift or VirtualKeys.RightShift or VirtualKeys.Shift => ModifierKeys.Shift,
            _ => ModifierKeys.None
        };

        return modifiers & ~ownModifier;
    }
}
