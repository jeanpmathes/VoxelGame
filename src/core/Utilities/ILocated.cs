// <copyright file="ILocated.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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

namespace VoxelGame.Core.Utilities;

/// <summary>
///     A resource which allows determining its file location just with a name.
///     This means the resource defines a path and file extension.
/// </summary>
public interface ILocated
{
    /// <summary>
    ///     The resource-relative path to the directory containing all resources of this type.
    /// </summary>
    static abstract String[] Path { get; }

    /// <summary>
    ///     The file extension associated with this resource type, without the leading dot.
    /// </summary>
    static abstract String FileExtension { get; }
}
