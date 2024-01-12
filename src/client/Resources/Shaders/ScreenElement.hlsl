//  <copyright file="ScreenElement.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Common.hlsl"

cbuffer CustomDataCB : register(b0)
{
    float4x4 gMVP;
    float4 gTextureColor;
};

cbuffer UseTextureCB : register(b1)
{
    bool gUseTexture;
};

Texture2D gTexture : register(t0);
SamplerState gSampler : register(s0);

struct PSInput
{
    float4 position : SV_POSITION;
    float2 uv : TEXCOORD;
    float4 color : COLOR;
};

PSInput VSMain(const float2 position : POSITION, const float2 uv : TEXCOORD, const float4 color : COLOR)
{
    PSInput result;

    result.uv = uv;
    result.color = gUseTexture ? gTextureColor : color;
    result.position = mul(float4(position, 0.0, 1.0), gMVP);

    return result;
}

float4 PSMain(const PSInput input) : SV_TARGET
{
    float4 color = input.color;

    if (gUseTexture)
    {
        color *= gTexture.Sample(gSampler, input.uv);
    }

    return color;
}
