//  <copyright file="ScreenElement.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Common.hlsl"
#include "Draw2D.hlsl"

struct CustomData
{
    float4x4 mvp;
    float4   textureColor;
};

ConstantBuffer<CustomData> cb : register(b0);

struct PSInput
{
    float4 position : SV_POSITION;
    float2 uv : TEXCOORD;
    float4 color : COLOR;
};

PSInput VSMain(float2 const position : POSITION, float2 const uv : TEXCOORD, float4 const color : COLOR)
{
    PSInput result;

    result.uv       = uv;
    result.color    = native::draw2d::useTexture.value ? cb.textureColor : color;
    result.position = mul(float4(position, 0.0f, 1.0f), cb.mvp);

    return result;
}

float4 PSMain(PSInput const input) : SV_TARGET
{
    float4 color = input.color;

    if (native::draw2d::useTexture.value) color *= native::draw2d::texture[0].Sample(native::draw2d::sampler, input.uv);

    return color;
}
