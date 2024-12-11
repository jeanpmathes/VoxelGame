// <copyright file="NoiseBuilder.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics.CodeAnalysis;

namespace VoxelGame.Toolkit.Noise;

#pragma warning disable CA1033 // Used to create builder.

/// <summary>
/// Some common methods of all builder parts.
/// </summary>
public interface INoiseBuilder
{
    /// <summary>
    /// Get the noise builder.
    /// </summary>
    NoiseBuilder And();

    /// <summary>
    /// Builds and returns the configured <see cref="NoiseGenerator"/>.
    /// </summary>
    NoiseGenerator Build();
}

/// <summary>
/// Step to define the fractal noise.
/// </summary>
public interface IFractalNoiseBuilder : INoiseBuilder
{
    /// <summary>
    /// Set the number of fractal octaves.
    /// </summary>
    IFractalNoiseBuilder WithOctaves(Int32 octaves);

    /// <summary>
    /// Set the lacunarity.
    /// </summary>
    IFractalNoiseBuilder WithLacunarity(Single lacunarity);

    /// <summary>
    /// Set the gain.
    /// </summary>
    IFractalNoiseBuilder WithGain(Single gain);

    /// <summary>
    /// Set the weighted strength.
    /// </summary>
    IFractalNoiseBuilder WithWeightedStrength(Single weightedStrength);
}

/// <summary>
/// Use this to build instances of <see cref="NoiseGenerator"/>.
/// </summary>
[SuppressMessage("Design", "CA1033:Interface methods should be callable by child types")]
public class NoiseBuilder : IFractalNoiseBuilder
{
    private NoiseDefinition definition;

    private NoiseBuilder(Int32 seed)
    {
        definition = new NoiseDefinition {Seed = seed};
    }

    NoiseBuilder INoiseBuilder.And()
    {
        return this;
    }

    /// <inheritdoc />
    public NoiseGenerator Build()
    {
        return new NoiseGenerator(definition);
    }

    /// <summary>
    /// Start building a new <see cref="NoiseGenerator"/>.
    /// </summary>
    public static NoiseBuilder Create(Int32 seed)
    {
        return new NoiseBuilder(seed);
    }

    /// <summary>
    /// Sets the type of noise generator.
    /// </summary>
    public NoiseBuilder WithType(NoiseType type)
    {
        definition.Type = type;

        return this;
    }

    /// <summary>
    /// Sets the frequency of the generated noise.
    /// </summary>
    public NoiseBuilder WithFrequency(Single frequency)
    {
        definition.Frequency = frequency;

        return this;
    }

    /// <summary>
    /// Enables and returns a fractal noise builder.
    /// </summary>
    public IFractalNoiseBuilder WithFractals()
    {
        definition.UseFractal = true;

        return this;
    }

    /// <summary>
    /// Disables fractal noise.
    /// </summary>
    public NoiseBuilder WithoutFractals()
    {
        definition.UseFractal = false;

        return this;
    }

    #region IFractalNoiseBuilder

    IFractalNoiseBuilder IFractalNoiseBuilder.WithOctaves(Int32 octaves)
    {
        definition.FractalOctaves = octaves;

        return this;
    }

    IFractalNoiseBuilder IFractalNoiseBuilder.WithLacunarity(Single lacunarity)
    {
        definition.FractalLacunarity = lacunarity;

        return this;
    }

    IFractalNoiseBuilder IFractalNoiseBuilder.WithGain(Single gain)
    {
        definition.FractalGain = gain;

        return this;
    }

    IFractalNoiseBuilder IFractalNoiseBuilder.WithWeightedStrength(Single weightedStrength)
    {
        definition.FractalWeightedStrength = weightedStrength;

        return this;
    }

    #endregion IFractalNoiseBuilder
}
