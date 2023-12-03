#include "Section.hlsl"

[shader("anyhit")]
void BasicTransparentSectionAnyHit(inout HitInfo payload, const Attributes attributes)
{
    const Info info = GetCurrentInfo(attributes);
    float4 baseColor = GetBasicBaseColor(info);

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
void BasicTransparentSectionClosestHit(inout HitInfo payload, const Attributes attributes)
{
    const Info info = GetCurrentInfo(attributes);
    const float3 baseColor = payload.color;

    SET_HIT_INFO(payload, info, CalculateShading(info, baseColor.rgb));
}

[shader("anyhit")]
void BasicTransparentShadowAnyHit(inout ShadowHitInfo, const Attributes attributes)
{
    const Info info = GetCurrentInfo(attributes);
    float4 baseColor = GetBasicBaseColor(info);

    const bool isHittingFront = dot(info.normal, WorldRayDirection()) > 0.0;

    if (baseColor.a >= 0.1 && isHittingFront) AcceptHitAndEndSearch();
    else IgnoreHit();
}

[shader("closesthit")]
void BasicTransparentShadowClosestHit(inout ShadowHitInfo hitInfo, Attributes)
{
    hitInfo.isHit = true;
}
