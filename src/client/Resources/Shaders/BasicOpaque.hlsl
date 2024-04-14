//  <copyright file="BasicOpaque.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Payload.hlsl"
#include "Section.hlsl"

[shader("closesthit")]void BasicOpaqueSectionClosestHit(
    inout native::rt::HitInfo payload, native::rt::Attributes const attributes)
{
    float const path = vg::ray::GetPathLength(payload);
    
    vg::spatial::Info const info      = vg::spatial::GetCurrentInfo(attributes);
    float4                  baseColor = vg::section::GetBasicBaseColor(path, info);

    if (baseColor.a >= 0.3f) baseColor *= vg::decode::GetTintColor(info.data);

    SET_FINAL_HIT_INFO(payload, info, RGBA(CalculateShading(info, baseColor.rgb)));
}

[shader("closesthit")]void BasicOpaqueShadowClosestHit(inout native::rt::ShadowHitInfo hitInfo, native::rt::Attributes)
{
    hitInfo.isHit = true;
}
