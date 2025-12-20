// <copyright file="IGrid.cs" company="VoxelGame">
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

using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Voxels;

namespace VoxelGame.Core.Logic;

/// <summary>
///     A <see cref="IGrid" /> which only allows reading.
/// </summary>
public interface IReadOnlyGrid
{
    /// <summary>
    ///     Get content at a given position.
    /// </summary>
    /// <param name="position">The position to get the content from.</param>
    /// <returns>The content at the given position, or null if the position is out of bounds.</returns>
    Content? GetContent(Vector3i position);
}

/// <summary>
///     Represents a grid of block positions, filled with content.
/// </summary>
public interface IGrid : IReadOnlyGrid
{
    /// <summary>
    ///     Set content at a given position. If the position is out of bounds, nothing happens.
    /// </summary>
    /// <param name="content">The content to set.</param>
    /// <param name="position">The position to set the content at.</param>
    void SetContent(Content content, Vector3i position);
}
