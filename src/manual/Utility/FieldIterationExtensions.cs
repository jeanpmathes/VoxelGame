// <copyright file="FieldIterationExtensions.cs" company="VoxelGame">
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
using System.Linq;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Manual.Utility;

/// <summary>
///     Provides functions to iterate over fields of a class.
/// </summary>
public static class FieldIterationExtensions
{
    /// <summary>
    ///     Get the values for all fields with a certain type, and the corresponding field documentation.
    /// </summary>
    public static IEnumerable<(T, String)> GetDocumentedValues<T>(this Object obj, Documentation documentation) where T : class
    {
        return Reflections.GetPropertiesOfType<T>(obj)
            .Where(info => info.GetValue(obj) != null)
            .Select(info => ((T) info.GetValue(obj)!, documentation.GetPropertySummary(info)));
    }
}
