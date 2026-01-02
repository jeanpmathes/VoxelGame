// <copyright file="Atmosphere.hlsl" company="VoxelGame">
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

// The below code is based on the following tutorial:
// https://cpp-rendering.io/sky-and-atmosphere-rendering/ - "Sky and Atmosphere Rendering: A physical approach" by Antoine Morrier

#include "Common.hlsl"

static float const Hr = 8000.0f;
static float const Hm = 1200.0f;
static float const Ho = 8000.0f;

static float3 const Rayleigh         = float3(5.8f, 13.5f, 33.1f) * 1e-6f;
static float3 const Mie              = float3(21.0f, 21.0f, 21.0f) * 1e-6f;
static float3 const Ozone            = float3(3.426f, 8.298f, 0.356f) * 0.06f * 1e-5f;
static float const  RadiusEarth      = 6360e3f;
static float const  RadiusAtmosphere = 6460e3f;
static float const  ZenithHeight     = RadiusAtmosphere - RadiusEarth;
static float3 const OriginView       = float3(0.0f, RadiusEarth + 1.0f, 0.0f);
static float const  OriginHeight     = OriginView.y - RadiusEarth;
static int const    IntegrationSteps = 16;

static float const SunRadiusSize = radians(0.53f / 2.0f);
static float const SunSolidAngle = 2.0f * PI * (1.0f - cos(SunRadiusSize));

float IntersectRayAndSphere(float3 const rayOrigin, float3 const rayDirection, float radius)
{
    float const b            = dot(rayOrigin, rayDirection);
    float const c            = dot(rayOrigin, rayOrigin) - radius * radius;
    float const discriminant = b * b - c;
    float const t            = -b + sqrt(discriminant);
    return t;
}

float RayleighPhase(float3 viewDirection, float3 sunDirection)
{
    float const mu = dot(viewDirection, sunDirection);
    return 3.0f / (16.0f * PI) * (1.0f + mu * mu);
}

float MiePhase(float3 viewDirection, float3 sunDirection)
{
    float const mu          = dot(viewDirection, sunDirection);
    float const g           = 0.76f;
    float const denominator = 1.0f + g * g - 2.0f * g * mu;
    return (1.0f - g * g) / (4.0f * PI * pow(denominator, 1.5f));
}

float3 SigmaSRayleigh(float3 position)
{
    float const h = length(position) - RadiusEarth;
    return Rayleigh * exp(-h / Hr);
}

float3 SigmaSMie(float3 position)
{
    float const h = length(position) - RadiusEarth;
    return Mie * exp(-h / Hm);
}

float3 SigmaAOzone(float3 position)
{
    float const h = length(position) - RadiusEarth;
    return Ozone * exp(-h / Ho);
}

float3 SigmaT(float3 position) { return SigmaSRayleigh(position) + 1.11f * SigmaSMie(position) + SigmaAOzone(position); }

float3 IntegrateSigmaT(float3 from, float3 to)
{
    float3 const ds           = (to - from) / float(IntegrationSteps);
    float3       accumulation = float3(0.0f, 0.0f, 0.0f);

    for (int step = 0; step < IntegrationSteps; step++)
    {
        float3 const s = from + (step + 0.5f) * ds;
        accumulation   += SigmaT(s);
    }

    return accumulation * length(ds);
}

float3 GetTransmittance(float3 from, float3 to)
{
    float3 const integral = IntegrateSigmaT(from, to);
    return exp(-integral);
}

static float3 const TransmittanceZenith = GetTransmittance(OriginView, float3(0.0f, RadiusEarth + ZenithHeight, 0.0f));
static float3 const IlluminanceGround   = 2.0f * PI * float3(1.0f, 1.0f, 1.0f);
static float3 const LuminanceOuterspace = IlluminanceGround / SunSolidAngle / TransmittanceZenith;

float3 GetInScatteringTermWithSunOnly(float3 position, float3 viewDirection, float3 sunDirection)
{
    float const  distanceOutOfAtmosphere = IntersectRayAndSphere(position, sunDirection, RadiusAtmosphere);
    float3 const vectorOutOfAtmosphere   = position + sunDirection * distanceOutOfAtmosphere;

    float3 const transmittanceToSun = GetTransmittance(position, vectorOutOfAtmosphere);
    float3 const rayleighDiffusion  = SigmaSRayleigh(position) * RayleighPhase(viewDirection, sunDirection);
    float3 const mieDiffusion       = SigmaSMie(position) * MiePhase(viewDirection, sunDirection);

    return LuminanceOuterspace * transmittanceToSun * SunSolidAngle * (rayleighDiffusion + mieDiffusion);
}

float3 ComputeLuminance(float3 vectorOutOfAtmosphere, float3 sunDirection)
{
    float3 const ds           = (vectorOutOfAtmosphere - OriginView) / IntegrationSteps;
    float3 const direction    = normalize(ds);
    float3       accumulation = 0.0f;

    for (int step = 0; step < IntegrationSteps; step++)
    {
        float3 const s = OriginView + (step + 0.5f) * ds;
        accumulation   += GetTransmittance(OriginView, s) * GetInScatteringTermWithSunOnly(s, direction, sunDirection);
    }

    return accumulation * length(ds);
}

float3 GetDirectLightFromSun(float3 direction, float3 vectorOutOfAtmosphere, float3 sunDirection)
{
    float cosTheta = dot(direction, sunDirection);

    float const angle = acos(cosTheta);
    float const disk  = 1.0f - smoothstep(SunRadiusSize * 0.95f, SunRadiusSize * 1.05f, angle);

    return disk * LuminanceOuterspace * GetTransmittance(OriginView, vectorOutOfAtmosphere);
}

float3 GetAtmosphereColor(float3 viewDirection, float3 sunDirection)
{
    float angle = acos(dot(viewDirection, float3(0.0f, 1.0f, 0.0f)));
    if (angle > radians(100.0f)) return float3(0.0f, 0.0f, 0.0f);

    float const  distanceOutOfAtmosphere = IntersectRayAndSphere(OriginView, viewDirection, RadiusAtmosphere);
    float3 const viewOutOfAtmosphere     = OriginView + viewDirection * distanceOutOfAtmosphere;

    return ComputeLuminance(viewOutOfAtmosphere, sunDirection) + GetDirectLightFromSun(viewDirection, viewOutOfAtmosphere, sunDirection);
}
