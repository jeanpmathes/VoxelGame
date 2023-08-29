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

HitInfo GetEmptyHitInfo()
{
    HitInfo payload;
    
    payload.color = float3(0, 0, 0);
    payload.alpha = 0;
    payload.normal = float3(0, 0, 0);
    payload.distance = VG_RAY_DISTANCE; // Allows any-hit to check if it is closer then write to payload.

    return payload;
}

void Trace(const float3 origin, const float3 direction, const float min, inout HitInfo payload)
{
    RayDesc ray;
    ray.Origin = origin;
    ray.Direction = direction;
    ray.TMin = min;
    ray.TMax = VG_RAY_DISTANCE;

    TraceRay(gSpaceBVH, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, VG_MASK_VISIBLE, VG_HIT_ARG(0), ray, payload);
}

float GetReflectance(const float3 normal, const float3 incident, const float3 transmission, const float n1,
                     const float n2)
{
    if (!any(transmission)) return 1.0;

    // Schlick's approximation:

    const float r0 = POW2((n1 - n2) / (n1 + n2));
    const float cosThetaI = dot(normal, incident) * -1;
    const float cosThetaT = dot(normal, transmission);
    const bool totalInternalReflection = n1 > n2 && POW2(n1 / n2) * (1.0 - POW2(cosThetaI)) > 1.0;

    if (totalInternalReflection) return 1.0;

    const float cos = n1 <= n2 ? cosThetaI : cosThetaT;
    return r0 + (1.0 - r0) * POW5(1.0 - cos);
}

[shader("raygeneration")]
void RayGen()
{
    uint2 launchIndex = DispatchRaysIndex().xy;
    float2 dimensions = float2(DispatchRaysDimensions().xy);
    float2 d = (((launchIndex.xy + 0.5) / dimensions.xy) * 2.0 - 1.0);
    float4 target = mul(projectionI, float4(d.x, d.y, 1, 1));

    float3 origin = mul(viewI, float4(0, 0, 0, 1)).xyz;
    float3 direction = mul(viewI, float4(target.xyz, 0)).xyz;
    float min = 0;
    
    int iteration = 0;
    float4 color = float4(0, 0, 0, 0);

    float reflectance = 0.0;
    HitInfo reflectionHit = GetEmptyHitInfo();

    while (color.a < 1.0 && iteration < 10 && any(direction))
    {
        HitInfo hit = GetEmptyHitInfo();
        Trace(origin, direction, min, hit);

        const bool incoming = dot(direction, hit.normal) < 0;
        const float n1 = incoming ? 1.0 : 1.33;
        const float n2 = incoming ? 1.33 : 1.0;

        const float3 refracted = normalize(refract(direction, hit.normal, n1 / n2));
        const float3 reflected = normalize(reflect(direction, hit.normal));

        hit.color = lerp(hit.color, reflectionHit.color, reflectance);
        hit.alpha = lerp(hit.alpha, reflectionHit.alpha, reflectance);

        const float a = color.a + (1.0 - color.a) * hit.alpha;
        const float3 rgb = (color.rgb * color.a + hit.color * (1.0 - color.a) * hit.alpha) / a;

        color.rgb = rgb;
        color.a = a;

        min = VG_RAY_EPSILON;
        iteration++;

        // The reflection ray is traced now and the result is used next iteration when the refraction ray is traced.

        reflectance = hit.alpha < 1.0 ? GetReflectance(hit.normal, direction, refracted, n1, n2) : 0.0;

        origin += direction * hit.distance;
        direction = refracted;

        if (reflectance > 0.0)
        {
            reflectionHit = GetEmptyHitInfo();
            Trace(origin, reflected, min, reflectionHit);
        }
    }

    gColorOutput[launchIndex] = float4(color.rgb, 1.0);
}
