//  <copyright file="Hit.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Basic.hlsl"

[shader("closesthit")]
void BasicOpaqueSectionClosestHit(inout HitInfo payload, const Attributes attributes)
{
    const Info info = GetCurrentInfo(attributes);
    float4 baseColor = GetBasicBaseColor(info);

    if (baseColor.a >= 0.3)
    {
        baseColor *= decode::GetTintColor(info.data);
    }
    
    baseColor.a = 1.0;

    payload.colorAndDistance = float4(CalculateShading(info.normal, baseColor.rgb), RayTCurrent());
}

[shader("closesthit")]
void BasicOpaqueShadowClosestHit(inout ShadowHitInfo hitInfo, Attributes)
{
    hitInfo.isHit = true;
}
