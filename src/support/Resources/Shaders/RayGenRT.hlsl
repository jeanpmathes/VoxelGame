//  <copyright file="RayGenRT.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#ifndef NATIVE_SHADER_RAYGEN_RT_HLSL
#define NATIVE_SHADER_RAYGEN_RT_HLSL

#include "CameraRT.hlsl"

/**
 * \brief Bindings required only for the ray generation shader.
 */
namespace native
{
    namespace rt
    {
        /**
         * \brief The output color buffer. The shader must write to this buffer.
         */
        RWTexture2D<float4> colorOutput : register(u0);

        /**
         * \brief The output depth buffer. The shader must write to this buffer.
         */
        RWTexture2D<float> depthOutput : register(u1);

        /**
         * \brief The acceleration structure for the space.
         */
        RaytracingAccelerationStructure spaceBVH : register(t0);
    }
}

#endif
