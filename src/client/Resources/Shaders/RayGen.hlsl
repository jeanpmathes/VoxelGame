//  <copyright file="RayGen.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Common.hlsl"

RWTexture2D<float4> gColorOutput : register(u0);

RaytracingAccelerationStructure gSpaceBVH : register(t0);

cbuffer CameraParams : register(b0)
{
float4x4 view; float4x4 projection; float4x4 viewI; float4x4 projectionI;
}

void ResetHitInfo(inout HitInfo payload)
{
    payload.color = float3(0, 0, 0);
    payload.alpha = 0;
    payload.normal = float3(0, 0, 0);
    payload.distance = VG_RAY_DISTANCE; // Allows any-hit to check if it is closer then write to payload.
}

[shader("raygeneration")]
void RayGen()
{
    uint2 launchIndex = DispatchRaysIndex().xy;
    float2 dimensions = float2(DispatchRaysDimensions().xy);
    float2 d = (((launchIndex.xy + 0.5) / dimensions.xy) * 2.0 - 1.0);
    float4 target = mul(projectionI, float4(d.x, d.y, 1, 1));

    RayDesc ray;
    ray.Origin = mul(viewI, float4(0, 0, 0, 1)).xyz;
    ray.Direction = mul(viewI, float4(target.xyz, 0)).xyz;
    ray.TMin = 0;
    ray.TMax = VG_RAY_DISTANCE;

    HitInfo payload;
    int iteration = 0;
    float4 color = float4(0, 0, 0, 0);
    while (color.a < 1.0 && iteration < 10)
    {
        ResetHitInfo(payload);
        TraceRay(gSpaceBVH, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, VG_MASK_VISIBLE, VG_HIT_ARG(0), ray, payload);

        const float a = color.a + (1.0 - color.a) * payload.alpha;
        const float3 rgb = (color.rgb * color.a + payload.color * (1.0 - color.a) * payload.alpha) / a;

        color.rgb = rgb;
        color.a = a;

        ray.Origin += ray.Direction * payload.distance;
        ray.Direction = ray.Direction;
        ray.TMin = VG_RAY_EPSILON;
        ray.TMax = VG_RAY_DISTANCE;

        iteration++;
    }

    gColorOutput[launchIndex] = float4(color.rgb, 1.0);
}
