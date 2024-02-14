//  <copyright file="Selection.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Common.hlsl"
#include "Effect.hlsl"

struct CustomData
{
    float3 darkColor;
    float3 brightColor;
};

ConstantBuffer<CustomData> cb : register(b0);

struct PSInput
{
    float4 position : SV_POSITION;
};

Texture2D colorFromRT : register(t0);

PSInput VSMain(float3 const position : POSITION, uint const data : DATA)
{
    PSInput result;

    result.position = mul(float4(position, 1.0f), native::effect::data.mvp);

    return result;
}

float4 PSMain(PSInput const input, out float depth : SV_DEPTH) : SV_TARGET
{
    depth = input.position.z - 0.0001f;

    int3 const pixel = int3(input.position.xy, 0);

    float3    accumulator = 0;
    int const kernel      = 3;
    for (int x = -kernel; x <= kernel; x++)
        for (int y = -kernel; y <= kernel; y++)
        {
            int3 const   offset     = int3(x, y, 0);
            float4 const background = colorFromRT.Load(pixel + offset);

            accumulator.r += POW2(background.r);
            accumulator.g += POW2(background.g);
            accumulator.b += POW2(background.b);
        }

    float const  count      = POW2(kernel * 2 + 1);
    float3 const background = float3(
        sqrt(accumulator.r / count),
        sqrt(accumulator.g / count),
        sqrt(accumulator.b / count));

    float const  luminance = native::GetLuminance(background);
    float3 const color     = luminance > 0.2f ? cb.darkColor : cb.brightColor;

    return float4(color, 1.0);
}
