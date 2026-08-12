// <copyright file="Functions.cs" company="VoxelGame">
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

namespace VoxelGame.Toolkit.Utilities;

/// <summary>
///     Extensions for working with functions, actions, and delegates.
/// </summary>
public static class Functions
{
    /// <summary>
    ///     Converts an action to a function that returns <see cref="Void.Instance" />.
    /// </summary>
    /// <param name="action">The action to convert.</param>
    /// <returns>The converted function.</returns>
    public static Func<Void> ToFunction(this Action action)
    {
        return () =>
        {
            action();
            return Void.Instance;
        };
    }
}
