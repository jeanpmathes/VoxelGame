// <copyright file="IStructureProvider.cs" company="VoxelGame">
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

using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Contents.Structures;

/// <summary>
///     Provides structures for given resource identifiers.
/// </summary>
public interface IStructureProvider : IResourceProvider
{
    /// <summary>
    ///     Get the structure for the given identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the structure.</param>
    /// <returns>The structure, or a fallback structure if the structure is not found.</returns>
    Structure GetStructure(RID identifier);
}
