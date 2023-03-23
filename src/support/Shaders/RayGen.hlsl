#include "Common.hlsl"

RWTexture2D<float4> gColorOutput : register(u0);

RaytracingAccelerationStructure gSpaceBVH : register(t0);

cbuffer CameraParams : register(b0)
{
float4x4 view; float4x4 projection; float4x4 viewI; float4x4 projectionI;
}

[shader("raygeneration")]
void RayGen()
{
    HitInfo payload;
    payload.colorAndDistance = float4(0, 0, 0, 0);

    uint2 launchIndex = DispatchRaysIndex().xy;
    float2 dimensions = float2(DispatchRaysDimensions().xy);
    float2 d = (((launchIndex.xy + 0.5f) / dimensions.xy) * 2.f - 1.f);
    float4 target = mul(projectionI, float4(d.x, d.y, 1, 1));

    RayDesc ray;
    ray.Origin = mul(viewI, float4(0, 0, 0, 1));
    ray.Direction = mul(viewI, float4(target.xyz, 0));
    ray.TMin = 0;
    ray.TMax = 100000;

    TraceRay(gSpaceBVH, RAY_FLAG_NONE,
             0xFF, 0, 0, 0,
             ray, payload);

    gColorOutput[launchIndex] = float4(payload.colorAndDistance.rgb, 1.f);
}
