// <copyright file="Packing.hlsl" company="VoxelGame">
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

#ifndef NATIVE_SHADER_PACKING_HLSL
#define NATIVE_SHADER_PACKING_HLSL

/**
 * \brief Utility functions for making data fit into a smaller space.
 */
namespace native
{
    namespace packing
    {
        float2 GetNonZeroSign(in float2 const value) { return float2(value.x >= 0.0f ? 1.0f : -1.0f, value.y >= 0.0f ? 1.0f : -1.0f); }

        /**
         * Encode the normal vector.
         * This uses the 'oct' method, see:
         * Cigolle, Z. H., Donow, S., Evangelakos, D., Mara, M., McGuire, M., & Meyer, Q. (2014). A survey of efficient representations for independent unit vectors. Journal of Computer Graphics Techniques, 3(2).
         */
        float2 PackNormal(in float3 const normal)
        {
            float2 const projection = normal.xy * (1.0f / (abs(normal.x) + abs(normal.y) + abs(normal.z)));
            return normal.z <= 0.0f ? (1.0f - abs(projection.yx)) * GetNonZeroSign(projection) : projection;
        }

        /**
         * Decode the normal vector.
         * This uses the 'oct' method, see:
         * Cigolle, Z. H., Donow, S., Evangelakos, D., Mara, M., McGuire, M., & Meyer, Q. (2014). A survey of efficient representations for independent unit vectors. Journal of Computer Graphics Techniques, 3(2).
         */
        float3 UnpackNormal(in float2 const encoded)
        {
            float3 normal = float3(encoded.xy, 1.0f - abs(encoded.x) - abs(encoded.y));
            if (normal.z < 0.0f) normal.xy = (1.0f - abs(normal.yx)) * GetNonZeroSign(normal.xy);
            return normalize(normal);
        }

        /**
         * Pack a RGBA color into two integers, each containing two channels.
         * Every channel is stored in 16 bits.
         */
        uint2 PackColor4(in float4 const color)
        {
            float4 const clamped = saturate(color);

            uint rg = uint(clamped.r * 65535.0f) | int(clamped.g * 65535.0f) << 16;
            uint ba = uint(clamped.b * 65535.0f) | int(clamped.a * 65535.0f) << 16;

            return uint2(rg, ba);
        }

        /**
         * Unpack a RGBA color from two integers, each containing two channels.
         */
        float4 UnpackColor4(in uint2 const packed)
        {
            return float4(float(packed.x & 0xFFFF) / 65535.0f, float(packed.x >> 16) / 65535.0f, float(packed.y & 0xFFFF) / 65535.0f, float(packed.y >> 16) / 65535.0f);
        }

        /**
         * Pack a RGB color into an integer.
         */
        uint PackColor3(in float3 const color) { return uint(color.r * 255.0f) | int(color.g * 255.0f) << 8 | int(color.b * 255.0f) << 16; }

        /**
         * Unpack a RGB color from an integer.
         */
        float3 UnpackColor3(in uint const packed) { return float3(float(packed & 0xFF) / 255.0f, float(packed >> 8 & 0xFF) / 255.0f, float(packed >> 16 & 0xFF) / 255.0f); }
    }
}

#endif
