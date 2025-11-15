//  <copyright file="Selection.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Common.hlsl"
#include "Effect.hlsl"

struct CustomData
{
    float3 darkColor;
    float3 brightColor;
};

ConstantBuffer<CustomData> cb : register(b0);

struct PSInput
{
    float4 position : SV_POSITION;
    bool   isDark   : DATA;
};

Texture2D colorFromRT : register(t0);
Texture2D depthFromRT : register(t1);

float DepthToViewZ(float depth)
{
    float const n = native::effect::data.near;
    float const f = native::effect::data.far;
    
    return n * f / (f - depth * (f - n));
}

float ViewZToDepth(float viewZ)
{
    float const n = native::effect::data.near;
    float const f = native::effect::data.far;
    
    return f * (viewZ - n) / (viewZ * (f - n));
}

PSInput VSMain(float3 const position : POSITION, uint const data : DATA)
{
    PSInput result;

    result.position = mul(float4(position, 1.0f), native::effect::data.mvp);
    result.isDark   = (data & 1) != 0;

    return result;
}

float4 PSMain(PSInput const input, out float depth : SV_DEPTH) : SV_TARGET
{
    int3 const pixel = int3(input.position.xy, 0);
    
    float selfDepth = input.position.z;
    float rtDepth   = depthFromRT.Load(pixel).r;

    float3 const baseColor = input.isDark ? cb.brightColor : cb.darkColor;
    
    if (rtDepth > 0.9999f)
    {
        depth = selfDepth;
        return float4(baseColor, 1.0);
    }
    
    float selfZ = DepthToViewZ(selfDepth);
    float rtZ   = DepthToViewZ(rtDepth);

    float const threshold = 0.02f;
    
    if (selfZ > rtZ + threshold)
        discard;

    float const bias = 0.005f;
    
    float const targetZ = max(rtZ - bias, native::effect::data.near + 0.0001f);

    depth = ViewZToDepth(targetZ);
    
    float const distanceDelta = selfZ - rtZ;
    float const blendFactor   = distanceDelta > 0.0f ? saturate(distanceDelta / threshold) : 0.0f;
    
    float3 const rtColor    = colorFromRT.Load(pixel).rgb;
    float3 const finalColor = lerp(baseColor, rtColor, blendFactor * 0.5f);

    return float4(finalColor, 1.0);
}
