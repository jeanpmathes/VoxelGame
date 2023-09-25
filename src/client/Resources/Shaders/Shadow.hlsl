//  <copyright file="Shadow.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Payloads.hlsl"

[shader("miss")]
void ShadowMiss(inout ShadowHitInfo hitInfo)
{
    hitInfo.isHit = false;
}
