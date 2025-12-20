// <copyright file="Catalog.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Generation.Worlds.Water;

/// <summary>
///     The resource catalog of this world generator.
/// </summary>
public class Catalog : ResourceCatalog
{
    /// <summary>
    ///     Create a new instance of the catalog.
    /// </summary>
    public Catalog() : base([]) {}
}
