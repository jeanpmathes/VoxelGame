//  <copyright file="CameraRT.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#ifndef NATIVE_SHADER_CAMERA_RT_HLSL
#define NATIVE_SHADER_CAMERA_RT_HLSL

/**
 * \brief Camera data for ray tracing.
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

            float spread;
        };

        ConstantBuffer<CameraParameters> camera : register(b0);
    }
}

#endif
