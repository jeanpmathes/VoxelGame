//  <copyright file="Post.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace pp
{
    Texture2D    texture : register(t0);
    SamplerState sampler : register(s0);
}

struct PSInput
{
    float4 position : SV_POSITION;
    float2 uv : TEXCOORD;
};

PSInput VSMain(float4 const position : POSITION, float2 const uv : TEXCOORD)
{
    PSInput result;

    result.position = position;
    result.uv       = uv;

    return result;
}

float4 PSMain(PSInput const input) : SV_TARGET { return pp::texture.Sample(pp::sampler, input.uv); }
