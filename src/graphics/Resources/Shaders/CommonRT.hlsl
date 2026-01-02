// <copyright file="CommonRT.hlsl" company="VoxelGame">
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

#ifndef NATIVE_SHADER_COMMON_RT_HLSL
#define NATIVE_SHADER_COMMON_RT_HLSL

#include "Common.hlsl"

#define RT_HIT_ARG(index) index, 0, index

namespace native
{
    namespace rt
    {
        /**
         * \brief The attributes of a ray hit.
         */
        struct Attributes
        {
            float2 barycentrics;
        };

        /**
         * \brief Get the barycentric coordinates of the hit.
         * \param attributes The attributes of the hit.
         * \return The barycentric coordinates of the hit.
         */
        float3 GetBarycentrics(in Attributes attributes)
        {
            return float3(1.0f - attributes.barycentrics.x - attributes.barycentrics.y, attributes.barycentrics.x, attributes.barycentrics.y);
        }

        static float const RAY_DISTANCE = 100000.0f;
        static float const RAY_EPSILON  = 0.0001f;

        static int const MASK_VISIBLE = 1 << 0;
        static int const MASK_SHADOW  = 1 << 1;
    }
}

#endif
