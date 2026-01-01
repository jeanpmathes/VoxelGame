// <copyright file="RayGenRT.hlsl" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
