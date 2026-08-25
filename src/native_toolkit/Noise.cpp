#include "stdafx.h"

Noise::Noise(NoiseDefinition const& definition)
    : seed(definition.seed)
{
    if (definition.type == NoiseType::GRADIENT)
    {
        auto baseGenerator = FastNoise::New<FastNoise::Simplex>();
        baseGenerator->SetScale(1.0f);
        generator = baseGenerator;
    }
    else if (definition.type == NoiseType::CELLULAR)
    {
        auto baseGenerator = FastNoise::New<FastNoise::CellularValue>();
        baseGenerator->SetScale(1.0f);
        generator = baseGenerator;
    }
    else generator = FastNoise::New<FastNoise::Constant>();

    if (definition.useFractal)
    {
        auto fbm = FastNoise::New<FastNoise::FractalFBm>();
        fbm->SetSource(generator);
        generator = fbm;

        fbm->SetOctaveCount(definition.fractalOctaves);
        fbm->SetLacunarity(definition.fractalLacunarity);
        fbm->SetGain(definition.fractalGain);
        fbm->SetWeightedStrength(definition.fractalWeightedStrength);
    }

    if (definition.frequency != 1.0f)
    {
        auto frequency = FastNoise::New<FastNoise::DomainScale>();
        frequency->SetSource(generator);
        generator = frequency;

        frequency->SetScaling(definition.frequency);
    }
}

float Noise::GetNoise(float const x, float const y) const { return generator->GenSingle2D(x, y, seed); }

float Noise::GetNoise(float const x, float const y, float const z) const
{
    return generator->GenSingle3D(x, y, z, seed);
}

void Noise::GetGrid(int const x, int const y, int const width, int const height, float* out) const
{
    generator->GenUniformGrid2D(out, static_cast<float>(x), static_cast<float>(y), width, height, 1.0f, 1.0f, seed);
}

void Noise::GetGrid(int const x, int const y, int const z, int const width, int const height, int const depth, float* out) const
{
    generator->GenUniformGrid3D(out, static_cast<float>(x), static_cast<float>(y), static_cast<float>(z), width, height, depth, 1.0f, 1.0f, 1.0f, seed);
}
