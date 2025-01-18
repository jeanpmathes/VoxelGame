// <copyright file="Biome.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using VoxelGame.Core.Generation.Worlds.Default.Decorations;
using VoxelGame.Core.Generation.Worlds.Default.Palettes;
using VoxelGame.Core.Generation.Worlds.Default.Structures;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Generation.Worlds.Default.Biomes;

/// <summary>
///     A biome is a collection of attributes of an area in the world.
/// </summary>
/// <param name="name">The name of the biome.</param>
/// <param name="palette">The block palette to use.</param>
public sealed class BiomeDefinition(String name, Palette palette) : IResource
{
    private IList<Layer> layers;

    private (Layer layer, Int32 depth)[] upperHorizon;
    private (Layer layer, Int32 depth)[] lowerHorizon;

    /// <summary>
    ///     The name of the biome.
    /// </summary>
    public String Name { get; } = name;

    /// <summary>
    ///     A color representing the biome.
    /// </summary>
    public ColorS Color { get; init; }

    /// <summary>
    ///     Get the normal width of the ice layer on oceans.
    /// </summary>
    public Int32 IceWidth { get; init; }

    /// <summary>
    ///     The amplitude of the noise used to generate the biome.
    /// </summary>
    public Single Amplitude { get; init; }

    /// <summary>
    ///     The frequency of the noise used to generate the biome.
    /// </summary>
    public Single Frequency { get; init; }

    /// <summary>
    ///     All layers that are part of the biome.
    /// </summary>
    public required IList<Layer> Layers
    {
        get => layers;

        [MemberNotNull(nameof(layers))]
        [MemberNotNull(nameof(upperHorizon))]
        [MemberNotNull(nameof(lowerHorizon))]
        [MemberNotNull(nameof(Dampen))]
        init
        {
            layers = value;
            SetUpLayers();
        }
    }

    /// <summary>
    ///     Get all decorations of this biome.
    /// </summary>
    public ICollection<Decoration> Decorations { get; init; } = new List<Decoration>();

    /// <summary>
    ///     Get the structure of this biome, if any.
    ///     Each biome can only have one structure.
    /// </summary>
    public StructureGeneratorDefinition? Structure { get; init; }

    /// <summary>
    ///     Get the cover of the biome.
    /// </summary>
    public Cover Cover { get; init; } = null!;

    /// <summary>
    ///     The width of the dampening layer.
    /// </summary>
    public Int32 MaxDampenWidth { get; private set; }

    /// <summary>
    ///     The depth to the dampening layer.
    /// </summary>
    public Int32 DepthToDampen { get; private set; }

    /// <summary>
    ///     The depth to the solid layer, without dampening.
    /// </summary>
    public Int32 MinDepthToSolid { get; private set; }

    /// <summary>
    ///     The minimum width of the biome, without dampening.
    /// </summary>
    public Int32 MinWidth { get; private set; }

    /// <summary>
    ///     The dampening layer.
    /// </summary>
    public Layer Dampen { get; private set; }

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

    [MemberNotNull(nameof(upperHorizon))]
    [MemberNotNull(nameof(lowerHorizon))]
    [MemberNotNull(nameof(Dampen))]
    private void SetUpLayers()
    {
        MinWidth = 0;
        MinDepthToSolid = 0;

        var hasReachedSolid = false;

        List<(Layer layer, Int32 depth)> newUpperHorizon = [];
        List<(Layer layer, Int32 depth)> newLowerHorizon = [];

        List<(Layer layer, Int32 depth)> currentHorizon = newUpperHorizon;

        foreach (Layer layer in Layers)
        {
            layer.SetPalette(palette);

            if (!hasReachedSolid && layer.IsSolid)
            {
                hasReachedSolid = true;
                MinDepthToSolid = MinWidth;
            }

            if (layer.IsDampen)
            {
                DepthToDampen = MinWidth;
                MaxDampenWidth = layer.Width;
                Dampen = layer;

                currentHorizon = newLowerHorizon;

                continue;
            }

            MinWidth += layer.Width;

            for (var depth = 0; depth < layer.Width; depth++) currentHorizon.Add((layer, depth));
        }

        Debug.Assert(hasReachedSolid);
        Debug.Assert(Dampen != null);

        upperHorizon = newUpperHorizon.ToArray();
        lowerHorizon = newLowerHorizon.ToArray();
    }

    /// <summary>
    ///     Get the layer at the given depth in the upper horizon.
    ///     The upper horizon is the part of the biome above the dampening layer.
    /// </summary>
    /// <param name="depth">The depth in the upper horizon.</param>
    /// <returns>The layer and depth inside the layer.</returns>
    public (Layer layer, Int32 depth) GetUpperHorizon(Int32 depth)
    {
        return upperHorizon[depth];
    }

    /// <summary>
    ///     Get the layer at the given depth in the lower horizon.
    ///     The lower horizon is the part of the biome below the dampening layer.
    /// </summary>
    /// <param name="depth">The depth in the lower horizon.</param>
    /// <returns>The layer and depth inside the layer.</returns>
    public (Layer layer, Int32 depth) GetLowerHorizon(Int32 depth)
    {
        return lowerHorizon[depth];
    }
}
