// <copyright file="ISubBiomeDefinitionProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Generation.Worlds.Standard.SubBiomes;

/// <summary>
///     Provides sub-biome definitions for the world generator.
/// </summary>
public interface ISubBiomeDefinitionProvider
{
    /// <summary>
    ///     Get a sub-biome definition by its identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the sub-biome.</param>
    /// <returns>The sub-biome definition, or a fallback sub-biome definition if the sub-biome is not found.</returns>
    public SubBiomeDefinition GetSubBiomeDefinition(RID identifier);
}
