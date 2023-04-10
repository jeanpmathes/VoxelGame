//  <copyright file="GUI.hlsl" company="Gwen.Net">
//      Copyright (c) Gwen.Net.
//      MIT License
//  </copyright>
//  <author>Gwen.Net, jeanpmathes</author>

cbuffer SpaceConstantBuffer : register(b0)
{
float2 gScreenSize;
};

cbuffer UseTextureConstantBuffer : register(b1)
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
    result.color = color;

    float2 ncdPosition = 2.0f * (position / gScreenSize) - 1.0f;
    ncdPosition.y *= -1.0f;

    result.position = float4(ncdPosition, 0.0f, 1.0f);

    return result;
}

float4 PSMain(PSInput input) : SV_TARGET
{
    if (!gUseTexture) return input.color;

    return input.color * gTexture.Sample(gSampler, input.uv);
}
