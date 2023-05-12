//  <copyright file="Shadow.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Common.hlsl"

[shader("closesthit")]
void ShadowClosestHit(inout ShadowHitInfo hitInfo, Attributes)
{
    hitInfo.isHit = true;
}

[shader("miss")]
void ShadowMiss(inout ShadowHitInfo hitInfo : SV_RayPayload)
{
    hitInfo.isHit = false;
}
