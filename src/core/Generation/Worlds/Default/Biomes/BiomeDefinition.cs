﻿// <copyright file="Biome.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using VoxelGame.Core.Generation.Worlds.Default.SubBiomes;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Generation.Worlds.Default.Biomes;

/// <summary>
///     A biome is a collection of attributes of an area in the world.
/// </summary>
/// <param name="name">The name of the biome.</param>
public sealed class BiomeDefinition(String name) : IResource
{
    private readonly IReadOnlyList<(SubBiomeDefinition, Int32)>? oceanicSubBiomes;

    /// <summary>
    ///     The name of the biome.
    /// </summary>
    public String Name { get; } = name;

    /// <summary>
    ///     A color representing the biome.
    /// </summary>
    public required ColorS Color { get; init; }

    /// <summary>
    /// The sub-biomes of this biome, as tuples.
    /// Each tuple associates a sub-biome with a ticket count.
    /// A higher count number means that the sub-biome is more likely to be chosen.
    /// Must contain at least one sub-biome.
    /// </summary>
    public required IReadOnlyList<(SubBiomeDefinition, Int32)> SubBiomes { get; init; }

    /// <summary>
    ///     Similar to <see cref="SubBiomes" />, but for oceanic sub-biomes.
    /// </summary>
    public IReadOnlyList<(SubBiomeDefinition, Int32)>? OceanicSubBiomes
    {
        get => oceanicSubBiomes;

        init
        {
            oceanicSubBiomes = value;

            Debug.Assert(oceanicSubBiomes?.All(((SubBiomeDefinition subBiome, Int32 tickets) entry) => entry.subBiome.IsOceanic) ?? true);
        }
    }

    /// <summary>
    ///     Indicates if the biome is oceanic.
    ///     Oceanic biomes use two layers of sub-biomes.
    /// </summary>
    [MemberNotNullWhen(returnValue: true, nameof(OceanicSubBiomes))]
    public Boolean IsOceanic => OceanicSubBiomes != null;

    /// <inheritdoc />
    public RID Identifier { get; } = RID.Named<BiomeDefinition>(name);

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.Biome;

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose.
    }

    #endregion DISPOSABLE
}
