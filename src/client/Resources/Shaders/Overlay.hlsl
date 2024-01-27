//  <copyright file="Overlay.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Common.hlsl"
#include "TextureAnimation.hlsl"
#include "Draw2D.hlsl"

static const int BLOCK_MODE = 0;
static const int FLUID_MODE = 1;

struct CustomData
{
    float4x4 mvp;
    uint4 attributes;
    float lowerBound;
    float upperBound;
    int mode;
    int firstFluidTextureIndex;
};

ConstantBuffer<CustomData> cb : register(b0);

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

    const bool isBlockMode = cb.mode == BLOCK_MODE;

    const int offset = isBlockMode ? 0 : cb.firstFluidTextureIndex;

    result.uv = uv;
    result.color = color;
    result.position = mul(float4(position, 0.0f, 1.0f), cb.mvp);
    result.height = (result.position.y + 1.0f) * 0.5f;
    result.index = vg::animation::GetAnimatedTextureIndex(cb.attributes, 0, native::draw2d::time.value, isBlockMode) +
        offset;

    return result;
}

float4 PSMain(const PSInput input) : SV_TARGET
{
    if (input.height < cb.lowerBound || input.height > cb.upperBound)
        discard;

    float4 color = native::draw2d::texture[input.index].Sample(native::draw2d::sampler, input.uv);

    switch (cb.mode)
    {
    case BLOCK_MODE:
        if (color.a < 0.1f) discard;

        if (color.a >= 0.3f)
            color *= vg::decode::GetTintColor(cb.attributes);

        color.a = 1.0f;
        break;

    case FLUID_MODE:
        color *= vg::decode::GetTintColor(cb.attributes);
        break;

    default:
        break;
    }

    return color;
}
