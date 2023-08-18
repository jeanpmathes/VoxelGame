//  <copyright file="Miss.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Common.hlsl"

[shader("miss")]
void Miss(inout HitInfo payload : SV_RayPayload)
{
    payload.color = float3(0.5, 0.8, 0.9);
    payload.distance = VG_RAY_DISTANCE;
    payload.normal = float3(0.0, 0.0, 0.0);
    payload.alpha = 1.0;
}
