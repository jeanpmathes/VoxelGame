//  <copyright file="Selection.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

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

    result.position = mul(float4(position, 1.0), gMVP);

    return result;
}

float4 PSMain(const PSInput input) : SV_TARGET

{
    const int3 pixel = int3(input.position.xy, 0);

    const float4 background = gColorFromRT.Load(pixel);
    const float brightness = (background.r + background.g + background.b) / 3.0;

    // Dark and bright are swapped to increase contrast.
    const float3 color = lerp(gBrightColor, gDarkColor, brightness);
    return float4(color, 1.0);
}
