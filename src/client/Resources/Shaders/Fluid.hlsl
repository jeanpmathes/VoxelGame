//  <copyright file="Fluid.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Payload.hlsl"
#include "Section.hlsl"

[shader("closesthit")]void FluidSectionClosestHit(
    inout native::rt::HitInfo payload, native::rt::Attributes const attributes)
{
    float const path = vg::ray::GetPathLength(payload);
    
    vg::spatial::Info const info  = vg::spatial::GetCurrentInfo(attributes);
    float4 const            tint  = vg::decode::GetTintColor(info.data);
    float4 const            color = vg::section::GetFluidBaseColor(path, info) * tint;

    float4 const dominant = vg::section::GetFluidDominantColor(info);
    vg::ray::SetFogColor(payload, dominant.rgb * tint.rgb);

    SET_FINAL_HIT_INFO(payload, info, float4(vg::spatial::CalculateShading(info, color.rgb), color.a));
}

[shader("closesthit")]void FluidShadowClosestHit(inout native::rt::ShadowHitInfo hitInfo, native::rt::Attributes)
{
    hitInfo.isHit = true;
}
