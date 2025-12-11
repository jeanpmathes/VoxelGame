// <copyright file="Generators.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using VoxelGame.Core.Generation.Worlds;
using VoxelGame.Core.Generation.Worlds.Standard;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Generation;

/// <summary>
///     Resource catalog with all world generators.
/// </summary>
public class Generators : ResourceCatalog
{
    /// <summary>
    ///     Create a new instance of the generators catalog.
    /// </summary>
    public Generators() : base([
        .. GetGeneratorEntries<Generator>(),
        .. GetGeneratorEntries<Worlds.Water.Generator>()
    ]) {}

    private static IEnumerable<ICatalogEntry> GetGeneratorEntries<T>() where T : IWorldGenerator
    {
        return
        [
            T.CreateResourceCatalog(),
            new GeneratorLinker<T>()
        ];
    }

    private sealed class GeneratorLinker<T> : IResourceLinker where T : IWorldGenerator
    {
        public void Link(IResourceContext context)
        {
            T.LinkResources(context);
        }
    }
}
