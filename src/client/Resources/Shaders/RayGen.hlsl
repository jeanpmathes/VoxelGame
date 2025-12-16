//  <copyright file="RayGen.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "CommonRT.hlsl"
#include "RayGenRT.hlsl"

#include "Custom.hlsl"
#include "Hash.hlsl"
#include "Payload.hlsl"

/**
 * \brief Trace a ray.
 * \param origin The origin of the ray.
 * \param direction The direction of the ray.
 * \param min The minimum distance of the ray.
 * \param path The total path length of a ray chain, must be the total length over all previous rays.
 *             This is used to calculate the ray footprint and is carried to the hit shaders trough the alpha component.
 * \return The result of the trace.
 */
vg::ray::TraceResult Trace(float3 const origin, float3 const direction, float const min, float const path)
{
    RayDesc ray;
    ray.Origin    = origin;
    ray.Direction = direction;
    ray.TMin      = min;
    ray.TMax      = native::rt::RAY_DISTANCE;

    native::rt::HitInfo payload = vg::ray::GetInitialHitInfo(path);

    TraceRay(
        native::rt::spaceBVH,
        RAY_FLAG_CULL_BACK_FACING_TRIANGLES,
        native::rt::MASK_VISIBLE,
        RT_HIT_ARG(0),
        ray,
        payload);

    return vg::ray::GetTraceResult(payload, origin);
}

/**
 * \brief Parameters for fog.
 */
struct Fog
{
    float3 color;
    float  density;
    float  path;

    /**
     * \brief Create a default configuration, creating weak air-colored fog.
     */
    static Fog CreateDefault()
    {
        Fog fog;
        fog.color   = vg::custom.airFogColor;
        fog.density = vg::custom.airFogDensity;
        fog.path    = 0.0f;
        return fog;
    }

    /**
     * \brief Create a fog configuration for a fog volume.
     * \param color The color of the volume.
     * \return The fog configuration.
     */
    static Fog CreateVolume(float3 const color)
    {
        Fog fog;
        fog.color   = color;
        fog.density = 0.02000f;
        fog.path    = 0.0f;
        return fog;
    }

    /**
     * \brief Apply the fog to a trace result.
     * \param trace The trace result.
     */
    void Apply(inout vg::ray::TraceResult trace)
    {
        float const c   = path + trace.distance;
        float const fog = 1.0f - exp(-density * POW2(c));
        trace.color     = lerp(trace.color, float4(color, 1.0f), clamp(fog, 0.0f, 1.0f));
    }

    /**
     * \brief Extend the path length.
     * \param distance The distance by which to extend the path.
     */
    void Extend(float const distance) { path += distance; }
};

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
    float3 const normal,
    float3 const incident,
    float3 const transmission,
    float const  n1,
    float const  n2)
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

float2 GetSampleOffset(uint2 launchIndex, uint sampleIndex, uint gridSize, float strength)
{
    uint sx = sampleIndex % gridSize;
    uint sy = sampleIndex / gridSize;

    float2 base = (float2(sx, sy) + 0.5f) / gridSize;

    uint  seed = launchIndex.x * 1973u ^ launchIndex.y * 9277u ^ sampleIndex * 26699u;
    float jx   = (Random(seed + 0u) - 0.5f) / gridSize;
    float jy   = (Random(seed + 1u) - 0.5f) / gridSize;

    return saturate(base + float2(jx, jy) * strength);
}

void SampleAt(float2 offset, uint2 launchIndex, float2 dimensions, out float4 color, out float depth)
{
    // Given the dimension (w, h), the launch index is in the range [0, w - 1] x [0, h - 1].
    // This range is transformed to NDC space, i.e. [-1, 1] x [-1, 1].
    // Before that, a jitter in [0, 1] x [0, 1] is added to the launch index for antialiasing.
    float2 pixel = (float2(launchIndex) + offset) / dimensions * 2.0f - 1.0f;

    // DirectX textures have their origin in the top-left corner, while NDC has it in the bottom-left corner.
    // Therefore, the y coordinate is inverted.
    float4 const targetInProjectionSpace = float4(pixel.x, pixel.y * -1.0f, 1.0f, 1.0f);

    float3 targetInViewSpace = mul(targetInProjectionSpace, native::rt::camera.projectionI).xyz;
    float3 direction         = mul(float4(targetInViewSpace.xyz, 0.0f), native::rt::camera.viewI).xyz;
    float3 origin            = mul(float4(0.0f, 0.0f, 0.0f, 1.0f), native::rt::camera.viewI).xyz;

    float3 normal = float3(0.0f, 0.0f, 0.0f);
    float  min    = native::rt::camera.near * length(direction);
    direction     = normalize(direction);

    float const relativeY = 1.0f - (pixel.y + 1.0f) / 2.0f;
    Fog         fog       = Fog::CreateDefault();

    if ((vg::custom.fogOverlapSize > 0.0f && relativeY < vg::custom.fogOverlapSize) || (vg::custom.fogOverlapSize < 0.0f
        && relativeY > vg::custom.fogOverlapSize + 1.0f)) fog = Fog::CreateVolume(vg::custom.fogOverlapColor);

    color = 0;
    depth = 0;

    int   iteration = 0;
    float path      = length(direction);

    float                reflectance = 0.0f;
    vg::ray::TraceResult reflection  = vg::ray::GetEmptyTraceResult();

    while (color.a < 1.0f && iteration < 5 && any(direction))
    {
        vg::ray::TraceResult main = Trace(origin - normal * native::rt::RAY_EPSILON, direction, min, path);

        fog.Apply(main);
        fog.Extend(main.distance);

        path += main.distance;

        float const alpha = main.color.a;

        main.color = lerp(main.color, reflection.color, reflectance);

        float const  a   = color.a + (1.0f - color.a) * main.color.a;
        float3 const rgb = (color.rgb * color.a + main.color.rgb * (1.0f - color.a) * main.color.a) / a;

        color.rgb = rgb;
        color.a   = a;

        if (iteration == 0)
        {
            float4 const hitInViewSpace = mul(float4(main.position, 1), native::rt::camera.view);
            float4 const hitInClipSpace = mul(hitInViewSpace, native::rt::camera.projection);

            depth = hitInClipSpace.z / hitInClipSpace.w;
        }

        if (color.a >= 1.0f) break;

        bool const incoming = dot(direction, main.normal) < 0;
        bool const outgoing = !incoming;

        float const n1 = incoming ? 1.00f : 1.33f;
        float const n2 = incoming ? 1.33f : 1.00f;

        if (outgoing) main.normal *= -1.0f;

        float3 const refracted = normalize(refract(direction, main.normal, n1 / n2));
        float3 const reflected = normalize(reflect(direction, main.normal));

        // If a reflectance ray is needed for the current hit, it is traced this iteration.
        // The main ray of the next iteration is the refraction ray, except if the reflectance is total.
        reflectance = alpha < 1.0f
                          ? GetReflectance(incoming ? main.normal : main.normal * -1.0f, direction, refracted, n1, n2)
                          : 0.0f;

        origin    = main.position;
        normal    = main.normal;
        direction = refracted;

        min = 0;
        iteration++;

        bool mainRayIsReflection = false;

        if (reflectance > 0.0f)
        {
            // This shader normally has a main ray which can pass trough transparent objects.
            // At each hit, a reflection ray can be traced.

            if (reflectance < 1.0f)
            {
                // This is the base case. The reflection is not continued and thus not added to the path.

                reflection = Trace(origin + normal * native::rt::RAY_EPSILON, reflected, min, path);

                fog.Apply(reflection);

                // Intentionally not updating the path length.
            }
            else
            {
                // This is the total reflection case. The main ray becomes the reflection ray.

                reflectance = 0.0f;
                normal      = normal * -1.0f;
                direction   = reflected;

                mainRayIsReflection = true;
            }
        }

        if (alpha < 1.0f && !mainRayIsReflection)
        {
            if (incoming) fog = Fog::CreateVolume(main.fogColor);
            else fog          = Fog::CreateDefault();
        }
    }
}

[shader("raygeneration")]void RayGen()
{
    uint2 const  launchIndex = DispatchRaysIndex().xy;
    float2 const dimensions  = float2(DispatchRaysDimensions().xy);

    uint const minGrid = vg::custom.antiAliasing.isEnabled ? vg::custom.antiAliasing.minimumSamplingGridSize : 1;
    uint const maxGrid = vg::custom.antiAliasing.isEnabled ? vg::custom.antiAliasing.maximumSamplingGridSize : 1;

    float const varianceThreshold = vg::custom.antiAliasing.varianceThreshold;
    float const depthThreshold    = vg::custom.antiAliasing.depthThreshold;

    float const offsetStrength = vg::custom.antiAliasing.isEnabled ? 1.0f : 0.0f;

    uint const minSampleCount = minGrid * minGrid;
    uint const maxSampleCount = maxGrid * maxGrid;

    uint samples = 0;

    // Variance is estimated using the Welford online algorithm.
    float meanLuminance         = 0;
    float sumOfSquaresLuminance = 0;

    float3 accumulator = 0;
    float  minDepth    = 1.0f;
    float  maxDepth    = 0.0f;

    float4 color;
    float  depth;

    for (uint index = 0; index < minSampleCount; index++)
    {
        SampleAt(GetSampleOffset(launchIndex, index, minGrid, offsetStrength), launchIndex, dimensions, color, depth);

        accumulator += color.rgb;
        minDepth    = min(minDepth, depth);
        maxDepth    = max(maxDepth, depth);
        samples     += 1;

        float luminance = native::GetLuminance(color.rgb);
        float delta     = luminance - meanLuminance;

        meanLuminance         += delta / samples;
        sumOfSquaresLuminance += delta * (luminance - meanLuminance);
    }

    float variance = samples > 1 ? sumOfSquaresLuminance / samples : 0;

    if (vg::custom.antiAliasing.isEnabled && (variance > varianceThreshold || (maxDepth - minDepth) > depthThreshold))
        for (uint index = minSampleCount; index < maxSampleCount; index++)
        {
            SampleAt(
                GetSampleOffset(launchIndex, index, maxGrid, offsetStrength),
                launchIndex,
                dimensions,
                color,
                depth);

            accumulator += color.rgb;
            minDepth    = min(minDepth, depth);
            // Maximum depth was only needed for the threshold check.
            samples += 1;
        }

    float4 result = RGBA(accumulator / samples);

    if (vg::custom.antiAliasing.showSamplingRate)
    {
        bool  hasUsedMaxSamples = samples > minSampleCount;
        float luminance         = native::GetLuminance(result.rgb);

        float3 visualization = hasUsedMaxSamples ? float3(1.0f, 0.0f, luminance) : float3(0.0f, 1.0f, luminance);
        result               = RGBA(visualization);
    }

    native::rt::colorOutput[launchIndex] = result;
    native::rt::depthOutput[launchIndex] = minDepth;
}
