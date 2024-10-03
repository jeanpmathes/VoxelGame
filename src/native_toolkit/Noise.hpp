// <copyright file="Noise.hpp" company="VoxelGame">
// MIT License
// For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#pragma once

enum class NoiseType : INT8
{
    GRADIENT = 0,
    CELLULAR = 1,
};

struct NoiseDefinition
{
    INT32 seed;

    NoiseType type;
    FLOAT     frequency;

    BOOL  useFractal;
    INT32 fractalOctaves;
    FLOAT fractalLacunarity;
    FLOAT fractalGain;
    FLOAT fractalWeightedStrength;
};

/**
 * Wraps the noise library FastNoise2.
 */
class Noise
{
public:
    explicit Noise(NoiseDefinition const& definition);

    [[nodiscard]] float GetNoise(float x, float y) const;
    [[nodiscard]] float GetNoise(float x, float y, float z) const;

    void GetGrid(int x, int y, int width, int height, float* out) const;
    void GetGrid(int x, int y, int z, int width, int height, int depth, float* out) const;

private:
    int m_seed;

    FastNoise::SmartNode<> m_generator = {};
};
