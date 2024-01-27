//  <copyright file="RayGenRT.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#ifndef NATIVE_SHADER_RAYGEN_RT_HLSL
#define NATIVE_SHADER_RAYGEN_RT_HLSL

/**
 * \brief Bindings required only for the ray generation shader.
 */
namespace native
{
    namespace rt
    {
        /**
         * \brief The camera data.
         */
        struct CameraParameters
        {
            float4x4 view;
            float4x4 projection;
            float4x4 viewI;
            float4x4 projectionI;
            float near;
            float far;
        };

        ConstantBuffer<CameraParameters> camera : register(b0);

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
