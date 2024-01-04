//  <copyright file="Selection.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

cbuffer CustomDataCB : register(b0)
{
    float3 gColor;
}

cbuffer EffectDataCB : register(b1)
{
    float4x4 gMVP;
}

struct PSInput
{
    float4 position : SV_POSITION;
};

PSInput VSMain(const float3 position : POSITION, const uint data : DATA)
{
    PSInput result;

    result.position = mul(float4(position, 1.0), gMVP);

    return result;
}

float4 PSMain(const PSInput) : SV_TARGET
{
    return float4(gColor, 1.0);
}
