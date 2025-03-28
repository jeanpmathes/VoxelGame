// <copyright file="Biome.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
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
    /// <summary>
    ///     The name of the biome.
    /// </summary>
    public String Name { get; } = name;

    /// <summary>
    ///     A color representing the biome.
    /// </summary>
    public required ColorS Color { get; init; }

    /// <summary>
    /// The sub-biome that is part of this biome.
    /// </summary>
    public required SubBiomeDefinition SubBiome { get; init; }

    /// <inheritdoc />
    public RID Identifier { get; } = RID.Named<BiomeDefinition>(name);

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.Biome;

    #region DISPOSING

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose.
    }

    #endregion DISPOSING
}
