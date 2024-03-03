//  <copyright file="SpatialRT.hlsl" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#ifndef NATIVE_SHADER_SPATIAL_RT_HLSL
#define NATIVE_SHADER_SPATIAL_RT_HLSL

#include "CommonRT.hlsl"
#include "PayloadRT.hlsl"

#include "Space.hlsl"

/**
 * \brief Bindings required for all hit shaders of spatial objects.
 */
namespace native
{
    namespace rt
    {
        /**
         * \brief Per-material data.
         */
        struct Material
        {
            uint materialIndex;
        };

        ConstantBuffer<Material> material : register(b3);

        /**
         * \brief The instance data array.
         */
        ConstantBuffer<spatial::MeshData> instances[] : register(b4);

        /**
         * \brief The current acceleration structure.
         */
        RaytracingAccelerationStructure spaceBVH : register(t0);

        /**
         * \brief All vertex buffers.
         */
        StructuredBuffer<spatial::SpatialVertex> vertices[] : register(t1);

        Texture2D textureSlotOne[] : register(t0, space1);
        Texture2D textureSlotTwo[] : register(t0, space2);
    }
}

#endif
