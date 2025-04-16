// <copyright file="SubBiome.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Generation.Worlds.Default.SubBiomes;

/// <summary>
///     A sub-biome is similar to a biome, but tied to a parent biome.
///     It creates variation within a biome.
/// </summary>
public sealed class SubBiome : IDisposable
{
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
        return noise.GetNoise(position) * Definition.Amplitude + Definition.Offset;
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
    ///     Get the total width of the sub-biome, depending on the dampening.
    /// </summary>
    /// <param name="dampening">The dampening.</param>
    /// <returns>The total width of the sub-biome.</returns>
    public Int32 GetTotalWidth(Dampening dampening)
    {
        return Definition.MinWidth + dampening.Width;
    }

    /// <summary>
    ///     Get the sub-biome content for a given depth beneath the surface level.
    /// </summary>
    /// <param name="depthBelowSurface">The depth beneath the terrain surface level.</param>
    /// <param name="y">The y coordinate of the current depth.</param>
    /// <param name="dampening">The dampening to apply to the column.</param>
    /// <param name="stoneType">The stone type of the column.</param>
    /// <param name="isFilled">Whether this column is filled with water.</param>
    /// <returns>The sub-biome content.</returns>
    public Content GetContent(Int32 depthBelowSurface, Int32 y, Dampening dampening, Map.StoneType stoneType, Boolean isFilled)
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

        return current.GetContent(depthInLayer, actualOffset, stoneType, y, isFilledAtCurrentDepth);
    }

    /// <summary>
    ///     Get the cover content for a given position.
    /// </summary>
    /// <param name="position">The position of the block.</param>
    /// <param name="isFilled">Whether the block is filled with water because it is below sea level.</param>
    /// <param name="heightFraction">The fraction of the height, above the integer terrain height.</param>
    /// <param name="sample">The current map sample.</param>
    /// <returns>The cover content.</returns>
    public Content GetCoverContent(Vector3i position, Boolean isFilled, Double heightFraction, in Map.Sample sample)
    {
        return Definition.Cover.GetContent(position, isFilled, heightFraction, sample);
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
