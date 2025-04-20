// <copyright file="StructureGeneratorDefinitionLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Logic.Definitions.Structures;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Generation.Worlds.Default.Structures;

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
