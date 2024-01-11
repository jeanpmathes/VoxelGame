//  <copyright file="Selection.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Common.hlsl"

cbuffer CustomDataCB : register(b0)
{
    float3 gDarkColor;
    float3 gBrightColor;
}

cbuffer EffectDataCB : register(b1)
{
    float4x4 gMVP;
}

struct PSInput
{
    float4 position : SV_POSITION;
};

Texture2D gColorFromRT : register(t0);

PSInput VSMain(const float3 position : POSITION, const uint data : DATA)
{
    PSInput result;

    result.position = mul(float4(position, 1.0), gMVP); // todo: use the f suffix for floats in all shaders

    return result;
}

float4 PSMain(const PSInput input, out float depth : SV_DEPTH) : SV_TARGET
{
    depth = input.position.z - 0.0001f;
    
    const int3 pixel = int3(input.position.xy, 0);

    float3 accumulator = 0;
    const int kernel = 3;
    for (int x = -kernel; x <= kernel; x++)
    {
        for (int y = -kernel; y <= kernel; y++)
        {
            const int3 offset = int3(x, y, 0);
            const float4 background = gColorFromRT.Load(pixel + offset);

            accumulator.r += POW2(background.r);
            accumulator.g += POW2(background.g);
            accumulator.b += POW2(background.b);
        }
    }

    const float count = POW2(kernel * 2 + 1);
    const float3 background = float3(sqrt(accumulator.r / count), sqrt(accumulator.g / count),
                                     sqrt(accumulator.b / count));

    const float luminance = GetLuminance(background);
    const float3 color = luminance > 0.2f ? gDarkColor : gBrightColor;
    
    return float4(color, 1.0);
}
