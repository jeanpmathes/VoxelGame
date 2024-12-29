// <copyright file="Biome.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Generation.Worlds.Default.Structures;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Toolkit.Noise;

namespace VoxelGame.Core.Generation.Worlds.Default.Biomes;

/// <summary>
///     Combines a biome definition with a noise generator.
/// </summary>
public sealed class Biome : IDisposable
{
    private readonly NoiseGenerator noise;
    private readonly NoiseGenerator coverNoise;

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

        Structure = definition.Structure != null ? structureMap[definition.Structure] : null;

        noise = factory.CreateNext()
            .WithType(NoiseType.GradientNoise)
            .WithFrequency(definition.Frequency)
            .WithFractals()
            .WithOctaves(octaves: 3)
            .WithLacunarity(lacunarity: 2.0f)
            .WithGain(gain: 0.5f)
            .WithWeightedStrength(weightedStrength: 0.0f)
            .Build();

        coverNoise = factory.CreateNext()
            .WithType(NoiseType.GradientNoise)
            .WithFrequency(frequency: 0.5f)
            .Build();
    }

    /// <summary>
    ///     The definition of the biome.
    /// </summary>
    public BiomeDefinition Definition { get; }

    /// <summary>
    ///     The structure generator of the biome, if any.
    /// </summary>
    public StructureGenerator? Structure { get; }

    #region DISPOSING

    /// <inheritdoc />
    public void Dispose()
    {
        noise.Dispose();
        coverNoise.Dispose();
    }

    #endregion DISPOSING

    /// <summary>
    ///     Get an offset value for the given column, which can be applied to the height.
    /// </summary>
    /// <param name="position">The position of the column.</param>
    /// <returns>The offset value.</returns>
    public Single GetOffset(Vector2i position)
    {
        return noise.GetNoise(position) * Definition.Amplitude;
    }

    /// <summary>
    ///     Calculate the dampening that is applied to a column, depending on the offset.
    /// </summary>
    /// <param name="originalOffset">The offset of the colum.</param>
    /// <returns>The applied dampening.</returns>
    public Dampening CalculateDampening(Int32 originalOffset)
    {
        const Int32 dampenThreshold = 2;
        Int32 normalWidth = Definition.MaxDampenWidth / 2;

        if (Math.Abs(originalOffset) <= dampenThreshold) return new Dampening(originalOffset, originalOffset, normalWidth);

        Int32 maxDampening = Definition.MaxDampenWidth / 2;
        Int32 dampenedOffset = Math.Clamp(Math.Abs(originalOffset) - dampenThreshold, min: 0, maxDampening) * Math.Sign(originalOffset);

        return new Dampening(dampenedOffset, originalOffset, normalWidth + dampenedOffset);
    }

    /// <summary>
    ///     Get the total width of the biome, depending on the dampening.
    /// </summary>
    /// <param name="dampening">The dampening.</param>
    /// <returns>The total width of the biome.</returns>
    public Int32 GetTotalWidth(Dampening dampening)
    {
        return Definition.MinWidth + dampening.Width;
    }

    /// <summary>
    ///     Get the biome content for a given depth beneath the surface level.
    /// </summary>
    /// <param name="depthBelowSurface">The depth beneath the terrain surface level.</param>
    /// <param name="dampening">The dampening to apply to the column.</param>
    /// <param name="stoneType">The stone type of the column.</param>
    /// <param name="isFilled">Whether this column is filled with water.</param>
    /// <returns>The biome content.</returns>
    public Content GetContent(Int32 depthBelowSurface, Dampening dampening, Map.StoneType stoneType, Boolean isFilled)
    {
        Layer current;
        Int32 depthInLayer;
        Int32 actualOffset;

        Boolean isInUpperHorizon = depthBelowSurface < Definition.DepthToDampen;

        if (isInUpperHorizon)
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

        return current.GetContent(depthInLayer, actualOffset, stoneType, isFilledAtCurrentDepth);
    }

    /// <summary>
    ///     Get the cover content for a given position.
    /// </summary>
    /// <param name="position">The position of the block.</param>
    /// <param name="isFilled">Whether the block is filled with water because it is below sea level.</param>
    /// <param name="sample">The current map sample.</param>
    /// <returns>The cover content.</returns>
    public Content GetCoverContent(Vector3i position, Boolean isFilled, in Map.Sample sample)
    {
        return Definition.Cover.GetContent(coverNoise, position, isFilled, sample);
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
