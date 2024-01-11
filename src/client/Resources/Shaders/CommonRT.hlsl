//  <copyright file="CommonRT.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#ifndef VG_SHADER_COMMON_RT_H
#define VG_SHADER_COMMON_RT_H

// ReSharper disable once CppUnusedIncludeDirective
#include "Common.hlsl"

struct Attributes
{
    float2 barycentrics;
};

float3 GetBarycentrics(in Attributes attributes)
{
    return float3(
        1.0 - attributes.barycentrics.x - attributes.barycentrics.y,
        attributes.barycentrics.x,
        attributes.barycentrics.y);
}

// todo: replace the following defines with static const variables

#define VG_RAY_DISTANCE 100000.0
#define VG_RAY_EPSILON 0.0001

#define VG_MASK_VISIBLE (1 << 0)
#define VG_MASK_SHADOW (1 << 1)

#define VG_HIT_ARG(index) index, 0, index

#endif
