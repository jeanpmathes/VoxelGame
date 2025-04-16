// <copyright file="SubBiomeDefinition.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Generation.Worlds.Default.SubBiomes;

/// <summary>
///     Definition of a <see cref="SubBiome" />.
/// </summary>
/// <param name="name">The name of the sub-biome.</param>
/// <param name="palette">The block palette to use.</param>
public sealed class SubBiomeDefinition(String name, Palette palette) : IResource
{
    private IList<Layer> layers;

    private (Layer layer, Int32 depth)[] upperHorizon;
    private (Layer layer, Int32 depth)[] lowerHorizon;

    /// <summary>
    ///     The name of the sub-biome.
    /// </summary>
    public String Name { get; } = name;

    /// <summary>
    ///     Get the normal width of the ice layer on oceans.
    /// </summary>
    public Int32 IceWidth { get; init; }

    /// <summary>
    ///     The amplitude of the noise used to generate the sub-biome.
    /// </summary>
    public Single Amplitude { get; init; }

    /// <summary>
    ///     The frequency of the noise used to generate the sub-biome.
    /// </summary>
    public Single Frequency { get; init; }

    /// <summary>
    ///     An overall offset to apply to the sub-biome height.
    /// </summary>
    public Int32 Offset { get; init; }

    /// <summary>
    ///     All layers that are part of the sub-biome.
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
    ///     Get all decorations of this sub-biome, combined with their rarity.
    ///     A higher rarity indicates a lower chance of placement.
    /// </summary>
    public ICollection<(Decoration decoration, Single rarity)> Decorations { get; init; } = new List<(Decoration, Single)>();

    /// <summary>
    ///     Get the structure of this sub-biome, if any.
    ///     Each sub-biome can only have one structure.
    /// </summary>
    public StructureGeneratorDefinition? Structure { get; init; }

    /// <summary>
    ///     Get the cover of the sub-biome.
    ///     Cover is placed on top of the highest layer of the sub-biome.
    /// </summary>
    public Cover Cover { get; init; } = null!;

    /// <summary>
    ///     Get the stuffer of the sub-biome.
    ///     It can be used to stuff the space between the global height and the local height.
    ///     Only applied if the <see cref="Offset" /> is reached.
    /// </summary>
    public Stuffer? Stuffer { get; init; }

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
    ///     The minimum width of the sub-biome, without dampening.
    /// </summary>
    public Int32 MinWidth { get; private set; }

    /// <summary>
    ///     The dampening layer.
    /// </summary>
    public Layer Dampen { get; private set; }

    /// <inheritdoc />
    public RID Identifier { get; } = RID.Named<SubBiomeDefinition>(name);

    /// <inheritdoc />
    public ResourceType Type => ResourceTypes.SubBiome;

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose.
    }

    #endregion DISPOSABLE

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
    ///     The upper horizon is the part of the sub-biome above the dampening layer.
    /// </summary>
    /// <param name="depth">The depth in the upper horizon.</param>
    /// <returns>The layer and depth inside the layer.</returns>
    public (Layer layer, Int32 depth) GetUpperHorizon(Int32 depth)
    {
        return upperHorizon[depth];
    }

    /// <summary>
    ///     Get the layer at the given depth in the lower horizon.
    ///     The lower horizon is the part of the sub-biome below the dampening layer.
    /// </summary>
    /// <param name="depth">The depth in the lower horizon.</param>
    /// <returns>The layer and depth inside the layer.</returns>
    public (Layer layer, Int32 depth) GetLowerHorizon(Int32 depth)
    {
        return lowerHorizon[depth];
    }
}
