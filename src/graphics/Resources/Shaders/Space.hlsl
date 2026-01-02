// <copyright file="Space.hlsl" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
            uint   data;
        };

        /**
         * \brief Data available for all shaders in the space rendering.
         */
        struct Global
        {
            float time;
            uint3 textureSize;

            float3 lightDirection;
            float  lightIntensity;
            float3 lightColor;
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

        static uint const VERTICES_PER_QUAD = 4;

        SamplerState sampler : register(s0);
    }
}

#endif
