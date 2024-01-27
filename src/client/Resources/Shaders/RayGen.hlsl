//  <copyright file="RayGen.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "CommonRT.hlsl"
#include "RayGenRT.hlsl"
#include "PayloadRT.hlsl"

/**
 * \brief Create an empty hit info struct.
 * \return The empty hit info struct.
 */
native::rt::HitInfo GetEmptyHitInfo()
{
    native::rt::HitInfo payload;

    payload.color = float3(0.0f, 0.0f, 0.0f);
    payload.alpha = 0.0f;
    payload.normal = float3(0.0f, 0.0f, 0.0f);
    payload.distance = native::rt::RAY_DISTANCE; // Allows any-hit to check if it is closer then write to payload.

    return payload;
}

/**
 * \brief Trace a ray.
 * \param origin The origin of the ray.
 * \param direction The direction of the ray.
 * \param min The minimum distance of the ray.
 * \param payload The payload to write to.
 */
void Trace(const float3 origin, const float3 direction, const float min, inout native::rt::HitInfo payload)
{
    RayDesc ray;
    ray.Origin = origin;
    ray.Direction = direction;
    ray.TMin = min;
    ray.TMax = native::rt::RAY_DISTANCE;

    TraceRay(native::rt::spaceBVH, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, native::rt::MASK_VISIBLE, RT_HIT_ARG(0), ray,
             payload);
}

/**
 * \brief Calculate the reflectance factor at a hit.
 * \param normal The normal at the hit.
 * \param incident The incident ray direction.
 * \param transmission The transmission ray direction.
 * \param n1 The refractive index of the medium the incident ray is in.
 * \param n2 The refractive index of the medium the transmission ray is in.
 * \return The reflectance factor at the hit.
 */
float GetReflectance(
    const float3 normal, const float3 incident, const float3 transmission,
    const float n1, const float n2)
{
    if (!any(transmission)) return 1.0f;

    // Schlick's approximation:

    const float r0 = POW2((n1 - n2) / (n1 + n2));
    const float cosThetaI = dot(normal, incident) * -1;
    const float cosThetaT = dot(normal, transmission);
    const bool totalInternalReflection = n1 > n2 && POW2(n1 / n2) * (1.0f - POW2(cosThetaI)) > 1.0f;

    if (totalInternalReflection) return 1.0f;

    const float cos = n1 <= n2 ? cosThetaI : cosThetaT;
    return r0 + (1.0f - r0) * POW5(1.0f - cos);
}

[shader("raygeneration")]
void RayGen()
{
    const uint2 launchIndex = DispatchRaysIndex().xy;
    const float2 dimensions = float2(DispatchRaysDimensions().xy);

    // Given the dimension (Dx, Dy), the launch index is in the range [0, Dx - 1] x [0, Dy - 1].
    // This range is transformed to NDC space, i.e. [-1, 1] x [-1, 1].
    const float2 d = (float2(launchIndex) + 0.5f) / dimensions * 2.0f - 1.0f;

    // DirectX textures have their origin at the top-left corner, while NDC has it at the bottom-left corner.
    // Therefore, the y coordinate is inverted.
    const float4 pixel = float4(d.x, d.y * -1, 1, 1);

    float3 targetInViewSpace = mul(pixel, native::rt::camera.projectionI).xyz;
    float3 direction = mul(float4(targetInViewSpace.xyz, 0), native::rt::camera.viewI).xyz;
    float3 origin = mul(float4(0, 0, 0, 1), native::rt::camera.viewI).xyz;
    
    float3 normal = float3(0, 0, 0);
    float min = native::rt::camera.near * length(direction);
    direction = normalize(direction);
     
    int iteration = 0;
    float4 color = 0;
    float depth = 0;

    float reflectance = 0.0f;
    native::rt::HitInfo reflectionHit = GetEmptyHitInfo();

    while (color.a < 1.0f && iteration < 10 && any(direction))
    {
        native::rt::HitInfo hit = GetEmptyHitInfo();
        Trace(origin - normal * native::rt::RAY_EPSILON, direction, min, hit);

        const bool incoming = dot(direction, hit.normal) < 0;
        const float n1 = incoming ? 1.00f : 1.33f;
        const float n2 = incoming ? 1.33f : 1.00f;

        const float3 refracted = normalize(refract(direction, hit.normal, n1 / n2));
        const float3 reflected = normalize(reflect(direction, hit.normal));

        hit.color = lerp(hit.color, reflectionHit.color, reflectance);
        hit.alpha = lerp(hit.alpha, reflectionHit.alpha, reflectance);

        const float a = color.a + (1.0f - color.a) * hit.alpha;
        const float3 rgb = (color.rgb * color.a + hit.color * (1.0f - color.a) * hit.alpha) / a;

        color.rgb = rgb;
        color.a = a;

        // If an reflectance ray is needed for the current hit, it is traced this iteration.
        // The main ray of the next iteration is the refraction ray.
        reflectance = hit.alpha < 1.0f ? GetReflectance(hit.normal, direction, refracted, n1, n2) : 0.0f;

        origin += direction * hit.distance;
        normal = hit.normal;
        direction = refracted;

        if (iteration == 0)
        {
            const float4 hitInViewSpace = mul(float4(origin, 1), native::rt::camera.view);
            const float4 hitInClipSpace = mul(hitInViewSpace, native::rt::camera.projection);

            depth = hitInClipSpace.z / hitInClipSpace.w;
        }

        min = 0;
        iteration++;

        if (reflectance > 0.0f)
        {
            reflectionHit = GetEmptyHitInfo();
            Trace(origin + normal * native::rt::RAY_EPSILON, reflected, min, reflectionHit);
        }
    }

    native::rt::colorOutput[launchIndex] = float4(color.rgb, 1.0f);
    native::rt::depthOutput[launchIndex] = depth;
}
