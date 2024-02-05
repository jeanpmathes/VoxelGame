//  <copyright file="Fluid.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Section.hlsl"

[shader("closesthit")]
void FluidSectionClosestHit(inout native::rt::HitInfo payload, const native::rt::Attributes attributes)
{
    const vg::spatial::Info info = vg::spatial::GetCurrentInfo(attributes);
    const float4 baseColor = vg::section::GetFluidBaseColor(GET_PATH, info) * vg::decode::GetTintColor(info.data);

    SET_HIT_INFO(payload, info, vg::spatial::CalculateShading(info, baseColor.rgb));
    
    payload.alpha = baseColor.a;
}

[shader("closesthit")]
void FluidShadowClosestHit(inout native::rt::ShadowHitInfo hitInfo, native::rt::Attributes)
{
    hitInfo.isHit = true;
}
