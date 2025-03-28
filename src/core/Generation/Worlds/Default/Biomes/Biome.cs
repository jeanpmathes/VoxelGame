// <copyright file="Biome.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Generation.Worlds.Default.Structures;
using VoxelGame.Core.Generation.Worlds.Default.SubBiomes;

namespace VoxelGame.Core.Generation.Worlds.Default.Biomes;

/// <summary>
///     Combines a biome definition with a noise generator.
/// </summary>
public sealed class Biome : IDisposable
{
    /// <summary>
    ///     Create a new biome.
    /// </summary>
    /// <param name="factory">The noise factory to use.</param>
    /// <param name="definition">The definition of the biome.</param>
    /// <param name="structureMap">Mapping from structure generator definitions to structure generators.</param>
    public Biome(
        NoiseFactory factory, BiomeDefinition definition,
        IReadOnlyDictionary<StructureGeneratorDefinition, StructureGenerator> structureMap)
    {
        Definition = definition;

        SubBiome = new SubBiome(factory, definition.SubBiome, structureMap);
    }

    /// <summary>
    ///     The definition of the biome.
    /// </summary>
    public BiomeDefinition Definition { get; }

    /// <summary>
    ///     The sub-biome that is part of this biome.
    /// </summary>
    public SubBiome SubBiome { get; }

    #region DISPOSING

    /// <inheritdoc />
    public void Dispose()
    {
        SubBiome.Dispose();
    }

    #endregion DISPOSING
}
