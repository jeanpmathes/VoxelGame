#include "Section.hlsl"

[shader("anyhit")]
void FoliageSectionAnyHit(inout HitInfo payload, const Attributes attributes)
{
    const Info info = GetCurrentInfo(attributes);
    float4 baseColor = GetFoliageBaseColor(info);

    if (baseColor.a >= 0.1)
    {
        const float currentRayT = RayTCurrent();
        const float storedRayT = payload.distance;

        if (currentRayT < storedRayT)
        {
            if (baseColor.a >= 0.3)
            {
                baseColor *= decode::GetTintColor(info.data);
            }

            SET_HIT_INFO(payload, info, baseColor.rgb);
        }
    }
    else IgnoreHit();
}

[shader("closesthit")]
void FoliageSectionClosestHit(inout HitInfo payload, const Attributes attributes)
{
    const Info info = GetCurrentInfo(attributes);
    const float3 baseColor = payload.color;

    SET_HIT_INFO(payload, info, CalculateShading(info, baseColor));
}

[shader("anyhit")]
void FoliageShadowAnyHit(inout ShadowHitInfo, const Attributes attributes)
{
    const Info info = GetCurrentInfo(attributes);
    float4 baseColor = GetFoliageBaseColor(info);

    const bool isHittingFront = dot(info.normal, WorldRayDirection()) > 0.0;

    if (baseColor.a >= 0.1 && isHittingFront) AcceptHitAndEndSearch();
    else IgnoreHit();
}

[shader("closesthit")]
void FoliageShadowClosestHit(inout ShadowHitInfo hitInfo, Attributes)
{
    hitInfo.isHit = true;
}
