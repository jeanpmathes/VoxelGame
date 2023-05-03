#include "Common.hlsl"

[shader("closesthit")]
void ShadowClosestHit(inout ShadowHitInfo hitInfo, Attributes attributes)
{
    hitInfo.isHit = true;
}

[shader("miss")]
void ShadowMiss(inout ShadowHitInfo hitInfo : SV_RayPayload)
{
    hitInfo.isHit = false;
}
