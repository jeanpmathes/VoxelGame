// <copyright file="SubBiome.cs" company="VoxelGame">
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
using OpenTK.Mathematics;
using VoxelGame.Core.Generation.Worlds.Standard.Structures;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Units;
using VoxelGame.Toolkit.Noise;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Standard.SubBiomes;

/// <summary>
///     A sub-biome is similar to a biome, but tied to a parent biome.
///     It creates variation within a biome.
/// </summary>
public sealed class SubBiome : IDisposable
{
    private readonly Int32 dampeningFactor;
    private readonly NoiseGenerator noise;

    /// <summary>
    ///     Create a new sub-biome.
    /// </summary>
    /// <param name="factory">The noise factory to use.</param>
    /// <param name="definition">The definition of the sub-biome.</param>
    /// <param name="structureMap">Mapping from structure generator definitions to structure generators.</param>
    public SubBiome(
        NoiseFactory factory, SubBiomeDefinition definition,
        IReadOnlyDictionary<StructureGeneratorDefinition, StructureGenerator> structureMap)
    {
        Definition = definition;

        Structure = definition.Structure != null ? structureMap[definition.Structure] : null;

        noise = factory.CreateNext()
            .WithType(NoiseType.GradientNoise)
            .WithFrequency(definition.Frequency)
            .WithFractals()
            .WithOctaves(octaves: 5)
            .WithLacunarity(lacunarity: 2.0f)
            .WithGain(gain: 0.5f)
            .WithWeightedStrength(weightedStrength: 0.0f)
            .Build();

        dampeningFactor = Definition.Dampen != null ? 1 : 0;
    }

    /// <summary>
    ///     The definition of the sub-biome.
    /// </summary>
    public SubBiomeDefinition Definition { get; }

    /// <summary>
    ///     The structure generator of the sub-biome, if any.
    /// </summary>
    public StructureGenerator? Structure { get; }

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        noise.Dispose();
    }

    #endregion DISPOSABLE

    /// <summary>
    ///     Get an offset value for the given column, which can be applied to the height.
    /// </summary>
    /// <param name="position">The position of the column.</param>
    /// <returns>The offset value.</returns>
    public Single GetOffset(Vector2i position)
    {
        if (MathTools.NearlyZero(Definition.Amplitude))
            return Definition.Offset;

        Single noiseOffset = noise.GetNoise(position) * Definition.Amplitude;

        noiseOffset = Definition.Direction switch
        {
            NoiseDirection.Both => noiseOffset,
            NoiseDirection.Up => Math.Abs(noiseOffset),
            NoiseDirection.Down => -Math.Abs(noiseOffset),
            _ => throw Exceptions.UnsupportedEnumValue(Definition.Direction)
        };

        return noiseOffset + Definition.Offset;
    }

    /// <summary>
    ///     Calculate the dampening which is applied to a column, depending on the offset.
    /// </summary>
    /// <param name="originalOffset">The offset of the colum.</param>
    /// <returns>The applied dampening.</returns>
    public Dampening CalculateDampening(Int32 originalOffset)
    {
        if (Definition.Dampen == null)
            return new Dampening(originalOffset, originalOffset, Width: 0);

        Int32 normalWidth = Definition.MaxDampenWidth / 2;

        const Int32 dampenThreshold = 2;

        if (Math.Abs(originalOffset) <= dampenThreshold) return new Dampening(originalOffset, originalOffset, normalWidth);

        Int32 maxDampening = Definition.MaxDampenWidth / 2;
        Int32 dampenedOffset = Math.Clamp(Math.Abs(originalOffset) - dampenThreshold, min: 0, maxDampening) * Math.Sign(originalOffset);

        return new Dampening(dampenedOffset, originalOffset, normalWidth + dampenedOffset);
    }

    /// <summary>
    ///     Get the total width of the sub-biome, depending on the dampening.
    /// </summary>
    /// <param name="dampening">The dampening.</param>
    /// <returns>The total width of the sub-biome.</returns>
    public Int32 GetTotalWidth(Dampening dampening)
    {
        return Definition.MinWidth + dampening.Width * dampeningFactor;
    }

    /// <summary>
    ///     Get the sub-biome content for a given depth beneath the surface level.
    /// </summary>
    /// <param name="depthBelowSurface">The depth beneath the terrain surface level.</param>
    /// <param name="y">The y coordinate of the current depth.</param>
    /// <param name="isFilled">Whether this column is filled with water.</param>
    /// <param name="dampening">The dampening to apply to the column.</param>
    /// <param name="stoneType">The stone type of the column.</param>
    /// <param name="temperature">The temperature at the current position.</param>
    /// <returns>The sub-biome content.</returns>
    public Content GetContent(Int32 depthBelowSurface, Int32 y, Boolean isFilled, Dampening dampening, Map.StoneType stoneType, Temperature temperature)
    {
        Layer current;
        Int32 depthInLayer;
        Int32 actualOffset;

        Boolean isInUpperHorizon = depthBelowSurface < Definition.DepthToDampen;

        if (isInUpperHorizon || Definition.Dampen == null)
        {
            (current, depthInLayer) = Definition.GetUpperHorizon(depthBelowSurface);
            actualOffset = dampening.OriginalOffset;
        }
        else
        {
            (actualOffset, _, Int32 usedWidth) = dampening;
            Int32 depthToLowerHorizon = Definition.DepthToDampen + usedWidth;

            if (depthBelowSurface < depthToLowerHorizon) (current, depthInLayer) = (Definition.Dampen, depthBelowSurface - Definition.DepthToDampen);
            else (current, depthInLayer) = Definition.GetLowerHorizon(depthBelowSurface - depthToLowerHorizon);
        }

        Int32 actualDepthToSolid = Definition.MinDepthToSolid + dampening.Width;
        Boolean isFilledAtCurrentDepth = depthBelowSurface < actualDepthToSolid && isFilled;

        return current.GetContent(depthInLayer, actualOffset, y, stoneType, isFilledAtCurrentDepth, temperature);
    }

    /// <summary>
    ///     Get the cover content for a given position.
    /// </summary>
    /// <param name="position">The position of the block.</param>
    /// <param name="isFilled">Whether the block is filled with water because it is below sea level.</param>
    /// <param name="heightFraction">The fraction of the height, above the integer terrain height.</param>
    /// <param name="climate">The climate of the position.</param>
    /// <returns>The cover content.</returns>
    public Content GetCoverContent(Vector3i position, Boolean isFilled, Double heightFraction, in Map.PositionClimate climate)
    {
        return Definition.Cover.GetContent(position, isFilled, heightFraction, climate);
    }

    /// <summary>
    ///     Get the depth to the first solid layer, depending on the dampening.
    /// </summary>
    public Int32 GetDepthToSolid(Dampening dampening)
    {
        return Definition.MinDepthToSolid + dampening.Width;
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return Definition.Name;
    }

    /// <summary>
    ///     The dampening applied to a column.
    /// </summary>
    public record struct Dampening(Int32 DampenedOffset, Int32 OriginalOffset, Int32 Width);
}
