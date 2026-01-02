// <copyright file="Miss.hlsl" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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

#include "CommonRT.hlsl"
#include "Space.hlsl"

#include "Custom.hlsl"
#include "Hash.hlsl"
#include "Payload.hlsl"

#include "Atmosphere.hlsl"

float GetCelestialBodyIntensity(float3 rayDirection, float3 bodyDirection, float factor) { return min(pow(max(dot(rayDirection, bodyDirection), 0.0f), factor * 1000.0f), 1.0f); }

float GetStarIntensity(float3 rayDirection)
{
    float3 const quantized = floor(rayDirection * 500.0f);
    uint const   hash      = asuint(quantized.x) * 73856093u ^ asuint(quantized.y) * 19349663u ^ asuint(quantized.z) * 83492791u;
    float const  noise     = Random(hash);

    float const star      = step(0.9975f, noise);
    float const sharpness = smoothstep(0.9995f, 1.0f, noise);

    return star * sharpness;
}

[shader("miss")]void Miss(inout native::rt::HitInfo payload)
{
    // The time of day goes from 0.0 (morning) over 0.5 (evening) up to, but not including, 1.0 (next morning).
    float const timeOfDay      = vg::custom.timeOfDay;
    float const dawnDuskWindow = 0.06f;

    float dayAmount, nightAmount;

    if (timeOfDay >= dawnDuskWindow && timeOfDay < 0.5f - dawnDuskWindow)
    {
        dayAmount   = 1.0f;
        nightAmount = 0.0f;
    }
    else if (timeOfDay >= 0.5f - dawnDuskWindow && timeOfDay < 0.5f)
    {
        float const dayToDusk = smoothstep(0.5f - dawnDuskWindow, 0.5f, timeOfDay);
        dayAmount             = 1.0f - dayToDusk;
        nightAmount           = 0.0f;
    }
    else if (timeOfDay >= 0.5f && timeOfDay < 0.5f + dawnDuskWindow)
    {
        float const duskToNight = smoothstep(0.5f, 0.5f + dawnDuskWindow, timeOfDay);
        dayAmount               = 0.0f;
        nightAmount             = duskToNight;
    }
    else if (timeOfDay >= 0.5f + dawnDuskWindow && timeOfDay < 1.0f - dawnDuskWindow)
    {
        dayAmount   = 0.0f;
        nightAmount = 1.0f;
    }
    else if (timeOfDay >= 1.0f - dawnDuskWindow && timeOfDay < 1.0f)
    {
        float const nightToDawn = smoothstep(1.0f - dawnDuskWindow, 1.0f, timeOfDay);
        dayAmount               = 0.0f;
        nightAmount             = 1.0f - nightToDawn;
    }
    else
    {
        float const dawnToDay = smoothstep(0.0f, dawnDuskWindow, timeOfDay);
        dayAmount             = dawnToDay;
        nightAmount           = 0.0f;
    }

    float3 const rayDirection = normalize(WorldRayDirection());
    float3 const sunDirection = -native::spatial::global.lightDirection;

    float3 color = float3(0.005f, 0.02f, 0.06f);

    if (dayAmount > 0.0f) color += GetAtmosphereColor(rayDirection, sunDirection) * dayAmount;

    if (nightAmount > 0.0f)
    {
        float3 moonDirection = sunDirection; // The moon replaces the sun at night.
        float  moonIntensity = GetCelestialBodyIntensity(rayDirection, moonDirection, 10.0f);
        color                += native::spatial::global.lightColor * moonIntensity * nightAmount;

        color += float3(1.0f, 1.0f, 1.0f) * GetStarIntensity(rayDirection) * nightAmount;
    }

    SET_MISS_INFO(payload, RGBA(color));
}
