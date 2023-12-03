//  <copyright file="Hit.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Section.hlsl"

[shader("closesthit")]
void FluidSectionClosestHit(inout HitInfo payload, const Attributes attributes)
{
    const Info info = GetCurrentInfo(attributes);
    const float4 baseColor = GetFluidBaseColor(info) * decode::GetTintColor(info.data);

    SET_HIT_INFO(payload, info, CalculateShading(info, baseColor.rgb));
    payload.alpha = baseColor.a;
}

[shader("closesthit")]
void FluidShadowClosestHit(inout ShadowHitInfo hitInfo, Attributes)
{
    hitInfo.isHit = true;
}
