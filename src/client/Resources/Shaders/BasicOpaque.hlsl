//  <copyright file="BasicOpaque.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Section.hlsl"

[shader("closesthit")]void BasicOpaqueSectionClosestHit(
    inout native::rt::HitInfo payload, native::rt::Attributes const attributes)
{
    vg::spatial::Info const info      = vg::spatial::GetCurrentInfo(attributes);
    float4                  baseColor = vg::section::GetBasicBaseColor(GET_PATH, info);

    if (baseColor.a >= 0.3f) baseColor *= vg::decode::GetTintColor(info.data);

    baseColor.a = 1.0f;

    SET_HIT_INFO(payload, info, CalculateShading(info, baseColor.rgb));
}

[shader("closesthit")]void BasicOpaqueShadowClosestHit(inout native::rt::ShadowHitInfo hitInfo, native::rt::Attributes)
{
    hitInfo.isHit = true;
}
