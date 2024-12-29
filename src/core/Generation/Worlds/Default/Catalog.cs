// <copyright file="Catalog.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>


using VoxelGame.Core.Generation.Worlds.Default.Biomes;
using VoxelGame.Core.Generation.Worlds.Default.Decorations;
using VoxelGame.Core.Generation.Worlds.Default.Palettes;
using VoxelGame.Core.Generation.Worlds.Default.Structures;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Generation.Worlds.Default;

/// <summary>
/// The resource catalog of this world generator.
/// </summary>
public class Catalog : ResourceCatalog
{
    /// <summary>
    /// Create a new instance of the catalog.
    /// </summary>
    public Catalog() : base([
        new PaletteLoader(),
        new DecorationLoader(),
        new DecorationProvider(),
        new StructureGeneratorDefinitionLoader(),
        new StructureGeneratorDefinitionProvider(),
        new BiomeLoader()
    ]) {}
}
