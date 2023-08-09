//  <copyright file="Shadow.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Common.hlsl"

[shader("miss")]
void ShadowMiss(inout ShadowHitInfo hitInfo : SV_RayPayload)
{
    hitInfo.isHit = false;
}
