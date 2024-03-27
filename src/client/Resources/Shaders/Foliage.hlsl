//  <copyright file="Foliage.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Section.hlsl"

[shader("anyhit")]void FoliageSectionAnyHit(inout native::rt::HitInfo payload, native::rt::Attributes const attributes)
{
    vg::spatial::Info const info      = vg::spatial::GetCurrentInfo(attributes);
    float4                  baseColor = vg::section::GetFoliageBaseColor(GET_PATH, info);

    if (baseColor.a >= 0.1f)
    {
        float const currentRayT = RayTCurrent();
        float const storedRayT  = payload.distance;

        if (currentRayT < storedRayT)
        {
            if (baseColor.a >= 0.3f) baseColor *= vg::decode::GetTintColor(info.data);

            SET_INTERMEDIATE_HIT_INFO(payload, info, baseColor.rgb);
        }
    }
    else IgnoreHit();
}

[shader("closesthit")]void FoliageSectionClosestHit(
    inout native::rt::HitInfo payload, native::rt::Attributes const attributes)
{
    vg::spatial::Info const info      = vg::spatial::GetCurrentInfo(attributes);
    float3 const            baseColor = payload.color;

    SET_FINAL_HIT_INFO(payload, info, CalculateShading(info, baseColor), 1.0f);
}

[shader("anyhit")]void FoliageShadowAnyHit(inout native::rt::ShadowHitInfo, native::rt::Attributes const attributes)
{
    vg::spatial::Info const info      = vg::spatial::GetCurrentInfo(attributes);
    float4                  baseColor = vg::section::GetFoliageBaseColor(GET_SHADOW_PATH, info);

    bool const isHittingFront = dot(info.normal, WorldRayDirection()) > 0.0f;

    if (baseColor.a >= 0.1f && isHittingFront) AcceptHitAndEndSearch();
    else IgnoreHit();
}

[shader("closesthit")]void FoliageShadowClosestHit(inout native::rt::ShadowHitInfo hitInfo, native::rt::Attributes)
{
    hitInfo.isHit = true;
}
