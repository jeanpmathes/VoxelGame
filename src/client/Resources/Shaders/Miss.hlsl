//  <copyright file="Miss.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "CommonRT.hlsl"
#include "PayloadRT.hlsl"

[shader("miss")]void Miss(inout native::rt::HitInfo payload)
{
    payload.color  = float3(0.5f, 0.8f, 0.9f);
    payload.alpha  = 1.0f;
    payload.normal = float3(0.0f, 0.0f, 0.0f);
    payload.distance = native::rt::RAY_DISTANCE;
}
