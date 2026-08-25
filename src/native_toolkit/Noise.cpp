#include "stdafx.h"

Noise::Noise(NoiseDefinition const& definition)
    : m_seed(definition.seed)
{
    if (definition.type == NoiseType::GRADIENT)
    {
        auto generator = FastNoise::New<FastNoise::Simplex>();
        generator->SetScale(1.0f);
        m_generator = generator;
    }
    else if (definition.type == NoiseType::CELLULAR)
    {
        auto generator = FastNoise::New<FastNoise::CellularValue>();
        generator->SetScale(1.0f);
        m_generator = generator;
    }
    else m_generator = FastNoise::New<FastNoise::Constant>();

    if (definition.useFractal)
    {
        auto fbm = FastNoise::New<FastNoise::FractalFBm>();
        fbm->SetSource(m_generator);
        m_generator = fbm;

        fbm->SetOctaveCount(definition.fractalOctaves);
        fbm->SetLacunarity(definition.fractalLacunarity);
        fbm->SetGain(definition.fractalGain);
        fbm->SetWeightedStrength(definition.fractalWeightedStrength);
    }

    if (definition.frequency != 1.0f)
    {
        auto frequency = FastNoise::New<FastNoise::DomainScale>();
        frequency->SetSource(m_generator);
        m_generator = frequency;

        frequency->SetScaling(definition.frequency);
    }
}

float Noise::GetNoise(float const x, float const y) const { return m_generator->GenSingle2D(x, y, m_seed); }

float Noise::GetNoise(float const x, float const y, float const z) const
{
    return m_generator->GenSingle3D(x, y, z, m_seed);
}

void Noise::GetGrid(int const x, int const y, int const width, int const height, float* out) const
{
    m_generator->GenUniformGrid2D(out, static_cast<float>(x), static_cast<float>(y), width, height, 1.0f, 1.0f, m_seed);
}

void Noise::GetGrid(int const x, int const y, int const z, int const width, int const height, int const depth, float* out) const
{
    m_generator->GenUniformGrid3D(out, static_cast<float>(x), static_cast<float>(y), static_cast<float>(z), width, height, depth, 1.0f, 1.0f, 1.0f, m_seed);
}
