//  <copyright file="Miss.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#include "CommonRT.hlsl"

#include "Custom.hlsl"
#include "Payload.hlsl"

[shader("miss")]void Miss(inout native::rt::HitInfo payload)
{
    SET_MISS_INFO(payload, RGBA(vg::SKY_COLOR));
}
