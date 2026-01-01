// <copyright file="Overlay.hlsl" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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

#include "Common.hlsl"
#include "Draw2D.hlsl"
#include "TextureAnimation.hlsl"

static int const BLOCK_MODE = 0;
static int const FLUID_MODE = 1;

struct CustomData
{
    float4x4 mvp;
    uint4    attributes;
    float    lowerBound;
    float    upperBound;
    int      mode;
    int      firstFluidTextureIndex;
};

ConstantBuffer<CustomData> cb : register(b0);

struct PSInput
{
    float4              position : SV_POSITION;
    float2              uv : TEXCOORD;
    float4              color : COLOR;
    float               height : HEIGHT;
    nointerpolation int index : TEXTURE_INDEX;
};

PSInput VSMain(float2 const position : POSITION, float2 const uv : TEXCOORD, float4 const color : COLOR)
{
    PSInput result;

    bool const isBlockMode = cb.mode == BLOCK_MODE;

    int const offset = isBlockMode ? 0 : cb.firstFluidTextureIndex;

    result.uv       = uv;
    result.color    = color;
    result.position = mul(float4(position, 0.0f, 1.0f), cb.mvp);
    result.height   = (result.position.y + 1.0f) * 0.5f;
    result.index    = vg::animation::GetAnimatedTextureIndex(cb.attributes, 0, native::draw2d::time.value, isBlockMode) + offset;

    return result;
}

float4 PSMain(PSInput const input) : SV_TARGET
{
    if (input.height < cb.lowerBound || input.height > cb.upperBound) discard;

    float4 color = native::draw2d::texture[input.index].Sample(native::draw2d::sampler, input.uv);

    switch (cb.mode)
    {
    case BLOCK_MODE:
        if (color.a < 0.1f) discard;

        if (color.a >= 0.3f) color *= vg::decode::GetTintColor(cb.attributes);

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
