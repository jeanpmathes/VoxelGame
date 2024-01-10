//  <copyright file="Overlay.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "TextureAnimation.hlsl"

static const int BLOCK_MODE = 0;
static const int FLUID_MODE = 1;

cbuffer CustomDataCB : register(b0)
{
    float4x4 gMVP;
    uint4 gAttributes;
    float gLowerBound;
    float gUpperBound;
    int gMode;
    int gFirstFluidTextureIndex;
}

cbuffer UseTextureCB : register(b1)
{
    bool gUseTexture;
}

cbuffer TimeCB : register(b0, space1)
{
    float gTime;
}

Texture2D gTexture[] : register(t0);
SamplerState gSampler : register(s0);

struct PSInput
{
    float4 position : SV_POSITION;
    float2 uv : TEXCOORD;
    float4 color : COLOR;
    float height : HEIGHT;
    nointerpolation int index : TEXTURE_INDEX;
};

PSInput VSMain(const float2 position : POSITION, const float2 uv : TEXCOORD, const float4 color : COLOR)
{
    PSInput result;

    const bool isBlockMode = gMode == BLOCK_MODE;

    result.uv = uv;
    result.color = color;
    result.position = mul(float4(position, 0.0, 1.0), gMVP);
    result.height = (result.position.y + 1.0) * 0.5;
    result.index = GetAnimatedTextureIndex(gAttributes, 0, gTime, isBlockMode) + (isBlockMode
        ? 0
        : gFirstFluidTextureIndex);

    return result;
}

float4 PSMain(const PSInput input) : SV_TARGET
{
    if (input.height < gLowerBound || input.height > gUpperBound)
        discard;

    float4 color = gTexture[input.index].Sample(gSampler, input.uv);

    switch (gMode)
    {
    case BLOCK_MODE:
        if (color.a < 0.1) discard;

        if (color.a >= 0.3)
            color *= decode::GetTintColor(gAttributes);

        color.a = 1.0;
        break;

    case FLUID_MODE:
        color *= decode::GetTintColor(gAttributes);
        break;

    default:
        break;
    }

    return color;
}
