//  <copyright file="RayGen.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "CommonRT.hlsl"
#include "PayloadRT.hlsl"
#include "RayGenRT.hlsl"

/**
 * \brief Create an empty hit info / ray payload struct.
 * \return The empty hit info struct.
 */
native::rt::HitInfo GetEmptyHitInfo()
{
    native::rt::HitInfo payload;

    payload.color    = float3(0.0f, 0.0f, 0.0f);
    payload.alpha    = 0.0f;
    payload.normal   = float3(0.0f, 0.0f, 0.0f);
    payload.distance = native::rt::RAY_DISTANCE; // Allows any-hit to check if it is closer then write to payload.

    return payload;
}

/**
 * \brief Trace a ray.
 * \param origin The origin of the ray.
 * \param direction The direction of the ray.
 * \param min The minimum distance of the ray.
 * \param payload The payload to write to.
 * \param path The total path length of a ray chain, must be the total length over all previous rays.
 *             This is used to calculate the ray footprint and is carried to the hit shaders trough the alpha component.
 * \return The total path length of the ray chain, including the current ray. 
 */
float Trace(
    float3 const origin, float3 const direction, float const min, inout native::rt::HitInfo payload, inout float path)
{
    RayDesc ray;
    ray.Origin    = origin;
    ray.Direction = direction;
    ray.TMin      = min;
    ray.TMax      = native::rt::RAY_DISTANCE;

    payload.alpha = path;

    TraceRay(
        native::rt::spaceBVH,
        RAY_FLAG_CULL_BACK_FACING_TRIANGLES,
        native::rt::MASK_VISIBLE,
        RT_HIT_ARG(0),
        ray,
        payload);

    return path + payload.distance;
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
    float3 const normal, float3 const incident, float3 const transmission, float const n1, float const n2)
{
    if (!any(transmission)) return 1.0f;

    // Schlick's approximation:

    float const r0                      = POW2((n1 - n2) / (n1 + n2));
    float const cosThetaI               = dot(normal, incident) * -1;
    float const cosThetaT               = dot(normal, transmission);
    bool const  totalInternalReflection = n1 > n2 && POW2(n1 / n2) * (1.0f - POW2(cosThetaI)) > 1.0f;

    if (totalInternalReflection) return 1.0f;

    float const cos = n1 <= n2 ? cosThetaI : cosThetaT;

    return clamp(r0 + (1.0f - r0) * POW5(1.0f - cos), 0.0f, 1.0f);
}

[shader("raygeneration")]void RayGen()
{
    uint2 const  launchIndex = DispatchRaysIndex().xy;
    float2 const dimensions  = float2(DispatchRaysDimensions().xy);

    // Given the dimension (w, h), the launch index is in the range [0, w - 1] x [0, h - 1].
    // This range is transformed to NDC space, i.e. [-1, 1] x [-1, 1].
    float2 const pixel = (float2(launchIndex) + 0.5f) / dimensions * 2.0f - 1.0f;

    // DirectX textures have their origin at the top-left corner, while NDC has it at the bottom-left corner.
    // Therefore, the y coordinate is inverted.
    float4 const targetInProjectionSpace = float4(pixel.x, pixel.y * -1.0f, 1.0f, 1.0f);

    float3 targetInViewSpace = mul(targetInProjectionSpace, native::rt::camera.projectionI).xyz;
    float3 direction         = mul(float4(targetInViewSpace.xyz, 0.0f), native::rt::camera.viewI).xyz;
    float3 origin            = mul(float4(0.0f, 0.0f, 0.0f, 1.0f), native::rt::camera.viewI).xyz;

    float3 normal = float3(0.0f, 0.0f, 0.0f);
    float  min    = native::rt::camera.near * length(direction);
    direction     = normalize(direction);

    int    iteration = 0;
    float4 color     = 0;
    float  depth     = 0;
    float  path      = 0;

    float               reflectance   = 0.0f;
    native::rt::HitInfo reflectionHit = GetEmptyHitInfo();

    while (color.a < 1.0f && iteration < 10 && any(direction))
    {
        native::rt::HitInfo hit = GetEmptyHitInfo();
        path += Trace(origin - normal * native::rt::RAY_EPSILON, direction, min, hit, path);

        hit.color = lerp(hit.color, reflectionHit.color, reflectance);
        hit.alpha = lerp(hit.alpha, reflectionHit.alpha, reflectance);

        float const  a   = color.a + (1.0f - color.a) * hit.alpha;
        float3 const rgb = (color.rgb * color.a + hit.color * (1.0f - color.a) * hit.alpha) / a;

        color.rgb = rgb;
        color.a   = a;

        bool const incoming = dot(direction, hit.normal) < 0;
        bool const outgoing = !incoming;
        
        float const n1       = incoming ? 1.00f : 1.33f;
        float const n2       = incoming ? 1.33f : 1.00f;

        float3 const sn = hit.normal;

        if (outgoing) hit.normal *= -1.0f;
        
        float3 const refracted = normalize(refract(direction, hit.normal, n1 / n2));
        float3 const reflected = normalize(reflect(direction, hit.normal));

        // If an reflectance ray is needed for the current hit, it is traced this iteration.
        // The main ray of the next iteration is the refraction ray.
        reflectance = hit.alpha < 1.0f ? GetReflectance(sn, direction, refracted, n1, n2) : 0.0f;

        origin += direction * hit.distance;
        normal    = hit.normal;
        direction = refracted;

        if (iteration == 0)
        {
            float4 const hitInViewSpace = mul(float4(origin, 1), native::rt::camera.view);
            float4 const hitInClipSpace = mul(hitInViewSpace, native::rt::camera.projection);

            depth = hitInClipSpace.z / hitInClipSpace.w;
        }

        min = 0;
        iteration++;

        if (reflectance > 0.0f)
        {
            // This shader normally has a main ray which can pass trough transparent objects.
            // At each hit, a reflection ray can be traced.

            if (reflectance < 1.0f)
            {
                // This is the base case. The reflection is not continued and thus not added to the path.

                reflectionHit = GetEmptyHitInfo();
                Trace(origin + normal * native::rt::RAY_EPSILON, reflected, min, reflectionHit, path);
            }
            else
            {
                // This is the total reflection case. The main ray becomes the reflection ray.

                reflectance = 0.0f;
                normal      = sn;
                direction   = reflected;
            }
        }
    }
    
    native::rt::colorOutput[launchIndex] = float4(color.rgb, 1.0f);
    native::rt::depthOutput[launchIndex] = depth;
}
