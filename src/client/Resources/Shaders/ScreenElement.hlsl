// <copyright file="ScreenElement.hlsl" company="VoxelGame">
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
