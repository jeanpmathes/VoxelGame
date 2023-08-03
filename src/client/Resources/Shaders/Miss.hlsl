//  <copyright file="Miss.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "Common.hlsl"

[shader("miss")]
void Miss(inout HitInfo payload : SV_RayPayload)
{
    payload.colorAndDistance = float4(0.5, 0.8, 0.9, -1.0);
}
