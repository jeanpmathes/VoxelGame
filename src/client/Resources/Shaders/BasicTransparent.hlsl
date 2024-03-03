//  <copyright file="BasicTransparent.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Section.hlsl"

[shader("anyhit")]void BasicTransparentSectionAnyHit(
    inout native::rt::HitInfo payload, native::rt::Attributes const attributes)
{
    vg::spatial::Info const info      = vg::spatial::GetCurrentInfo(attributes);
    float4                  baseColor = vg::section::GetBasicBaseColor(GET_PATH, info);

    if (baseColor.a >= 0.1f)
    {
        float const currentRayT = RayTCurrent();
        float const storedRayT  = payload.distance;

        if (currentRayT < storedRayT)
        {
            if (baseColor.a >= 0.3f) baseColor *= vg::decode::GetTintColor(info.data);

            SET_HIT_INFO(payload, info, baseColor.rgb);
        }
    }
    else IgnoreHit();
}

[shader("closesthit")]void BasicTransparentSectionClosestHit(
    inout native::rt::HitInfo payload, native::rt::Attributes const attributes)
{
    vg::spatial::Info const info      = vg::spatial::GetCurrentInfo(attributes);
    float3 const            baseColor = payload.color;

    SET_HIT_INFO(payload, info, CalculateShading(info, baseColor.rgb));
}

[shader("anyhit")]void BasicTransparentShadowAnyHit(
    inout native::rt::ShadowHitInfo, native::rt::Attributes const attributes)
{
    vg::spatial::Info const info      = vg::spatial::GetCurrentInfo(attributes);
    float4                  baseColor = vg::section::GetBasicBaseColor(GET_SHADOW_PATH, info);

    bool const isHittingFront = dot(info.normal, WorldRayDirection()) > 0.0f;

    if (baseColor.a >= 0.1f && isHittingFront) AcceptHitAndEndSearch();
    else IgnoreHit();
}

[shader("closesthit")]void BasicTransparentShadowClosestHit(
    inout native::rt::ShadowHitInfo hitInfo, native::rt::Attributes) { hitInfo.isHit = true; }
