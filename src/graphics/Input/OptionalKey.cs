// <copyright file="OptionalKey.cs" company="VoxelGame">
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
using VoxelGame.Graphics.Definition;

namespace VoxelGame.Graphics.Input;

/// <summary>
///     Stores are key or the default state, used for serialization.
/// </summary>
[Serializable]
public class OptionalKey
{
    /// <summary>
    ///     Get or set whether the key should use the default value.
    /// </summary>
    public Boolean Default { get; set; } = true;

    /// <summary>
    ///     The key, if <see cref="Default" /> is false, or an invalid value if <see cref="Default" /> is true.
    /// </summary>
    public VirtualKeys Key { get; set; } = VirtualKeys.Undefined;
}

/// <summary>
///     Extension methods for <see cref="OptionalKey" />.
/// </summary>
public static class OptionalKeyExtensions
{
    /// <summary>
    ///     Get an optional key value to serialize a <see cref="VirtualKeys" /> value.
    /// </summary>
    /// <param name="key">The key to serialize.</param>
    /// <param name="isDefault">Whether to use a default value instead.</param>
    /// <returns>The optional key.</returns>
    public static OptionalKey GetSettings(this VirtualKeys key, Boolean isDefault)
    {
        return isDefault ? new OptionalKey() : new OptionalKey {Default = false, Key = key};
    }
}
