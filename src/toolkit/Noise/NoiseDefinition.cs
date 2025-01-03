﻿// <copyright file="NoiseDefinition.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices.Marshalling;
using JetBrains.Annotations;

namespace VoxelGame.Toolkit.Noise;

#pragma warning disable S3898 // No equality comparison used.

/// <summary>
///     Defines the available types of noise generators.
/// </summary>
public enum NoiseType
{
    /// <summary>
    ///     Simplex-based gradient noise.
    /// </summary>
    GradientNoise = 0,

    /// <summary>
    ///     Cellular noise, giving the cell value.
    /// </summary>
    CellularNoise = 1
}

/// <summary>
///     The definition of a noise generator.
/// </summary>
[NativeMarshalling(typeof(NoiseDefinitionMarshaller))]
public struct NoiseDefinition
{
    /// <summary>
    ///     Creates a new noise definition.
    /// </summary>
    public NoiseDefinition() {}

    /// <summary>
    ///     The seed of the noise generator.
    /// </summary>
    public Int32 Seed { get; init; } = 1337;

    /// <summary>
    ///     The type of noise generator.
    /// </summary>
    public NoiseType Type { get; set; } = NoiseType.GradientNoise;

    /// <summary>
    ///     The frequency of the generated noise.
    /// </summary>
    public Single Frequency { get; set; } = 0.01f;

    /// <summary>
    ///     Whether to use fractal noise.
    /// </summary>
    public Boolean UseFractal { get; set; }

    /// <summary>
    ///     The number of octaves in the fractal noise.
    /// </summary>
    public Int32 FractalOctaves { get; set; } = 3;

    /// <summary>
    ///     The lacunarity of the fractal noise.
    /// </summary>
    public Single FractalLacunarity { get; set; } = 2.0f;

    /// <summary>
    ///     The gain of the fractal noise.
    /// </summary>
    public Single FractalGain { get; set; } = 0.5f;

    /// <summary>
    ///     The weighted strength of the fractal noise.
    /// </summary>
    public Single FractalWeightedStrength { get; set; }
}

[CustomMarshaller(typeof(NoiseDefinition), MarshalMode.ManagedToUnmanagedIn, typeof(NoiseDefinitionMarshaller))]
internal static class NoiseDefinitionMarshaller
{
    internal static Unmanaged ConvertToUnmanaged(NoiseDefinition managed)
    {
        return new Unmanaged
        {
            seed = managed.Seed,
            type = (Byte) managed.Type,
            frequency = managed.Frequency,
            useFractal = managed.UseFractal.ToInt(),
            fractalOctaves = managed.FractalOctaves,
            fractalLacunarity = managed.FractalLacunarity,
            fractalGain = managed.FractalGain,
            fractalWeightedStrength = managed.FractalWeightedStrength
        };
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal struct Unmanaged
    {
        internal Int32 seed;
        internal Byte type;
        internal Single frequency;
        internal Int32 useFractal;
        internal Int32 fractalOctaves;
        internal Single fractalLacunarity;
        internal Single fractalGain;
        internal Single fractalWeightedStrength;
    }
}
