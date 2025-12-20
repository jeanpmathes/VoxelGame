// <copyright file="Animation.hlsl" company="VoxelGame">
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

#ifndef NATIVE_SHADER_ANIMATION_HLSL
#define NATIVE_SHADER_ANIMATION_HLSL

#include "Space.hlsl"

#define SPACE space3

/*
 * This contains the required structures and bindings for the animation system.
 */
namespace native
{
    namespace animation
    {
        struct WorkInfo
        {
            uint value;
        };

        /**
         * \brief Index of the work load.
         */
        ConstantBuffer<WorkInfo> index : register(b0, space1);

        /**
         * \brief Size of the work load.
         */
        ConstantBuffer<WorkInfo> size : register(b1, space1);

        /**
         * \brief The source vertex data buffer. This data is read and transformed by the animation shader.
         */
        StructuredBuffer<spatial::SpatialVertex> source[] : register(t0, SPACE);

        /**
         * \brief The destination vertex data buffer. This data is written to by the animation shader.
         */
        RWStructuredBuffer<spatial::SpatialVertex> destination[] : register(u0, SPACE);
    }
}

#endif
