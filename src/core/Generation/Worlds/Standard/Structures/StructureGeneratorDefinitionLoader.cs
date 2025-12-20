// <copyright file="StructureGeneratorDefinitionLoader.cs" company="VoxelGame">
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
using System.Collections.Generic;
using VoxelGame.Core.Logic.Contents.Structures;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Generation.Worlds.Standard.Structures;

/// <summary>
///     Loads all structures for this world generator.
/// </summary>
public class StructureGeneratorDefinitionLoader : IResourceLoader
{
    String? ICatalogEntry.Instance => null;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context)
    {
        return context.Require<IStructureProvider>(structures =>
        [
            new StructureGeneratorDefinition("SmallPyramid",
                StructureGeneratorDefinition.Kind.Surface,
                structures.GetStructure(RID.File<StaticStructure>("small_pyramid")),
                rarity: 2.0f,
                (0, -6, 0)),
            new StructureGeneratorDefinition("LargeTropicalTree",
                StructureGeneratorDefinition.Kind.Surface,
                structures.GetStructure(RID.File<StaticStructure>("large_tropical_tree")),
                rarity: 0.0f,
                (0, -5, 0)),
            new StructureGeneratorDefinition("OldTower",
                StructureGeneratorDefinition.Kind.Surface,
                structures.GetStructure(RID.File<StaticStructure>("old_tower")),
                rarity: 2.0f,
                (0, -2, 0)),
            new StructureGeneratorDefinition("BuriedTower",
                StructureGeneratorDefinition.Kind.Underground,
                structures.GetStructure(RID.File<StaticStructure>("buried_tower")),
                rarity: 2.0f,
                (0, -2, 0))
        ]);
    }
}
