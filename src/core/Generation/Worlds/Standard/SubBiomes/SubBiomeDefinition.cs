// <copyright file="SubBiomeDefinition.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using VoxelGame.Core.Generation.Worlds.Standard.Decorations;
using VoxelGame.Core.Generation.Worlds.Standard.Palettes;
using VoxelGame.Core.Generation.Worlds.Standard.Structures;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Generation.Worlds.Standard.SubBiomes;

/// <summary>
///     Definition of a <see cref="SubBiome" />.
/// </summary>
/// <param name="name">The name of the sub-biome.</param>
/// <param name="palette">The block palette to use.</param>
public sealed class SubBiomeDefinition(String name, Palette palette) : IResource
{
    private readonly IList<Layer> layers;
    private (Layer layer, Int32 depth)[] lowerHorizon;

    private (Layer layer, Int32 depth)[] upperHorizon;

    /// <summary>
    ///     The name of the sub-biome.
    /// </summary>
    public String Name { get; } = name;

    /// <summary>
    ///     The amplitude of the noise used to generate the sub-biome.
    /// </summary>
    public Single Amplitude { get; init; }

    /// <summary>
    ///     The direction of the noise used to generate the sub-biome.
    /// </summary>
    public NoiseDirection Direction { get; init; } = NoiseDirection.Both;

    /// <summary>
    ///     The frequency of the noise used to generate the sub-biome.
    /// </summary>
    public Single Frequency { get; init; }

    /// <summary>
    ///     An overall offset to apply to the sub-biome height.
    ///     A negative value will lower the sub-biome, a positive value will raise it.
    /// </summary>
    public Int32 Offset { get; init; }

    /// <summary>
    ///     Whether this sub-biome ignores the blended offset calculated based on the neighboring sub-biomes.
    /// </summary>
    public Boolean IgnoresBlendedOffset { get; init; }

    /// <summary>
    ///     Whether this sub-biome is oceanic.
    ///     If this is set, it must be set before the layers are set.
    ///     Oceanic sub-biomes can be used at oceanic height (sea level) above the ground.
    ///     Oceanic sub-biomes do not need a dampening layer.
    /// </summary>
    public Boolean IsOceanic { get; init; }

    /// <summary>
    ///     Whether this sub-biome is empty, meaning it has no layers.
    ///     This also means that <see cref="Amplitude" />, <see cref="Frequency" /> and <see cref="Offset" /> must be zero.
    /// </summary>
    public Boolean IsEmpty => Layers.Count == 0;

    /// <summary>
    ///     All layers that are part of the sub-biome.
    ///     Must contain a dampening layer and a solid layer below it.
    ///     This restriction does not apply to oceanic sub-biomes, which can even have no layers at all.
    /// </summary>
    public required IList<Layer> Layers
    {
        get => layers;

        [MemberNotNull(nameof(layers))]
        [MemberNotNull(nameof(upperHorizon))]
        [MemberNotNull(nameof(lowerHorizon))]
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
    ///     Cover is placed on top of the highest layer in the sub-biome.
    /// </summary>
    public required Cover Cover { get; init; }

    /// <summary>
    ///     Get the stuffer of the sub-biome.
    ///     It can be used to stuff the space between the global height and the local height.
    ///     Only applied if the <see cref="Offset" /> is reached.
    /// </summary>
    public IStuffer? Stuffer { get; init; }

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
    public Layer? Dampen { get; private set; }

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

            if (!hasReachedSolid && layer.IsSolid && Dampen != null)
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

        Debug.Assert((!IsOceanic).Implies(Dampen != null));
        Debug.Assert((Dampen != null).Implies(hasReachedSolid));
        Debug.Assert((Layers.Count == 0).Implies(MathTools.NearlyZero(Amplitude) && MathTools.NearlyZero(Frequency) && Offset == 0));

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
