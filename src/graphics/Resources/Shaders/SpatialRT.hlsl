// <copyright file="SpatialRT.hlsl" company="VoxelGame">
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
