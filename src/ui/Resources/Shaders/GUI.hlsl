//  <copyright file="GUI.hlsl" company="Gwen.Net">
//      Copyright (c) Gwen.Net.
//      MIT License
//  </copyright>
//  <author>Gwen.Net, jeanpmathes</author>

#include "Common.hlsl"
#include "Draw2D.hlsl"

struct ScreenCB
{
    float2 size;
};

ConstantBuffer<ScreenCB> screen : register(b0);

struct PSInput
{
    float4 position : SV_POSITION;
    float2 uv : TEXCOORD;
    float4 color : COLOR;
};

PSInput VSMain(float2 const position : POSITION, float2 const uv : TEXCOORD, float4 const color : COLOR)
{
    PSInput result;

    result.uv    = uv;
    result.color = color;

    float2 ndcPosition = 2.0f * (position / screen.size) - 1.0f;
    ndcPosition.y *= -1.0f;

    result.position = float4(ndcPosition, 0.0f, 1.0f);

    return result;
}

float4 PSMain(PSInput input) : SV_TARGET
{
    if (!native::draw2d::useTexture.value) return input.color;

    return input.color * native::draw2d::texture[0].Sample(native::draw2d::sampler, input.uv);
}
