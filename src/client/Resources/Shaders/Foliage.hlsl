//  <copyright file="Foliage.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Payload.hlsl"
#include "Section.hlsl"

[shader("anyhit")]void FoliageSectionAnyHit(inout native::rt::HitInfo payload, native::rt::Attributes const attributes)
{
    float const path = vg::ray::GetPathLength(payload);
    
    vg::spatial::Info const info  = vg::spatial::GetCurrentInfo(attributes);
    float4                  color = vg::section::GetFoliageBaseColor(path, info);

    if (color.a >= 0.1f)
    {
        float const currentRayT = RayTCurrent();
        float const storedRayT  = vg::ray::GetRayDistance(payload);

        if (currentRayT < storedRayT)
        {
            if (color.a >= 0.3f) color *= vg::decode::GetTintColor(info.data);

            SET_INTERMEDIATE_HIT_INFO(payload, info, RGBA(color));
        }
    }
    else IgnoreHit();
}

[shader("closesthit")]void FoliageSectionClosestHit(
    inout native::rt::HitInfo payload, native::rt::Attributes const attributes)
{
    vg::spatial::Info const info  = vg::spatial::GetCurrentInfo(attributes);
    float3 const            color = vg::ray::GetColor(payload).rgb;

    SET_FINAL_HIT_INFO(payload, info, RGBA(CalculateShading(info, color)));
}

[shader("anyhit")]void FoliageShadowAnyHit(inout native::rt::ShadowHitInfo, native::rt::Attributes const attributes)
{
    vg::spatial::Info const info  = vg::spatial::GetCurrentInfo(attributes);
    float4                  color = vg::section::GetFoliageBaseColor(vg::section::SHADOW_PATH, info);

    bool const isHittingFront = dot(info.normal, WorldRayDirection()) > 0.0f;

    if (color.a >= 0.1f && isHittingFront) AcceptHitAndEndSearch();
    else IgnoreHit();
}

[shader("closesthit")]void FoliageShadowClosestHit(inout native::rt::ShadowHitInfo hitInfo, native::rt::Attributes)
{
    hitInfo.isHit = true;
}
