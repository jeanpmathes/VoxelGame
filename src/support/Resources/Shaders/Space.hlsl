//  <copyright file="Space.hlsl" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

#ifndef NATIVE_SHADER_SPACE_HLSL
#define NATIVE_SHADER_SPACE_HLSL

/**
 * \brief Basic data required for the spatial rendering.
 */
namespace native
{
    namespace spatial
    {
        /**
         * \brief The type used to define the vertices of a spatial mesh.
         */
        struct SpatialVertex
        {
            float3 position;
            uint data;
        };

        /**
         * \brief Data available for all shaders in the space rendering.
         */
        struct Global
        {
            float time;
            uint3 textureSize;
            
            float3 lightDir;
            float minLight;
            float minShadow;
        };

        ConstantBuffer<Global> global : register(b2);

        /**
         * \brief Per-instance data for each mesh.
         */
        struct MeshData
        {
            float4x4 world;
            float4x4 worldNormal;
        };

        static const uint VERTICES_PER_QUAD = 4;
    }
}

#endif
