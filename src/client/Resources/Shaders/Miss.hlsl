//  <copyright file="Miss.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "CommonRT.hlsl"
#include "PayloadRT.hlsl"

#include "Custom.hlsl"

[shader("miss")]void Miss(inout native::rt::HitInfo payload)
{
    payload.color = vg::SKY_COLOR;
    payload.alpha  = 1.0f;
    payload.normal = float3(0.0f, 0.0f, 0.0f);
    payload.distance = native::rt::RAY_DISTANCE;
}
