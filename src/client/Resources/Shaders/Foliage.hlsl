//  <copyright file="Foliage.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Section.hlsl"

[shader("anyhit")]
void FoliageSectionAnyHit(inout native::rt::HitInfo payload, const native::rt::Attributes attributes)
{
    const vg::spatial::Info info = vg::spatial::GetCurrentInfo(attributes);
    float4 baseColor = vg::section::GetFoliageBaseColor(info);

    if (baseColor.a >= 0.1f)
    {
        const float currentRayT = RayTCurrent();
        const float storedRayT = payload.distance;

        if (currentRayT < storedRayT)
        {
            if (baseColor.a >= 0.3f)
            {
                baseColor *= vg::decode::GetTintColor(info.data);
            }

            SET_HIT_INFO(payload, info, baseColor.rgb);
        }
    }
    else IgnoreHit();
}

[shader("closesthit")]
void FoliageSectionClosestHit(inout native::rt::HitInfo payload, const native::rt::Attributes attributes)
{
    const vg::spatial::Info info = vg::spatial::GetCurrentInfo(attributes);
    const float3 baseColor = payload.color;

    SET_HIT_INFO(payload, info, CalculateShading(info, baseColor));
}

[shader("anyhit")]
void FoliageShadowAnyHit(inout native::rt::ShadowHitInfo, const native::rt::Attributes attributes)
{
    const vg::spatial::Info info = vg::spatial::GetCurrentInfo(attributes);
    float4 baseColor = vg::section::GetFoliageBaseColor(info);

    const bool isHittingFront = dot(info.normal, WorldRayDirection()) > 0.0f;

    if (baseColor.a >= 0.1f && isHittingFront) AcceptHitAndEndSearch();
    else IgnoreHit();
}

[shader("closesthit")]
void FoliageShadowClosestHit(inout native::rt::ShadowHitInfo hitInfo, native::rt::Attributes)
{
    hitInfo.isHit = true;
}
