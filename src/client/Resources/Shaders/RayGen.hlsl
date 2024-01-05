//  <copyright file="RayGen.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Common.hlsl"
#include "Payloads.hlsl"

cbuffer CameraParameters : register(b0)
{
float4x4 view;
float4x4 projection;
float4x4 viewI;
float4x4 projectionI;
}

RWTexture2D<float4> gColorOutput : register(u0);
RWTexture2D<float> gDepthOutput : register(u1);

RaytracingAccelerationStructure gSpaceBVH : register(t0);

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

float GetReflectance(
    const float3 normal, const float3 incident, const float3 transmission,
    const float n1, const float n2)
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
    const uint2 launchIndex = DispatchRaysIndex().xy;
    const float2 dimensions = float2(DispatchRaysDimensions().xy);

    // Given the dimension (Dx, Dy), the launch index is in the range [0, Dx - 1] x [0, Dy - 1].
    // This range is transformed to NDC space, i.e. [-1, 1] x [-1, 1].
    const float2 d = (float2(launchIndex) + 0.5) / dimensions * 2.0 - 1.0;

    // DirectX textures have their origin at the top-left corner, while NDC has it at the bottom-left corner.
    // Therefore, the y coordinate is inverted.
    const float4 pixel = float4(d.x, d.y * -1, 1, 1);

    float3 targetInViewSpace = mul(pixel, projectionI).xyz;
    float3 direction = mul(float4(targetInViewSpace.xyz, 0), viewI).xyz;
    float3 origin = mul(float4(0, 0, 0, 1), viewI).xyz;
    
    float3 normal = float3(0, 0, 0);
    float min = 0;
    
    int iteration = 0;
    float4 color = 0;
    float depth = 0;

    float reflectance = 0.0;
    HitInfo reflectionHit = GetEmptyHitInfo();

    while (color.a < 1.0 && iteration < 10 && any(direction))
    {
        HitInfo hit = GetEmptyHitInfo();
        Trace(origin - normal * VG_RAY_EPSILON, direction, min, hit);

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

        // If an reflectance ray is needed for the current hit, it is traced this iteration.
        // The main ray of the next iteration is the refraction ray.
        reflectance = hit.alpha < 1.0 ? GetReflectance(hit.normal, direction, refracted, n1, n2) : 0.0;

        origin += direction * hit.distance;
        normal = hit.normal;
        direction = refracted;

        if (iteration == 0)
        {
            const float4 hitInViewSpace = mul(float4(origin, 1), view);
            const float4 hitInClipSpace = mul(hitInViewSpace, projection);

            depth = hitInClipSpace.z / hitInClipSpace.w;
        }

        min = 0;
        iteration++;

        if (reflectance > 0.0)
        {
            reflectionHit = GetEmptyHitInfo();
            Trace(origin + normal * VG_RAY_EPSILON, reflected, min, reflectionHit);
        }
    }

    gColorOutput[launchIndex] = float4(color.rgb, 1.0);
    gDepthOutput[launchIndex] = depth;
}
