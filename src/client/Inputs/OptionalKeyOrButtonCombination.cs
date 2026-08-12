// <copyright file="OptionalKeyOrButtonCombination.cs" company="VoxelGame">
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
using System.Xml.Serialization;
using VoxelGame.Graphics.Definition;
using VoxelGame.Graphics.Input;
using VoxelGame.GUI.Input;

namespace VoxelGame.Client.Inputs;

/// <summary>
///     A serializable keybind setting that either uses the defined default or stores an explicit key or pointer button
///     combination.
/// </summary>
[Serializable]
[XmlRoot("OptionalKey")]
public class OptionalKeyOrButtonCombination
{
    /// <summary>
    ///     Get or set whether the defined default should be used.
    /// </summary>
    [XmlElement("Default")] public Boolean UseDefault { get; set; } = true;

    /// <summary>
    ///     Get or set the stored key or pointer button. This value is ignored when <see cref="UseDefault" /> is true.
    /// </summary>
    [XmlElement("Key")] public VirtualKeys KeyOrButton { get; set; } = VirtualKeys.Undefined;

    /// <summary>
    ///     Get or set the required modifier keys. This value is ignored when <see cref="UseDefault" /> is true.
    /// </summary>
    [XmlElement("Modifiers")] public ModifierKeys Modifiers { get; set; } = ModifierKeys.None;
}

/// <summary>
///     Extension methods for <see cref="OptionalKeyOrButtonCombination" />.
/// </summary>
public static class OptionalKeyOrButtonCombinationExtensions
{
    /// <summary>
    ///     Create the value stored in the settings for a key or pointer button combination.
    /// </summary>
    /// <param name="combination">The combination to store.</param>
    /// <param name="useDefault">Whether the defined default should be used instead.</param>
    /// <returns>The serializable settings value.</returns>
    public static OptionalKeyOrButtonCombination ToSettingsValue(this KeyOrButtonCombination combination, Boolean useDefault)
    {
        return useDefault
            ? new OptionalKeyOrButtonCombination()
            : new OptionalKeyOrButtonCombination
            {
                UseDefault = false,
                KeyOrButton = combination.KeyOrButton,
                Modifiers = combination.Modifiers
            };
    }
}
