// <copyright file="Biome.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using VoxelGame.Core.Generation.Worlds.Default.SubBiomes;

namespace VoxelGame.Core.Generation.Worlds.Default.Biomes;

/// <summary>
///     Combines a biome definition with a noise generator.
/// </summary>
public sealed class Biome : IDisposable
{
    private readonly List<(SubBiome, Single)> subBiomes;
    private readonly List<(SubBiome, Single)> oceanicSubBiomes;

    /// <summary>
    ///     Create a new biome.
    /// </summary>
    /// <param name="definition">The definition of the biome.</param>
    /// <param name="subBiomeMap">Mapping from sub-biome definitions to sub-biomes generators.</param>
    public Biome(BiomeDefinition definition, IReadOnlyDictionary<SubBiomeDefinition, SubBiome> subBiomeMap)
    {
        Definition = definition;

        subBiomes = SetUpSubBiomes(definition.SubBiomes, subBiomeMap);
        oceanicSubBiomes = definition.IsOceanic ? SetUpSubBiomes(definition.OceanicSubBiomes, subBiomeMap) : [];
    }

    /// <summary>
    ///     The definition of the biome.
    /// </summary>
    public BiomeDefinition Definition { get; }

    /// <summary>
    /// Get all sub-biomes used by this biome.
    /// </summary>
    public IEnumerable<SubBiome> SubBiomes
    {
        get
        {
            foreach ((SubBiome subBiome, _) in subBiomes)
                yield return subBiome;
        }
    }

    /// <summary>
    ///     Get all oceanic sub-biomes used by this biome.
    /// </summary>
    public IEnumerable<SubBiome> OceanicSubBiomes
    {
        get
        {
            foreach ((SubBiome subBiome, _) in oceanicSubBiomes)
                yield return subBiome;
        }
    }

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        // Sub-biomes are disposed by the map class.
    }

    #endregion DISPOSABLE

    private static List<(SubBiome, Single)> SetUpSubBiomes(IReadOnlyList<(SubBiomeDefinition, Int32)> subBiomes, IReadOnlyDictionary<SubBiomeDefinition, SubBiome> subBiomeMap)
    {
        Single tickets = 0;

        foreach ((_, Int32 count) in subBiomes) tickets += count;

        Debug.Assert(tickets > 0);

        List<(SubBiome, Single)> result = [];

        Single sum = 0;

        foreach ((SubBiomeDefinition subBiomeDefinition, Int32 count) in subBiomes)
        {
            SubBiome subBiome = subBiomeMap[subBiomeDefinition];

            sum += count / tickets;

            result.Add((subBiome, sum));
        }

        return result;
    }

    /// <summary>
    ///     Choose a sub-biome based on a value.
    /// </summary>
    /// <param name="value">A value between 0 and 1.</param>
    /// <returns>The chosen sub-biome.</returns>
    public SubBiome ChooseSubBiome(Single value)
    {
        return ChooseSubBiome(subBiomes, value);
    }

    /// <summary>
    ///     Choose an oceanic sub-biome based on a value.
    /// </summary>
    /// <param name="value">A value between 0 and 1.</param>
    /// <returns>The chosen sub-biome.</returns>
    public SubBiome ChooseOceanicSubBiome(Single value)
    {
        return ChooseSubBiome(oceanicSubBiomes, value);
    }

    private static SubBiome ChooseSubBiome(List<(SubBiome, Single)> subBiomes, Single value)
    {
        Debug.Assert(value is >= 0 and <= 1);

        foreach ((SubBiome subBiome, Single threshold) in subBiomes)
            if (value < threshold)
                return subBiome;

        (SubBiome last, _) = subBiomes[^1];

        return last;
    }
}
