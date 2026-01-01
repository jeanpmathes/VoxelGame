// <copyright file="PayloadRT.hlsl" company="VoxelGame">
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

// ReSharper disable CppRedundantEmptyStatement
// ReSharper disable CppInconsistentNaming
// @formatter:off

#ifndef NATIVE_SHADER_PAYLOAD_RT_HLSL
#define NATIVE_SHADER_PAYLOAD_RT_HLSL

/**
 * \brief The payloads passed along with the rays.
 */
namespace native
{
    namespace rt
    {
        /**
         * \brief The payload passed along with the standard rays.
         */
        struct [raypayload] HitInfo
        {
            uint2 color : read(caller, anyhit, closesthit, miss) : write(caller, anyhit, closesthit, miss);
            float2 normal : read(caller, anyhit, closesthit, miss) : write(caller, closesthit, miss);
            float3 position : read(caller, anyhit, closesthit, miss) : write(caller, anyhit, closesthit, miss);
            uint1 data : read(caller, anyhit, closesthit, miss) : write(caller, anyhit, closesthit, miss);
        };

        /**
         * \brief The payload passed along with the shadow rays.
         */
        struct [raypayload] ShadowHitInfo
        {
            bool isHit : read(caller) : write(caller, closesthit, miss);
        };
    }
}

// @formatter:on

#endif
